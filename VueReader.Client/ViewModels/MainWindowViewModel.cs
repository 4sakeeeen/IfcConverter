﻿using IfcConverter.Client.Services;
using IfcConverter.Client.Services.Model;
using IfcConverter.Client.ViewModels.Base;
using IfcConverter.Client.Windows;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Xbim.Ifc4.PresentationAppearanceResource;
using Xbim.Ifc4.RepresentationResource;

namespace IfcConverter.Client.ViewModels
{
    internal sealed class MainWindowViewModel : ViewModel
    {
        private VueFile? _VueFile = null;

        private string? _AuthorProduct;

        public string? AuthorProduct
        {
            get { return _AuthorProduct; }
            set { _ = SetProperty(ref _AuthorProduct, value); }
        }

        private string? _SorceFileName;

        public string? SorceFileName
        {
            get { return _SorceFileName; }
            set { _ = SetProperty(ref _SorceFileName, value); }
        }

        private string? _ViewerProduct;

        public string? ViewerProduct
        {
            get { return _ViewerProduct; }
            set { SetProperty(ref _ViewerProduct, value); }
        }

        private string? _FileMajorVersion;

        public string? FileMajorVersion
        {
            get { return _FileMajorVersion; }
            set { _ = SetProperty(ref _FileMajorVersion, value); }
        }

        private string? _FileMinorVersion;

        public string? FileMinorVersion
        {
            get { return _FileMinorVersion; }
            set { _ = SetProperty(ref _FileMinorVersion, value); }
        }

        private ObservableCollection<VueHierarchyElement>? _ProductTreeItems;

        public ObservableCollection<VueHierarchyElement>? ProductTreeItems
        {
            get { return _ProductTreeItems; }
            set { _ = SetProperty(ref _ProductTreeItems, value); }
        }

        public ICommand UploadModelCommand
        {
            get { return new RelayCommandAsync(Upload, (ex) => App.Logger.Error(ex, "Upload vue file error")); }
        }

        public ICommand OpenSettingsCommand
        {
            get
            {
                return new RelayCommand<Window>((owner) =>
                {
                    var win = new SettingsWindow { Owner = owner };
                    win.ShowDialog();
                });
            }
        }

        public ICommand PerformConvertionCommand
        {
            get { return new RelayCommandAsync(PerformConvertion, (ex) => App.Logger.Error(ex, "Convertion error")); }
        }

        private async Task Upload()
        {
            await Task.Run(() =>
            {
                var ofd = new OpenFileDialog { Filter = "VUE files (*.vue)|*.vue" };
                if (ofd.ShowDialog() == true)
                {
                    try
                    {
                        _VueFile = new VueFile(ofd.FileName, tessellationTolerance: 20);
                        ProductTreeItems = new ObservableCollection<VueHierarchyElement>(_VueFile.Hierarchy.RootElements);
                        AuthorProduct = _VueFile.AuthorProduct;
                        SorceFileName = _VueFile.SorceFileName;
                        ViewerProduct = _VueFile.ViewerProduct;
                        FileMajorVersion = _VueFile.FileMajorVersion;
                        FileMinorVersion = _VueFile.FileMinorVersion;
                    }
                    catch (Exception ex)
                    {
                        throw new Exception("Upload command failed", ex);
                    }
                }
            });
        }

        private async Task PerformConvertion()
        {
            await Task.Run(() =>
            {
                _VueFile?.SaveToIfc(projectName: SorceFileName ?? "Nonamed Project", "C:\\Users\\Windows 11\\source\\repos\\IfcConverter\\DataExamples\\TestEnv.ifc");
            });
        }

        private IEnumerable<VueHierarchyElement> GetSelectedItems(IEnumerable<VueHierarchyElement> items)
        {
            var result = new List<VueHierarchyElement>();
            
            foreach (VueHierarchyElement item in items)
            {
                if (item.IsSelected)
                {
                    result.Add(item);
                }
                else
                {
                    result.AddRange(GetSelectedItems(item.HierarchyItems));
                }
            }

            return result;
        }
    }
}
