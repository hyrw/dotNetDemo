using Avalonia.Input;
using AvaloniaWithOpenCV.ViewModels;
using OpenCvSharp;
using System.IO;

namespace AvaloniaWithOpenCV.Views;

public partial class MainWindow : Avalonia.Controls.Window
{
    public MainWindow()
    {
        InitializeComponent();
    }

    public Mat? Mat { get; set; }

    async void Drop(object? sender, DragEventArgs e)
    {
        var storeItem = e.DataTransfer.TryGetFile();
        string localPath = storeItem?.Path.LocalPath ?? string.Empty;
        if (!File.Exists(localPath)) return;

        if (this.DataContext is not MainWindowViewModel vm) return;

        vm.FilePath = localPath;
        Mat?.Dispose();
        Mat = Cv2.ImRead(localPath);
    }
}
