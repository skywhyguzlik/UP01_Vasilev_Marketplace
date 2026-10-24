using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using MarketplaceApp.Classes;

namespace MarketplaceApp.Pages
{
    
    public partial class OrdersTabPage : Page
    {
        public OrdersTabPage()
        {
            InitializeComponent();

            var statuses = Core.Context.OrderStatuses.OrderBy(s => s.StatusId).ToList();
            NewStatusBox.ItemsSource = statuses;
            NewStatusBox.SelectedIndex = 0;

            var filterStatuses = statuses.ToList();
            filterStatuses.Insert(0, new OrderStatuses { StatusId = 0, StatusName = "Все статусы" });
            StatusBox.ItemsSource = filterStatuses;
            StatusBox.SelectedIndex = 0;

            IsVisibleChanged += Page_IsVisibleChanged;
            UpdateData();
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

            List<Orders> all = Core.Context.Orders.ToList();
            List<Orders> list = all;

            string search = SearchBox.Text.Trim().ToLower();
            if (search.Length > 0)
                list = list.Where(o => o.OrderId.ToString() == search ||
                                       o.Users.FIO.ToLower().Contains(search)).ToList();

            var status = StatusBox.SelectedItem as OrderStatuses;
            if (status != null && status.StatusId != 0)
                list = list.Where(o => o.StatusId == status.StatusId).ToList();

            switch (SortBox.SelectedIndex)
            {
                case 1: list = list.OrderBy(o => o.OrderDate).ToList(); break;
                case 2: list = list.OrderByDescending(o => o.TotalAmount).ToList(); break;
                case 3: list = list.OrderBy(o => o.TotalAmount).ToList(); break;
                default: list = list.OrderByDescending(o => o.OrderDate).ToList(); break;
            }

            DataGridOrder.ItemsSource = list;
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
            StatusBox.SelectedIndex = 0;
            SortBox.SelectedIndex = 0;
        }

        private void ButtonAdd_Click(object sender, RoutedEventArgs e)
        {
            NavigationService?.Navigate(new AddOrderPage(null));
        }

        private void ButtonEdit_Click(object sender, RoutedEventArgs e)
        {
            NavigationService?.Navigate(new AddOrderPage((sender as Button).DataContext as Orders));
        }

        
        private void ButtonStatus_Click(object sender, RoutedEventArgs e)
        {
            var selected = DataGridOrder.SelectedItems.Cast<Orders>().ToList();
            var newStatus = NewStatusBox.SelectedItem as OrderStatuses;
            if (selected.Count == 0 || newStatus == null)
            {
                MessageHelper.Warning("Выделите заказы в таблице и выберите новый статус.");
                return;
            }
            if (!MessageHelper.Confirm("Установить статус «" + newStatus.StatusName + "» для заказов: " + selected.Count + "?"))
                return;

            try
            {
                foreach (var order in selected)
                {
                    order.StatusId = newStatus.StatusId;
                    if (newStatus.StatusId == 4 && order.DeliveryDate > DateTime.Today)   
                        order.DeliveryDate = DateTime.Today;
                }
                Core.Context.SaveChanges();
                MessageHelper.Info("Статус заказов изменён.");
            }
            catch (Exception ex)
            {
                Core.RefreshAll();
                MessageHelper.Error(Core.GetErrorMessage(ex));
            }
            UpdateData();
        }

        
        private void ButtonDel_Click(object sender, RoutedEventArgs e)
        {
            var ordersForRemoving = DataGridOrder.SelectedItems.Cast<Orders>().ToList();
            if (ordersForRemoving.Count == 0)
            {
                MessageHelper.Warning("Выделите записи, которые нужно удалить.");
                return;
            }
            if (!MessageHelper.Confirm("Вы точно хотите удалить записи в количестве " + ordersForRemoving.Count + " элементов?\n\n" +
                                       "Вместе с заказами будет удалён их состав."))
                return;

            try
            {
                foreach (var entity in ordersForRemoving)
                    Core.Context.Orders.Remove(entity);
                Core.Context.SaveChanges();
                MessageHelper.Info("Данные успешно удалены!");
            }
            catch (Exception ex)
            {
                Core.RefreshAll();
                MessageHelper.Error("Не удалось удалить данные.\n" + Core.GetErrorMessage(ex));
            }
            UpdateData();
        }
    }
}
