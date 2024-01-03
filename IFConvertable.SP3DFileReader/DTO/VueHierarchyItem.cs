namespace IFConvertable.SP3DFileReader.DTO
{
    public enum HierarchyItemType
    {
        FOLDER,
        SPATIAL_ELEMENT,
        ELEMENT,
        SYSTEM
    }


    public sealed class VueHierarchyItem : IEquatable<VueHierarchyItem>
    {
        public required SmartId Ident { get; init; }

        public required string Name { get; init; }

        public required HierarchyItemType Type { get; init; }

        public List<VueHierarchyItem> Items { get; init; } = new();

        public bool Equals(VueHierarchyItem? other)
        {
            if (ReferenceEquals(this, other)) return true;

            if (other is null) return false;
            
            return Ident.UID == other.Ident.UID;
        }

        public override bool Equals(object? obj)
        {
            return obj is VueHierarchyItem item && Equals(item);
        }

        public override int GetHashCode()
        {
            return Ident.UID.GetHashCode();
        }

        public static bool operator ==(VueHierarchyItem? left, VueHierarchyItem? right)
        {
            return ReferenceEquals(left, right) || (left is not null && left.Equals(right));
        }

        public static bool operator !=(VueHierarchyItem left, VueHierarchyItem? right)
        {
            return !(left == right);
        }
    }
}
