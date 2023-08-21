using System;
using System.Windows;
using System.Windows.Controls;

namespace WPFUserControlDemo.UserControls
{
    /// <summary>
    /// ActionBar.xaml 的交互逻辑
    /// </summary>
    public partial class ActionBar : UserControl
    {

        #region Text

        public string Text
        {
            get { return (string)GetValue(TextProperty); }
            set { SetValue(TextProperty, value); }
        }
        public static readonly DependencyProperty TextProperty =
            DependencyProperty.Register(nameof(Text), typeof(string), typeof(ActionBar), new PropertyMetadata(string.Empty));

        #endregion

        #region BeginTime

        public DateTime? BeginTime
        {
            get { return (DateTime?)GetValue(BeginTimeProperty); }
            set { SetValue(BeginTimeProperty, value); }
        }

        public static readonly DependencyProperty BeginTimeProperty =
            DependencyProperty.Register(nameof(BeginTime), typeof(DateTime?), typeof(ActionBar), new PropertyMetadata(default(DateTime?)));



        #endregion
        public ActionBar()
        {
            InitializeComponent();
        }
    }
}
