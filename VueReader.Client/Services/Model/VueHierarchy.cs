using DynamicExpresso;
using DynamicExpresso.Exceptions;
using IfcConverter.Client.Services.Filter;
using IfcConverter.Client.Services.Model.Base;
using IfcConverter.Client.ViewModels;
using IfcConverter.Client.ViewModels.Base;
using Serilog;
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
        private readonly List<HierarchyItemViewModel> _HierarchyElements = new();

        public IfcSpatialStructureElement? Root { get; set; }

        public ObjectFilter? Filter { get; set; }

        public HierarchyItemViewModel BuildingStoreyItem {  get; set; }

        public IEnumerable<HierarchyItemViewModel> RootElements
        {
            get
            {
                return _HierarchyElements.Where(element => element.Parent == null);
            }
        }

        public VueHierarchy()
        {
            var projectItem = new HierarchySpatialElementViewModel(new IFConvertable.SP3DFileReader.DTO.VueHierarchyItem { Ident = new IFConvertable.SP3DFileReader.DTO.SmartId(), Name = "The Project", Type = IFConvertable.SP3DFileReader.DTO.HierarchyItemType.SPATIAL_ELEMENT });
            _HierarchyElements.Add(projectItem);

            var siteItem = new HierarchySpatialElementViewModel(new IFConvertable.SP3DFileReader.DTO.VueHierarchyItem { Ident = new IFConvertable.SP3DFileReader.DTO.SmartId(), Name = "The Site", Type = IFConvertable.SP3DFileReader.DTO.HierarchyItemType.SPATIAL_ELEMENT });
            _HierarchyElements.Add(siteItem);
            
            BuildingStoreyItem = new HierarchySpatialElementViewModel(new IFConvertable.SP3DFileReader.DTO.VueHierarchyItem { Ident = new IFConvertable.SP3DFileReader.DTO.SmartId(), Name = "The Building Storey", Type = IFConvertable.SP3DFileReader.DTO.HierarchyItemType.SPATIAL_ELEMENT });
            _HierarchyElements.Add(BuildingStoreyItem);
        }

        public void Insert(VueGraphicElement element)
        {
            element.Attributes.TryGetValue("Name", out string? elementName);
            element.Attributes.TryGetValue("System Path", out string? elementSystemPath);

            try
            {
                if (elementSystemPath is null)
                {
                    //_HierarchyElements.Add(new HierarchyItemViewModel(elementName ?? "<<no_name>>", string.Empty, BuildingStoreyItem, element));
                    Log.Logger.Warning($"System path not found for element '{elementName}'.");
                    return;
                }

                HierarchyItemViewModel? suggestParent = null;
                // лист, который будет каждый раз добавлять в себя папку
                var dymamicPathNodes = new List<string>();

                foreach (string pathNode in elementSystemPath.Split('\\'))
                {
                    dymamicPathNodes.Add(pathNode);
                    HierarchyItemViewModel? existingElement = _HierarchyElements.Where(x => x.FullPath == string.Join('\\', dymamicPathNodes)).FirstOrDefault();

                    if (suggestParent == null)
                    {
                        if (existingElement == null)
                        {
                            //suggestParent = new HierarchyItemViewModel(pathNode, string.Join('\\', dymamicPathNodes), BuildingStoreyItem);
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
                            //var newHierarchyElement = new HierarchyItemViewModel(pathNode, string.Join('\\', dymamicPathNodes), suggestParent);
                            //_HierarchyElements.Add(newHierarchyElement);
                            //suggestParent = newHierarchyElement;
                        }
                        else
                        {
                            suggestParent = existingElement;
                        }
                    }
                }

                if (suggestParent != null)
                {
                    //var hierarchyElement = new HierarchyItemViewModel(elementName ?? "<<no_name>>", string.Join('\\', elementSystemPath, elementName ?? "<<no_name>>"), suggestParent, element);
                    //_HierarchyElements.Add(hierarchyElement);
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

        private struct RelWrapAndConverted
        {
            public IfcProduct? Converted;
            public VueGraphicElement? Wrapper;
        }

        public IfcSpatialStructureElement? Convert(IModel model)
        {
            var createdProxies = new Dictionary<string, IfcBuildingElementProxy>();
            var selectedOrFilteredElements = _HierarchyElements.Where(e => (Filter != null && (e.GraphicElement == null || ApplyFilter(e.GraphicElement, Filter)) || e.IsSelected) && e.DisplayName != "The Project" && e.DisplayName != "The Site" && e.DisplayName != "The Building Storey");
            int idx = 1;

            List<RelWrapAndConverted> selectedConvertedElements
                = selectedOrFilteredElements.Where(e => e.GraphicElement != null).Select(e => new RelWrapAndConverted
                {
                    Converted = e.GraphicElement?.Convert(model),
                    Wrapper = e.GraphicElement
                }).ToList();

            using ITransaction transaction = model.BeginTransaction("Create hierarchy");

            foreach (HierarchyItemViewModel hierarchyElement in selectedOrFilteredElements)
            {
                try
                {
                    if (hierarchyElement.GraphicElement == null)
                    {
                        if (!createdProxies.ContainsKey(hierarchyElement.FullPath))
                        {
                            var proxy = model.Instances.New<IfcBuildingElementProxy>(proxy => proxy.Name = hierarchyElement.DisplayName);
                            createdProxies.Add(hierarchyElement.FullPath, proxy);

                            if (hierarchyElement.Parent != null)
                            {
                                model.Instances.New<IfcRelAggregates>(aggregates =>
                                {
                                    aggregates.RelatedObjects.Add(proxy);

                                    // checks created proxies
                                    createdProxies.TryGetValue(hierarchyElement.Parent.FullPath, out IfcBuildingElementProxy? parentByPathAsProxy);

                                    if (parentByPathAsProxy != null)
                                    {
                                        aggregates.RelatingObject = parentByPathAsProxy;
                                    }
                                    else
                                    {
                                        // if no created proxies then parent may be as IFC element
                                        // maybe obsoletted
                                        IEnumerable<RelWrapAndConverted> parentByPathAsElements
                                            = selectedConvertedElements.Where(e => string.Join('\\', e.Wrapper?.Attributes["System Path"], e.Wrapper?.Attributes["Name"]) == hierarchyElement.Parent.FullPath);

                                        if (parentByPathAsElements.Count() > 1)
                                        {
                                            throw new Exception("Element has parent, but parent has not created as proxy. Unable to find parent as IFC element for another IFC element.");
                                        }

                                        if (parentByPathAsElements.Count() == 1)
                                        {
                                            aggregates.RelatingObject = parentByPathAsElements.First().Converted;
                                        }
                                        else
                                        {
                                            aggregates.RelatingObject = Root;
                                            throw new Exception($"Parent by path '{hierarchyElement.Parent.FullPath}' not created as hierarchy proxy. May not be found before.");
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
                        model.Instances.New<IfcRelAggregates>(aggregates =>
                        {
                            IEnumerable<RelWrapAndConverted> createdElements
                                = selectedConvertedElements.Where(e => e.Wrapper?.SPFUID == hierarchyElement.GraphicElement.SPFUID);

                            if (createdElements.Count() > 1) throw new Exception("Occurred IFC element has converted more than one time.");
                            if (!createdElements.Any()) throw new Exception("Occurred IFC element has not converted");
                            aggregates.RelatedObjects.Add(createdElements.First().Converted);

                            if (hierarchyElement.Parent != null)
                            {
                                // checks created proxies
                                createdProxies.TryGetValue(hierarchyElement.Parent.FullPath, out IfcBuildingElementProxy? parentByPathAsProxy);

                                if (parentByPathAsProxy != null)
                                {
                                    aggregates.RelatingObject = parentByPathAsProxy;
                                }
                                else
                                {
                                    // if no created proxies then parent may be as IFC element
                                    IEnumerable<RelWrapAndConverted> parentByPathAsElements
                                        = selectedConvertedElements.Where(e => e.Wrapper?.SPFUID == hierarchyElement.Parent.GraphicElement?.SPFUID);

                                    if (parentByPathAsElements.Count() > 1) throw new Exception("Occurred IFC element has converted as parent IFC element more than one time.");
                                    if (!parentByPathAsElements.Any()) throw new Exception("Occurred IFC element has not converted parent as IFC element.");
                                    aggregates.RelatingObject = parentByPathAsElements.First().Converted;
                                }
                            }
                            else
                            {
                                aggregates.RelatingObject = Root;
                            }
                        });
                    }
                }
                catch (Exception ex)
                {
                    Log.Logger.Error(ex, "Convert hierarchy failed. One of contains elements can not be converted.");
                }

                Log.Logger.Information($"Converted {idx++} of {_HierarchyElements.Where(e => e.IsSelected).Count()} elements.");
            }

            transaction.Commit();

            return Root;
        }

        private bool ApplyFilter(VueGraphicElement element, ObjectFilter filter)
        {
            try
            {
                var interpreter = new Interpreter();

                foreach (KeyValuePair<string, string> attrib in element.Attributes)
                {
                    interpreter.SetVariable(attrib.Key.Replace(" ", "_"), attrib.Value);
                }

                return interpreter.Eval<bool>(filter.Criteria);
            }
            catch (Exception ex)
            {
                if (ex is UnknownIdentifierException) return false;
                throw new Exception("Apply filtering failed", ex);
            }
        }
    }
}
