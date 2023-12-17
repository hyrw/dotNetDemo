
var menuList = new List<ITree<int>>
{
    new Menu{ID = 1, ParentID = Menu.INVALID_ID, Name = "视图管理"},
    new Menu{ID = 11, ParentID = 1, Name = "库位视图"},
    new Menu{ID = 12, ParentID = 1, Name = "通道视图"},
    new Menu{ID = 2, ParentID = Menu.INVALID_ID, Name = "流程管理"},
    new Menu{ID = 21, ParentID = 2, Name = "流程编辑"},
    new Menu{ID = 3, ParentID = Menu.INVALID_ID, Name = "库位管理"},
};

//var menu = new Menu { ID = 3, ParentID = Menu.INVALID_ID, Name = "库位管理" };
var menuTree = TreeUtils.GenerateTree(menuList);
var menuTree2 = TreeUtils.GenerateTreeLambda(menuTree,
                                             menu => menu.Id,
                                             menu => menu.ParentId,
                                             menu => menu.IsRoot,
                                             menu => menu.Children,
                                             (menu, list) => menu.Children = list);
public class TreeUtils
{
    public static List<ITree<T>> GenerateTree<T>(List<ITree<T>> trees) where T : notnull, new()
    {
        if (trees is null)
        {
            throw new ArgumentNullException();
        }

        List<ITree<T>> list = new();
        Dictionary<T, ITree<T>> dict = trees.ToDictionary(item => item.Id);

        foreach (var item in dict)
        {
            if (item.Value.IsRoot)
            {
                list.Add(item.Value);
            }
            else
            {
                if (dict.TryGetValue(item.Value.ParentId, out ITree<T>? parentTree))
                {
                    parentTree.Children ??= new List<ITree<T>>();
                    parentTree.Children.Add(item.Value);
                }
            }
        }
        return list;
    }

    public static List<T> GenerateTreeLambda<TKey, T>(List<T> trees,
                                                      Func<T, TKey> getId,
                                                      Func<T, TKey> getParentId,
                                                      Func<T, bool> isRoot,
                                                      Func<T, List<T>> getChildren,
                                                      Action<T, List<T>> setChildren)
        where TKey : notnull
        where T : notnull
    {
        if (trees is null)
        {
            throw new ArgumentNullException();
        }

        List<T> rootTree = new();
        Dictionary<TKey, T> dict = trees.ToDictionary(getId);

        foreach (var item in dict)
        {
            if (isRoot(item.Value))
            {
                rootTree.Add(item.Value);
            }
            else
            {
                if (dict.TryGetValue(getParentId(item.Value), out T? parentTree))
                {
                    List<T>? children = getChildren(parentTree);
                    if (children == null)
                    {
                        children = new List<T>();
                        setChildren(item.Value, children);
                    }
                    children.Add(item.Value);
                }
            }
        }
        return rootTree;
    }
}


public interface ITree<T> where T : notnull
{
    T Id { get; }
    T ParentId { get; }
    bool IsRoot { get; }

    List<ITree<T>> Children { get; set; }

}

public class Menu : ITree<int>
{
    public void TestFunc()
    {
        
    }

    public static int INVALID_ID = int.MaxValue;

    int ITree<int>.Id => this.ID;

    int ITree<int>.ParentId => this.ParentID;

    bool ITree<int>.IsRoot => this.ParentID == INVALID_ID;

    List<ITree<int>> ITree<int>.Children 
    { 
        get => children;
        set => children = value;
    }

    public int ID { get; set; } 
    public int ParentID { get; set; }
    public string Name { get; set; }
    private List<ITree<int>> children;

    public Menu()
    {
        this.Name = string.Empty;
        this.children = new List<ITree<int>>();
    }
}
