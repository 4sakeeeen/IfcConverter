using System.Text.Json;
using IFConvertable.SP3DFileReader.DTO;
using Serilog;

namespace IFConvertable.SP3DFileReader {
    public sealed class VueHierarchy
    {
        public VueHierarchyItem Root { get; } = new() { Ident = new SmartId(), Name = "ROOT", Type = HierarchyItemType.SYSTEM };

        public VueHierarchyItem Undefinded { get; } = new() { Ident = new SmartId(), Name = "UNDEFINDED", Type = HierarchyItemType.SYSTEM };

        /// <summary>
        /// Create new hierarchy item and insert it to general hierarchy tree by using Children property.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="hierarchyNodeName"></param>
        /// <param name="sysPathNodes"></param>
        /// <exception cref="Exception"></exception>
        public void CreateHierarchicalNode(SmartId id, string hierarchyNodeName, string[] sysPathNodes)
        {
            VueHierarchyItem suggestParentNode = Root;
            var item = new VueHierarchyItem
            {
                Ident = id,
                Name = hierarchyNodeName,
                Type = HierarchyItemType.ELEMENT
            };

            if (sysPathNodes.Length != 0)
            {
                foreach (string sysPathNode in sysPathNodes)
                {
                    IEnumerable<VueHierarchyItem> existsParents = suggestParentNode.Items.Where(child => child.Name == sysPathNode);
                    
                    if (!existsParents.Any())
                    {
                        var newFolderNode = new VueHierarchyItem
                        {
                            Ident = new SmartId(),
                            Name = sysPathNode,
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
                            Log.Warning($"Found more than one elements by name '{sysPathNode}' to suggest it as parent. Observable node: '{suggestParentNode.Name}'.");
                        }
                    }
                }

                suggestParentNode.Items.Add(item);
            }
            else
            {
                Undefinded.Items.Add(item);
            }
        }

        public string SerializeToJson()
        {
            return JsonSerializer.Serialize(Root.Items, new JsonSerializerOptions { WriteIndented = true });
        }
    }
}
