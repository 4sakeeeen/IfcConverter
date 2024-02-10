using IfcConverter.Client.Services;
using IfcConverter.Client.Services.Filter;
using IfcConverter.Client.Services.Model;
using IfcConverter.Client.ViewModels.Base;
using IfcConverter.Client.Windows;
using IFConvertable.SP3DFileReader.DTO;
using Microsoft.Win32;
using Serilog;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace IfcConverter.Client.ViewModels
{
    public sealed class MainWindowViewModel : ViewModel
    {
        private VueFile? _VueFile = null;

        private VueFileMetadata _FileInfo = new();

        public VueFileMetadata FileInfo
        {
            get { return _FileInfo; }
            set { SetProperty(ref _FileInfo, value); }
        }

        private string? _ProgressName;

        public string? ProgressName
        {
            get { return _ProgressName; }
            set { SetProperty(ref _ProgressName, value); }
        }

        private string? _ProgressInfo;

        public string? ProgressInfo
        {
            get { return _ProgressInfo; }
            set { SetProperty(ref _ProgressInfo, value); }
        }

        private string? _ProgressAdditionalInfo;

        public string? ProgressAdditionalInfo
        {
            get { return _ProgressAdditionalInfo; }
            set { SetProperty(ref _ProgressAdditionalInfo, value); }
        }

        private bool _IsLoading = false;

        public bool IsLoading
        {
            get { return _IsLoading; }
            set { SetProperty(ref _IsLoading, value); }
        }

        public IEnumerable<ObjectFilter>? Filters
        {
            get
            {
                return JsonSerializer.Deserialize<List<ObjectFilter>>(File.ReadAllText("data\\filters.json"));
            }
        }

        private ObjectFilter? _SelectedFilter;

        public ObjectFilter? SelectedFilter
        {
            get { return _SelectedFilter; }
            set { SetProperty(ref _SelectedFilter, value); }
        }

        private ObservableCollection<HierarchyItemViewModel>? _VueHierarchyItems;

        public ObservableCollection<HierarchyItemViewModel>? ProductTreeItems
        {
            get { return _VueHierarchyItems; }
            set { SetProperty(ref _VueHierarchyItems, value); }
        }

        public ICommand UploadModelCommand
        {
            get { return new RelayCommandAsync(Upload, (ex) => Log.Logger.Error(ex, "Upload vue file error")); }
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
            get { return new RelayCommandAsync(PerformConvertion, (ex) => Log.Logger.Error(ex, "Convertion error")); }
        }

        private async Task Upload()
        {
            await Task.Run(() =>
            {
                OpenFileDialog ofd = new()
                {
                    Title = "Select smart plant 3D model file",
                    Filter = "VUE File (*.vue)|*.vue"
                };

                if (ofd.ShowDialog().GetValueOrDefault())
                {
                    try
                    {
                        IsLoading = true;
                        int read = 0;

                        IFConvertable.SP3DFileReader.VueFileReader reader = new(ofd.FileName);
                        reader.ReadPerformed += (e) =>
                        {
                            if (e.Action == IFConvertable.SP3DFileReader.ReaderActions.GET_ATTRIBUTES && e.Context != null)
                            {
                                ((Dictionary<string, string>)e.Context).TryGetValue("Name", out string? elementName);
                                ProgressName = "Reading...";
                                ProgressInfo = $"{elementName}";
                                ProgressAdditionalInfo = $"Processed {read++} graphic elements";
                            }
                            Log.Information($"{e.Action}: {e.ResultCode}");
                        };

                        ProgressName = "Creating reader instance...";
                        FileInfo = reader.ReadFileMetadata();
                        ProductTreeItems = new ObservableCollection<HierarchyItemViewModel> { new HierarchySystemViewModel(reader.ReadHierarchy().Root) };
                    }
                    catch (Exception ex)
                    {
                        Log.Logger.Error(ex, "Upload .vue file failed");
                        MessageBox.Show(ex.Message, "Upload command failed", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                    finally
                    {
                        IsLoading = false;
                    }
                }
            });
        }

        private async Task PerformConvertion()
        {
            await Task.Run(() =>
            {
                try
                {
                    SaveFileDialog ofd = new()
                    {
                        Title = "Save converted model",
                        Filter = "International Foundation Class (*.ifc)|*.ifc"
                    };

                    if (ofd.ShowDialog().GetValueOrDefault())
                    {
                        IsLoading = true;
                        ProgressAdditionalInfo = "Converting elements...";

                        _VueFile?.SaveToIfc(projectName: FileInfo.SourceName ?? "Nonamed Project", ofd.FileName, SelectedFilter);
                    }
                }
                catch (Exception ex)
                {
                    Log.Logger.Error(ex, "Convertation process failed");
                    MessageBox.Show(ex.Message, "Convertation failed", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                finally
                {
                    IsLoading = false;
                }
            });
        }

        private IEnumerable<HierarchyItemViewModel> GetSelectedItems(IEnumerable<HierarchyItemViewModel> items)
        {
            var result = new List<HierarchyItemViewModel>();
            
            foreach (HierarchyItemViewModel item in items)
            {
                if (item.IsSelected)
                {
                    result.Add(item);
                }
                else
                {
                    result.AddRange(GetSelectedItems(item.Children));
                }
            }

            return result;
        }
    }
}
