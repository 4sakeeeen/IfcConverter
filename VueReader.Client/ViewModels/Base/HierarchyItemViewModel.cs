using IfcConverter.Client.Services;
using IfcConverter.Client.Services.Model.Base;
using IFConvertable.SP3DFileReader.DTO;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace IfcConverter.Client.ViewModels.Base
{
    /// <summary>
    /// Base class of hierarchy items for tree view elements presentation.
    /// </summary>
    public abstract class HierarchyItemViewModel : ViewModel
    {
        /// <summary>
        /// Display name of hierarchy node in tree view.
        /// </summary>
        public string DisplayName { get; }

        [Obsolete("Old")]
        public string FullPath { get; } = string.Empty;

        [Obsolete("Old")]
        public VueGraphicElement? GraphicElement { get; }

        [Obsolete("Old")]
        public HierarchyItemViewModel? Parent { get; }

        /// <summary>
        /// Hierarchy children items.
        /// </summary>
        public ObservableCollection<HierarchyItemViewModel> Children { get; } = new();

        private bool _isSelected;

        /// <summary>
        /// If property is set it also to all children items automaticly.
        /// </summary>
        public bool IsSelected
        {
            get { return _isSelected; }
            set
            {
                SetProperty(ref _isSelected, value);
                SelectAllChildren(Children, value);
            }
        }

        public ICommand SelectItemCommand
        {
            get { return new RelayCommand(() => IsSelected ^= true); }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="hierarchyItem">DTO class.</param>
        /// <exception cref="NotImplementedException"></exception>
        public HierarchyItemViewModel(VueHierarchyItem hierarchyItem)
        {
            DisplayName = hierarchyItem.Name;
            hierarchyItem.Items.ForEach(x =>
            {
                switch (x.Type)
                {
                    case HierarchyItemType.ELEMENT: Children.Add(new HierarchyElementViewModel(x)); break;
                    case HierarchyItemType.SYSTEM: Children.Add(new HierarchySystemViewModel(x)); break;
                    case HierarchyItemType.FOLDER: Children.Add(new HierarchyFolderViewModel(x)); break;
                    case HierarchyItemType.SPATIAL_ELEMENT: Children.Add(new HierarchySpatialElementViewModel(x)); break;
                    default: throw new NotImplementedException($"Not implemented view for {x.Type}.");
                }
            });
        }

        private void SelectAllChildren(IEnumerable<HierarchyItemViewModel> children, bool isSelected)
        {
            foreach (HierarchyItemViewModel child in children)
            {
                child.IsSelected = isSelected;
                SelectAllChildren(child.Children, isSelected);
            }
        }
    }
}
