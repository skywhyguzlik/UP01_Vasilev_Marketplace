using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using MarketplaceApp.Classes;

namespace MarketplaceApp.Pages
{
    
    public partial class OrderItemsTabPage : Page
    {
        public OrderItemsTabPage()
        {
            InitializeComponent();
            FillOrders();
            IsVisibleChanged += Page_IsVisibleChanged;
            UpdateData();
        }

        private void FillOrders()
        {
            var orders = Core.Context.Orders.OrderByDescending(o => o.OrderId).ToList();
            orders.Insert(0, new Orders { OrderId = 0 });
            OrderBox.ItemsSource = orders;
            OrderBox.SelectedIndex = 0;
        }

        private void Page_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (IsVisible)
            {
                Core.RefreshAll();
                UpdateData();
            }
        }

        private void UpdateData()
        {
            if (!IsInitialized)
                return;

            List<OrderItems> all = Core.Context.OrderItems.ToList();
            List<OrderItems> list = all;

            string search = SearchBox.Text.Trim().ToLower();
            if (search.Length > 0)
                list = list.Where(i => i.Products.ProductName.ToLower().Contains(search)).ToList();

            var order = OrderBox.SelectedItem as Orders;
            if (order != null && order.OrderId != 0)
                list = list.Where(i => i.OrderId == order.OrderId).ToList();

            switch (SortBox.SelectedIndex)
            {
                case 1: list = list.OrderBy(i => i.OrderId).ToList(); break;
                case 2: list = list.OrderByDescending(i => i.LineTotal).ToList(); break;
                default: list = list.OrderByDescending(i => i.OrderId).ToList(); break;
            }

            DataGridOrderItem.ItemsSource = list;
            CountText.Text = "Записей: " + list.Count + " из " + all.Count;
        }

        private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            UpdateData();
        }

        private void Filter_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateData();
        }

        private void ClearFiltersButton_Click(object sender, RoutedEventArgs e)
        {
            SearchBox.Text = "";
            OrderBox.SelectedIndex = 0;
            SortBox.SelectedIndex = 0;
        }

        private void ButtonAdd_Click(object sender, RoutedEventArgs e)
        {
            var order = OrderBox.SelectedItem as Orders;
            NavigationService?.Navigate(new AddOrderItemPage(null, order != null && order.OrderId != 0 ? order.OrderId : 0));
        }

        private void ButtonEdit_Click(object sender, RoutedEventArgs e)
        {
            NavigationService?.Navigate(new AddOrderItemPage((sender as Button).DataContext as OrderItems, 0));
        }

        private void ButtonDel_Click(object sender, RoutedEventArgs e)
        {
            var itemsForRemoving = DataGridOrderItem.SelectedItems.Cast<OrderItems>().ToList();
            if (itemsForRemoving.Count == 0)
            {
                MessageHelper.Warning("Выделите записи, которые нужно удалить.");
                return;
            }
            if (!MessageHelper.Confirm("Вы точно хотите удалить записи в количестве " + itemsForRemoving.Count + " элементов?"))
                return;

            try
            {
                foreach (var entity in itemsForRemoving)
                    Core.Context.OrderItems.Remove(entity);
                Core.Context.SaveChanges();
                MessageHelper.Info("Данные успешно удалены!");
            }
            catch (Exception ex)
            {
                MessageHelper.Error("Не удалось удалить данные.\n" + Core.GetErrorMessage(ex));
            }
            Core.RefreshAll();   
            UpdateData();
        }
    }
}
