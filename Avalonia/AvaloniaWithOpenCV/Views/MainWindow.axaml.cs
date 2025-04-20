using Avalonia.Controls;
using Avalonia.Interactivity;

namespace AvaloniaWithOpenCV.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
    }

    private async void Button_Click(object? sender, RoutedEventArgs e)
    {
        var thresholdWindow = new ThresholdWindow();
        await thresholdWindow.ShowDialog(this);
    }
}
