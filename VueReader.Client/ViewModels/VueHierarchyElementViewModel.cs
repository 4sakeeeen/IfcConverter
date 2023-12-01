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

        public string FullPath { get; }

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
                //if (Parent != null) SelectParentBranch(Parent, value);
            }
        }

        public ICommand SelectItemCommand
        {
            get { return new RelayCommand(() => IsSelected ^= true); }
        }

        public VueHierarchyElementViewModel(string name, string fullPath, VueHierarchyElementViewModel? parent = null, VueGraphicElement? graphicElement = null)
        {
            Name = name;
            FullPath = fullPath;
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

        private void SelectParentBranch(VueHierarchyElementViewModel parent, bool isSelected)
        {
            parent.IsSelected = isSelected;
            if (parent.Parent != null) SelectParentBranch(parent.Parent, isSelected);
        }
    }
}
