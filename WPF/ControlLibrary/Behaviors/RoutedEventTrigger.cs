using Microsoft.Xaml.Behaviors;
using System.Windows;

namespace ControlLibrary.Behaviors;

internal class RoutedEventTrigger : EventTriggerBase<object>
{
    public required RoutedEvent RoutedEvent { get; set; }

    protected override void OnAttached()
    {
        base.OnAttached();
        var element = AssociatedObject as UIElement;
        element!.AddHandler(RoutedEvent, (RoutedEventHandler)OnRoutedEvent);
    }

    protected override void OnDetaching()
    {
        base.OnDetaching();
        var element = AssociatedObject as UIElement;
        element!.RemoveHandler(RoutedEvent, (RoutedEventHandler)OnRoutedEvent);
    }

    private void OnRoutedEvent(object sender, RoutedEventArgs args) => OnEvent(args);

    protected override string GetEventName()
    {
        return RoutedEvent.Name;
    }
}
