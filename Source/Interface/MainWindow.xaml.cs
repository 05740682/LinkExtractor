using System;
using System.Windows;

namespace 云盘解析
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                textbox1.Text = await ALLClass.云盘解析.关键字解析(textbox2.Text);

                textbox1.ScrollToHome();
            }
            catch (Exception ex)
            {
                textbox1.Text = ex.Message;
            }
        }
    }
}
