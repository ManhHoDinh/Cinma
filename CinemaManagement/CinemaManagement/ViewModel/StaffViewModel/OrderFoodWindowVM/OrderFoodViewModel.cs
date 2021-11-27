﻿using CinemaManagement.DTOs;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace CinemaManagement.ViewModel.StaffViewModel.OrderFoodWindowVM
{
    public class OrderFoodViewModel : BaseViewModel
    {
        public ICommand CloseWindowCM { get; set; }
        public ICommand MinimizeWindowCM { get; set; }
        public ICommand GoBackCommand { get; set; }
        public ICommand MouseMoveCommand { get; set; }
        public ICommand FilterAllProductsCommand { get; set;}
        public ICommand FilterFoodCommand { get; set; }
        public ICommand FilterDrinkCommand { get; set; }
        public ICommand SelectedProductCommand { get; set; }
        public ICommand MinusCommand { get; set; }
        public ICommand PlusCommand { get; set; }
        public ICommand DeleteProductCommand { get; set; }
        public ICommand DeleteAllOrderCommand { get; set; }
        public ICommand BuyCommand { get; set; }


        private decimal _TotalPrice;
        public decimal TotalPrice
        {
            get => _TotalPrice;
            set
            {
                _TotalPrice = value;
                OnPropertyChanged();
            }
        }

        private TabItem _TabProduct;
        public TabItem TabProduct
        {
            get => _TabProduct;
            set
            {
                _TabProduct = value;
                OnPropertyChanged();
            }
        }

        private ProductDTO _SelectedProductToOrder;
        public ProductDTO SelectedProductToOrder
        {
            get => _SelectedProductToOrder;
            set
            {
                _SelectedProductToOrder = value;
                OnPropertyChanged();
            }
        }

        private ProductDTO _SelectedProductToBill;
        public ProductDTO SelectedProductToBill
        {
            get => _SelectedProductToBill;
            set
            {
                _SelectedProductToBill = value;
                OnPropertyChanged();
            }
        }

        private ObservableCollection<ProductDTO> _AllProduct;
        public ObservableCollection<ProductDTO> AllProduct
        {
            get => _AllProduct;
            set
            {
                _AllProduct = value;
                OnPropertyChanged();
            }
        }

        private ObservableCollection<ProductDTO> _menuList;
        public ObservableCollection<ProductDTO> MenuList
        {
            get => _menuList;
            set
            {
                _menuList = value;
                OnPropertyChanged();
            }
        }

        private ObservableCollection<ProductDTO> _orderList;
        public ObservableCollection<ProductDTO> OrderList
        {
            get => _orderList;
            set
            {
                _orderList = value;
                OnPropertyChanged();
            }
        }

        public OrderFoodViewModel()
        {
            AllProduct = new ObservableCollection<ProductDTO>();
            OrderList = new ObservableCollection<ProductDTO>();
            MenuList = new ObservableCollection<ProductDTO>();

            //Khởi tạo giá trị ban đầu cho tổng giá tiền
            TotalPrice = 0;

            //Gán giá trị demo mẫu đồ ăn và thức uống
            Demo();

            //Khởi tạo giá trị ban đầu các item cho MenuList
            MenuList = AllProduct;

            //Filter All Products
            FilterAllProductsCommand = new RelayCommand<object>((p) => { return true; }, (p) =>
           {
               MenuList = new ObservableCollection<ProductDTO>(AllProduct);
           });

            //Filter đồ ăn
            FilterFoodCommand = new RelayCommand<object>((p) => { return true; }, (p) =>
            {
                FilterFood();
            });

            //Filter thức uống
            FilterDrinkCommand = new RelayCommand<object>((p) => { return true; }, (p) =>
            {
                FilterDrink();
            });

            //Chọn đồ ăn chuyển qua OrderList
            SelectedProductCommand = new RelayCommand<Label>((p) => { return true; }, (p) =>
            {
                if (SelectedProductToOrder != null)
                {
                    if (SelectedProductToOrder.Quantity > 0)
                    {
                        for (int i = 0; i < AllProduct.Count; ++i)
                        {
                            if (AllProduct[i].Id == SelectedProductToOrder.Id)
                            {
                                AllProduct[i].Quantity -= 1;
                                FilterMenuList();
                                for (int j = 0; j < OrderList.Count; ++j)
                                {
                                    if (OrderList[j].Id == AllProduct[i].Id)
                                    {
                                        OrderList[j].Quantity += 1;
                                        OrderList = new ObservableCollection<ProductDTO>(OrderList);
                                        ReCalculate();
                                        return;
                                    }
                                }
                                //Thêm mới sản phẩm Order
                                OrderList.Add(new ProductDTO()
                                {
                                    Id = AllProduct[i].Id,
                                    DisplayName = AllProduct[i].DisplayName,
                                    Category = AllProduct[i].Category,
                                    Price = AllProduct[i].Price,
                                    Image = AllProduct[i].Image,
                                    Quantity = 1
                                });
                                ReCalculate();
                            }
                        }
                    }
                    else
                    {
                        MessageBox.Show("Hết hàng!", "Cảnh báo");
                    }
                }
            });

            //Xóa tất cả sản phẩm order
            DeleteAllOrderCommand = new RelayCommand<object>((p) => { return true; }, (p) =>
            {
                if (OrderList.Count == 0)
                {
                    MessageBox.Show("Danh sách rỗng", "Thông báo");
                }
                else
                {
                    if (MessageBox.Show("Bạn muốn xóa tất cả?", "Xóa", MessageBoxButton.OKCancel) == MessageBoxResult.OK)
                    {
                        for (int i = 0; i < OrderList.Count; ++i)
                        {
                            for (int j = 0; j < AllProduct.Count; ++j)
                            {
                                if (OrderList[i].Id == AllProduct[j].Id)
                                {
                                    AllProduct[j].Quantity += OrderList[i].Quantity;
                                    break;
                                }
                            }
                        }
                        FilterMenuList();
                        OrderList.Clear();
                        TotalPrice = 0;
                    }
                }
            });

            //Xóa sản phẩm order
            DeleteProductCommand = new RelayCommand<Label>((p) => { return true; }, (p) =>
            {
                if (SelectedProductToBill != null)
                {
                    for (int i = 0; i < AllProduct.Count; ++i)
                    {
                        if (SelectedProductToBill.Id == AllProduct[i].Id)
                        {
                            AllProduct[i].Quantity += SelectedProductToBill.Quantity;
                            FilterMenuList();
                            OrderList.Remove(SelectedProductToBill);
                            ReCalculate();
                            return;
                        }
                    }
                }
            });

            //Giảm số lượng trong danh sách Order
            MinusCommand = new RelayCommand<TextBox>((p) => { return true; }, (p) =>
            {
                if (SelectedProductToBill != null)
                {
                    if (SelectedProductToBill.Quantity <= 1)
                    {
                        MessageBox.Show("Đã đạt số lượng tối thiểu!", "Cảnh báo");
                        return;
                    }
                    else
                    {
                        SelectedProductToBill.Quantity -= 1;
                        FilterOrderList();
                        PlusAllProductList(SelectedProductToBill);
                        ReCalculate();
                    }
                }
            });

            //Tăng số lượng trong danh sách Order
            PlusCommand = new RelayCommand<TextBox>((p) => { return true; }, (p) =>
            {
                if (SelectedProductToBill != null)
                {
                    for (int i = 0; i < AllProduct.Count; ++i)
                    {
                        if (AllProduct[i].Id == SelectedProductToBill.Id)
                        {
                            if (AllProduct[i].Quantity > 0)
                            {
                                SelectedProductToBill.Quantity += 1;
                                FilterOrderList();
                                MinusAllProductList(SelectedProductToBill);
                                ReCalculate();
                            }
                            else
                            {
                                MessageBox.Show("Số lượng còn lại không đủ!", "Cảnh báo");
                            }
                            return;
                        }
                    }
                }
            });


            //Mua hàng, lưu xuống bill
            BuyCommand = new RelayCommand<object>((p) => { return true; }, (p) =>
            {
                if (MessageBox.Show("Xác nhận thanh toán? Nhấn OK.", "Xác nhận", MessageBoxButton.OKCancel) == MessageBoxResult.OK)
                {
                    //var prodList = ProductService.Ins.GetAllProduct();
                    //BillDTO bill = new BillDTO { CustomerId = "KH0002", StaffId = "NV002", TotalPrice = 100000 };
                    //List<ProductBillInfoDTO> orderedProds = new List<ProductBillInfoDTO>();
                    //orderedProds.Add(new ProductBillInfoDTO { ProductId = prodList[1].Id, PricePerItem = prodList[1].Price, Quantity = 3 });
                    //orderedProds.Add(new ProductBillInfoDTO { ProductId = prodList[2].Id, PricePerItem = prodList[2].Price, Quantity = 2 });
                    ////ticketList.Add(new TicketDTO { SeatId=43, Price = showtime.TicketPrice , ShowtimeId = showtime.Id });
                    ////ticketList.Add(new TicketDTO { SeatId=44, Price = showtime.TicketPrice , ShowtimeId = showtime.Id });
                    //(bool isSuccess, string message) = BookingService.Ins.CreateProductOrder(bill, orderedProds);
                }
            });


            CloseWindowCM = new RelayCommand<Window>((p) => { return p == null ? false : true; }, (p) =>
            {

                if (p != null)
                {
                    p.Close();
                }

            });
            MinimizeWindowCM = new RelayCommand<Window>((p) => { return p == null ? false : true; }, (p) =>
            {

                if (p != null)
                {
                    p.WindowState = WindowState.Minimized;
                }
            });

            GoBackCommand = new RelayCommand<Page>((p) => { return true; }, (p) =>
            {
                if (p!=null)
                {
                    
                }
            });

                MouseMoveCommand = new RelayCommand<Window>((p) => { return p == null ? false : true; }, (p) =>
            {
                Window window = GetWindowParent(p);
                Window w = window as Window;
                if (w != null)
                {
                    w.DragMove();
                }
            }
           );
        }
        private Window GetWindowParent(Window p)
        {
            Window parent = p;

            while (parent.Parent != null)
            {
                parent = parent.Parent as Window;
            }

            return parent;
        }
        public void ReCalculate()
        {
            TotalPrice = 0;
            foreach (ProductDTO item in OrderList)
            {
                TotalPrice += item.Price * item.Quantity;
            }
        }
        public void FilterFood()
        {
            ObservableCollection<ProductDTO> temp = new ObservableCollection<ProductDTO>();
            foreach (ProductDTO item in AllProduct)
            {
                if (item.Category == "Đồ ăn")
                    temp.Add(item);
            }
            MenuList = new ObservableCollection<ProductDTO>(temp);
        }
        public void FilterDrink()
        {
            ObservableCollection<ProductDTO> temp = new ObservableCollection<ProductDTO>();
            foreach (ProductDTO item in AllProduct)
            {
                if (item.Category == "Thức uống")
                    temp.Add(item);
            }
            MenuList = new ObservableCollection<ProductDTO>(temp);
        }
        public void FilterMenuList()
        {
            if (TabProduct.Header.ToString() == "Tất cả")
            {
                MenuList = new ObservableCollection<ProductDTO>(AllProduct);
            }
            else
            {
                if (TabProduct.Header.ToString() != "Thức uống")
                {
                    FilterFood();
                }
                else
                {
                    FilterDrink();
                }
            }
        }
        public void FilterOrderList()
        {
            ObservableCollection<ProductDTO> temp = new ObservableCollection<ProductDTO>();
            OrderList = new ObservableCollection<ProductDTO>(OrderList);
        }
        public void MinusAllProductList(ProductDTO item)
        {
            for (int i = 0; i < AllProduct.Count; ++i)
            {
                if (AllProduct[i].Id == item.Id)
                {
                    AllProduct[i].Quantity -= 1;
                    FilterMenuList();
                    return;
                }
            }
        }
        public void PlusAllProductList(ProductDTO item)
        {
            for (int i = 0; i < AllProduct.Count; ++i)
            {
                if (AllProduct[i].Id == item.Id)
                {
                    AllProduct[i].Quantity += 1;
                    FilterMenuList();
                    return;
                }
            }
        }

        public void Demo()
        {
            AllProduct.Add(new ProductDTO(1, "Burger gà", "Đồ ăn", 45000, "/Resources/FoodLayout/Food/Burger/Chicken.jpg", 5));
            AllProduct.Add(new ProductDTO(2, "Trà sữa", "Thức uống", 20000, "/Resources/FoodLayout/Food/Drink/Milk Tea.jpg", 9));
            AllProduct.Add(new ProductDTO(3, "Coca Cola", "Thức uống", 25000, "/Resources/FoodLayout/Food/Drink/Coca Cola.jpg", 10));
            AllProduct.Add(new ProductDTO(4, "Bỏng ngô Caramel", "Đồ ăn", 30000, "/Resources/FoodLayout/Food/Popcorn/Caramel.jpg", 50));
            AllProduct.Add(new ProductDTO(5, "Snack Ostar", "Đồ ăn", 15000, "/Resources/FoodLayout/Food/Snack/Ostar.jpg", 25));
            AllProduct.Add(new ProductDTO(6, "Burger bò", "Đồ ăn", 45000, "/Resources/FoodLayout/Food/Burger/Beef.jpg", 10));
            AllProduct.Add(new ProductDTO(7, "Snack Poca", "Đồ ăn", 15000, "/Resources/FoodLayout/Food/Snack/Poca.jpg", 50));
            AllProduct.Add(new ProductDTO(8, "Snack Tonies", "Đồ ăn", 15000, "/Resources/FoodLayout/Food/Snack/Tonies.jpg", 30));
            AllProduct.Add(new ProductDTO(9, "Hotdog Choripan", "Đồ ăn", 30000, "/Resources/FoodLayout/Food/Hotdog/Choripan.jpg", 40));
            AllProduct.Add(new ProductDTO(10, "Cà phê Espresso", "Thức uống", 50000, "/Resources/FoodLayout/Food/Drink/Espresso Coffee.jpg", 90));
            AllProduct.Add(new ProductDTO(11, "Trà đào", "Thức uống", 20000, "/Resources/FoodLayout/Food/Drink/Peach Tea.jpg", 120));
            AllProduct.Add(new ProductDTO(12, "Pepsi", "Thức uống", 25000, "/Resources/FoodLayout/Food/Drink/Pepsi.jpg", 100));
        }
    }
}