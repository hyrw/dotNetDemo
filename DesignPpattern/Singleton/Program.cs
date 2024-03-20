Console.WriteLine();

public class Singleton_0
{
    private static readonly Singleton_0 instance = new();
    public static Singleton_0 Instance => instance;

    private Singleton_0() { }
}

public class Singleton_1
{
    private readonly static object lockObj = new();
    private static Singleton_1 instance;
    public static Singleton_1 Instance
    {
        get
        { 
            if (instance == null)
            {
                lock (lockObj)
                {
                    instance ??= new Singleton_1();
                }
            }
            return instance;
        }
    }

    private Singleton_1() { }
}

public class Singleton_2
{
    private readonly static Lazy<Singleton_2> instance = new();
    public static Singleton_2 Instance
    {
        get => instance.Value;
    }

    public Singleton_2() { }
}