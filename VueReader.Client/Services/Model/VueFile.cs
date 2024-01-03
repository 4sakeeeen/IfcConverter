using IfcConverter.Client.Services.Filter;
using IfcConverter.Client.Services.Model.Base;
using IngrDataReadLib;
using System;
using System.Collections.Generic;
using System.Linq;
using Xbim.Common;
using Xbim.Common.ExpressValidation;
using Xbim.Common.Step21;
using Xbim.Ifc;
using Xbim.Ifc4.GeometricConstraintResource;
using Xbim.Ifc4.GeometryResource;
using Xbim.Ifc4.Interfaces;
using Xbim.Ifc4.Kernel;
using Xbim.Ifc4.PresentationAppearanceResource;
using Xbim.Ifc4.ProductExtension;
using Xbim.Ifc4.RepresentationResource;
using Xbim.Ifc4.SharedBldgElements;
using Xbim.Ifc4.TopologyResource;
using Xbim.IO;

namespace IfcConverter.Client.Services.Model
{
    public sealed class VueFile
    {
        private IfcStore? _Model;

        public string AuthorProduct { get; }

        public string SorceFileName { get; }

        public string ViewerProduct { get; }

        public string FileMajorVersion { get; }

        public string FileMinorVersion { get; }

        public List<VueGraphicElement> GraphicElements { get; } = new();

        public VueHierarchy Hierarchy { get; } = new();

        public VueFile(string filePath, ushort tessellationTolerance = 20)
        {
            try
            {
                RdMaterialProperties? materialProperties = null;
                IngrGeom? geometry = null;
                var errorCode = eErrorCode.E_OK;
                var reader = new IngrDataReader();
                reader.SetTessellatorForType(eGraphicType.INVALID_TYPE);
                reader.SetTessellatorTolerance(tessellationTolerance);

                reader.OpenVueFile(filePath, out errorCode);
                if (errorCode != eErrorCode.E_OK) throw new Exception("Open vue file was failed.");

                SourceFileInfo sourceFileInfo = new();
                reader.GetSourceFileInfo(ref sourceFileInfo);
                AuthorProduct = sourceFileInfo.AuthorProduct;
                SorceFileName = sourceFileInfo.SorceFileName;
                ViewerProduct = sourceFileInfo.ViewerProduct;
                FileMajorVersion = sourceFileInfo.FileMajorVersion;
                FileMinorVersion = sourceFileInfo.FileMinorVersion;

                while (errorCode != eErrorCode.E_EOF)
                {
                    reader.GetCurrentGraphicIdentifier(out string linkage, out string moniker, out string spfuid);

                    reader.GetCurrentGraphicLabelProperties(out Array propertyArray, out errorCode);
                    if (errorCode != eErrorCode.E_OK) throw new Exception("Unable to get label properties of current graphic element.");

                    reader.GetCurrentGraphicElement(out eGraphicType gtype, ref geometry, ref materialProperties, out errorCode);
                    if (errorCode != eErrorCode.E_OK) throw new Exception("Unable to get graphic data of current element.");

                    var graphicElement = new VueGraphicElement(propertyArray, linkage, moniker, spfuid, gtype, geometry);
                    GraphicElements.Add(graphicElement);

                    reader.GetNextGraphicElement(out errorCode);
                    if (errorCode != eErrorCode.E_OK && errorCode != eErrorCode.E_EOF) throw new Exception("Unable to get next graphic element.");
                }

                reader.CloseVueFile();

                GraphicElements
                    .OrderBy(x =>
                    {
                        x.Attributes.TryGetValue("System Path", out string? systemPath);
                        return systemPath;
                    })
                    .ToList().ForEach(Hierarchy.Insert);
            }
            catch (Exception ex)
            {
                throw new Exception("Initialize vue file failed", ex);
            }
        }
      
        public void SaveToIfc(string projectName, string targetFilePath, ObjectFilter? filter = null)
        {
            _Model = IfcStore.Create(
                new XbimEditorCredentials
                {
                    ApplicationDevelopersName = "App Devs Name",
                    ApplicationFullName = "Full name of Application",
                    ApplicationIdentifier = "Application ID",
                    ApplicationVersion = "App Ver.1",
                    EditorsFamilyName = "Erokhov",
                    EditorsGivenName = "Aleksandr",
                    EditorsOrganisationName = "Default Company"
                },
                XbimSchemaVersion.Ifc4,
                XbimStoreType.EsentDatabase);

            try
            {
                using ITransaction transaction = _Model.BeginTransaction("Create IFC file");

                var project = _Model.Instances.New<IfcProject>();
                project.Name = $"{projectName}: Project";
                project.Initialize(ProjectUnits.SIUnitsUK);

                var site = _Model.Instances.New<IfcSite>();
                site.Name = $"{projectName}: Site";
                site.CompositionType = IfcElementCompositionEnum.ELEMENT;
                site.ObjectPlacement = _Model.Instances.New<IfcLocalPlacement>(
                    placement => placement.RelativePlacement = _Model.Instances.New<IfcAxis2Placement3D>(
                        axis3d => axis3d.Location = _Model.Instances.New<IfcCartesianPoint>(
                            point => point.SetXYZ(0, 0, 0))));
                project.AddSite(site);

                var building = _Model.Instances.New<IfcBuilding>();
                building.Name = $"{projectName}: Building";
                building.CompositionType = IfcElementCompositionEnum.ELEMENT;
                building.ObjectPlacement = _Model.Instances.New<IfcLocalPlacement>(placement =>
                {
                    placement.RelativePlacement = _Model.Instances.New<IfcAxis2Placement3D>(
                        axis3d => axis3d.Location = _Model.Instances.New<IfcCartesianPoint>(
                            point => point.SetXYZ(0, 0, 0)));
                });
                site.AddBuilding(building);

                var buildingStorey = _Model.Instances.New<IfcBuildingStorey>();
                buildingStorey.Name = $"{projectName}: Building Storey";
                buildingStorey.CompositionType = IfcElementCompositionEnum.ELEMENT;
                buildingStorey.ObjectPlacement = _Model.Instances.New<IfcLocalPlacement>(placement =>
                {
                    placement.RelativePlacement = _Model.Instances.New<IfcAxis2Placement3D>(
                        axis3d => axis3d.Location = _Model.Instances.New<IfcCartesianPoint>(
                            point => point.SetXYZ(0, 0, 0)));
                });
                building.AddToSpatialDecomposition(buildingStorey);

                transaction.Commit();

                Hierarchy.Filter = filter;
                Hierarchy.Root = buildingStorey;
                Hierarchy.Convert(_Model);
            }
            catch (Exception ex)
            {
                throw new Exception("Convert and save to IFC failed", ex);
            }
            finally
            {
                var validations = new List<ValidationResult>();

                using ITransaction transaction = _Model.BeginTransaction("Start validation");

                validations.AddRange(_Model.Instances
                    .OfType<IfcCartesianPoint>()
                    .SelectMany(cartesianPoint => cartesianPoint.Validate()));

                validations.AddRange(_Model.Instances
                    .OfType<IfcPolyline>()
                    .SelectMany(polyline => polyline.Validate()));

                validations.AddRange(_Model.Instances
                    .OfType<IfcPolyline>()
                    .SelectMany(polyline => polyline.Validate()));

                validations.AddRange(_Model.Instances
                    .OfType<IfcPolyline>()
                    .SelectMany(polyline => polyline.Validate()));

                validations.AddRange(_Model.Instances
                    .OfType<IfcFace>()
                    .SelectMany(face => face.Validate()));

                validations.AddRange(_Model.Instances
                    .OfType<IfcShapeRepresentation>()
                    .SelectMany(shapeRepresentation => shapeRepresentation.Validate()));

                validations.AddRange(_Model.Instances
                    .OfType<IfcAxis2Placement3D>()
                    .SelectMany(axis3d => axis3d.Validate()));

                validations.AddRange(_Model.Instances
                    .OfType<IfcLocalPlacement>()
                    .SelectMany(localPlacement => localPlacement.Validate()));

                validations.AddRange(_Model.Instances
                    .OfType<IfcMember>()
                    .SelectMany(member => member.Validate()));

                validations.AddRange(_Model.Instances
                    .OfType<IfcStyledItem>()
                    .SelectMany(styled => styled.Validate()));

                validations.AddRange(_Model.Instances
                    .OfType<IfcSurfaceStyle>()
                    .SelectMany(surfaceStyle => surfaceStyle.Validate()));

                transaction.Commit();
                _Model.SaveAs(targetFilePath, StorageType.Ifc);
            }
        }
    }
}
