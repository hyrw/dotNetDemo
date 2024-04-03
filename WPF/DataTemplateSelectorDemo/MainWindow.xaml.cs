using System.Windows;

namespace DataTemplateSelectorDemo;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{

    public List<Person> Persons { get; set; }

    public MainWindow()
    {
        InitializeComponent();
        this.DataContext = this;
        LoadPersons();
    }

    private void LoadPersons()
    {
        Persons =
        [
            new Person("zhangsan", 18),
            new Person("lisi", 20),
            new Employee("研发", "wangwu", 20),
        ];
    }
}

