using System.Collections.Concurrent;

namespace SingletonTest;

public class SingletonTest
{

    [Fact]
    [Trait("状态模式", "")]
    public async Task TestStateMatchineAsync()
    {

        Equipment equpment = new();
        equpment.Init();
        equpment.Start();
        equpment.Stop();
        await Task.Delay(TimeSpan.FromSeconds(30));
    }

    [Fact]
    [Trait("单例", "静态初始化")]
    public void TestStaticNew()
    {
        ConcurrentBag<Singleton_0> bag = [];
        Parallel.For(0, 10000, _ =>
        {
            var instance = Singleton_0.Instance;
            bag.Add(instance);
        });
        _ = bag.Aggregate((x, y) =>
        {
            Assert.Same(x, y);
            return x;
        });
    }

    [Fact]
    [Trait("单例", "双重检查")]
    public void TestDoubleCheck()
    {
        ConcurrentBag<Singleton_1> bag = [];
        Parallel.For(0, 10000, _ =>
        {
            var instance = Singleton_1.Instance;
            bag.Add(instance);
        });
        _ = bag.Aggregate((x, y) =>
        {
            Assert.Same(x, y);
            return x;
        });
    }

    [Fact]
    [Trait("单例", "Lazy")]
    public void TestLazy()
    {
        ConcurrentBag<Singleton_2> bag = [];
        Parallel.For(0, 10000, _ =>
        {
            var instance = Singleton_2.Instance;
            bag.Add(instance);
        });
        _ = bag.Aggregate(Singleton_2.Instance, (x, y) =>
        {
            Assert.Same(x, y);
            return x;
        });

        var obj = Singleton_3<Object>.Instance;
    }

    [Fact]
    [Trait("单例", "Lazy 范型")]
    public void TestLazyWithT()
    {
        ConcurrentBag<Object> bag = [];
        Parallel.For(0, 10000, _ =>
        {
            var instance = Singleton_3<Object>.Instance;
            bag.Add(instance);
        });
        _ = bag.Aggregate(Singleton_3<Object>.Instance, (x, y) =>
        {
            Assert.Same(x, y);
            return x;
        });
    }
}
