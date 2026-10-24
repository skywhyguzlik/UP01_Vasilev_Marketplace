using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using MarketplaceApp.Classes;

namespace MarketplaceApp.Pages
{
    
    public partial class ReviewsTabPage : Page
    {
        public ReviewsTabPage()
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

            List<Reviews> all = Core.Context.Reviews.ToList();
            List<Reviews> list = all;

            string search = SearchBox.Text.Trim().ToLower();
            if (search.Length > 0)
                list = list.Where(r => r.Products.ProductName.ToLower().Contains(search) ||
                                       r.Users.FIO.ToLower().Contains(search) ||
                                       r.ReviewText.ToLower().Contains(search)).ToList();

            if (RatingBox.SelectedIndex > 0)
            {
                int rating = 6 - RatingBox.SelectedIndex;   
                list = list.Where(r => r.Rating == rating).ToList();
            }

            switch (SortBox.SelectedIndex)
            {
                case 1: list = list.OrderBy(r => r.ReviewDate).ToList(); break;
                case 2: list = list.OrderByDescending(r => r.Rating).ToList(); break;
                case 3: list = list.OrderBy(r => r.Rating).ToList(); break;
                default: list = list.OrderByDescending(r => r.ReviewDate).ToList(); break;
            }

            DataGridReview.ItemsSource = list;
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
            RatingBox.SelectedIndex = 0;
            SortBox.SelectedIndex = 0;
        }

        private void ButtonAdd_Click(object sender, RoutedEventArgs e)
        {
            NavigationService?.Navigate(new AddReviewPage(null, null));
        }

        private void ButtonEdit_Click(object sender, RoutedEventArgs e)
        {
            NavigationService?.Navigate(new AddReviewPage((sender as Button).DataContext as Reviews, null));
        }

        private void ButtonPublish_Click(object sender, RoutedEventArgs e)
        {
            var selected = DataGridReview.SelectedItems.Cast<Reviews>().ToList();
            if (selected.Count == 0)
            {
                MessageHelper.Warning("Выделите отзывы в таблице.");
                return;
            }
            try
            {
                foreach (var review in selected)
                    review.IsPublished = !review.IsPublished;
                Core.Context.SaveChanges();
                MessageHelper.Info("Видимость отзывов изменена: " + selected.Count);
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
            var reviewsForRemoving = DataGridReview.SelectedItems.Cast<Reviews>().ToList();
            if (reviewsForRemoving.Count == 0)
            {
                MessageHelper.Warning("Выделите записи, которые нужно удалить.");
                return;
            }
            if (!MessageHelper.Confirm("Вы точно хотите удалить записи в количестве " + reviewsForRemoving.Count + " элементов?"))
                return;

            try
            {
                foreach (var entity in reviewsForRemoving)
                    Core.Context.Reviews.Remove(entity);
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
