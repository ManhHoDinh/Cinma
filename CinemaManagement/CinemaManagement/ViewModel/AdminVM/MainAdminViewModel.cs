﻿using CinemaManagement.Views.Admin.FoodManagementPage;
using CinemaManagement.Views.Admin.Import_ExportManagement;
using CinemaManagement.Views.Admin.MovieManagement;
using CinemaManagement.Views.Admin.QuanLyNhanVienPage;
using CinemaManagement.Views.Admin.ShowtimeManagementVM;
using CinemaManagement.Views.Admin.StatisticalManagement;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace CinemaManagement.ViewModel
{

    public class MainAdminViewModel : BaseViewModel
    {
        public ICommand SignoutCM { get; set; }
        public ICommand LoadQLPPageCM { get; set; }
        public ICommand LoadQLNVPageCM { get; set; }
        public ICommand LoadSuatChieuPageCM { get; set; }
        public ICommand LoadLSPage { get; set; }
        public ICommand LoadTKPageCM { get; set; }
        public ICommand LoadFoodPageCM { get; set; }

        private string _SelectedFuncName;
        public string SelectedFuncName
        {
            get { return _SelectedFuncName; }
            set { _SelectedFuncName = value; OnPropertyChanged(); }
        }


        public MainAdminViewModel()
        {
            SelectedFuncName = "Quản lý suất chiếu";

            SignoutCM = new RelayCommand<FrameworkElement>((p) => { return p == null ? false : true; }, (p) =>
               {
                   FrameworkElement window = GetParentWindow(p);
                   var w = window as Window;
                   if (w != null)
                   {
                       w.Hide();
                       LoginWindow w1 = new LoginWindow();
                       w1.ShowDialog();
                       w.Close();
                   }
               });
            LoadQLPPageCM = new RelayCommand<Frame>((p) => { return p != null; }, (p) =>
            {
                SelectedFuncName = "Quản lý phim";
                if (p != null)
                    p.Content = new MovieManagementWindow();
            });
            LoadSuatChieuPageCM = new RelayCommand<Frame>((p) => { return p != null; }, (p) =>
            {
                SelectedFuncName = "Quản lý suất chiếu";
                if (p != null)
                    p.Content = new ShowtimeManagement();
            });
            LoadQLNVPageCM = new RelayCommand<Frame>((p) => { return p != null; }, (p) =>
            {
                SelectedFuncName = "Quản lý nhân sự";
                if (p != null)
                    p.Content = new NhanVienPage();
            });
            LoadLSPage = new RelayCommand<Frame>((p) => { return p != null; }, (p) =>
            {
                SelectedFuncName = "Lịch sử";
                if (p != null)
                    p.Content = new Import_Export();
            });
            LoadTKPageCM = new RelayCommand<Frame>((p) => { return p != null; }, (p) =>
            {
                SelectedFuncName = "Thống kê";
                if (p != null)
                    p.Content = new StatisticalManagement();
            });
            LoadFoodPageCM = new RelayCommand<Frame>((p) => { return p != null; }, (p) =>
              {
                  SelectedFuncName = "Quản lý sản phẩm";
                  if (p != null)
                      p.Content = new FoodPage();

              });


            FrameworkElement GetParentWindow(FrameworkElement p)
            {
                FrameworkElement parent = p;

                while (parent.Parent != null)
                {
                    parent = parent.Parent as FrameworkElement;
                }
                return parent;
            }
        }
    }
}
