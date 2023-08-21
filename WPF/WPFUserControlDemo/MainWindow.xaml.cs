using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices.JavaScript;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace WPFUserControlDemo
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }
    }

    public class MainWindowViewModel : INotifyPropertyChanged
    {

        public ObservableCollection<string> Items { get; private set; }
        public ICommand AddItem { get; private set; }
        public ICommand RemoveItem { get; private set; }

        public string Input
        {
            get { return _input; }
            set { SetField(ref _input, value); }
        }
        private string _input = string.Empty;

        private DateTime? _beginTime;
        public DateTime? BeginTime
        {
            get => _beginTime;
            set => SetField(ref _beginTime, value);
        }


        public MainWindowViewModel()
        {
            Items = new ObservableCollection<string>();
            AddItem = new BaseCommand(ExecAddItem);
            RemoveItem = new BaseCommand(ExecRemoveItem);
        }

        private void ExecAddItem(object? obj)
        {
            var random = new Random();
            Items.Add(random.NextInt64().ToString()); ;
        }

        private void ExecRemoveItem(object? obj)
        {
            Items.Remove(obj as string);
        }

        #region  INotifyPropertyChanged

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected bool SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, value)) return false;
            field = value;
            OnPropertyChanged(propertyName);
            return true;
        }
        
        #endregion
    }

    public class BaseCommand : ICommand
    {
        private Predicate<object> _canExecute;
        private Action<object> _method;
        public event EventHandler? CanExecuteChanged;

        public BaseCommand(Action<object> method)
        {
            _method = method;
        }

        public bool CanExecute(object? parameter)
        {
            return true;
        }

        public void Execute(object? parameter)
        {
            _method.Invoke(parameter);
        }
    }

}