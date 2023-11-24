﻿using IfcConverter.Client.Services.Model;
using IfcConverter.Client.Services.Model.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using Xbim.Common;
using Xbim.Ifc4.Kernel;
using Xbim.Ifc4.ProductExtension;
using Xbim.Ifc4.SharedBldgElements;

namespace IfcConverter.Client.Services
{
    public sealed class VueHierarchy : IConvertable<IfcSpatialStructureElement?>
    {
        private readonly List<VueHierarchyElement> _HierarchyElements = new();

        public IfcSpatialStructureElement? Root { get; set; }

        public IEnumerable<VueHierarchyElement> RootElements
        {
            get
            {
                return _HierarchyElements.Where(element => element.Parent == null);
            }
        }

        public void Insert(VueGraphicElement element)
        {
            try
            {
                VueHierarchyElement? suggestParent = null;
                var dymamicPathNodes = new List<string>();

                foreach (string pathNode in element.Attributes["System Path"].Split('\\'))
                {
                    dymamicPathNodes.Add(pathNode);
                    VueHierarchyElement? existingElement = _HierarchyElements.Where(x => x.Path == string.Join('\\', dymamicPathNodes)).FirstOrDefault();

                    if (suggestParent != null)
                    {
                        if (existingElement == null)
                        {
                            var newHierarchyElement = new VueHierarchyElement(pathNode, string.Join('\\', dymamicPathNodes), suggestParent);
                            _HierarchyElements.Add(newHierarchyElement);
                            suggestParent.HierarchyItems.Add(newHierarchyElement);
                            suggestParent = newHierarchyElement;
                        }
                        else
                        {
                            suggestParent = existingElement;
                        }
                    }
                    else
                    {
                        if (existingElement == null)
                        {
                            suggestParent = new VueHierarchyElement(pathNode, string.Join('\\', dymamicPathNodes));
                            _HierarchyElements.Add(suggestParent);
                        }
                        else
                        {
                            suggestParent = existingElement;
                        }
                    }
                }

                if (suggestParent != null)
                {
                    var hierarchyElement = new VueHierarchyElement(element.Attributes["Name"], string.Join('\\', element.Attributes["System Path"], element.Attributes["Name"]), suggestParent, element);
                    suggestParent.HierarchyItems.Add(hierarchyElement);
                    _HierarchyElements.Add(hierarchyElement);
                }
                else
                {
                    throw new Exception("Element has no parent");
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Build hierarchy insert failed", ex);
            }
        }

        public IfcSpatialStructureElement? Convert(IModel model)
        {
            var createdProxies = new Dictionary<string, IfcBuildingElementProxy>();

            foreach (VueHierarchyElement hierarchyElement in _HierarchyElements.Where(e => e.IsSelected))
            {
                if (hierarchyElement.GraphicElement == null)
                {
                    if (!createdProxies.ContainsKey(hierarchyElement.Path))
                    {
                        var proxy = model.Instances.New<IfcBuildingElementProxy>(proxy => proxy.Name = hierarchyElement.Name);
                        createdProxies.Add(hierarchyElement.Path, proxy);

                        if (hierarchyElement.Parent != null)
                        {
                            model.Instances.New<IfcRelAggregates>(aggregates =>
                            {
                                aggregates.RelatingObject = createdProxies[hierarchyElement.Parent.Path];
                                aggregates.RelatedObjects.Add(proxy);
                            });
                        }
                        else
                        {
                            model.Instances.New<IfcRelAggregates>(aggregates =>
                            {
                                aggregates.RelatingObject = Root;
                                aggregates.RelatedObjects.Add(proxy);
                            });
                        }
                    }
                }
                else
                {
                    if (hierarchyElement.Parent == null)
                    {
                        throw new Exception("Can not add element to parent. Parent is null");
                    }
                    model.Instances.New<IfcRelAggregates>(aggregates =>
                    {
                        aggregates.RelatingObject = createdProxies[hierarchyElement.Parent.Path];
                        aggregates.RelatedObjects.Add(hierarchyElement.GraphicElement.Convert(model));
                    });
                }
            }

            return Root;
        }
    }
}
