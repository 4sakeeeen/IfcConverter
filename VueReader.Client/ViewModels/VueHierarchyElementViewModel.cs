using IfcConverter.Client.Services;
using IfcConverter.Client.Services.Model.Base;
using IfcConverter.Client.ViewModels.Base;
using System.Collections.Generic;
using System.Windows.Input;

namespace IfcConverter.Client.ViewModels
{
    public sealed class VueHierarchyElementViewModel : ViewModel
    {
        public string Name { get; }

        public string Path { get; }

        public VueGraphicElement? GraphicElement { get; }

        public VueHierarchyElementViewModel? Parent { get; }

        public List<VueHierarchyElementViewModel> HierarchyItems { get; } = new();

        private bool _isSelected;

        public bool IsSelected
        {
            get { return _isSelected; }
            set
            {
                SetProperty(ref _isSelected, value);
                SelectAllChildren(HierarchyItems, value);
            }
        }

        public ICommand SelectItemCommand
        {
            get { return new RelayCommand(() => IsSelected ^= true); }
        }

        public VueHierarchyElementViewModel(string name, string path, VueHierarchyElementViewModel? parent = null, VueGraphicElement? graphicElement = null)
        {
            Name = name;
            Path = path;
            Parent = parent;
            GraphicElement = graphicElement;
        }

        private void SelectAllChildren(IEnumerable<VueHierarchyElementViewModel> children, bool isSelected)
        {
            foreach (VueHierarchyElementViewModel child in children)
            {
                child.IsSelected = isSelected;
                SelectAllChildren(child.HierarchyItems, isSelected);
            }
        }
    }
}
