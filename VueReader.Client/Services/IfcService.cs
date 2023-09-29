using IfcConverter.Domain.Models.Vue.Common.Collections;
using System;
using System.Linq;
using Xbim.Common;
using Xbim.Common.Step21;
using Xbim.Ifc;
using Xbim.Ifc4.GeometricConstraintResource;
using Xbim.Ifc4.GeometricModelResource;
using Xbim.Ifc4.GeometryResource;
using Xbim.Ifc4.Interfaces;
using Xbim.Ifc4.Kernel;
using Xbim.Ifc4.ProductExtension;
using Xbim.Ifc4.RepresentationResource;
using Xbim.Ifc4.SharedBldgElements;
using Xbim.Ifc4.TopologyResource;
using Xbim.IO;

namespace IfcConverter.Client.Services
{
    public static class IfcService
    {
        public static void InTransaction(IfcStore model, string transactionName, Action action)
        {
            using ITransaction transaction = model.BeginTransaction(transactionName);
            action.Invoke();
            transaction.Commit();
        }


        public static IfcStore CreateAndInitModel(string projectName)
        {
            XbimEditorCredentials credentials = new()
            {
                ApplicationDevelopersName = "App Devs Name",
                ApplicationFullName = "Full name of Application",
                ApplicationIdentifier = "Application ID",
                ApplicationVersion = "App Ver.1",
                EditorsFamilyName = "Erokhov",
                EditorsGivenName = "Aleksandr",
                EditorsOrganisationName = "Enter Engineerings Ptd."
            };
            var model = IfcStore.Create(credentials, XbimSchemaVersion.Ifc4, XbimStoreType.InMemoryModel);

            InTransaction(model, $"Creating and initializing project '{projectName}'", () =>
            {
                IfcProject project = model.Instances.New<IfcProject>();
                project.Initialize(ProjectUnits.SIUnitsUK);
                project.Name = projectName;
            });

            return model;
        }


        public static IfcBuilding CreateBuilding(IfcStore model, string name)
        {
            IfcBuilding? building = null;

            InTransaction(model, $"Creating building '{name}'", () =>
            {
                building = model.Instances.New<IfcBuilding>();
                building.Name = name;
                building.CompositionType = IfcElementCompositionEnum.ELEMENT;
                building.ObjectPlacement = model.Instances.New<IfcLocalPlacement>(
                    place => place.RelativePlacement = model.Instances.New<IfcAxis2Placement3D>(
                        axis3d => axis3d.Location = model.Instances.New<IfcCartesianPoint>(p => p.SetXYZ(0, 0, 0))));
                model.Instances.OfType<IfcProject>().First().AddBuilding(building);
            });

            return building ?? throw new Exception();
        }


        public static IfcProduct CreateProduct(IfcStore model, IfcBuilding building, Elements geometryElements)
        {
            IfcMember? member = null;

            InTransaction(model, "Creating a product", () =>
            {
                member = model.Instances.New<IfcMember>();
                member.Name = "A Member";
                member.Description = "Out first member with representations";
                member.Representation = model.Instances.New<IfcProductDefinitionShape>();
                member.Representation.Representations.Add(model.Instances.New<IfcShapeRepresentation>(shrep =>
                {
                    shrep.ContextOfItems = model.Instances.OfType<IfcGeometricRepresentationContext>().FirstOrDefault();
                    geometryElements.Planes.ForEach(
                        plane => shrep.Items.Add(model.Instances.New<IfcFacetedBrep>(
                            brep => brep.Outer = model.Instances.New<IfcClosedShell>(shell => shell.CfsFaces.Add(model.Instances.New<IfcFace>(face =>
                                face.Bounds.Add(model.Instances.New<IfcFaceOuterBound>(
                                        bound => bound.Bound = model.Instances.New<IfcPolyLoop>(loop =>
                                        {
                                            if (plane.Boundaries == null) { throw new Exception("Boundary of plane is null"); }
                                            plane.GetAllPositions().ToList().ForEach(p => loop.Polygon.Add(model.Instances.New<IfcCartesianPoint>(point => point.SetXYZ(p.X * 1000, p.Y * 1000, p.Z * 1000))));
                                        })
                                    ))
                                ))
                            )
                        ))
                    );
                }));
                member.ObjectPlacement = model.Instances.New<IfcLocalPlacement>(placement =>
                {
                    placement.RelativePlacement = model.Instances.New<IfcAxis2Placement3D>(axis3d =>
                    {
                        axis3d.Location = model.Instances.New<IfcCartesianPoint>(point => point.SetXYZ(0, 0, 0));
                        axis3d.RefDirection = model.Instances.New<IfcDirection>(dir => dir.SetXYZ(0, 1, 0));
                        axis3d.Axis = model.Instances.New<IfcDirection>(dir => dir.SetXYZ(0, 0, 1));
                    });
                });
                building.AddElement(member);
            });

            return member ?? throw new Exception();
        }
    }
}
