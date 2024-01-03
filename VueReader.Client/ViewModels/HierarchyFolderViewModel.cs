using IfcConverter.Client.ViewModels.Base;
using IFConvertable.SP3DFileReader.DTO;

namespace IfcConverter.Client.ViewModels
{
    public sealed class HierarchyFolderViewModel : HierarchyItemViewModel
    {
        public HierarchyFolderViewModel(VueHierarchyItem hierarchyItem) : base(hierarchyItem)
        {
        }
    }
}
