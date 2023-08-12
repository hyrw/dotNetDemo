using Microsoft.Xaml.Behaviors;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace InteractionDemo.Behaviors
{
    /// <summary>
    /// 文本框水印 行为Demo
    /// </summary>
    public class TextBoxWaterMarkBehavior : Behavior<UIElement>
    {

        private TextBox? textBox;  //文本框
        private string isNotNullForeground; //不为空的文本颜色
        private string waterMarkText = ""; //水印文本
        private Brush waterMarkForeground;//水印颜色

        public string IsNotNullForeground
        {
            set => isNotNullForeground = value;
        }

        public string WaterMarkText
        {
            set => waterMarkText = value;
        }

        /// <summary>
        /// 最重要重新方法之一必须  +=后双击tab 会补全代码
        /// </summary>
        protected override void OnAttached()
        {
            base.OnAttached();
            // AssociatedObject 是行为的关联对象，类型为我们指定的FrameworkElement
            AssociatedObject.LostFocus += AssociatedObject_LostFocus;
            AssociatedObject.GotFocus += AssociatedObject_GotFocus;
        }
        /// <summary>
        /// 最重要重新方法之一必须  +=后双击tab 会不全代码
        /// </summary>
        protected override void OnDetaching()
        {
            base.OnDetaching();
            // 移除
            AssociatedObject.LostFocus -= AssociatedObject_LostFocus;
            AssociatedObject.GotFocus -= AssociatedObject_GotFocus;
        }
        private void AssociatedObject_GotFocus(object sender, RoutedEventArgs e)
        {

            if (AssociatedObject is not null)
            {

                textBox = this.AssociatedObject as TextBox;
                if (textBox != null && textBox.Text == waterMarkText)
                {
                    textBox.Text = "";
                    waterMarkForeground = textBox.Foreground;
                    textBox.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString(isNotNullForeground));
                }


            }
        }

        private void AssociatedObject_LostFocus(object sender, RoutedEventArgs e)
        {
            if (textBox is not null && textBox.Text == "")
            {

                textBox.Foreground = waterMarkForeground;
                textBox.Text = waterMarkText;
            }
            else
            {
                //    textBox.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString(textBox.Foreground));
            }

        }



    }
}
