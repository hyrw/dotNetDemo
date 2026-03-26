using Avalonia.Controls;
using CommunityToolkit.Mvvm.Messaging;
using OpenCvToolkit.Messages;

namespace OpenCvToolkit.Views;

public partial class MainView : UserControl, IRecipient<UpdateImageMessage>
{
    public MainView()
    {
        InitializeComponent();
        this.Loaded += MainView_Loaded;
        this.Unloaded += MainView_Unloaded;
    }

    private void MainView_Unloaded(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        WeakReferenceMessenger.Default.Unregister<UpdateImageMessage>(this);
    }

    private void MainView_Loaded(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        WeakReferenceMessenger.Default.Register(this);
    }

    void IRecipient<UpdateImageMessage>.Receive(UpdateImageMessage message)
    {
        beforeViewer?.InvalidateVisual();
        afterViewer?.InvalidateVisual();
    }
}
