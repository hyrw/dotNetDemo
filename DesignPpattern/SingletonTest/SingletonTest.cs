using System.Collections.Concurrent;

namespace SingletonTest
{
    public class SingletonTest
    {
        [Fact]
        [Trait("单例", "静态初始化")]
        public void TestStaticNew()
        {
            ConcurrentBag<Singleton_0> bag = [];
            Parallel.For(0, 10000, _ =>
            {
                bag.Add(Singleton_0.Instance);
            });
            _ = bag.Aggregate((x, y) =>
            {
                Assert.Equal(x, y);
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
                bag.Add(Singleton_1.Instance);
            });
            _ = bag.Aggregate((x, y) =>
            {
                Assert.Equal(x, y);
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
                bag.Add(Singleton_2.Instance);
            });
            _ = bag.Aggregate(Singleton_2.Instance, (x, y) =>
            {
                Assert.Equal(x, y);
                return x;
            });
        }
    }
}