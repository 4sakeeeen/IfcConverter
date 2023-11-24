using IfcConverter.Client.ViewModels.Base;
using IfcConverter.Domain.Models;
using System.Collections.ObjectModel;
using System.Linq;

namespace IfcConverter.Client.ViewModels
{
    public class IfcClassViewModel : ViewModel
    {
        private readonly IfcClass _IfcClass;

        public string Name
        {
            get { return _IfcClass.Name; }
        }

        public ObservableCollection<IfcClassViewModel> Inherits
        {
            get { return new ObservableCollection<IfcClassViewModel>(_IfcClass.Inherits.Select(ifcClass => new IfcClassViewModel(ifcClass))); }
        }

        public IfcClassViewModel(IfcClass ifcClass)
        {
            _IfcClass = ifcClass;
        }
    }
}
