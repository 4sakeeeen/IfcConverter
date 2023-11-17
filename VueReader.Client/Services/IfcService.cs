using IfcConverter.Domain.Models.Vue;
using IfcConverter.Domain.Models.Vue.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using Xbim.Common;
using Xbim.Ifc;
using Xbim.Ifc4.GeometricConstraintResource;
using Xbim.Ifc4.GeometricModelResource;
using Xbim.Ifc4.GeometryResource;
using Xbim.Ifc4.Kernel;
using Xbim.Ifc4.MeasureResource;
using Xbim.Ifc4.ProductExtension;
using Xbim.Ifc4.PropertyResource;
using Xbim.Ifc4.RepresentationResource;
using Xbim.Ifc4.SharedBldgElements;
using Xbim.Ifc4.TopologyResource;

namespace IfcConverter.Client.Services
{
    public sealed class IfcService
    {
        private readonly IfcStore? _IfcStore;


        public static void InTransaction(IModel model, string transactionName, Action action)
        {
            using ITransaction transaction = model.BeginTransaction(transactionName);
            action.Invoke();
            transaction.Commit();
        }


        public IfcElement CreateProduct(IfcBuilding building, GraphicElement graphicElement)
        {
            IfcElement? element = null;
            Dictionary<SmartClassID, IfcClassID> mapping = new()
            {
                { SmartClassID.CSPS_MEMBER_PART_PPRISMATIC, IfcClassID.IFCMEMBER },
                { SmartClassID.CSPS_SLAB_ENTITY, IfcClassID.IFCSLAB }
            };

            InTransaction(this._IfcStore, $"Creating element", () =>
            {
                element = mapping[graphicElement.Smart3DClass] switch
                {
                    IfcClassID.IFCMEMBER => this._IfcStore.Instances.New<IfcMember>(),
                    IfcClassID.IFCSLAB => this._IfcStore.Instances.New<IfcSlab>(),
                    _ => throw new Exception(),
                };
                element.Name = graphicElement.LabelProperties["Name"];
                element.Representation = this._IfcStore.Instances.New<IfcProductDefinitionShape>();
                element.Representation.Representations.Add(this._IfcStore.Instances.New<IfcShapeRepresentation>(shrep =>
                {
                    shrep.ContextOfItems = this._IfcStore.Instances.OfType<IfcGeometricRepresentationContext>().FirstOrDefault();
                    // converting PLANE_TYPE
                    graphicElement.Geometry.Elements.Planes.ForEach(
                        dataPlane => shrep.Items.Add(this._IfcStore.Instances.New<IfcFacetedBrep>(
                            brep => brep.Outer = this._IfcStore.Instances.New<IfcClosedShell>(shell => shell.CfsFaces.Add(this._IfcStore.Instances.New<IfcFace>(face =>
                                face.Bounds.Add(this._IfcStore.Instances.New<IfcFaceOuterBound>(
                                        bound => bound.Bound = this._IfcStore.Instances.New<IfcPolyLoop>(loop =>
                                        {
                                            if (dataPlane.Boundaries == null) { throw new Exception("Boundary of plane is null"); }
                                            dataPlane.GetAllPositions().ToList().ForEach(p => loop.Polygon.Add(this._IfcStore.Instances.New<IfcCartesianPoint>(point => point.SetXYZ(p.X * 1000, p.Y * 1000, p.Z * 1000))));
                                        })
                                    ))
                                ))
                            )
                        ))
                    );
                    // converting LINE_TYPE
                    graphicElement.Geometry.Elements.Lines.ForEach(
                        dataLine => shrep.Items.Add(this._IfcStore.Instances.New<IfcPolyline>(
                            line =>
                            {
                                if (dataLine.StartPoint == null || dataLine.EndPoint == null) { throw new Exception(); }
                                line.Points.Add(this._IfcStore.Instances.New<IfcCartesianPoint>(
                                    point => point.SetXYZ(dataLine.StartPoint.X * 1000, dataLine.StartPoint.Y * 1000, dataLine.StartPoint.Z * 1000)));
                                line.Points.Add(this._IfcStore.Instances.New<IfcCartesianPoint>(
                                    point => point.SetXYZ(dataLine.EndPoint.X * 1000, dataLine.EndPoint.Y * 1000, dataLine.EndPoint.Z * 1000)));
                            })));
                }));
                // object placement direved from geometry, then placement set to (0, 0, 0)
                element.ObjectPlacement = this._IfcStore.Instances.New<IfcLocalPlacement>(
                    placement =>
                        placement.RelativePlacement = this._IfcStore.Instances.New<IfcAxis2Placement3D>(
                            axis3d => axis3d.Location = this._IfcStore.Instances.New<IfcCartesianPoint>(point => point.SetXYZ(0, 0, 0))));
                this._IfcStore.Instances.New<IfcRelDefinesByProperties>(
                    rdbp =>
                    {
                        rdbp.Name = "PropertySetAssociation#1";
                        rdbp.Description = "Property set association #1";
                        rdbp.RelatedObjects.Add(element);
                        rdbp.RelatingPropertyDefinition = this._IfcStore.Instances.New<IfcPropertySet>(
                            pset =>
                            {
                                pset.Name = "Smart Plant 3D";
                                pset.Description = "Exported from Smart3D model";
                                pset.HasProperties.Add(this._IfcStore.Instances.New<IfcPropertySingleValue>(
                                    prop =>
                                    {
                                        prop.Name = "Class ID";
                                        prop.Description = "))";
                                        prop.NominalValue = new IfcText(graphicElement.Smart3DClass.ToString());
                                    }));

                                foreach (KeyValuePair<string, string?> propValue in graphicElement.LabelProperties)
                                {
                                    pset.HasProperties.Add(this._IfcStore.Instances.New<IfcPropertySingleValue>(
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
                    this._IfcStore.Instances.New<IfcRelAggregates>(
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
