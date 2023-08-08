using PluginDemo.Core;

namespace PluginDemo.PluginA;

public class PluginA : IPlugin
{
    void IPlugin.Execute()
    {
        Console.WriteLine("Plugin A Execute...");
    }
}