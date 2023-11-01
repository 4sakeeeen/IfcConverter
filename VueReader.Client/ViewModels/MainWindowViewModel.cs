using IfcConverter.Client.Services;
using IfcConverter.Client.Services.Model;
using IfcConverter.Client.Windows;
using IngrDataReadLib;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Xbim.Common;
using Xbim.Ifc;
using Xbim.Ifc4.GeometricModelResource;
using Xbim.Ifc4.GeometryResource;
using Xbim.Ifc4.Kernel;
using Xbim.Ifc4.ProductExtension;
using Xbim.Ifc4.RepresentationResource;
using Xbim.Ifc4.SharedBldgElements;
using Xbim.Ifc4.TopologyResource;

namespace IfcConverter.Client.ViewModels
{
    internal sealed class MainWindowViewModel : Base.ViewModel
    {
        private string _AuthorProduct = string.Empty;

        public string AuthorProduct
        {
            get => this._AuthorProduct;
            set => this.SetProperty(ref this._AuthorProduct, value);
        }


        private string _SorceFileName = string.Empty;

        public string SorceFileName
        {
            get => this._SorceFileName;
            set => this.SetProperty(ref this._SorceFileName, value);
        }


        private string _ViewerProduct = string.Empty;

        public string ViewerProduct
        {
            get => this._ViewerProduct;
            set => this.SetProperty(ref this._ViewerProduct, value);
        }


        private string _FileMajorVersion = string.Empty;

        public string FileMajorVersion
        {
            get => this._FileMajorVersion;
            set => this.SetProperty(ref this._FileMajorVersion, value);
        }


        private string _FileMinorVersion = string.Empty;

        public string FileMinorVersion
        {
            get => this._FileMinorVersion;
            set => this.SetProperty(ref this._FileMinorVersion, value);
        }


        public ObservableCollection<ProductTreeItem> ProductTreeItems { get; set; } = new();


        public ICommand UploadModelCommand
        {
            get => new RelayCommand(() =>
            {
                var ofd = new OpenFileDialog { Filter = "VUE files (*.vue)|*.vue" };
                if (ofd.ShowDialog() == true)
                {
                    IIngrDataReader reader = new IngrDataReader();
                    eErrorCode errorCode = eErrorCode.E_OK;
                    RdMaterialProperties? materialProperties = null;
                    IngrGeom? oGeom = null;

                    IfcStore ifcModel = IfcService.CreateAndInitModel("HPU_400: Site");
                    IfcBuilding ifcBuilding = IfcService.CreateBuilding(ifcModel, "HPU_400: Building: Building");
                    IfcBuildingStorey buildingStorey = IfcService.CreateBuildingStorey(ifcBuilding, "HPU_400: Building Storey");
                    var vueGroups = new List<VueGroup>();

                    try
                    {
                        reader.OpenVueFile(ofd.FileName, out errorCode);
                        SourceFileInfo sourceFileInfo = new();
                        reader.GetSourceFileInfo(ref sourceFileInfo);
                        this.AuthorProduct = sourceFileInfo.AuthorProduct;
                        this.SorceFileName = sourceFileInfo.SorceFileName;
                        this.ViewerProduct = sourceFileInfo.ViewerProduct;
                        this.FileMajorVersion = sourceFileInfo.FileMajorVersion;
                        this.FileMinorVersion = sourceFileInfo.FileMinorVersion;


                        while (errorCode != eErrorCode.E_EOF)
                        {
                            reader.GetCurrentGraphicLabelProperties(out Array propertyArray, out errorCode);
                            if (errorCode != eErrorCode.E_OK) { throw new Exception($"Unable to fetch label properties [{errorCode}]"); }
                            var properties = new Dictionary<string, string>(propertyArray.Cast<string>().Select((v) => new KeyValuePair<string, string>(v.Split(" : ")[0], v.Split(" : ")[1])).GroupBy(x => x.Key).Select(x => x.First()));

                            reader.GetCurrentGraphicIdentifier(out string linkage, out string moniker, out string spfuid);
                            reader.GetCurrentGraphicElement(out eGraphicType gtype, ref oGeom, ref materialProperties, out errorCode);
                            if (errorCode != eErrorCode.E_OK) { throw new Exception($"Unable to fetch graphic geometry of object '{properties["Name"]}' [{errorCode}]"); }

                            IfcService.InTransaction(ifcModel, "Create product", () =>
                            {
                                IfcMember element = ifcModel.Instances.New<IfcMember>(member => member.Name = properties["Name"]);
                                properties.TryGetValue("System Path", out string? systemPath);
                                IfcService.BuildAndDecomposeHierarchy(buildingStorey, element, systemPath);

                                switch (gtype)
                                {
                                    case eGraphicType.GROUP_TYPE:
                                        vueGroups.Add(VueService.ConvertGroupTypeGeometry(oGeom as IRdGroup ?? throw new Exception()));
                                        break;
                                        if (oGeom is IRdGroup group)
                                        {
                                            IngrGeom? geom = null;
                                            RdMaterialProperties? material = null;
                                            group.GetElementCountFromGroup(out int count);

                                            for (int i = 0; i < count; i++)
                                            {
                                                group.GetElementFromGroup(i, out eGraphicType _type, ref geom, ref material);
                                                group.GetAspectFromGroup(i, out int aspect);

                                                switch (_type)
                                                {
                                                    case eGraphicType.PLANE_TYPE:
                                                        RdBoundary? boundary = null;

                                                        if (geom is IRdPlane plane)
                                                        {
                                                            plane.GetNormal(out Position normal);
                                                            plane.GetuDirection(out Position dir);
                                                            plane.ReadBoundary(ref boundary);
                                                            element.Representation = ifcModel.Instances.New<IfcProductDefinitionShape>();
                                                            element.Representation.Representations.Add(ifcModel.Instances.New<IfcShapeRepresentation>(shrep =>
                                                            {
                                                                shrep.ContextOfItems = ifcModel.Instances.OfType<IfcGeometricRepresentationContext>().FirstOrDefault();
                                                                shrep.Items.Add(ifcModel.Instances.New<IfcFacetedBrep>(
                                                                    brep => brep.Outer = ifcModel.Instances.New<IfcClosedShell>(shell => shell.CfsFaces.Add(ifcModel.Instances.New<IfcFace>(face =>
                                                                        face.Bounds.Add(ifcModel.Instances.New<IfcFaceOuterBound>(
                                                                            bound => bound.Bound = ifcModel.Instances.New<IfcPolyLoop>(loop =>
                                                                            {
                                                                                var uniqueVertices = new List<Position>();
                                                                                boundary.GetBoundaryCount(out int boundaryCount);

                                                                                for (int y = 0; y < boundaryCount; y++)
                                                                                {
                                                                                    boundary.GetBoundaryCurveCount(y, out int curveCount);

                                                                                    for (int c = 0; c < curveCount; c++)
                                                                                    {
                                                                                        IngrGeom? curveGeom = null;
                                                                                        boundary.GetBoundaryCurve(y, c, ref curveGeom, out eGraphicType curveType);

                                                                                        switch (curveType)
                                                                                        {
                                                                                            case eGraphicType.LINESTRING_TYPE:
                                                                                                if (curveGeom is IRdLineString lineString)
                                                                                                {
                                                                                                    lineString.GetVertices(out Array vertices, out int verticeCount);
                                                                                                    vertices.Cast<Position>().ToList().ForEach(v =>
                                                                                                    {
                                                                                                        if (!uniqueVertices.Contains(v))
                                                                                                        {
                                                                                                            uniqueVertices.Add(v);
                                                                                                        }
                                                                                                    });
                                                                                                }
                                                                                                break;

                                                                                            case eGraphicType.BSPCURVE_TYPE:
                                                                                                break;

                                                                                            case eGraphicType.LINE_TYPE:
                                                                                                if (curveGeom is IRdLine line)
                                                                                                {
                                                                                                    line.GetLineEndPoints(out Position startPoint, out Position endPoint);
                                                                                                    if (!uniqueVertices.Contains(startPoint)) { uniqueVertices.Add(startPoint); }
                                                                                                    if (!uniqueVertices.Contains(endPoint)) { uniqueVertices.Add(endPoint); }
                                                                                                }
                                                                                                break;

                                                                                            case eGraphicType.ARC_TYPE:
                                                                                                break;

                                                                                            case eGraphicType.ELLIPSE_TYPE:
                                                                                                break;

                                                                                            default:
                                                                                                throw new Exception($"Invalid curve TYPE: {curveType}");
                                                                                        }
                                                                                    }
                                                                                }

                                                                                uniqueVertices.ForEach(p => loop.Polygon.Add(ifcModel.Instances.New<IfcCartesianPoint>(point => point.SetXYZ(p.m_xPosition, p.m_yPosition, p.m_zPosition))));
                                                                            })
                                                                        ))
                                                                    )))
                                                                ));
                                                            }));
                                                        }
                                                        break;
                                                }
                                            }
                                        }
                                        break;

                                    default:
                                        throw new Exception($"Invalid graphic element TYPE: {gtype}");
                                }
                            });

                            reader.GetNextGraphicElement(out errorCode);
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message);
                    }
                    finally
                    {
                        reader.CloseVueFile();
                        // ifcModel.SaveAs("C:\\Users\\Windows 11\\source\\repos\\IfcConverter\\DataExamples\\TestEnv.ifc", Xbim.IO.StorageType.Ifc);
                        MessageBox.Show("Finished");
                    }

                    Stack<IfcElement> elements = new();

                    ProductTreeItems.Add(GenerateTreeViewContextItems(ifcModel) ?? throw new Exception());
                }
            });
        }


        public ICommand OpenSettingsCommand
        {
            get => new RelayCommand<Window>((owner) =>
            {
                var win = new SettingsWindow { Owner = owner };
                win.ShowDialog();
            });
        }


        private ProductTreeItem? GenerateTreeViewContextItems(IModel model)
        {
            var buidingStoreyAggregate = model.Instances.Where<IfcRelContainedInSpatialStructure>(x => x.RelatingStructure is IfcBuildingStorey);
            var aggregates = new Queue<IfcRelAggregates>(model.Instances.Where<IfcRelAggregates>(x => x.Equals(x)));
            List<ProductTreeItem> allItems = new();
            ProductTreeItem? root = null;

            if (buidingStoreyAggregate.Count() == 1)
            {
                root = new ProductTreeItem(buidingStoreyAggregate.First().RelatingStructure.Name ?? string.Empty, buidingStoreyAggregate.First().RelatingStructure.GlobalId);
                allItems.Add(root);
                buidingStoreyAggregate.First().RelatedElements.ToList().ForEach(x => root.Children.Add(new ProductTreeItem(x.Name ?? string.Empty, x.GlobalId)));
                allItems.AddRange(root.Children);
            }

            while (aggregates.Count > 3)
            {
                IfcRelAggregates currAggregate = aggregates.Dequeue();
                ProductTreeItem? currItem = allItems.FirstOrDefault(x => x.Guid == currAggregate.RelatingObject.GlobalId);

                if (currItem != null)
                {
                    currAggregate.RelatedObjects.ToList().ForEach(x =>
                    {
                        var t = new ProductTreeItem(x.Name ?? string.Empty, x.GlobalId);
                        allItems.Add(t);
                        currItem.Children.Add(t);
                    });
                }
                else
                {
                    aggregates.Enqueue(currAggregate);
                }
            }

            return root;
        }
    }
}
