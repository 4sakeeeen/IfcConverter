using IfcConverter.Domain.Models;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Data;

namespace IfcConverter.Client.ViewModels.Base
{
    internal class ClassMapping : ViewModel
    {
        public string SmartCategory { get; set; }

        public string SmartClass { get; set; }

        public string IfcClass { get; set; }


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
                
                if (this.SelectedValue != null)
                {
                    view.Source = new ObservableCollection<IfcClass>(ifcClasses.Where(x => x.Name.Contains(this.SelectedValue.Name)));                    
                }
                else
                {
                    view.Source = new ObservableCollection<IfcClass>(ifcClasses);
                }

                return view;
            }
        }

        public ClassMapping(string smartClass, string ifcClass, string smartCategory)
        {
            this.SmartClass = smartClass;
            this.IfcClass = ifcClass;
            this.SmartCategory = smartCategory;
        }
    }
}
