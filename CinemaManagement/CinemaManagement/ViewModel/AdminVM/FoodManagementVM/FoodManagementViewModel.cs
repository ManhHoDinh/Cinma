﻿using CinemaManagement.DTOs;
using CinemaManagement.Models.Services;
using CinemaManagement.Utils;
using CinemaManagement.Views.Admin.FoodManagementPage;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Net.Cache;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace CinemaManagement.ViewModel.AdminVM.FoodManagementVM
{
    public partial class FoodManagementViewModel : BaseViewModel
    {
        #region Biến lưu dữ liệu thêm

        private string _DisplayName;
        public string DisplayName
        {
            get { return _DisplayName; }
            set { _DisplayName = value; OnPropertyChanged(); }
        }

        private ComboBoxItem _Category;
        public ComboBoxItem Category
        {
            get { return _Category; }
            set { _Category = value; OnPropertyChanged(); }
        }

        private decimal _Price;
        public decimal Price
        {
            get { return _Price; }
            set { _Price = value; OnPropertyChanged(); }
        }

        private ImageSource _ImageSource;
        public ImageSource ImageSource
        {
            get { return _ImageSource; }
            set { _ImageSource = value; OnPropertyChanged(); }
        }

        #endregion

        string filepath;
        string appPath;
        string imgName;
        string imgfullname;
        string extension;
        string oldMovieName;
        bool IsImageChanged = false;
        bool IsAddingMovie = false;

        private ObservableCollection<ProductDTO> _foodList;
        public ObservableCollection<ProductDTO> FoodList
        {
            get => _foodList;
            set
            {
                _foodList = value;
                OnPropertyChanged();
            }
        }

        public ICommand OpenAddFoodCommand { get; set; }
        public ICommand OpenEditFoodCommand { get; set; }
        public ICommand OpenDeleteFoodCommand { get; set; }

        public ICommand AddFoodCommand { get; set; }
        public ICommand EditFoodCommand { get; set; }
        public ICommand DeleteFoodCommand { get; set; }

        public ICommand UpLoadImageCommand { get; set; }

        public ICommand MouseMoveCommand { get; set; }
        public ICommand CloseCommand { get; set; }

        private ProductDTO _SelectedItem;
        public ProductDTO SelectedItem
        {
            get { return _SelectedItem; }
            set { _SelectedItem = value; OnPropertyChanged(); }
        }



        public FoodManagementViewModel()
        {
            
            LoadProductListView(Operation.READ);
            //IsImageChanged = false;
            OpenAddFoodCommand = new RelayCommand<object>((p) => { return true; },
                (p) => {
                    AddFoodWindow wd = new AddFoodWindow();
                    DisplayName = null;
                    Category = null;
                    ImageSource = null;
                    IsAddingMovie = true;
                    wd.ShowDialog();

                });
            AddFoodCommand = new RelayCommand<Window>((p) => { return true; },
                (p) => {
                    AddFood(p);
                    p.Close();
                });


            OpenEditFoodCommand = new RelayCommand<object>((p) => { return true; },
                (p) => {
                    EditFoodWindow wd = new EditFoodWindow();
                    ProductDTO a = new ProductDTO();
                    wd._displayName.Text = SelectedItem.DisplayName;
                    wd._category.Text = SelectedItem.Category;
                    wd._price.Text = SelectedItem.Price.ToString();
                    wd.ShowDialog();

                });

            EditFoodCommand = new RelayCommand<Window>((p) => { return true; },
                (p) => {
                    EditFood(p);
                });

            OpenDeleteFoodCommand = new RelayCommand<object>((p) => { return true; },
                (p) => {
                    DeleteFoodWindow wd = new DeleteFoodWindow();
                    wd.ShowDialog();

                });

            DeleteFoodCommand = new RelayCommand<Window>((p) => { return true; },
                (p) => {
                    DeleteFood(p);
                });

            CloseCommand = new RelayCommand<Window>((p) => { return p == null ? false : true; }, (p) => {
                Window window = GetWindowParent(p);
                var w = window as Window;
                if (w != null)
                {
                    w.Close();
                }
            }
            );

            UpLoadImageCommand = new RelayCommand<Window>((p) => { return true; },
                (p) => {
                    OpenFileDialog openfile = new OpenFileDialog();
                    openfile.Title = "Select an image";
                    openfile.Filter = "Image File (*.jpg;*.jpeg;*.png)|*.jpg;*.jpeg; *.png";
                    if (openfile.ShowDialog() == DialogResult.OK)
                    {
                        IsImageChanged = true;
                        filepath = openfile.FileName;
                        extension = openfile.SafeFileName.Split('.')[1];
                        LoadImage();
                        return;
                    }
                    IsImageChanged = false;
                });

            MouseMoveCommand = new RelayCommand<Window>((p) => { return p == null ? false : true; }, (p) =>
            {
                Window window = GetWindowParent(p);
                var w = window as Window;
                if (w != null)
                {
                    
                    w.DragMove();
                }
            }
           );

        }

        public void LoadImage()
        {
            BitmapImage _image = new BitmapImage();
            _image.BeginInit();
            _image.CacheOption = BitmapCacheOption.None;
            _image.UriCachePolicy = new RequestCachePolicy(RequestCacheLevel.BypassCache);
            _image.CacheOption = BitmapCacheOption.OnLoad;
            _image.CreateOptions = BitmapCreateOptions.IgnoreImageCache;
            _image.UriSource = new Uri(filepath, UriKind.RelativeOrAbsolute);
            _image.EndInit();
            ImageSource = _image;
        }

        public void SaveImgToApp()
        {
            try
            {
                appPath = Helper.GetMovieImgPath(imgfullname);
                if (File.Exists(Helper.GetMovieImgPath(imgfullname)))
                    File.Delete(Helper.GetMovieImgPath(imgfullname));
                File.Copy(filepath, appPath, true);
            }
            catch (Exception exp)
            {
                System.Windows.MessageBox.Show("Unable to open file " + exp.Message);
            }
        }

        public void LoadProductListView(Operation oper, ProductDTO product = null)
        {

            switch (oper)
            {
                case Operation.READ:
                    try
                    {
                        FoodList = new ObservableCollection<ProductDTO>(ProductService.Ins.GetAllProduct());
                    }
                    catch (InvalidOperationException e)
                    {
                        Console.WriteLine(e.Message);
                    }
                    catch (Exception e)
                    {
                        System.Windows.MessageBox.Show("Lỗi hệ thống " + e.Message);
                    }
                    break;
                case Operation.CREATE:
                    FoodList.Add(product);
                    break;
                case Operation.UPDATE:
                    var productFound = FoodList.FirstOrDefault(s => s.Id == product.Id);
                    FoodList[FoodList.IndexOf(productFound)] = product;
                    break;
                case Operation.DELETE:
                    for (int i = 0; i < FoodList.Count; i++)
                    {
                        if (FoodList[i].Id == SelectedItem?.Id)
                        {
                            FoodList.Remove(FoodList[i]);
                            break;
                        }
                    }
                    break;
                default:
                    break;
            }
        }

        private (bool valid, string error) IsValidData(Operation oper)
        {

            if (string.IsNullOrEmpty(DisplayName) || Category is null || ImageSource is null)
            {
                return (false, "Thông tin đồ ăn thiếu! Vui lòng bổ sung");
            }
            return (true, null);
        }

        Window GetWindowParent(Window p)
        {
            Window parent = p;

            while (parent.Parent != null)
            {
                parent = parent.Parent as Window;
            }

            return parent;
        }

    }
    
}