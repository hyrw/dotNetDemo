using System.Windows;
using System.Windows.Controls;

namespace DataTemplateSelectorDemo;

public class PersonDataTempelateSelector : DataTemplateSelector
{
    public override DataTemplate SelectTemplate(object item, DependencyObject container)
    {
        var fe = container as FrameworkElement;
        if (item == null || fe == null) return base.SelectTemplate(item, container);

        return item switch
        {
            Employee => (DataTemplate)fe.TryFindResource("EmployeeTemplate"),
            Person => (DataTemplate)fe.TryFindResource("PersonTemplate"),
            _ => base.SelectTemplate(item, container)
        };
    }
}
