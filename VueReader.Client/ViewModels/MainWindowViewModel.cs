using IfcConverter.Client.Services;
using IfcConverter.Client.Services.Model;
using IfcConverter.Client.Windows;
using Microsoft.Win32;
using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using Serilog;
using Serilog.Core;

namespace IfcConverter.Client.ViewModels
{
    internal sealed class MainWindowViewModel : Base.ViewModel
    {
        private string _AuthorProduct = string.Empty;

        private Logger _Logger = new LoggerConfiguration().WriteTo.File("logs\\common.log", rollingInterval: RollingInterval.Day).CreateLogger();

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
                    try
                    {
                        var vueFile = new VueFile(ofd.FileName, tessellationTolerance: 20);
                        this.AuthorProduct = vueFile.AuthorProduct;
                        this.SorceFileName = vueFile.SorceFileName;
                        this.ViewerProduct = vueFile.ViewerProduct;
                        this.FileMajorVersion = vueFile.FileMajorVersion;
                        this.FileMinorVersion = vueFile.FileMinorVersion;
                        vueFile.SaveToIfc(projectName: this.SorceFileName, "C:\\Users\\Windows 11\\source\\repos\\IfcConverter\\DataExamples\\TestEnv.ifc");
                        // ProductTreeItems.Add(vueFile.GenerateTreeViewContextItems() ?? throw new Exception());
                    }
                    catch (Exception ex)
                    {
                        _Logger.Error(new Exception("Upload command failed", ex), "Upload vue file error");
                    }
                    finally
                    {
                        MessageBox.Show("Finished");
                    }
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


        public MainWindowViewModel()
        {
            new LoggerConfiguration().CreateLogger();
        }
    }
}
