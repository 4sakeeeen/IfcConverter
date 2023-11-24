using IfcConverter.Client.ViewModels.Base;
using IfcConverter.Domain.Models;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Data;

namespace IfcConverter.Client.ViewModels
{
    public class S3DClassMappingViewModel : ViewModel
    {
        private readonly S3DClassMapping _ClassMapping;

        public string Category
        {
            get { return _ClassMapping.Caterory; }
        }

        public string ClassName
        {
            get { return _ClassMapping.ClassName; }
        }

        public string MappedToClassIFC
        {
            get { return _ClassMapping.MappedClassIFC; }
        }


        private IfcClass? _SelectedValue;

        public IfcClass? SelectedValue
        {
            get => _SelectedValue;
            set => SetProperty(ref _SelectedValue, value);
        }

        public CollectionViewSource CollectionView
        {
            get
            {
                var view = new CollectionViewSource();
                view.GroupDescriptions.Add(new PropertyGroupDescription("Category"));
                var ifcClasses = new ObservableCollection<IfcClass>
                {

                };
                
                if (SelectedValue != null)
                {
                    view.Source = new ObservableCollection<IfcClass>(ifcClasses.Where(x => x.Name.Contains(SelectedValue.Name)));                    
                }
                else
                {
                    view.Source = new ObservableCollection<IfcClass>(ifcClasses);
                }

                return view;
            }
        }

        public S3DClassMappingViewModel(S3DClassMapping classMapping)
        {
            _ClassMapping = classMapping;
        }
    }
}
