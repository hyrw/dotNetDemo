// See https://aka.ms/new-console-template for more information
using PluginDemo.Core;
using System.Reflection;

var dllFiles = Directory.EnumerateFiles(AppContext.BaseDirectory, "*.dll");
var assemblys = new List<Assembly>();
foreach (var dllFile in dllFiles)
{
    assemblys.Add(Assembly.LoadFile(dllFile));
}

var pluginTypes = assemblys
    .SelectMany(i => i.GetExportedTypes())
    .Where(i => i.IsAssignableTo(typeof(IPlugin)));

foreach (var pluginType in pluginTypes)
{
    IPlugin? plugin = Activator.CreateInstance(pluginType) as IPlugin;
    plugin?.Execute();
}
