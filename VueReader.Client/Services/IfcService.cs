using System.Collections.Generic;
using System.Linq;
using Xbim.Ifc4.Kernel;
using Xbim.Ifc4.ProductExtension;
using Xbim.Ifc4.SharedBldgElements;

namespace IfcConverter.Client.Services
{
    public sealed class IfcService
    {
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
    }
}
