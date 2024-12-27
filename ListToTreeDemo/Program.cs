using System.Data;

var menuList = new List<Menu>
{
    new() {ID = 1,  Name = "视图管理"},
    new() {ID = 11, ParentID = 1, Name = "库位视图"},
    new() {ID = 12, ParentID = 1, Name = "通道视图"},
    new() {ID = 2, Name = "流程管理"},
    new() {ID = 21, ParentID = 2, Name = "流程编辑"},
    new() {ID = 3, Name = "库位管理"},
};

var result = TreeUtils.GenerateTreeLambdaV2(menuList, 
    e => e.ID, 
    e => e.ParentID, 
    e => e.ParentID == default, 
    treeItem => new Menu() 
    { 
        ID = treeItem.Value.ID, 
        ParentID = treeItem.Value.ParentID, 
        Children = treeItem.Children, 
        Name = treeItem.Value.Name 
    });
Console.ReadKey();

public class TreeUtils
{
    public static IEnumerable<T> GenerateTreeLambdaV2<TKey, T>(List<T> source,
                                                      Func<T, TKey> key,
                                                      Func<T, TKey> parentKey,
                                                      Func<T, bool> isRoot,
                                                      Func<TreeItem<T>, T> factory)
        where TKey : notnull
    {
        ArgumentNullException.ThrowIfNull(source);

        List<TreeItem<T>> rootTree = [];
        Dictionary<TKey, TreeItem<T>> dict = source.ToDictionary(key, e => new TreeItem<T> { Value = e });

        foreach (var (_, treeItem) in dict)
        {
            if (isRoot(treeItem.Value))
            {
                rootTree.Add(treeItem);
            }
            if (dict.TryGetValue(parentKey(treeItem.Value), out var parent))
            {
                parent.Children.Add(treeItem.Value);
            }
        }

        return rootTree.Select(factory);
    }

    public static IEnumerable<T> GenerateTreeLookup<TKey, T>(List<T> source,
                                                      Func<T, TKey> key,
                                                      Func<T, TKey> parentKey,
                                                      Func<T, bool> isRoot,
                                                      Func<T, IEnumerable<T>, T> factory)
    {
        ArgumentNullException.ThrowIfNull(source);
        var lookup = source.ToLookup(e => parentKey(e));

        var result = source.Select(e =>
        {
            var children = lookup[key(e)].ToArray();
            return factory(e, children);
        }).Where(isRoot);
        return result;
    }
}

public class TreeItem<T>
{
    public required T Value { get; set; }
    public List<T> Children { get; set; } = [];
}


public class Menu
{
    public int ID { get; set; }
    public int ParentID { get; set; }
    public string Name { get; set; } = string.Empty;
    public List<Menu> Children = [];
}
