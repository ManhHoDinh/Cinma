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
using System.Windows.Shapes;

namespace CinemaManagement.Views.Admin.QuanLyNhanVienPage
{
    /// <summary>
    /// Interaction logic for XoaNhanVienWindow.xaml
    /// </summary>
    public partial class XoaNhanVienWindow : Window
    {
        public XoaNhanVienWindow()
        {
            InitializeComponent();
            this.Owner = App.Current.MainWindow;
        }

    }
}
