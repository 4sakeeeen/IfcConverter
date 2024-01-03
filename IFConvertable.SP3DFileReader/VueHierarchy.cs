using IFConvertable.SP3DFileReader.DTO;
using Serilog;
using System.Reflection.Metadata.Ecma335;
using System.Text.Json;

namespace IFConvertable.SP3DFileReader
{
    public sealed class VueHierarchy
    {
        public VueHierarchyItem Root { get; } = new() { Ident = new SmartId(), Name = "ROOT", Type = HierarchyItemType.SYSTEM };

        /// <summary>
        /// Create new hierarchy item and insert it to general hierarchy tree by using Children property.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="hierarchyNode"></param>
        /// <param name="systemPathNodes"></param>
        /// <exception cref="Exception"></exception>
        public void Insert(SmartId id, string hierarchyNode, string[] systemPathNodes)
        {
            VueHierarchyItem suggestParentNode = Root;
            var item = new VueHierarchyItem
            {
                Ident = id,
                Name = hierarchyNode,
                Type = HierarchyItemType.ELEMENT
            };

            if (systemPathNodes.Length != 0)
            {
                foreach (string pathNode in systemPathNodes)
                {
                    IEnumerable<VueHierarchyItem> existsParents = suggestParentNode.Items.Where(child => child.Name == pathNode);
                    
                    if (!existsParents.Any())
                    {
                        var newFolderNode = new VueHierarchyItem
                        {
                            Ident = new SmartId(),
                            Name = pathNode,
                            Type = HierarchyItemType.FOLDER
                        };
                        suggestParentNode.Items.Add(newFolderNode);
                        suggestParentNode = newFolderNode;
                    }
                    else
                    {
                        suggestParentNode = existsParents.First();

                        if (existsParents.Count() > 1)
                        {
                            Log.Warning($"Found more than one elements by name '{pathNode}' to suggest it as parent");
                        }
                    }
                }
            }

            suggestParentNode.Items.Add(item);
        }

        public string SerializeToJson()
        {
            return JsonSerializer.Serialize(Root.Items, new JsonSerializerOptions { WriteIndented = true });
        }
    }
}
