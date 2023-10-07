using CommunityToolkit.Mvvm.Input;
using System.Windows;
using System.Windows.Input;
using WK.Libraries.SharpClipboardNS;

namespace NotifyIconAndHotKeyDemo
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        private SharpClipboard _sharpClipboard;

        public MainWindow()
        {
            InitializeComponent();
            this.DataContext = new MainWindowViewModel();
            _sharpClipboard = new SharpClipboard();
            _sharpClipboard.ClipboardChanged += _sharpClipboard_ClipboardChanged;
        }
        ~MainWindow()
        {
            _sharpClipboard.ClipboardChanged -= _sharpClipboard_ClipboardChanged;
        }

        private void _sharpClipboard_ClipboardChanged(object? sender, SharpClipboard.ClipboardChangedEventArgs e)
        {
            Console.WriteLine($"{e.ContentType}\t {e.SourceApplication}\t {e.Content}");
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            this.Visibility = Visibility.Collapsed;
        }

        private void myNotifyIcon_TrayMouseDoubleClick(object sender, RoutedEventArgs e)
        {
            this.Visibility = Visibility.Visible;
        }
    }

    public class MainWindowViewModel()
    {
        public ICommand TestCommand { get; } = new RelayCommand(ExecTest);

        private static void ExecTest()
        {
            MessageBox.Show("hot key");
        }
    }
}