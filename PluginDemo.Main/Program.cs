// See https://aka.ms/new-console-template for more information
using PluginDemo.Core;
using System.Reflection;

var dllFiles = Directory.EnumerateFiles(AppContext.BaseDirectory, "*.dll");

var pluginTypes = dllFiles
    .Select(file => Assembly.LoadFile(file))
    .SelectMany(i => i.GetExportedTypes())
    .Where(i => i.IsAssignableTo(typeof(IPlugin)));

foreach (var pluginType in pluginTypes)
{
    IPlugin? plugin = Activator.CreateInstance(pluginType) as IPlugin;
    plugin?.Execute();
}
