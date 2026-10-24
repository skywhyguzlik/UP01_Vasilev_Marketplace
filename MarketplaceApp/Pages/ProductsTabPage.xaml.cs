using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using MarketplaceApp.Classes;

namespace MarketplaceApp.Pages
{
    
    public partial class ProductsTabPage : Page
    {
        private readonly bool _sellerMode;

        public ProductsTabPage()
        {
            InitializeComponent();

            _sellerMode = Session.RoleId == Session.RoleSeller;
            if (_sellerMode)
            {
                Title = "Мои товары";
                SellerColumn.Visibility = Visibility.Collapsed;
                ModerationPanel.Visibility = Visibility.Collapsed;
            }

            var categories = Core.Context.Categories.OrderBy(c => c.CategoryName).ToList();
            categories.Insert(0, new Categories { CategoryId = 0, CategoryName = "Все категории" });
            CategoryBox.ItemsSource = categories;
            CategoryBox.SelectedIndex = 0;

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

            List<Products> all = Core.Context.Products.ToList();
            if (_sellerMode)
                all = all.Where(p => p.SellerId == Session.SellerId).ToList();
            List<Products> list = all;

            string search = SearchBox.Text.Trim().ToLower();
            if (search.Length > 0)
                list = list.Where(p => p.ProductName.ToLower().Contains(search) ||
                                       p.Brand.ToLower().Contains(search) ||
                                       p.Article.ToLower().Contains(search)).ToList();

            var category = CategoryBox.SelectedItem as Categories;
            if (category != null && category.CategoryId != 0)
                list = list.Where(p => p.CategoryId == category.CategoryId).ToList();

            if (StatusBox.SelectedIndex > 0)
            {
                string status = (StatusBox.SelectedItem as ComboBoxItem).Content.ToString();
                list = list.Where(p => p.ModerationStatus == status).ToList();
            }

            switch (SortBox.SelectedIndex)
            {
                case 1: list = list.OrderByDescending(p => p.ProductName).ToList(); break;
                case 2: list = list.OrderBy(p => p.Price).ToList(); break;
                case 3: list = list.OrderByDescending(p => p.Price).ToList(); break;
                case 4: list = list.OrderBy(p => p.StockQuantity).ToList(); break;
                default: list = list.OrderBy(p => p.ProductName).ToList(); break;
            }

            DataGridProduct.ItemsSource = list;
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
            CategoryBox.SelectedIndex = 0;
            StatusBox.SelectedIndex = 0;
            SortBox.SelectedIndex = 0;
        }

        private void ButtonAdd_Click(object sender, RoutedEventArgs e)
        {
            NavigationService?.Navigate(new AddProductPage(null));
        }

        private void ButtonEdit_Click(object sender, RoutedEventArgs e)
        {
            NavigationService?.Navigate(new AddProductPage((sender as Button).DataContext as Products));
        }

        
        private void SetModerationStatus(string status)
        {
            var selected = DataGridProduct.SelectedItems.Cast<Products>().ToList();
            if (selected.Count == 0)
            {
                MessageHelper.Warning("Выделите товары в таблице.");
                return;
            }
            try
            {
                foreach (var product in selected)
                    product.ModerationStatus = status;
                Core.Context.SaveChanges();
                MessageHelper.Info("Статус «" + status + "» установлен для товаров: " + selected.Count);
            }
            catch (Exception ex)
            {
                Core.RefreshAll();
                MessageHelper.Error(Core.GetErrorMessage(ex));
            }
            UpdateData();
        }

        private void ButtonApprove_Click(object sender, RoutedEventArgs e)
        {
            SetModerationStatus("Одобрен");
        }

        private void ButtonReject_Click(object sender, RoutedEventArgs e)
        {
            SetModerationStatus("Отклонён");
        }

        
        private void ButtonDel_Click(object sender, RoutedEventArgs e)
        {
            var productsForRemoving = DataGridProduct.SelectedItems.Cast<Products>().ToList();
            if (productsForRemoving.Count == 0)
            {
                MessageHelper.Warning("Выделите записи, которые нужно удалить.");
                return;
            }
            if (!MessageHelper.Confirm("Вы точно хотите удалить записи в количестве " + productsForRemoving.Count + " элементов?\n\n" +
                                       "Вместе с товарами будут удалены позиции заказов и отзывы о них."))
                return;

            try
            {
                foreach (var entity in productsForRemoving)
                    Core.Context.Products.Remove(entity);
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
