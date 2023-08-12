using System;
using System.Collections.Generic;
using System.Linq;
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

namespace EnumBinding;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        this.DataContext = new VM();
    }
}

public class VM
{
    public IEnumerable<ItemsSourceModel<FruitType>> Fruits { get; set; }

    public VM()
    {
        Fruits = new List<ItemsSourceModel<FruitType>>()
        {
            new ItemsSourceModel<FruitType> {Value = FruitType.Apple, Text ="苹果"},
            new ItemsSourceModel<FruitType> {Value = FruitType.Orange, Text ="橘子"},
        };
    }
}


