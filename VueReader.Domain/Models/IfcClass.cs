using System.Collections.ObjectModel;

namespace IfcConverter.Domain.Models
{
    public class IfcClass
    {
        public string Name { get; set; }

        public string? Category { get; set; }

        public ObservableCollection<IfcClass> Children { get; set; } = new();

        public IfcClass(string name, string category)
        {
            this.Name = name;
            this.Category = category;
        }

        public IfcClass(string name, IEnumerable<IfcClass>? children = null)
        {
            this.Name = name;
            if (children != null) this.Children = new ObservableCollection<IfcClass>(children);
        }
    }
}
