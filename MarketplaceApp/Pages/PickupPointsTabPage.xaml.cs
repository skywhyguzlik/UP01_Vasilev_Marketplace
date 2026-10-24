using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using MarketplaceApp.Classes;

namespace MarketplaceApp.Pages
{
    
    public partial class PickupPointsTabPage : Page
    {
        private const string AllCities = "Все города";

        public PickupPointsTabPage()
        {
            InitializeComponent();
            FillCities();
            IsVisibleChanged += Page_IsVisibleChanged;
            UpdateData();
        }

        private void FillCities()
        {
            var cities = Core.Context.PickupPoints.Select(p => p.City).Distinct().OrderBy(c => c).ToList();
            cities.Insert(0, AllCities);
            CityBox.ItemsSource = cities;
            CityBox.SelectedIndex = 0;
        }

        private void Page_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (IsVisible)
            {
                Core.RefreshAll();
                string selectedCity = CityBox.SelectedItem as string;
                FillCities();                                    
                if (selectedCity != null && CityBox.Items.Contains(selectedCity))
                    CityBox.SelectedItem = selectedCity;
                UpdateData();
            }
        }

        private void UpdateData()
        {
            if (!IsInitialized)
                return;

            List<PickupPoints> all = Core.Context.PickupPoints.ToList();
            List<PickupPoints> list = all;

            string search = SearchBox.Text.Trim().ToLower();
            if (search.Length > 0)
                list = list.Where(p => p.Address.ToLower().Contains(search)).ToList();

            string city = CityBox.SelectedItem as string;
            if (city != null && city != AllCities)
                list = list.Where(p => p.City == city).ToList();

            switch (SortBox.SelectedIndex)
            {
                case 1: list = list.OrderByDescending(p => p.City).ThenBy(p => p.Address).ToList(); break;
                case 2: list = list.OrderByDescending(p => p.Orders.Count).ToList(); break;
                default: list = list.OrderBy(p => p.City).ThenBy(p => p.Address).ToList(); break;
            }

            DataGridPickupPoint.ItemsSource = list;
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
            CityBox.SelectedIndex = 0;
            SortBox.SelectedIndex = 0;
        }

        private void ButtonAdd_Click(object sender, RoutedEventArgs e)
        {
            NavigationService?.Navigate(new AddPickupPointPage(null));
        }

        private void ButtonEdit_Click(object sender, RoutedEventArgs e)
        {
            NavigationService?.Navigate(new AddPickupPointPage((sender as Button).DataContext as PickupPoints));
        }

        
        private void ButtonDel_Click(object sender, RoutedEventArgs e)
        {
            var pointsForRemoving = DataGridPickupPoint.SelectedItems.Cast<PickupPoints>().ToList();
            if (pointsForRemoving.Count == 0)
            {
                MessageHelper.Warning("Выделите записи, которые нужно удалить.");
                return;
            }
            if (!MessageHelper.Confirm("Вы точно хотите удалить записи в количестве " + pointsForRemoving.Count + " элементов?\n\n" +
                                       "Вместе с пунктами выдачи будут удалены все заказы, оформленные в них."))
                return;

            try
            {
                foreach (var entity in pointsForRemoving)
                    Core.Context.PickupPoints.Remove(entity);
                Core.Context.SaveChanges();
                MessageHelper.Info("Данные успешно удалены!");
            }
            catch (Exception ex)
            {
                Core.RefreshAll();
                MessageHelper.Error("Не удалось удалить данные.\n" + Core.GetErrorMessage(ex));
            }
            FillCities();
            UpdateData();
        }
    }
}
