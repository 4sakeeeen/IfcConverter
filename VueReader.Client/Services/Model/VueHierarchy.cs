using IfcConverter.Client.Services.Model.Base;
using IfcConverter.Client.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using Xbim.Common;
using Xbim.Ifc4.Kernel;
using Xbim.Ifc4.ProductExtension;
using Xbim.Ifc4.SharedBldgElements;

namespace IfcConverter.Client.Services.Model
{
    public sealed class VueHierarchy : IConvertable<IfcSpatialStructureElement?>
    {
        private readonly List<VueHierarchyElementViewModel> _HierarchyElements = new();

        public IfcSpatialStructureElement? Root { get; set; }

        public IEnumerable<VueHierarchyElementViewModel> RootElements
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
                VueHierarchyElementViewModel? suggestParent = null;
                // лист, который будет каждый раз добавлять в себя папку
                var dymamicPathNodes = new List<string>();

                foreach (string pathNode in element.Attributes["System Path"].Split('\\'))
                {
                    dymamicPathNodes.Add(pathNode);
                    VueHierarchyElementViewModel? existingElement = _HierarchyElements.Where(x => x.Path == string.Join('\\', dymamicPathNodes)).FirstOrDefault();

                    if (suggestParent == null)
                    {
                        if (existingElement == null)
                        {
                            suggestParent = new VueHierarchyElementViewModel(pathNode, string.Join('\\', dymamicPathNodes));
                            _HierarchyElements.Add(suggestParent);
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
                            var newHierarchyElement = new VueHierarchyElementViewModel(pathNode, string.Join('\\', dymamicPathNodes), suggestParent);
                            _HierarchyElements.Add(newHierarchyElement);
                            suggestParent.HierarchyItems.Add(newHierarchyElement);
                            suggestParent = newHierarchyElement;
                        }
                        else
                        {
                            suggestParent = existingElement;
                        }
                    }
                }

                if (suggestParent != null)
                {
                    var hierarchyElement = new VueHierarchyElementViewModel(element.Attributes["Name"], string.Join('\\', element.Attributes["System Path"], element.Attributes["Name"]), suggestParent, element);
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
            int idx = 1;

            var selectedConvertedElements = _HierarchyElements
                .Where(e => e.IsSelected && e.GraphicElement != null)
                .Select(e => new { Converted = e.GraphicElement?.Convert(model), Wrapper = e.GraphicElement });

            foreach (VueHierarchyElementViewModel hierarchyElement in _HierarchyElements.Where(e => e.IsSelected))
            {
                try
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
                                    aggregates.RelatedObjects.Add(proxy);

                                    // checks created proxies
                                    createdProxies.TryGetValue(hierarchyElement.Parent.Path, out IfcBuildingElementProxy? parentByPathAsProxy);
                                    
                                    if (parentByPathAsProxy != null)
                                    {
                                        aggregates.RelatingObject = parentByPathAsProxy;
                                    }
                                    else
                                    {
                                        // if no created proxies then parent may be as IFC element
                                        IfcElement? parentByPathAsElement = selectedConvertedElements
                                           .Where(e => e?.Wrapper?.Attributes["System Path"] == hierarchyElement.Parent.Path)
                                           .Select(converted => converted.Converted).FirstOrDefault();

                                        if (parentByPathAsElement != null)
                                        {
                                            aggregates.RelatingObject = parentByPathAsElement;
                                        }
                                        else
                                        {
                                            aggregates.RelatingObject = Root;
                                            throw new Exception($"Parent by path '{hierarchyElement.Parent.Path}' not created as hierarchy proxy. May not be found before.");
                                        }
                                    }
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
                            aggregates.RelatedObjects.Add(hierarchyElement.GraphicElement.Convert(model));

                            // checks created proxies
                            createdProxies.TryGetValue(hierarchyElement.Parent.Path, out IfcBuildingElementProxy? parentByPathAsProxy);
                            
                            if (parentByPathAsProxy != null)
                            {
                                aggregates.RelatingObject = parentByPathAsProxy;
                            }
                            else
                            {
                                // if no created proxies then parent may be as IFC element
                                IfcElement? parentByPathAsElement = selectedConvertedElements
                                   .Where(e => e?.Wrapper?.Attributes["System Path"] == hierarchyElement.Parent.Path)
                                   .Select(converted => converted.Converted).FirstOrDefault();

                                if (parentByPathAsElement != null)
                                {
                                    aggregates.RelatingObject = parentByPathAsElement;
                                }
                                else
                                {
                                    aggregates.RelatingObject = Root;
                                    throw new Exception($"Parent by path '{hierarchyElement.Parent.Path}' not created as hierarchy proxy. May not be found before.");
                                }
                            }
                        });
                    }
                }
                catch (Exception ex)
                {
                    App.Logger.Error(ex, "Convert hierarchy failed. One of contains elements can not be converted.");
                }


                App.Logger.Information($"Converted {idx++} of {_HierarchyElements.Where(e => e.IsSelected).Count()} elements.");
            }

            return Root;
        }
    }
}
