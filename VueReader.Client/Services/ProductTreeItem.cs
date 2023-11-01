using System.Collections.ObjectModel;

namespace IfcConverter.Client.Services
{
    public class ProductTreeItem
    {
        public string Guid { get; set; }

        public string Name { get; set; }

        public ObservableCollection<ProductTreeItem> Children { get; set; }

        public ProductTreeItem(string name, string guid)
        {
            this.Name = name;
            this.Guid = guid;
            this.Children = new ObservableCollection<ProductTreeItem>();
        }
    }
}
