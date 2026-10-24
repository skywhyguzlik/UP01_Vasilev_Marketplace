using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using MarketplaceApp.Classes;

namespace MarketplaceApp.Pages
{
    
    public partial class CategoryTabPage : Page
    {
        public CategoryTabPage()
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

            List<Categories> all = Core.Context.Categories.ToList();
            List<Categories> list = all;

            string search = SearchBox.Text.Trim().ToLower();
            if (search.Length > 0)
                list = list.Where(c => c.CategoryName.ToLower().Contains(search) ||
                                       c.Description.ToLower().Contains(search)).ToList();

            if (FilterBox.SelectedIndex == 1)
                list = list.Where(c => c.Products.Count > 0).ToList();
            else if (FilterBox.SelectedIndex == 2)
                list = list.Where(c => c.Products.Count == 0).ToList();

            switch (SortBox.SelectedIndex)
            {
                case 1: list = list.OrderByDescending(c => c.CategoryName).ToList(); break;
                case 2: list = list.OrderByDescending(c => c.Products.Count).ToList(); break;
                default: list = list.OrderBy(c => c.CategoryName).ToList(); break;
            }

            DataGridCategory.ItemsSource = list;
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
            FilterBox.SelectedIndex = 0;
            SortBox.SelectedIndex = 0;
        }

        private void ButtonAdd_Click(object sender, RoutedEventArgs e)
        {
            NavigationService?.Navigate(new AddCategoryPage(null));
        }

        private void ButtonEdit_Click(object sender, RoutedEventArgs e)
        {
            NavigationService?.Navigate(new AddCategoryPage((sender as Button).DataContext as Categories));
        }

        

        private void ButtonDel_Click(object sender, RoutedEventArgs e)
        {
            var categoryForRemoving = DataGridCategory.SelectedItems.Cast<Categories>().ToList();
            if (categoryForRemoving.Count == 0)
            {
                MessageHelper.Warning("Выделите записи, которые нужно удалить.");
                return;
            }
            if (!MessageHelper.Confirm("Вы точно хотите удалить записи в количестве " + categoryForRemoving.Count + " элементов?\n\n" +
                                       "Вместе с категориями будут удалены все их товары, позиции заказов и отзывы."))
                return;

            try
            {
                foreach (var entity in categoryForRemoving)
                    Core.Context.Categories.Remove(entity);
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
