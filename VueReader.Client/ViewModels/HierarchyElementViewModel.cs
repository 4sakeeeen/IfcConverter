using IfcConverter.Client.ViewModels.Base;
using IFConvertable.SP3DFileReader.DTO;

namespace IfcConverter.Client.ViewModels
{
    public sealed class HierarchyElementViewModel : HierarchyItemViewModel
    {
        public HierarchyElementViewModel(VueHierarchyItem hierarchyItem) : base(hierarchyItem)
        {
        }
    }
}
