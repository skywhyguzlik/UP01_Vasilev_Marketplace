using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using MarketplaceApp.Classes;

namespace MarketplaceApp.Pages
{
    
    public partial class MyOrdersPage : Page
    {
        private const int StatusReceived = 4;   

        public MyOrdersPage()
        {
            InitializeComponent();

            var statuses = Core.Context.OrderStatuses.ToList();
            statuses.Insert(0, new OrderStatuses { StatusId = 0, StatusName = "Все статусы" });
            StatusBox.ItemsSource = statuses;
            StatusBox.SelectedIndex = 0;

            IsVisibleChanged += Page_IsVisibleChanged;
            UpdateOrders();
        }

        private void Page_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (IsVisible)
            {
                Core.RefreshAll();
                UpdateOrders();
            }
        }

        private void UpdateOrders()
        {
            if (!IsInitialized)
                return;

            int userId = Session.CurrentUser.UserId;
            List<Orders> allOrders = Core.Context.Orders.Where(o => o.BuyerId == userId).ToList();
            List<Orders> orders = allOrders;

            string search = SearchBox.Text.Trim().ToLower();
            if (search.Length > 0)
                orders = orders.Where(o => o.OrderItems.Any(i => i.Products.ProductName.ToLower().Contains(search))).ToList();

            var status = StatusBox.SelectedItem as OrderStatuses;
            if (status != null && status.StatusId != 0)
                orders = orders.Where(o => o.StatusId == status.StatusId).ToList();

            switch (SortBox.SelectedIndex)
            {
                case 1: orders = orders.OrderBy(o => o.OrderDate).ToList(); break;
                case 2: orders = orders.OrderByDescending(o => o.TotalAmount).ToList(); break;
                case 3: orders = orders.OrderBy(o => o.TotalAmount).ToList(); break;
                default: orders = orders.OrderByDescending(o => o.OrderDate).ToList(); break;
            }

            DataGridOrders.ItemsSource = orders;
            DataGridItems.ItemsSource = null;
            ItemsTitle.Text = "Состав заказа: выберите заказ в таблице выше";
            CountText.Text = "Заказов: " + orders.Count + " из " + allOrders.Count;
        }

        private void DataGridOrders_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var order = DataGridOrders.SelectedItem as Orders;
            if (order == null)
            {
                DataGridItems.ItemsSource = null;
                return;
            }
            DataGridItems.ItemsSource = order.OrderItems.ToList();
            ItemsTitle.Text = "Состав заказа №" + order.OrderId + " (" + order.OrderStatuses.StatusName + ")";
        }

        private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            UpdateOrders();
        }

        private void Filter_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateOrders();
        }

        private void ClearFiltersButton_Click(object sender, RoutedEventArgs e)
        {
            SearchBox.Text = "";
            StatusBox.SelectedIndex = 0;
            SortBox.SelectedIndex = 0;
        }

        private void ReviewButton_Click(object sender, RoutedEventArgs e)
        {
            var order = DataGridOrders.SelectedItem as Orders;
            var item = DataGridItems.SelectedItem as OrderItems;
            if (order == null || item == null)
            {
                MessageHelper.Warning("Выберите заказ, а затем товар в составе заказа.");
                return;
            }
            if (order.StatusId != StatusReceived)
            {
                MessageHelper.Warning("Отзыв можно оставить только на полученный товар.");
                return;
            }

            int userId = Session.CurrentUser.UserId;
            var existing = Core.Context.Reviews
                .FirstOrDefault(r => r.UserId == userId && r.ProductId == item.ProductId);
            if (existing != null)
            {
                if (MessageHelper.Confirm("Вы уже оставили отзыв на этот товар. Хотите его изменить?"))
                    NavigationService?.Navigate(new AddReviewPage(existing, null));
                return;
            }
            NavigationService?.Navigate(new AddReviewPage(null, item.Products));
        }
    }
}
