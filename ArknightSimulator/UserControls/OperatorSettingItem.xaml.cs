using System;
using System.Collections.Generic;
using System.ComponentModel;
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
using ArknightSimulator.Operators;

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
        }

        public string Img
        {
            get => (string)GetValue(ImgProperty);
            set => SetValue(ImgProperty, value);
        }
        public BitmapImage BitmapImg { get; set; }
        public BitmapImage PositionImg { get; set; }
        public string OpName
        {
            get => (string)GetValue(OpNameProperty);
            set => SetValue(OpNameProperty, value);
        }

        public int OpLevel
        {
            get => (int)GetValue(OpLevelProperty);
            set => SetValue(OpLevelProperty, value);
        }

        public static readonly DependencyProperty OpNameProperty =
        DependencyProperty.Register(
       "OpName",
        typeof(string),
        typeof(OpSettingItem),
        new PropertyMetadata(default(string), OnNamePropertyChanged));

        public static readonly DependencyProperty ImgProperty =
        DependencyProperty.Register(
       "Img",
        typeof(string),
        typeof(OpSettingItem),
        new PropertyMetadata(default(string), OnImgPropertyChanged));

        public static readonly DependencyProperty OpLevelProperty =
        DependencyProperty.Register(
        "OpLevel",
        typeof(int),
        typeof(OpSettingItem),
        new PropertyMetadata(default(int), OnLevelPropertyChanged));

        private static void OnNamePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            OpSettingItem item = d as OpSettingItem;
            // 职位
            switch (((OperatorTemplate)item.DataContext).Position)
            {
                case PositionType.Vanguard: item.PositionImg = new BitmapImage(new Uri(System.IO.Path.GetFullPath("./Image/Vanguard.png"))); break;
                case PositionType.Guard: item.PositionImg = new BitmapImage(new Uri(System.IO.Path.GetFullPath("./Image/Guard.png"))); break;
                case PositionType.Sniper: item.PositionImg = new BitmapImage(new Uri(System.IO.Path.GetFullPath("./Image/Sniper.png"))); break;
                case PositionType.Defender: item.PositionImg = new BitmapImage(new Uri(System.IO.Path.GetFullPath("./Image/Defender.png"))); break;
                case PositionType.Medic: item.PositionImg = new BitmapImage(new Uri(System.IO.Path.GetFullPath("./Image/Medic.png"))); break;
                case PositionType.Supporter: item.PositionImg = new BitmapImage(new Uri(System.IO.Path.GetFullPath("./Image/Supporter.png"))); break;
                case PositionType.Caster: item.PositionImg = new BitmapImage(new Uri(System.IO.Path.GetFullPath("./Image/Caster.png"))); break;
                case PositionType.Specialist: item.PositionImg = new BitmapImage(new Uri(System.IO.Path.GetFullPath("./Image/Specialist.png"))); break;
                default: break;
            }
        }

        private static void OnImgPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            OpSettingItem item = d as OpSettingItem;
            item.BitmapImg = new BitmapImage(new Uri(System.IO.Path.GetFullPath(item.Img)));
            //item.opImg.Source = null;
            //item.opImg.Source = item.BitmapImg;

        }

        private static void OnLevelPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {

        }



    }
}
