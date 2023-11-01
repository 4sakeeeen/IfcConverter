using IfcConverter.Client.ViewModels.Base;
using IfcConverter.Domain.Models;
using System.Collections.ObjectModel;
using System.Windows.Data;

namespace IfcConverter.Client.ViewModels
{
    internal sealed class SettingsWindowViewModel : ViewModel
    {
        private ClassMapping? _SelectedClassMapping;

        public ClassMapping? SelectedClassMapping
        {
            get => this._SelectedClassMapping;
            set => SetProperty(ref this._SelectedClassMapping, value);
        }

        public static ObservableCollection<IfcClass> IfcClasses
        {
            get => new()
            {
                new IfcClass("Proxy"),
                new IfcClass("Annotation"),
                new IfcClass("Element", new IfcClass[]
                {
                    new IfcClass("Civil Element"),
                    new IfcClass("Distribution Element", new IfcClass[]
                    {
                        new IfcClass("Distribution Control Element", new IfcClass[]
                        {
                            new IfcClass("Actuator"),
                            new IfcClass("Alarm"),
                            new IfcClass("Controller"),
                            new IfcClass("Flow Instrument"),
                            new IfcClass("Sensor"),
                            new IfcClass("Unitary Control Element"),
                            new IfcClass("Protective Device Tripping Unit")
                        }),
                        new IfcClass("Distribution Flow Element", new IfcClass[]
                        {
                            new IfcClass("Distribution Chamber Element"),
                            new IfcClass("Energy Conversion Device", new IfcClass[]
                            {
                                new IfcClass("Electric Generator"),
                                new IfcClass("Electric Motor"),
                                new IfcClass("Motor Connection"),
                                new IfcClass("Solar Device"),
                                new IfcClass("Transformer"),
                                new IfcClass("Air To Air Heat Recovery"),
                                new IfcClass("Boiler"),
                                new IfcClass("Burner"),
                                new IfcClass("Chiller"),
                                new IfcClass("Coil"),
                                new IfcClass("Condenser"),
                                new IfcClass("Cooled Beam"),
                                new IfcClass("Cooling Tower"),
                                new IfcClass("Engine"),
                                new IfcClass("Evaporative Cooler"),
                                new IfcClass("Evaporator"),
                                new IfcClass("Heat Exchanger"),
                                new IfcClass("Humidifier"),
                                new IfcClass("Tube Bundle"),
                                new IfcClass("Unitary Equipment")
                            }),
                            new IfcClass("Flow Controller", new IfcClass[]
                            {
                                new IfcClass("Electric Distribution Board"),
                                new IfcClass("Electric Time Control"),
                                new IfcClass("Protective Device"),
                                new IfcClass("Switching Device"),
                                new IfcClass("Air Terminal Box"),
                                new IfcClass("Damper"),
                                new IfcClass("Flow Meter"),
                                new IfcClass("Valve")
                            }),
                            new IfcClass("Flow Fitting", new IfcClass[]
                            {
                                new IfcClass("Cable Carrier Fitting"),
                                new IfcClass("Cable Fitting"),
                                new IfcClass("Junction Box"),
                                new IfcClass("Duct Fitting"),
                                new IfcClass("Pipe Fitting")
                            }),
                            new IfcClass("Flow Moving Device", new IfcClass[]
                            {
                                new IfcClass("Compressor"),
                                new IfcClass("Fan"),
                                new IfcClass("Pump")
                            }),
                            new IfcClass("Flow Segment", new IfcClass[]
                            {
                                new IfcClass("Cable Carrier Segment"),
                                new IfcClass("Cable Segment"),
                                new IfcClass("Duct Segment"),
                                new IfcClass("Pipe Segment")
                            }),
                            new IfcClass("Flow Storage Device", new IfcClass[]
                            {
                                new IfcClass("Electric Flow Storage Device"),
                                new IfcClass("Tank")
                            }),
                            new IfcClass("Flow Terminal", new IfcClass[]
                            {
                                new IfcClass("Audio Visual Appliance"),
                                new IfcClass("Communications Appliance"),
                                new IfcClass("Electric App liance"),
                                new IfcClass("Lamp"),
                                new IfcClass("Light Fixture"),
                                new IfcClass("Outlet"),
                                new IfcClass("Air Terminal"),
                                new IfcClass("Medical Device"),
                                new IfcClass("Space Heater"),
                                new IfcClass("Fire Suppression Terminal"),
                                new IfcClass("Sanitary Terminal"),
                                new IfcClass("Stack Terminal"),
                                new IfcClass("Waste Terminal")
                            }),
                            new IfcClass("Flow Treatment Device", new IfcClass[]
                            {
                                new IfcClass("Duct Silencer"),
                                new IfcClass("Filter"),
                                new IfcClass("Interceptor"),
                            })
                        })
                    }),
                    new IfcClass("Element Assembly"),
                    new IfcClass("Building Element", new IfcClass[]
                    {
                        new IfcClass("Beam"),
                        new IfcClass("Beam Standard Case"),
                        new IfcClass("Building Element Proxy"),
                        new IfcClass("Chimney"),
                        new IfcClass("Column"),
                        new IfcClass("Column Standard Case"),
                        new IfcClass("Covering"),
                        new IfcClass("Curtain Wall"),
                        new IfcClass("Door"),
                        new IfcClass("Door Standard Case"),
                        new IfcClass("Member"),
                        new IfcClass("Member Standard Case"),
                        new IfcClass("Plate"),
                        new IfcClass("Plate Standard Case"),
                        new IfcClass("Railing"),
                        new IfcClass("Ramp"),
                        new IfcClass("Ramp Flight"),
                        new IfcClass("Roof"),
                        new IfcClass("Shading Device"),
                        new IfcClass("Slab"),
                        new IfcClass("Slab Elemented Case"),
                        new IfcClass("Slab Standard Case"),
                        new IfcClass("Stair"),
                        new IfcClass("Stair Flight"),
                        new IfcClass("Wall"),
                        new IfcClass("Wall Elemented Case"),
                        new IfcClass("Wall Standard Case"),
                        new IfcClass("Window"),
                        new IfcClass("Window Standard Case"),
                        new IfcClass("Footing"),
                        new IfcClass("Pile")
                    }),
                    new IfcClass("Feature Element", new IfcClass[]
                    {
                        new IfcClass("Feature Element Addition", new IfcClass[] { new IfcClass("Projection Element") }),
                        new IfcClass("Feature Element Subtraction", new IfcClass[]
                        {
                            new IfcClass("Opening Element", new IfcClass[] { new IfcClass("Opening Standard Case") }),
                            new IfcClass("Voiding Feature")
                        }),
                        new IfcClass("Surface Feature")
                    }),
                    new IfcClass("Furnishing Element", new IfcClass[]
                    {
                        new IfcClass("Furniture"),
                        new IfcClass("System Furniture Element")
                    }),
                    new IfcClass("Geographic Element"),
                    new IfcClass("Transport Element"),
                    new IfcClass("Virtual Element"),
                    new IfcClass("Element Component", new IfcClass[]
                    {
                        new IfcClass("Vibration Isolator"),
                        new IfcClass("Building Element Part"),
                        new IfcClass("Discrete Accessory"),
                        new IfcClass("Fastener"),
                        new IfcClass("Mechanical Fastener"),
                        new IfcClass("Reinforcing Element", new IfcClass[]
                        {
                            new IfcClass("Reinforcing Bar"),
                            new IfcClass("Reinforcing Mesh"),
                            new IfcClass("Tendon"),
                            new IfcClass("Tendon Anchor")
                        })
                    })
                }),
                new IfcClass("Grid"),
                new IfcClass("Port", new IfcClass[] { new IfcClass("Distribution Port") }),
                new IfcClass("Positioning Element", new IfcClass[]
                {
                    new IfcClass("Linear Positioning Element", new IfcClass[] { new IfcClass("Alignment") }),
                    new IfcClass("Referent")
                }),
                new IfcClass("Spatial Element", new IfcClass[]
                {
                    new IfcClass("External Spatial Structure Element", new IfcClass[] { new IfcClass("External Spatial Element") }),
                    new IfcClass("Spatial Structure Element", new IfcClass[]
                    {
                        new IfcClass("Building"),
                        new IfcClass("Building Storey"),
                        new IfcClass("Site"),
                        new IfcClass("Space")
                    }),
                    new IfcClass("Spatial Zone")
                }),
                new IfcClass("Structural Activity", new IfcClass[]
                {
                    new IfcClass("Structural Action", new IfcClass[]
                    {
                        new IfcClass("Structural Curve Action", new IfcClass[] { new IfcClass("Structural Linear Action") }),
                        new IfcClass("Structural Point Action"),
                        new IfcClass("Structural Surface Action", new IfcClass[] { new IfcClass("Structural Planar Action") })
                    }),
                    new IfcClass("Structural Reaction", new IfcClass[]
                    {
                        new IfcClass("Structural Curve Reaction"),
                        new IfcClass("Structural Point Reaction"),
                        new IfcClass("Structural Surface Reaction")
                    }),
                }),
                new IfcClass("Structural Item", new IfcClass[]
                {
                    new IfcClass("Structural Connection", new IfcClass[]
                    {
                        new IfcClass("Structural Curve Connection"),
                        new IfcClass("Structural Point Connection"),
                        new IfcClass("Structural Surface Connection")
                    }),
                    new IfcClass("Structural Member", new IfcClass[]
                    {
                        new IfcClass("Structural Curve Member", new IfcClass[] { new IfcClass("Structural Curve Member Varying") }),
                        new IfcClass("Structural Surface Member", new IfcClass[] { new IfcClass("Structural Surface Member Varying") })
                    })
                })
            };
        }


        public CollectionViewSource SmartClassCollectionView
        {
            get
            {
                var view = new CollectionViewSource();
                view.GroupDescriptions.Add(new PropertyGroupDescription("SmartCategory"));
                view.Source = new ObservableCollection<ClassMapping>
                {
                    new ClassMapping("Structural System", "Building Element Proxy", "Системы"),
                    new ClassMapping("Generic System", "Building Element Proxy", "Системы"),
                    new ClassMapping("Conduit System", "Distribution System", "Системы"),
                    new ClassMapping("Ducting System", "Distribution System", "Системы"),
                    new ClassMapping("Electrical System", "Distribution System", "Системы"),
                    new ClassMapping("Equipment System", "Building Element Proxy", "Системы"),
                    new ClassMapping("Piping System", "Distribution System", "Системы"),
                    new ClassMapping("Wall System", "Wall", "Системы"),
                    new ClassMapping("Designed Member", "Member", "Металлоконструкции"),
                    new ClassMapping("Member System Curve", "Member", "Металлоконструкции"),
                    new ClassMapping("Member Part Prismatic", "Member", "Металлоконструкции"),
                    new ClassMapping("Handrail", "Railing", "Металлоконструкции"),
                    new ClassMapping("Slab", "Slab", "Металлоконструкции"),
                    new ClassMapping("Ladder", "Stair", "Металлоконструкции"),
                    new ClassMapping("Struct Insulation Spec", "Building Element Proxy", "Металлоконструкции"),
                    new ClassMapping("SPS Wall Part", "Wall", "Стройка"),
                    new ClassMapping("Stair", "Stair", "Стройка"),
                    new ClassMapping("Footing", "Footing", "Стройка"),
                    new ClassMapping("Foundation Component", "Distribution Flow Element", "Стройка"),
                    new ClassMapping("Equipment Foundation", "Distribution Flow Element", "Стройка"),
                    new ClassMapping("Footing Component", "Footing", "Стройка"),
                    new ClassMapping("Designed Equipment", "Distribution Flow Element", "Оборудование"),
                    new ClassMapping("Equipment Component", "Distribution Flow Element", "Оборудование"),
                    new ClassMapping("Duct Part", "Distribution Flow Element", "Воздуховоды"),
                    new ClassMapping("Duct Component", "Distribution Control Element", "Воздуховоды"),
                    new ClassMapping("Duct Run", "Distribution Control Element", "Воздуховоды"),
                    new ClassMapping("Duct Spool", "Flow Terminal", "Воздуховоды"),
                    new ClassMapping("Duct Along Leg Feature", "Distribution Control Element", "Воздуховоды"),
                    new ClassMapping("Duct Branch Feature", "Distribution Control Element", "Воздуховоды"),
                    new ClassMapping("Duct End Feature", "Distribution Control Element", "Воздуховоды"),
                    new ClassMapping("Duct Slant Transition Feature", "Distribution Control Element", "Воздуховоды"),
                    new ClassMapping("Duct Split Feature", "Distribution Control Element", "Воздуховоды"),
                    new ClassMapping("Duct Straight Feature", "Distribution Control Element", "Воздуховоды"),
                    new ClassMapping("Duct Surface Mount Feature", "Distribution Control Element", "Воздуховоды"),
                    new ClassMapping("Duct Transition Feature", "Distribution Control Element", "Воздуховоды"),
                    new ClassMapping("Duct Turn Feature", "Distribution Control Element", "Воздуховоды"),
                    new ClassMapping("Duct Turn Transition Feature", "Distribution Control Element", "Воздуховоды"),
                    new ClassMapping("Hvac Nozzle", "Distribution Control Element", "Воздуховоды"),
                    new ClassMapping("Conduit Occur", "Distribution Flow Element", "Кабелепроводы"),
                    new ClassMapping("Conduit Component", "Distribution Flow Element", "Кабелепроводы"),
                    new ClassMapping("Conduit Nozzle", "Distribution Control Element", "Кабелепроводы"),
                    new ClassMapping("Conduit Along Leg Feature", "Distribution Flow Element", "Кабелепроводы"),
                    new ClassMapping("Conduit Branch Feature", "Distribution Flow Element", "Кабелепроводы"),
                    new ClassMapping("Conduit End Feature", "Distribution Flow Element", "Кабелепроводы"),
                    new ClassMapping("Conduit Straight Feature", "Distribution Flow Element", "Кабелепроводы"),
                    new ClassMapping("Conduit Turn Feature", "Distribution Flow Element", "Кабелепроводы"),
                    new ClassMapping("Conduit Run", "Distribution Flow Element", "Кабелепроводы"),
                    new ClassMapping("Pipe Part", "Flow Controller", "Трубопроводы"),
                    new ClassMapping("Mechanical Implied Item", "Flow Fitting", "Трубопроводы"),
                    new ClassMapping("Pipe Gasket", "Flow Fitting", "Трубопроводы"),
                    new ClassMapping("Pipe Instrument", "Flow Fitting", "Трубопроводы"),
                    new ClassMapping("Pipeline System", "Flow Controller", "Трубопроводы"),
                    new ClassMapping("Pipe Nozzle", "Flow Moving Device", "Трубопроводы"),
                    new ClassMapping("Pipe Run", "Flow Segment", "Трубопроводы"),
                    new ClassMapping("Pipe Specialty Item", "Building Element Proxy", "Трубопроводы"),
                    new ClassMapping("Pipe Tap", "Flow Moving Device", "Трубопроводы"),
                    new ClassMapping("Implied Part Occurrence", "Flow Fitting", "Трубопроводы"),
                    new ClassMapping("Pipe Along Leg Feature", "Flow Controller", "Трубопроводы"),
                    new ClassMapping("Pipe Branch Feature", "Flow Controller", "Трубопроводы"),
                    new ClassMapping("Pipe End Feature", "Flow Controller", "Трубопроводы"),
                    new ClassMapping("Pipe Straight Feature", "Flow Controller", "Трубопроводы"),
                    new ClassMapping("Pipe Tap Feature", "Flow Controller", "Трубопроводы"),
                    new ClassMapping("Pipe Turn Feature", "Flow Controller", "Трубопроводы"),
                    new ClassMapping("Combined Support", "Building Element Proxy", "Опоры и подвесы для разных систем"),
                    new ClassMapping("Standard Support Component", "Building Element Proxy", "Опоры и подвесы для разных систем"),
                    new ClassMapping("Cable Tray Support", "Building Element Proxy", "Опоры и подвесы для разных систем"),
                    new ClassMapping("Conduit Support", "Building Element Proxy", "Опоры и подвесы для разных систем"),
                    new ClassMapping("Connection Support Component", "Building Element Proxy", "Опоры и подвесы для разных систем"),
                    new ClassMapping("Design Support", "Building Element Proxy", "Опоры и подвесы для разных систем"),
                    new ClassMapping("Duct Support", "Building Element Proxy", "Опоры и подвесы для разных систем"),
                    new ClassMapping("Logical Support Component", "Building Element Proxy", "Опоры и подвесы для разных систем"),
                    new ClassMapping("Pipe Support", "Building Element Proxy", "Опоры и подвесы для разных систем"),
                    new ClassMapping("Cableway Slant Transition Feature", "Distribution Flow Element", "Кабели"),
                    new ClassMapping("Cableway Straight Feature", "Distribution Flow Element", "Кабели"),
                    new ClassMapping("Cableway Transition Feature", "Distribution Flow Element", "Кабели"),
                    new ClassMapping("Cableway Turn Feature", "Distribution Flow Element", "Кабели"),
                    new ClassMapping("Cableway Turn Transition Feature", "Distribution Flow Element", "Кабели"),
                    new ClassMapping("Cableway", "Distribution Flow Element", "Кабели"),
                    new ClassMapping("Cableway Along Leg Feature", "Distribution Flow Element", "Кабели"),
                    new ClassMapping("Cableway Branch Feature", "Distribution Flow Element", "Кабели"),
                    new ClassMapping("Cableway End Feature", "Distribution Flow Element", "Кабели"),
                    new ClassMapping("Cable Tray Part", "Distribution Flow Element", "Кабели"),
                    new ClassMapping("Cable Tray Component", "Distribution Flow Element", "Кабели"),
                    new ClassMapping("Cable Tray Nozzle", "Distribution Control Element", "Кабели"),
                    new ClassMapping("Cable Part", "Distribution Flow Element", "Кабели"),
                    new ClassMapping("Cable Run", "Flow Segment", "Кабели"),
                    new ClassMapping("Cable Nozzle", "Distribution Control Element", "Кабели"),
                    new ClassMapping("Pipe Weld", "Building Element Proxy", "Сварка")
                };
                return view;
            }
        }
    }
}
