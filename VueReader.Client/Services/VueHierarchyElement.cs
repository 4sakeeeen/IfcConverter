using IfcConverter.Client.Services.Model;
using IfcConverter.Client.ViewModels.Base;
using System.Collections.Generic;
using System.Windows.Input;

namespace IfcConverter.Client.Services
{
    public sealed class VueHierarchyElement : ViewModel
    {
        public string Name { get; }

        public string Path { get; }

        public VueGraphicElement? GraphicElement { get; }

        public VueHierarchyElement? Parent { get; }

        public List<VueHierarchyElement> HierarchyItems { get; } = new();

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

        public VueHierarchyElement(string name, string path, VueHierarchyElement? parent = null, VueGraphicElement? graphicElement = null)
        {
            this.Name = name;
            this.Path = path;
            this.Parent = parent;
            this.GraphicElement = graphicElement;
        }

        private void SelectAllChildren(IEnumerable<VueHierarchyElement> children, bool isSelected)
        {
            foreach (VueHierarchyElement child in children)
            {
                child.IsSelected = isSelected;
                SelectAllChildren(child.HierarchyItems, isSelected);
            }
        }
    }
}
