using IfcConverter.Client.Services.Model.Base;
using IngrDataReadLib;
using System;
using System.Collections.Generic;
using System.Linq;
using Xbim.Common;
using Xbim.Ifc4.GeometricConstraintResource;
using Xbim.Ifc4.GeometryResource;
using Xbim.Ifc4.Kernel;
using Xbim.Ifc4.MeasureResource;
using Xbim.Ifc4.ProductExtension;
using Xbim.Ifc4.PropertyResource;
using Xbim.Ifc4.RepresentationResource;
using Xbim.Ifc4.SharedBldgElements;

namespace IfcConverter.Client.Services.Model
{
    public sealed class VueGraphicElement : IConvertable<IfcElement>
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
        public IfcElement Convert(IModel model)
        {
            try
            {
                var member = model.Instances.New<IfcMember>();
                member.Name = Attributes["Name"];
                member.Representation = model.Instances.New<IfcProductDefinitionShape>();
                member.Representation.Representations.AddRange(GeometryGroup.Convert(model));
                member.ObjectPlacement = model.Instances.New<IfcLocalPlacement>(
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
                    definesByProperties.RelatedObjects.Add(member);
                    definesByProperties.RelatingPropertyDefinition = propertySet;
                });

                return member;
            }
            catch (Exception ex)
            {
                throw new Exception("Creating product element failed", ex);
            }
        }
    }
}
