using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using MarketplaceApp.Classes;

namespace MarketplaceApp.Pages
{
    
    public partial class SellersTabPage : Page
    {
        public SellersTabPage()
        {
            InitializeComponent();
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

            List<Sellers> all = Core.Context.Sellers.ToList();
            List<Sellers> list = all;

            string search = SearchBox.Text.Trim().ToLower();
            if (search.Length > 0)
                list = list.Where(s => s.ShopName.ToLower().Contains(search) ||
                                       s.CompanyName.ToLower().Contains(search) ||
                                       s.INN.Contains(search)).ToList();

            if (VerifiedBox.SelectedIndex == 1)
                list = list.Where(s => s.IsVerified).ToList();
            else if (VerifiedBox.SelectedIndex == 2)
                list = list.Where(s => !s.IsVerified).ToList();

            switch (SortBox.SelectedIndex)
            {
                case 1: list = list.OrderByDescending(s => s.ShopName).ToList(); break;
                case 2: list = list.OrderByDescending(s => s.Rating).ToList(); break;
                case 3: list = list.OrderBy(s => s.Rating).ToList(); break;
                default: list = list.OrderBy(s => s.ShopName).ToList(); break;
            }

            DataGridSeller.ItemsSource = list;
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
            VerifiedBox.SelectedIndex = 0;
            SortBox.SelectedIndex = 0;
        }

        private void ButtonAdd_Click(object sender, RoutedEventArgs e)
        {
            NavigationService?.Navigate(new AddSellerPage(null));
        }

        private void ButtonEdit_Click(object sender, RoutedEventArgs e)
        {
            NavigationService?.Navigate(new AddSellerPage((sender as Button).DataContext as Sellers));
        }

        
        private void ButtonDel_Click(object sender, RoutedEventArgs e)
        {
            var sellersForRemoving = DataGridSeller.SelectedItems.Cast<Sellers>().ToList();
            if (sellersForRemoving.Count == 0)
            {
                MessageHelper.Warning("Выделите записи, которые нужно удалить.");
                return;
            }
            if (!MessageHelper.Confirm("Вы точно хотите удалить записи в количестве " + sellersForRemoving.Count + " элементов?\n\n" +
                                       "Вместе с магазинами будут удалены их товары, позиции заказов и отзывы."))
                return;

            try
            {
                foreach (var entity in sellersForRemoving)
                    Core.Context.Sellers.Remove(entity);
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
