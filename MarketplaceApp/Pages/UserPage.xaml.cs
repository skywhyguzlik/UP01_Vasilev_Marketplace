using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using MarketplaceApp.Classes;

namespace MarketplaceApp.Pages
{
   
    public partial class UserPage : Page
    {
        public UserPage()
        {
            InitializeComponent();

            var db = Core.Context;

            var categories = db.Categories.OrderBy(c => c.CategoryName).ToList();
            categories.Insert(0, new Categories { CategoryId = 0, CategoryName = "Все категории" });
            CategoryBox.ItemsSource = categories;
            CategoryBox.SelectedIndex = 0;

            PickupBox.ItemsSource = db.PickupPoints.OrderBy(p => p.City).ThenBy(p => p.Address).ToList();
            PickupBox.SelectedIndex = 0;

            PaymentBox.ItemsSource = new List<string> { "Банковская карта", "СБП", "Электронный кошелёк" };
            PaymentBox.SelectedIndex = 0;

            IsVisibleChanged += Page_IsVisibleChanged;
            UpdateProducts();
        }

        
        private void Page_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (IsVisible)
            {
                Core.RefreshAll();
                UpdateProducts();
            }
        }

        
        private void UpdateProducts()
        {
            if (!IsInitialized)
                return;

            try
            {
                List<Products> allProducts = Core.Context.Products
                    .Where(p => p.IsActive && p.ModerationStatus == "Одобрен")
                    .ToList();
                List<Products> products = allProducts;

                
                string search = SearchBox.Text.Trim().ToLower();
                if (search.Length > 0)
                    products = products.Where(p => p.ProductName.ToLower().Contains(search) ||
                                                   p.Brand.ToLower().Contains(search)).ToList();

                
                var category = CategoryBox.SelectedItem as Categories;
                if (category != null && category.CategoryId != 0)
                    products = products.Where(p => p.CategoryId == category.CategoryId).ToList();

                
                if (DiscountCheck.IsChecked == true)
                    products = products.Where(p => p.DiscountPercent > 0).ToList();

                
                switch (SortBox.SelectedIndex)
                {
                    case 1: products = products.OrderByDescending(p => p.ProductName).ToList(); break;
                    case 2: products = products.OrderBy(p => p.FinalPrice).ToList(); break;
                    case 3: products = products.OrderByDescending(p => p.FinalPrice).ToList(); break;
                    default: products = products.OrderBy(p => p.ProductName).ToList(); break;
                }

                ListProducts.ItemsSource = products;
                CountText.Text = "Найдено товаров: " + products.Count + " из " + allProducts.Count;
            }
            catch (Exception ex)
            {
                MessageHelper.Error(Core.GetErrorMessage(ex));
            }
        }

        private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            UpdateProducts();
        }

        private void Filter_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateProducts();
        }

        private void DiscountCheck_Changed(object sender, RoutedEventArgs e)
        {
            UpdateProducts();
        }

        private void ClearFiltersButton_Click(object sender, RoutedEventArgs e)
        {
            SearchBox.Text = "";
            CategoryBox.SelectedIndex = 0;
            SortBox.SelectedIndex = 0;
            DiscountCheck.IsChecked = false;
        }

        private void MyOrdersButton_Click(object sender, RoutedEventArgs e)
        {
            NavigationService?.Navigate(new MyOrdersPage());
        }

        
        private void BuyButton_Click(object sender, RoutedEventArgs e)
        {
            var product = (sender as Button).DataContext as Products;
            var pickupPoint = PickupBox.SelectedItem as PickupPoints;
            string payment = PaymentBox.SelectedItem as string;

            if (product == null)
                return;
            if (pickupPoint == null || payment == null)
            {
                MessageHelper.Warning("Выберите пункт выдачи и способ оплаты.");
                return;
            }
            if (product.StockQuantity < 1)
            {
                MessageHelper.Warning("Этого товара нет в наличии.");
                return;
            }
            if (!MessageHelper.Confirm("Оформить заказ?\n\nТовар: " + product.ProductName +
                                       "\nЦена: " + product.FinalPrice.ToString("N2") + " руб." +
                                       "\nПункт выдачи: " + pickupPoint.FullAddress +
                                       "\nОплата: " + payment))
                return;

            var db = Core.Context;
            var order = new Orders
            {
                BuyerId = Session.CurrentUser.UserId,
                PickupPointId = pickupPoint.PickupPointId,
                StatusId = 2,                       
                PaymentMethod = payment,
                OrderDate = DateTime.Now,
                DeliveryDate = DateTime.Today.AddDays(4),
                Comment = "Без комментария"
            };
            var item = new OrderItems
            {
                Orders = order,
                ProductId = product.ProductId,
                Quantity = 1,
                UnitPrice = product.FinalPrice
            };

            try
            {
                db.Orders.Add(order);
                db.OrderItems.Add(item);
                db.SaveChanges();     
                MessageHelper.Info("Заказ №" + order.OrderId + " оформлен!\nОжидаемая дата доставки: " +
                                   order.DeliveryDate.ToString("dd.MM.yyyy"));
            }
            catch (Exception ex)
            {
                MessageHelper.Error("Не удалось оформить заказ.\n" + Core.GetErrorMessage(ex));
            }
            Core.RefreshAll();
            UpdateProducts();
        }
    }
}
