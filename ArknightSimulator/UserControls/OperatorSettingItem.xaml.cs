using System;
using System.Collections.Generic;
using System.Globalization;
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
using ArknightSimulator.Operator;

namespace ArknightSimulator.UserControls
{
    /// <summary>
    /// OperatorSettingItem.xaml 的交互逻辑
    /// </summary>
    public partial class OpSettingItem : UserControl
    {
        public OpSettingItem()
        {
            InitializeComponent();
            //DataContext = this;
            //DataContext = new OperatorTemplate();
            //BitmapImage bmp = new BitmapImage(new Uri(Img));
            //OpName = "op";
            //opImg.Source = bmp;
            //opImg.DataContext = this;
        }

        public string Img
        {
            get
            {
                return (string)GetValue(ImgProperty);
            }
            set
            {
                SetValue(ImgProperty, value);
            }
        }


        public string OpName
        {
            get
            {
                return (string)GetValue(OpNameProperty);
            }
            set
            {
                SetValue(OpNameProperty, value);
            }
        }

        public static readonly DependencyProperty OpNameProperty =
        DependencyProperty.Register(
       "OpName",
        typeof(string),
        typeof(OpSettingItem),
        new PropertyMetadata(default(string), OnItemsPropertyChanged));

        public static readonly DependencyProperty ImgProperty =
        DependencyProperty.Register(
       "Img",
        typeof(string),
        typeof(OpSettingItem),
        new PropertyMetadata(default(string), OnImgPropertyChanged));

        private static void OnItemsPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            //AutocompleteTextBox source = d as AutocompleteTextBox;
            //Do something...
            //MessageBox.Show((string)e.NewValue);
        }

        private static void OnImgPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            //AutocompleteTextBox source = d as AutocompleteTextBox;
            //Do something...
            //MessageBox.Show((string)e.NewValue);

        }

    }
}
