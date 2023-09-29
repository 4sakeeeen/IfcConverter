using IfcConverter.Client.Services;
using IfcConverter.Domain.Models.Vue;
using IfcConverter.Domain.Models.Vue.Common.Collections;
using IfcConverter.Domain.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Windows;
using Xbim.Common;
using Xbim.Ifc;
using Xbim.Ifc4.GeometricConstraintResource;
using Xbim.Ifc4.GeometricModelResource;
using Xbim.Ifc4.GeometryResource;
using Xbim.Ifc4.Interfaces;
using Xbim.Ifc4.Kernel;
using Xbim.Ifc4.MaterialResource;
using Xbim.Ifc4.MeasureResource;
using Xbim.Ifc4.PresentationOrganizationResource;
using Xbim.Ifc4.ProductExtension;
using Xbim.Ifc4.ProfileResource;
using Xbim.Ifc4.PropertyResource;
using Xbim.Ifc4.QuantityResource;
using Xbim.Ifc4.RepresentationResource;
using Xbim.Ifc4.SharedBldgElements;
using Xbim.IO;

namespace IfcConverter.Client.Windows
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private IfcWallStandardCase CreateWall(IfcStore model, double length, double width, double height)
        {
            using ITransaction transaction = model.BeginTransaction("Create Wall");
            IfcWallStandardCase wall = model.Instances.New<IfcWallStandardCase>();
            wall.Name = "A Standard rectangular wall";

            IfcCartesianPoint insertPoint = model.Instances.New<IfcCartesianPoint>();
            insertPoint.SetXY(0, 400);

            //represent wall as a rectangular profile
            IfcRectangleProfileDef rectProfile = model.Instances.New<IfcRectangleProfileDef>();
            rectProfile.ProfileType = IfcProfileTypeEnum.AREA;
            rectProfile.XDim = width;
            rectProfile.YDim = length;
            rectProfile.Position = model.Instances.New<IfcAxis2Placement2D>();
            rectProfile.Position.Location = insertPoint;


            #region a profile

            var profile = model.Instances.New<IfcArbitraryOpenProfileDef>(p =>
            {
                p.Curve = model.Instances.New<IfcPolyline>(c =>
                {
                    c.Points.Add(model.Instances.New<IfcCartesianPoint>(x => x.SetXYZ(0, 0, 0)));
                    c.Points.Add(model.Instances.New<IfcCartesianPoint>(x => x.SetXYZ(360, 0, 0)));
                    c.Points.Add(model.Instances.New<IfcCartesianPoint>(x => x.SetXYZ(400, 200, 0)));
                    c.Points.Add(model.Instances.New<IfcCartesianPoint>(x => x.SetXYZ(400, 600, 0)));
                    c.Points.Add(model.Instances.New<IfcCartesianPoint>(x => x.SetXYZ(0, 290, 0)));
                    // c.Points.Add(model.Instances.New<IfcCartesianPoint>(x => x.SetXYZ(0, 0, 0)));
                });
                p.ProfileType = IfcProfileTypeEnum.CURVE;
            });

            #endregion


            //model as a swept area solid
            IfcExtrudedAreaSolid body = model.Instances.New<IfcExtrudedAreaSolid>();
            body.Depth = height;
            // body.SweptArea = rectProfile;
            body.SweptArea = profile;
            body.ExtrudedDirection = model.Instances.New<IfcDirection>();
            body.ExtrudedDirection.SetXYZ(0, 0, 1);

            //parameters to insert the geometry in the model
            IfcCartesianPoint origin = model.Instances.New<IfcCartesianPoint>();
            origin.SetXYZ(0, 0, 0);
            body.Position = model.Instances.New<IfcAxis2Placement3D>();
            body.Position.Location = origin;

            //Create a Definition shape to hold the geometry
            IfcShapeRepresentation shape = model.Instances.New<IfcShapeRepresentation>();
            IfcGeometricRepresentationContext? modelContext = model.Instances.OfType<IfcGeometricRepresentationContext>().FirstOrDefault();
            shape.ContextOfItems = modelContext;
            shape.RepresentationType = "SweptSolid";
            shape.RepresentationIdentifier = "Body";
            shape.Items.Add(body);

            //Create a Product Definition and add the model geometry to the wall
            IfcProductDefinitionShape rep = model.Instances.New<IfcProductDefinitionShape>();
            rep.Representations.Add(shape);
            wall.Representation = rep;

            //now place the wall into the model
            IfcLocalPlacement lp = model.Instances.New<IfcLocalPlacement>();
            IfcAxis2Placement3D ax3D = model.Instances.New<IfcAxis2Placement3D>();
            ax3D.Location = origin;
            ax3D.RefDirection = model.Instances.New<IfcDirection>();
            ax3D.RefDirection.SetXYZ(0, 1, 0);
            ax3D.Axis = model.Instances.New<IfcDirection>();
            ax3D.Axis.SetXYZ(0, 0, 1);
            lp.RelativePlacement = ax3D;
            wall.ObjectPlacement = lp;


            // Where Clause: The IfcWallStandard relies on the provision of an IfcMaterialLayerSetUsage 
            IfcMaterialLayerSetUsage ifcMaterialLayerSetUsage = model.Instances.New<IfcMaterialLayerSetUsage>();
            IfcMaterialLayerSet ifcMaterialLayerSet = model.Instances.New<IfcMaterialLayerSet>();
            IfcMaterialLayer ifcMaterialLayer = model.Instances.New<IfcMaterialLayer>();
            ifcMaterialLayer.LayerThickness = 10;
            ifcMaterialLayerSet.MaterialLayers.Add(ifcMaterialLayer);
            ifcMaterialLayerSetUsage.ForLayerSet = ifcMaterialLayerSet;
            ifcMaterialLayerSetUsage.LayerSetDirection = IfcLayerSetDirectionEnum.AXIS2;
            ifcMaterialLayerSetUsage.DirectionSense = IfcDirectionSenseEnum.NEGATIVE;
            ifcMaterialLayerSetUsage.OffsetFromReferenceLine = 150;

            // Add material to wall
            IfcMaterial material = model.Instances.New<IfcMaterial>();
            material.Name = "some material";
            IfcRelAssociatesMaterial ifcRelAssociatesMaterial = model.Instances.New<IfcRelAssociatesMaterial>();
            ifcRelAssociatesMaterial.RelatingMaterial = material;
            ifcRelAssociatesMaterial.RelatedObjects.Add(wall);

            ifcRelAssociatesMaterial.RelatingMaterial = ifcMaterialLayerSetUsage;

            // IfcPresentationLayerAssignment is required for CAD presentation in IfcWall or IfcWallStandardCase
            IfcPresentationLayerAssignment ifcPresentationLayerAssignment = model.Instances.New<IfcPresentationLayerAssignment>();
            ifcPresentationLayerAssignment.Name = "some ifcPresentationLayerAssignment";
            ifcPresentationLayerAssignment.AssignedItems.Add(shape);


            // linear segment as IfcPolyline with two points is required for IfcWall
            IfcPolyline ifcPolyline = model.Instances.New<IfcPolyline>();
            IfcCartesianPoint startPoint = model.Instances.New<IfcCartesianPoint>();
            startPoint.SetXY(0, 0);
            IfcCartesianPoint endPoint = model.Instances.New<IfcCartesianPoint>();
            endPoint.SetXY(4000, 0);
            ifcPolyline.Points.Add(startPoint);
            ifcPolyline.Points.Add(endPoint);

            IfcShapeRepresentation shape2D = model.Instances.New<IfcShapeRepresentation>();
            shape2D.ContextOfItems = modelContext;
            shape2D.RepresentationIdentifier = "Axis";
            shape2D.RepresentationType = "Curve2D";
            shape2D.Items.Add(ifcPolyline);
            rep.Representations.Add(shape2D);
            transaction.Commit();

            return wall;
        }

        private void AddPropertiesToWall(IfcStore model, IfcWallStandardCase wall)
        {
            ITransaction transaction = model.BeginTransaction("Create Wall");
            CreateElementQuantity(model, wall);
            CreateSimpleProperty(model, wall);
            transaction.Commit();
        }

        private void CreateElementQuantity(IfcStore model, IfcWallStandardCase wall)
        {
            //Create a IfcElementQuantity
            //first we need a IfcPhysicalSimpleQuantity,first will use IfcQuantityLength
            IfcQuantityLength ifcQuantityArea = model.Instances.New<IfcQuantityLength>(qa =>
            {
                qa.Name = "IfcQuantityArea:Area";
                qa.Description = "";
                qa.Unit = model.Instances.New<IfcSIUnit>(siu =>
                {
                    siu.UnitType = IfcUnitEnum.LENGTHUNIT;
                    siu.Prefix = IfcSIPrefix.MILLI;
                    siu.Name = IfcSIUnitName.METRE;
                });
                qa.LengthValue = 100.0;

            });
            //next quantity IfcQuantityCount using IfcContextDependentUnit
            IfcContextDependentUnit ifcContextDependentUnit = model.Instances.New<IfcContextDependentUnit>(cd =>
            {
                cd.Dimensions = model.Instances.New<IfcDimensionalExponents>(de =>
                {
                    de.LengthExponent = 1;
                    de.MassExponent = 0;
                    de.TimeExponent = 0;
                    de.ElectricCurrentExponent = 0;
                    de.ThermodynamicTemperatureExponent = 0;
                    de.AmountOfSubstanceExponent = 0;
                    de.LuminousIntensityExponent = 0;
                });
                cd.UnitType = IfcUnitEnum.LENGTHUNIT;
                cd.Name = "Elephants";
            });
            IfcQuantityCount ifcQuantityCount = model.Instances.New<IfcQuantityCount>(qc =>
            {
                qc.Name = "IfcQuantityCount:Elephant";
                qc.CountValue = 12;
                qc.Unit = ifcContextDependentUnit;
            });


            //next quantity IfcQuantityLength using IfcConversionBasedUnit
            IfcConversionBasedUnit ifcConversionBasedUnit = model.Instances.New<IfcConversionBasedUnit>(cbu =>
            {
                cbu.ConversionFactor = model.Instances.New<IfcMeasureWithUnit>(mu =>
                {
                    mu.ValueComponent = new IfcRatioMeasure(25.4);
                    mu.UnitComponent = model.Instances.New<IfcSIUnit>(siu =>
                    {
                        siu.UnitType = IfcUnitEnum.LENGTHUNIT;
                        siu.Prefix = IfcSIPrefix.MILLI;
                        siu.Name = IfcSIUnitName.METRE;
                    });

                });
                cbu.Dimensions = model.Instances.New<IfcDimensionalExponents>(de =>
                {
                    de.LengthExponent = 1;
                    de.MassExponent = 0;
                    de.TimeExponent = 0;
                    de.ElectricCurrentExponent = 0;
                    de.ThermodynamicTemperatureExponent = 0;
                    de.AmountOfSubstanceExponent = 0;
                    de.LuminousIntensityExponent = 0;
                });
                cbu.UnitType = IfcUnitEnum.LENGTHUNIT;
                cbu.Name = "Inch";
            });
            IfcQuantityLength ifcQuantityLength = model.Instances.New<IfcQuantityLength>(qa =>
            {
                qa.Name = "IfcQuantityLength:Length";
                qa.Description = "";
                qa.Unit = ifcConversionBasedUnit;
                qa.LengthValue = 24.0;
            });

            //lets create the IfcElementQuantity
            IfcElementQuantity ifcElementQuantity = model.Instances.New<IfcElementQuantity>(eq =>
            {
                eq.Name = "Test:IfcElementQuantity";
                eq.Description = "Measurement quantity";
                eq.Quantities.Add(ifcQuantityArea);
                eq.Quantities.Add(ifcQuantityCount);
                eq.Quantities.Add(ifcQuantityLength);
            });

            //need to create the relationship
            model.Instances.New<IfcRelDefinesByProperties>(rdbp =>
            {
                rdbp.Name = "Area Association";
                rdbp.Description = "IfcElementQuantity associated to wall";
                rdbp.RelatedObjects.Add(wall);
                rdbp.RelatingPropertyDefinition = ifcElementQuantity;
            });
        }

        private void CreateSimpleProperty(IfcStore model, IfcWallStandardCase wall)
        {
            IfcPropertySingleValue ifcPropertySingleValue = model.Instances.New<IfcPropertySingleValue>(psv =>
            {
                psv.Name = "IfcPropertySingleValue:Time";
                psv.Description = "";
                psv.NominalValue = new IfcTimeMeasure(150.0);
                psv.Unit = model.Instances.New<IfcSIUnit>(siu =>
                {
                    siu.UnitType = IfcUnitEnum.TIMEUNIT;
                    siu.Name = IfcSIUnitName.SECOND;
                });
            });
            IfcPropertyEnumeratedValue ifcPropertyEnumeratedValue = model.Instances.New<IfcPropertyEnumeratedValue>(pev =>
            {
                pev.Name = "IfcPropertyEnumeratedValue:Music";
                pev.EnumerationReference = model.Instances.New<IfcPropertyEnumeration>(pe =>
                {
                    pe.Name = "Notes";
                    pe.EnumerationValues.Add(new IfcLabel("Do"));
                    pe.EnumerationValues.Add(new IfcLabel("Re"));
                    pe.EnumerationValues.Add(new IfcLabel("Mi"));
                    pe.EnumerationValues.Add(new IfcLabel("Fa"));
                    pe.EnumerationValues.Add(new IfcLabel("So"));
                    pe.EnumerationValues.Add(new IfcLabel("La"));
                    pe.EnumerationValues.Add(new IfcLabel("Ti"));
                });
                pev.EnumerationValues.Add(new IfcLabel("Do"));
                pev.EnumerationValues.Add(new IfcLabel("Re"));
                pev.EnumerationValues.Add(new IfcLabel("Mi"));

            });
            IfcPropertyBoundedValue ifcPropertyBoundedValue = model.Instances.New<IfcPropertyBoundedValue>(pbv =>
            {
                pbv.Name = "IfcPropertyBoundedValue:Mass";
                pbv.Description = "";
                pbv.UpperBoundValue = new IfcMassMeasure(5000.0);
                pbv.LowerBoundValue = new IfcMassMeasure(1000.0);
                pbv.Unit = model.Instances.New<IfcSIUnit>(siu =>
                {
                    siu.UnitType = IfcUnitEnum.MASSUNIT;
                    siu.Name = IfcSIUnitName.GRAM;
                    siu.Prefix = IfcSIPrefix.KILO;
                });
            });

            List<IfcReal> definingValues = new() { new IfcReal(100.0), new IfcReal(200.0), new IfcReal(400.0), new IfcReal(800.0), new IfcReal(1600.0), new IfcReal(3200.0), };
            List<IfcReal> definedValues = new() { new IfcReal(20.0), new IfcReal(42.0), new IfcReal(46.0), new IfcReal(56.0), new IfcReal(60.0), new IfcReal(65.0), };
            IfcPropertyTableValue ifcPropertyTableValue = model.Instances.New<IfcPropertyTableValue>(ptv =>
            {
                ptv.Name = "IfcPropertyTableValue:Sound";
                foreach (IfcReal item in definingValues)
                {
                    ptv.DefiningValues.Add(item);
                }
                foreach (IfcReal item in definedValues)
                {
                    ptv.DefinedValues.Add(item);
                }
                ptv.DefinedUnit = model.Instances.New<IfcContextDependentUnit>(cd =>
                {
                    cd.Dimensions = model.Instances.New<IfcDimensionalExponents>(de =>
                    {
                        de.LengthExponent = 0;
                        de.MassExponent = 0;
                        de.TimeExponent = 0;
                        de.ElectricCurrentExponent = 0;
                        de.ThermodynamicTemperatureExponent = 0;
                        de.AmountOfSubstanceExponent = 0;
                        de.LuminousIntensityExponent = 0;
                    });
                    cd.UnitType = IfcUnitEnum.FREQUENCYUNIT;
                    cd.Name = "dB";
                });
            });
        }

        private void BtnOpenFile_Click(object sender, RoutedEventArgs e)
        {
            using var model = IfcStore.Open(TbFilePathToOpen.Text);
            IfcProduct select1 = model.Instances.FirstOrDefault<IfcProduct>(x => x.Name == "MemberPartPrismatic-1-17838");
            Type t1 = select1.GetType();
            IfcProduct select2 = model.Instances.FirstOrDefault<IfcProduct>(x => x.Name == "MemberPartPrismatic-1-17840");
            Type t2 = select2.GetType();
        }


        private void BtnOpenVueFile_Click(object sender, RoutedEventArgs e)
        {
            VueModel vueModel2 = VueService.ReadVueTextFile("C:\\Users\\Windows 11\\source\\repos\\IfcConverter\\DataExamples\\УЗК.txt");

            VueModel? vueModel = JsonSerializer.Deserialize<VueModel>(File.ReadAllText(TbVueFilePath.Text)) ?? throw new Exception("Json did not parsed correctly");
            Elements? geomElements = (vueModel.GraphicElements[0].Geometry?.Elements) ?? throw new Exception("Provided vue geometry elements not set");
            
            using IfcStore ifcModel = IfcService.CreateAndInitModel("УЗК") ?? throw new Exception("Model is null");
            IfcBuilding building = IfcService.CreateBuilding(ifcModel, "A Building");
            IfcService.CreateProduct(ifcModel, building, geomElements);

            ifcModel.SaveAs("C:\\Users\\Windows 11\\source\\repos\\IfcConverter\\DataExamples\\TestEnv.ifc", StorageType.Ifc);

            string jsonData = JsonSerializer.Serialize(vueModel, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(TbVueFilePath.Text, jsonData);
            MessageBox.Show("Model was created");
        }
    }
}
