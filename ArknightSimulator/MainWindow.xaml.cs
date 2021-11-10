﻿using System;
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
using System.Windows.Threading;
using ArknightSimulator.Manager;
using ArknightSimulator.Pages;

namespace ArknightSimulator
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private Page homePage;
        private Page editPage;
        private Page operationPage;
        private DispatcherTimer timer;

        public EventHandler OnChangeToHomePage;
        public EventHandler OnChangeToEditPage;
        public EventHandler OnChangeToOperationPage;
        public GameManager GameManager { get; private set; }
        public MainWindow()
        {
            InitializeComponent();

            Init();

            OnChangeToHomePage(this, null);
        }

        public void Init()
        {
            this.timer = new DispatcherTimer(DispatcherPriority.Normal, this.Dispatcher);
            GameManager = new GameManager(this.timer);

            MaxHeight = rootGrid.Height + 56;
            MaxWidth = rootGrid.Width + 22;
            MinHeight = rootGrid.Height + 56;
            MinWidth = rootGrid.Width + 22;

            OnChangeToHomePage += ChangeToHomePage;
            OnChangeToEditPage += ChangeToEditPage;
            OnChangeToOperationPage += ChangeToOperationPage;

           
        }

        private void ChangeToHomePage(object sender, EventArgs e)
        {
            if (homePage == null)
            {
                homePage = new HomePage(this);
            }

            contentControl.Content = new Frame() { Content = homePage };
        }

        private void ChangeToEditPage(object sender, EventArgs e)
        {
            if (editPage == null)
            {
                editPage = new EditPage(this);
            }

            contentControl.Content = new Frame() { Content = editPage };
        }

        private void ChangeToOperationPage(object sender, EventArgs e)
        {
            if (operationPage == null)
            {
                operationPage = new OperationPage(this);
            }

            contentControl.Content = new Frame() { Content = operationPage };
        }
    }
}
