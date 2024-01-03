using IfcConverter.Domain.Models;
using IngrDataReadLib;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using Xbim.Common;
using Xbim.Ifc4.GeometricConstraintResource;
using Xbim.Ifc4.GeometryResource;
using Xbim.Ifc4.Kernel;
using Xbim.Ifc4.MeasureResource;
using Xbim.Ifc4.PropertyResource;
using Xbim.Ifc4.RepresentationResource;
using Xbim.Ifc4.SharedBldgElements;
using Xbim.Ifc4.SharedBldgServiceElements;
using Xbim.Ifc4.StructuralElementsDomain;

namespace IfcConverter.Client.Services.Model.Base
{
    public sealed class VueGraphicElement : IConvertable<IfcProduct>
    {
        public Dictionary<string, string> Attributes { get; } = new();


        public VueGroup GeometryGroup { get; }


        public string Linkage { get; }


        public string Moniker { get; }


        public string SPFUID { get; }


        public VueGraphicElement(
            Array rawProperties,
            string linkage,
            string moniker,
            string spfuid,
            eGraphicType graphicType,
            IngrGeom geometry)
        {
            Attributes = new Dictionary<string, string>(
                rawProperties
                .Cast<string>()
                .Select(delegate (string rawProperty)
                {
                    string[] propertyParts = rawProperty.Split(" : ");
                    if (propertyParts.Length != 2) { throw new Exception($"Property {rawProperty} can not be converted."); }
                    return new KeyValuePair<string, string>(propertyParts.ElementAt(0), propertyParts.ElementAt(1));
                })
                .GroupBy(delegate (KeyValuePair<string, string> x)
                {
                    return x.Key;
                })
                .Select(delegate (IGrouping<string, KeyValuePair<string, string>> x)
                {
                    return x.First();
                }));
            Linkage = linkage;
            Moniker = moniker;
            SPFUID = spfuid;
            GeometryGroup = graphicType switch
            {
                eGraphicType.GROUP_TYPE => new VueGroup((IRdGroup)geometry),
                _ => throw new NotImplementedException("Graphic type GROUP_TYPE not encountered for a convertable element."),
            };
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public IfcProduct Convert(IModel model)
        {
            try
            {
                using ITransaction transaction = model.BeginTransaction("Create IFC element");

                var mapping = JsonSerializer.Deserialize<List<S3DClassMapping>>(File.ReadAllText("data\\class-mapping.json")) ?? throw new Exception("Error in retrieve class mapping.");
                int classId = int.Parse(SPFUID.Split("!!")[1].Split("##")[0]);
                string guid = SPFUID.Split("##")[1];

                if (guid.Length > 22) throw new Exception("Smart GUID length more than 22.");

                guid = guid.PadRight(22, '0');

                S3DClassMapping classMappingById = mapping.FirstOrDefault(map => map.ID == classId) ?? throw new Exception($"Cannot find mapping to S3D ID: {classId}.");

                IfcProduct product = classMappingById.MappedClassIFC switch
                {
                    "Buildings Element Proxy" => model.Instances.New<IfcBuildingElementProxy>(),
                    // "Distribution System" => model.Instances.New<IfcDistributionSystem>(),
                    "Wall" => model.Instances.New<IfcWall>(),
                    "Member" => model.Instances.New<IfcMember>(),
                    "Railing" => model.Instances.New<IfcRailing>(),
                    "Slab" => model.Instances.New<IfcSlab>(),
                    "Stair" => model.Instances.New<IfcStair>(),
                    "Footing" => model.Instances.New<IfcFooting>(),
                    "Distribution Flow Element" => model.Instances.New<IfcDistributionFlowElement>(),
                    "Distribution Control Element" => model.Instances.New<IfcDistributionControlElement>(),
                    "Flow Terminal" => model.Instances.New<IfcFlowTerminal>(),
                    "Flow Controller" => model.Instances.New<IfcFlowController>(),
                    "Flow Fitting" => model.Instances.New<IfcFlowFitting>(),
                    "Flow Moving Device" => model.Instances.New<IfcFlowMovingDevice>(),
                    "Flow Segment" => model.Instances.New<IfcFlowSegment>(),
                    "Plate" => model.Instances.New<IfcPlate>(),
                    _ => throw new NotImplementedException($"No implement for IFC class: {classMappingById.MappedClassIFC}.")
                };
                product.Name = Attributes["Name"];
                product.GlobalId = guid;
                product.Representation = model.Instances.New<IfcProductDefinitionShape>();
                product.Representation.Representations.AddRange(GeometryGroup.Convert(model));
                product.ObjectPlacement = model.Instances.New<IfcLocalPlacement>(
                    placement => placement.RelativePlacement = model.Instances.New<IfcAxis2Placement3D>(
                        axis3d => axis3d.Location = model.Instances.New<IfcCartesianPoint>(
                            point => point.SetXYZ(0, 0, 0))));

                var propertySet = model.Instances.New<IfcPropertySet>();
                propertySet.Name = "Smart Plant 3D";
                propertySet.HasProperties.AddRange(Attributes.Select(
                    attrib => model.Instances.New<IfcPropertySingleValue>(property =>
                    {
                        property.Name = attrib.Key;
                        property.NominalValue = new IfcText(attrib.Value);
                    })
                ));

                model.Instances.New<IfcRelDefinesByProperties>(definesByProperties =>
                {
                    definesByProperties.RelatedObjects.Add(product);
                    definesByProperties.RelatingPropertyDefinition = propertySet;
                });

                transaction.Commit();

                return product;
            }
            catch (Exception ex)
            {
                throw new Exception("Creating product element failed", ex);
            }
        }

        public override string ToString()
        {
            return Attributes["System Path"];
        }
    }
}
