﻿using CinemaManagement.DTOs;
using System;
using System.Windows.Controls;
using System.Windows.Data;

namespace CinemaManagement.Views.Admin.MovieManagement
{

    public partial class MovieManagementWindow : Page
    {
        public MovieManagementWindow()
        {
            InitializeComponent();
            CollectionView view = (CollectionView)CollectionViewSource.GetDefaultView(MovieListView.ItemsSource);
            view.Filter = Filter;
        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            CollectionViewSource.GetDefaultView(MovieListView.ItemsSource).Refresh();
        }

        private bool Filter(object item)
        {
            if (String.IsNullOrEmpty(FilterBox.Text))
                return true;
            else
                return ((item as MovieDTO).DisplayName.IndexOf(FilterBox.Text, StringComparison.OrdinalIgnoreCase) >= 0);
        }
    }
}
