using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ArknightSimulator.Pages
{
    /// <summary>
    /// HomePage.xaml 的交互逻辑
    /// </summary>
    public partial class HomePage : Page
    {
        private MainWindow mainWindow;
        public EventHandler OnChangeToEditPage;   // 跳转到编辑页事件
        public HomePage(MainWindow mainWindow)
        {
            InitializeComponent();
            this.mainWindow = mainWindow;
        }
        
        private void Grid_MouseDown(object sender, MouseEventArgs e)
        {
            OnChangeToEditPage(this, null);
        }
    }
}
