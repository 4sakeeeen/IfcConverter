using IngrDataReadLib;
using System;
using System.Collections.Generic;
using System.Linq;
using Xbim.Common;
using Xbim.Common.Step21;
using Xbim.Ifc;
using Xbim.Ifc2x3.FacilitiesMgmtDomain;
using Xbim.Ifc4.GeometricConstraintResource;
using Xbim.Ifc4.GeometryResource;
using Xbim.Ifc4.Interfaces;
using Xbim.Ifc4.Kernel;
using Xbim.Ifc4.ProductExtension;
using Xbim.Ifc4.RepresentationResource;
using Xbim.Ifc4.SharedBldgElements;
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
                if (errorCode != eErrorCode.E_OK) { throw new Exception("Open vue file was failed"); }

                SourceFileInfo sourceFileInfo = new();
                reader.GetSourceFileInfo(ref sourceFileInfo);
                this.AuthorProduct = sourceFileInfo.AuthorProduct;
                this.SorceFileName = sourceFileInfo.SorceFileName;
                this.ViewerProduct = sourceFileInfo.ViewerProduct;
                this.FileMajorVersion = sourceFileInfo.FileMajorVersion;
                this.FileMinorVersion = sourceFileInfo.FileMinorVersion;

                while (errorCode != eErrorCode.E_EOF)
                {
                    reader.GetCurrentGraphicLabelProperties(out Array propertyArray, out errorCode);
                    reader.GetCurrentGraphicIdentifier(out string linkage, out string moniker, out string spfuid);
                    reader.GetCurrentGraphicElement(out eGraphicType gtype, ref geometry, ref materialProperties, out errorCode);
                    reader.GetNextGraphicElement(out errorCode);
                    GraphicElements.Add(new VueGraphicElement(propertyArray, linkage, moniker, spfuid, gtype, geometry, materialProperties));
                }

                reader.CloseVueFile();
            }
            catch (Exception ex)
            {
                throw new Exception("Initialize vue file failed", ex);
            }
        }

        
        public void SaveToIfc(string projectName, string targetFilePath)
        {
            this._Model = IfcStore.Create(
                new XbimEditorCredentials
                {
                    ApplicationDevelopersName = "App Devs Name",
                    ApplicationFullName = "Full name of Application",
                    ApplicationIdentifier = "Application ID",
                    ApplicationVersion = "App Ver.1",
                    EditorsFamilyName = "Erokhov",
                    EditorsGivenName = "Aleksandr",
                    EditorsOrganisationName = "Enter Engineerings Ptd."
                },
                XbimSchemaVersion.Ifc4,
                XbimStoreType.InMemoryModel);

            try
            {
                using ITransaction transaction = this._Model.BeginTransaction("Create IFC file");

                var project = this._Model.Instances.New<IfcProject>();
                project.Name = $"{projectName}: Project";
                project.Initialize(ProjectUnits.SIUnitsUK);

                var site = this._Model.Instances.New<IfcSite>();
                site.Name = $"{projectName}: Site";
                site.CompositionType = IfcElementCompositionEnum.ELEMENT;
                site.ObjectPlacement = this._Model.Instances.New<IfcLocalPlacement>(
                    placement => placement.RelativePlacement = this._Model.Instances.New<IfcAxis2Placement3D>(
                        axis3d => axis3d.Location = this._Model.Instances.New<IfcCartesianPoint>(
                            point => point.SetXYZ(0, 0, 0))));
                project.AddSite(site);

                var building = this._Model.Instances.New<IfcBuilding>();
                building.Name = $"{projectName}: Building";
                building.CompositionType = IfcElementCompositionEnum.ELEMENT;
                building.ObjectPlacement = this._Model.Instances.New<IfcLocalPlacement>(placement =>
                {
                    placement.RelativePlacement = this._Model.Instances.New<IfcAxis2Placement3D>(
                        axis3d => axis3d.Location = this._Model.Instances.New<IfcCartesianPoint>(
                            point => point.SetXYZ(0, 0, 0)));
                });
                site.AddBuilding(building);

                var buildingStorey = this._Model.Instances.New<IfcBuildingStorey>();
                buildingStorey.Name = $"{projectName}: Building Storey";
                buildingStorey.CompositionType = IfcElementCompositionEnum.ELEMENT;
                buildingStorey.ObjectPlacement = this._Model.Instances.New<IfcLocalPlacement>(placement =>
                {
                    placement.RelativePlacement = this._Model.Instances.New<IfcAxis2Placement3D>(
                        axis3d => axis3d.Location = this._Model.Instances.New<IfcCartesianPoint>(
                            point => point.SetXYZ(0, 0, 0)));
                });
                building.AddToSpatialDecomposition(buildingStorey);

                int idx = 0;
                foreach (VueGraphicElement element in this.GraphicElements)
                {
                    var c = idx++ / this.GraphicElements.Count;

                    if (idx == 12000)
                    {
                        break;
                    }

                    var member = this._Model.Instances.New<IfcMember>();
                    member.Name = element.Attributes["Name"];
                    member.Representation = this._Model.Instances.New<IfcProductDefinitionShape>();
                    member.Representation.Representations.Add(element.GeometryGroup.Convert(this._Model));
                    member.ObjectPlacement = this._Model.Instances.New<IfcLocalPlacement>(placement =>
                    {
                        placement.RelativePlacement = this._Model.Instances.New<IfcAxis2Placement3D>(
                            axis3d => axis3d.Location = this._Model.Instances.New<IfcCartesianPoint>(
                                point => point.SetXYZ(0, 0, 0)));
                    });
                    buildingStorey.AddElement(member);
                }
                transaction.Commit();
                this._Model.SaveAs($"{targetFilePath}_converted", StorageType.Ifc);
            }
            catch (Exception ex)
            {
                throw new Exception("Convert and save to IFC failed", ex);
            }
        }


        public ProductTreeItem? GenerateTreeViewContextItems()
        {
            if (_Model == null) { return null; }

            var buidingStoreyAggregate = this._Model.Instances.Where<IfcRelContainedInSpatialStructure>(x => x.RelatingStructure is IfcBuildingStorey);
            var aggregates = new Queue<IfcRelAggregates>(this._Model.Instances.Where<IfcRelAggregates>(x => x.Equals(x)));
            List<ProductTreeItem> allItems = new();
            ProductTreeItem? root = null;

            if (buidingStoreyAggregate.Count() == 1)
            {
                root = new ProductTreeItem(buidingStoreyAggregate.First().RelatingStructure.Name ?? string.Empty, buidingStoreyAggregate.First().RelatingStructure.GlobalId);
                allItems.Add(root);
                buidingStoreyAggregate.First().RelatedElements.ToList().ForEach(x => root.Children.Add(new ProductTreeItem(x.Name ?? string.Empty, x.GlobalId)));
                allItems.AddRange(root.Children);
            }

            while (aggregates.Count > 3)
            {
                IfcRelAggregates currAggregate = aggregates.Dequeue();
                ProductTreeItem? currItem = allItems.FirstOrDefault(x => x.Guid == currAggregate.RelatingObject.GlobalId);

                if (currItem != null)
                {
                    currAggregate.RelatedObjects.ToList().ForEach(x =>
                    {
                        var t = new ProductTreeItem(x.Name ?? string.Empty, x.GlobalId);
                        allItems.Add(t);
                        currItem.Children.Add(t);
                    });
                }
                else
                {
                    aggregates.Enqueue(currAggregate);
                }
            }

            return root;
        }
    }
}
