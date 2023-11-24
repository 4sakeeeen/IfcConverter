using IfcConverter.Client.ViewModels.Base;
using IfcConverter.Domain.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Windows.Data;

namespace IfcConverter.Client.ViewModels
{
    internal sealed class SettingsWindowViewModel : ViewModel
    {
        private S3DClassMappingViewModel? _SelectedClassMapping;

        public S3DClassMappingViewModel? SelectedClassMapping
        {
            get { return _SelectedClassMapping; }
            set { SetProperty(ref _SelectedClassMapping, value); }
        }

        public static ObservableCollection<IfcClassViewModel> IfcClasses
        {
            get
            {
                List<IfcClass>? ifcClasses = JsonSerializer.Deserialize<List<IfcClass>>(File.ReadAllText($"data\\class-list-ifc4.json"));
                return ifcClasses != null
                    ? new ObservableCollection<IfcClassViewModel>(ifcClasses.Select(ifcClass => new IfcClassViewModel(ifcClass)))
                    : throw new Exception("Unable to extract IFC class list.");
            }
        }


        public ICollectionView ClassMappingsView
        {
            get
            {
                List<S3DClassMapping>? classMappings = JsonSerializer.Deserialize<List<S3DClassMapping>>(File.ReadAllText($"data\\class-mapping.json"))
                    ?? throw new Exception("Unable to extract class mappings.");
                var view = new CollectionViewSource();
                view.GroupDescriptions.Add(new PropertyGroupDescription("Category"));
                view.Source = new ObservableCollection<S3DClassMappingViewModel>(classMappings.Select(classMapping => new S3DClassMappingViewModel(classMapping)));
                return view.View;
            }
        }
    }
}
