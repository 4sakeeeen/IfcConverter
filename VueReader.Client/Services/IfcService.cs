using IfcConverter.Domain.Models.Vue;
using IfcConverter.Domain.Models.Vue.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using Xbim.Common;
using Xbim.Common.Step21;
using Xbim.Ifc;
using Xbim.Ifc4.ElectricalDomain;
using Xbim.Ifc4.GeometricConstraintResource;
using Xbim.Ifc4.GeometricModelResource;
using Xbim.Ifc4.GeometryResource;
using Xbim.Ifc4.Interfaces;
using Xbim.Ifc4.Kernel;
using Xbim.Ifc4.MeasureResource;
using Xbim.Ifc4.ProductExtension;
using Xbim.Ifc4.PropertyResource;
using Xbim.Ifc4.RepresentationResource;
using Xbim.Ifc4.SharedBldgElements;
using Xbim.Ifc4.SharedBldgServiceElements;
using Xbim.Ifc4.TopologyResource;
using Xbim.IO;

namespace IfcConverter.Client.Services
{
    public static class IfcService
    {
        public static void InTransaction(IModel model, string transactionName, Action action)
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


        public static IfcBuildingStorey CreateBuildingStorey(IfcBuilding building, string name)
        {
            IfcBuildingStorey? buildingStorey = null;

            InTransaction(building.Model, "Creating building story", () =>
            {
                buildingStorey = building.Model.Instances.New<IfcBuildingStorey>();
                buildingStorey.Name = name;
                buildingStorey.CompositionType = IfcElementCompositionEnum.ELEMENT;
                buildingStorey.ObjectPlacement = building.Model.Instances.New<IfcLocalPlacement>(
                    place => place.RelativePlacement = building.Model.Instances.New<IfcAxis2Placement3D>(
                        axis3d => axis3d.Location = building.Model.Instances.New<IfcCartesianPoint>(p => p.SetXYZ(0, 0, 0))));
                building.AddElement(buildingStorey);

            });

            return buildingStorey ?? throw new Exception();
        }


        public static IfcElement CreateProduct(IfcStore model, IfcBuilding building, GraphicElement graphicElement)
        {
            IfcElement? element = null;
            Dictionary<SmartClassID, IfcClassID> mapping = new()
            {
                { SmartClassID.CSPS_MEMBER_PART_PPRISMATIC, IfcClassID.IFCMEMBER },
                { SmartClassID.CSPS_SLAB_ENTITY, IfcClassID.IFCSLAB }
            };

            InTransaction(model, $"Creating element", () =>
            {
                element = mapping[graphicElement.Smart3DClass] switch
                {
                    IfcClassID.IFCMEMBER => model.Instances.New<IfcMember>(),
                    IfcClassID.IFCSLAB => model.Instances.New<IfcSlab>(),
                    _ => throw new Exception(),
                };
                element.Name = graphicElement.LabelProperties["Name"];
                element.Representation = model.Instances.New<IfcProductDefinitionShape>();
                element.Representation.Representations.Add(model.Instances.New<IfcShapeRepresentation>(shrep =>
                {
                    shrep.ContextOfItems = model.Instances.OfType<IfcGeometricRepresentationContext>().FirstOrDefault();
                    // converting PLANE_TYPE
                    graphicElement.Geometry.Elements.Planes.ForEach(
                        dataPlane => shrep.Items.Add(model.Instances.New<IfcFacetedBrep>(
                            brep => brep.Outer = model.Instances.New<IfcClosedShell>(shell => shell.CfsFaces.Add(model.Instances.New<IfcFace>(face =>
                                face.Bounds.Add(model.Instances.New<IfcFaceOuterBound>(
                                        bound => bound.Bound = model.Instances.New<IfcPolyLoop>(loop =>
                                        {
                                            if (dataPlane.Boundaries == null) { throw new Exception("Boundary of plane is null"); }
                                            dataPlane.GetAllPositions().ToList().ForEach(p => loop.Polygon.Add(model.Instances.New<IfcCartesianPoint>(point => point.SetXYZ(p.X * 1000, p.Y * 1000, p.Z * 1000))));
                                        })
                                    ))
                                ))
                            )
                        ))
                    );
                    // converting LINE_TYPE
                    graphicElement.Geometry.Elements.Lines.ForEach(
                        dataLine => shrep.Items.Add(model.Instances.New<IfcPolyline>(
                            line =>
                            {
                                if (dataLine.StartPoint == null || dataLine.EndPoint == null) { throw new Exception(); }
                                line.Points.Add(model.Instances.New<IfcCartesianPoint>(
                                    point => point.SetXYZ(dataLine.StartPoint.X * 1000, dataLine.StartPoint.Y * 1000, dataLine.StartPoint.Z * 1000)));
                                line.Points.Add(model.Instances.New<IfcCartesianPoint>(
                                    point => point.SetXYZ(dataLine.EndPoint.X * 1000, dataLine.EndPoint.Y * 1000, dataLine.EndPoint.Z * 1000)));
                            })));
                }));
                // object placement direved from geometry, then placement set to (0, 0, 0)
                element.ObjectPlacement = model.Instances.New<IfcLocalPlacement>(
                    placement =>
                        placement.RelativePlacement = model.Instances.New<IfcAxis2Placement3D>(
                            axis3d => axis3d.Location = model.Instances.New<IfcCartesianPoint>(point => point.SetXYZ(0, 0, 0))));
                model.Instances.New<IfcRelDefinesByProperties>(
                    rdbp =>
                    {
                        rdbp.Name = "PropertySetAssociation#1";
                        rdbp.Description = "Property set association #1";
                        rdbp.RelatedObjects.Add(element);
                        rdbp.RelatingPropertyDefinition = model.Instances.New<IfcPropertySet>(
                            pset =>
                            {
                                pset.Name = "Smart Plant 3D";
                                pset.Description = "Exported from Smart3D model";
                                pset.HasProperties.Add(model.Instances.New<IfcPropertySingleValue>(
                                    prop =>
                                    {
                                        prop.Name = "Class ID";
                                        prop.Description = "))";
                                        prop.NominalValue = new IfcText(graphicElement.Smart3DClass.ToString());
                                    }));

                                foreach (KeyValuePair<string, string?> propValue in graphicElement.LabelProperties)
                                {
                                    pset.HasProperties.Add(model.Instances.New<IfcPropertySingleValue>(
                                        prop =>
                                        {
                                            prop.Name = propValue.Key;
                                            prop.Description = "))";
                                            prop.NominalValue = new IfcText(propValue.Value);
                                        }));
                                }
                            });
                    });

                if (graphicElement.LabelProperties.ContainsKey("System Path"))
                {
                    //IfcBuildingElementProxy decomposesProxy = BuildHierarchyInModelAndGetLastProxy(building, graphicElement.LabelProperties["System Path"] ?? "");
                    model.Instances.New<IfcRelAggregates>(
                        aggregate =>
                        {
                            //aggregate.RelatingObject = decomposesProxy;
                            aggregate.RelatedObjects.Add(element);
                        });
                }
                else
                {
                    building.AddElement(element);
                }
            });

            return element ?? throw new Exception();
        }


        public static void BuildAndDecomposeHierarchy(IfcBuildingStorey storey, IfcProduct product, string? hierarchyPath)
        {
            if (hierarchyPath != null)
            {
                List<string> hierarchyPathNodes = hierarchyPath.Contains('\\') ? new List<string>(hierarchyPath.Split('\\')) : new List<string> { hierarchyPath };
                IfcBuildingElementProxy? firstInHierarchy = storey.Model.Instances.Where<IfcBuildingElementProxy>(p => !p.Decomposes.Any() && p.Name == hierarchyPathNodes.First()).FirstOrDefault();

                if (firstInHierarchy == null)
                {
                    firstInHierarchy = storey.Model.Instances.New<IfcBuildingElementProxy>(p => p.Name = hierarchyPathNodes.First());
                    storey.AddElement(firstInHierarchy);
                }

                if (hierarchyPathNodes.Count == 1)
                {
                    storey.Model.Instances.New<IfcRelAggregates>(
                       aggregate =>
                       {
                           aggregate.RelatingObject = firstInHierarchy;
                           aggregate.RelatedObjects.Add(product);
                       });
                    return;
                }

                IfcBuildingElementProxy lastHierarchyProxy = firstInHierarchy;
                IfcBuildingElementProxy? suggestion = null;

                hierarchyPathNodes.Skip(1).ToList().ForEach(node =>
                {
                    foreach (IfcRelAggregates aggregate in lastHierarchyProxy.IsDecomposedBy)
                    {
                        if (aggregate.RelatedObjects.FirstOrDefault(obj => obj.Name == node) is IfcBuildingElementProxy suggestionAsProxy && suggestionAsProxy is not null)
                        {
                            lastHierarchyProxy = suggestionAsProxy;
                            suggestion = suggestionAsProxy;
                            break;
                        }
                        else
                        {
                            suggestion = null;
                        }
                    }

                    if (suggestion == null)
                    {
                        storey.Model.Instances.New<IfcRelAggregates>(
                            aggregate =>
                            {
                                aggregate.RelatingObject = lastHierarchyProxy;
                                lastHierarchyProxy = storey.Model.Instances.New<IfcBuildingElementProxy>(proxy => proxy.Name = node);
                                aggregate.RelatedObjects.Add(lastHierarchyProxy);
                            });
                    }
                });

                storey.Model.Instances.New<IfcRelAggregates>(
                   aggregate =>
                   {
                       aggregate.RelatingObject = lastHierarchyProxy;
                       aggregate.RelatedObjects.Add(product);
                   });
            }
            else
            {
                storey.AddElement(product);
            }
        }


        public static IfcBuildingElementProxy BuildHierarchyInModelAndGetLastProxy(IfcBuildingStorey root, string hierarchyPath)
        {
            try
            {
                List<string> hierarchyPathNodes = hierarchyPath.Contains('\\') ? new List<string>(hierarchyPath.Split('\\')) : new List<string> { hierarchyPath };

                IfcBuildingElementProxy? firstInHierarchy = root.Model.Instances.Where<IfcBuildingElementProxy>(p => !p.Decomposes.Any() && p.Name == hierarchyPathNodes.First()).FirstOrDefault();
                if (firstInHierarchy == null)
                {
                    firstInHierarchy = root.Model.Instances.New<IfcBuildingElementProxy>(p => p.Name = hierarchyPathNodes.First());
                    root.AddElement(firstInHierarchy);
                }

                if (hierarchyPathNodes.Count == 1) { return firstInHierarchy; }

                IfcBuildingElementProxy lastHierarchyProxy = firstInHierarchy;
                IfcBuildingElementProxy? suggestion = null;

                hierarchyPathNodes.GetRange(1, hierarchyPathNodes.Count - 1).ForEach(node =>
                {
                    foreach (IfcRelAggregates aggregate in lastHierarchyProxy.IsDecomposedBy)
                    {
                        suggestion = (IfcBuildingElementProxy)aggregate.RelatedObjects.FirstOrDefault(obj => obj.Name == node);
                        if (suggestion != null)
                        {
                            lastHierarchyProxy = suggestion;
                            break;
                        }
                    }

                    if (suggestion == null)
                    {
                        root.Model.Instances.New<IfcRelAggregates>(
                            aggregate =>
                            {
                                aggregate.RelatingObject = lastHierarchyProxy;
                                lastHierarchyProxy = root.Model.Instances.New<IfcBuildingElementProxy>(proxy => proxy.Name = node);
                                aggregate.RelatedObjects.Add(lastHierarchyProxy);
                            });
                    }
                });

                return lastHierarchyProxy;
            }
            catch (Exception ex)
            {
                throw new Exception("Error in building hierarchy in model", ex);
            }

        }
    }
}
