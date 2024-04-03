using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;

namespace InteractionDemo.Adorners;

internal class PlaceholderAdorner : Adorner
{
    private readonly string _placeholder;

    public PlaceholderAdorner(UIElement adornedElement, string placeholder) : base(adornedElement)
    {
        this._placeholder = placeholder;
    }

    protected override void OnRender(DrawingContext drawingContext)
    {
        if (base.AdornedElement is not TextBox textBox) return;

        drawingContext.DrawText(new FormattedText(
            _placeholder,
            CultureInfo.CurrentCulture,
            textBox.FlowDirection,
            new Typeface(textBox.FontFamily.Source),
            textBox.FontSize,
            Brushes.Gray,
            VisualTreeHelper.GetDpi(this).PixelsPerDip
            ), new Point(4, 4));
    }
}
