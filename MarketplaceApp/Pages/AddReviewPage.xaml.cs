using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using MarketplaceApp.Classes;

namespace MarketplaceApp.Pages
{
   
    public partial class AddReviewPage : Page
    {
        private Reviews _currentReview = Reviews.CreateNew();
        private readonly bool _buyerMode;

        public AddReviewPage(Reviews selectedReview, Products product)
        {
            InitializeComponent();

            if (selectedReview != null)
                _currentReview = selectedReview;

            Title = _currentReview.ReviewId == 0 ? "Новый отзыв" : "Редактирование отзыва";
            HeaderText.Text = Title;

            var db = Core.Context;
            CBProduct.ItemsSource = db.Products.OrderBy(p => p.ProductName).ToList();
            CBUser.ItemsSource = db.Users.Where(u => u.RoleId == Session.RoleBuyer).OrderBy(u => u.LastName).ToList();
            CBRating.ItemsSource = new List<int> { 5, 4, 3, 2, 1 };

            _buyerMode = Session.RoleId == Session.RoleBuyer;
            if (_buyerMode)
            {
                if (_currentReview.ReviewId == 0)
                {
                    _currentReview.UserId = Session.CurrentUser.UserId;
                    if (product != null)
                        _currentReview.ProductId = product.ProductId;
                }
                CBProduct.IsEnabled = false;
                UserLabel.Visibility = Visibility.Collapsed;
                CBUser.Visibility = Visibility.Collapsed;
                DateLabel.Visibility = Visibility.Collapsed;
                DPDate.Visibility = Visibility.Collapsed;
                CBPublished.Visibility = Visibility.Collapsed;
            }

            DPDate.DisplayDateEnd = DateTime.Today;
            DataContext = _currentReview;
        }

        private void ButtonSave_Click(object sender, RoutedEventArgs e)
        {
            var db = Core.Context;
            StringBuilder errors = new StringBuilder();

            _currentReview.ReviewText = (_currentReview.ReviewText ?? "").Trim();

            if (_currentReview.ProductId == 0)
                errors.AppendLine("Выберите товар!");
            if (_currentReview.UserId == 0)
                errors.AppendLine("Выберите автора отзыва!");
            if (_currentReview.ProductId != 0 && _currentReview.UserId != 0 &&
                db.Reviews.Any(r => r.ProductId == _currentReview.ProductId && r.UserId == _currentReview.UserId &&
                                    r.ReviewId != _currentReview.ReviewId))
                errors.AppendLine("Этот покупатель уже оставил отзыв на этот товар!");
            if (_currentReview.Rating < 1 || _currentReview.Rating > 5)
                errors.AppendLine("Оценка должна быть от 1 до 5!");
            if (_currentReview.ReviewText.Length < 5)
                errors.AppendLine("Напишите текст отзыва (не короче 5 символов)!");
            if (_currentReview.ReviewDate.Date > DateTime.Today)
                errors.AppendLine("Дата отзыва не может быть в будущем!");

            if (errors.Length > 0)
            {
                MessageHelper.Warning(errors.ToString());
                return;
            }

            if (_buyerMode)
                _currentReview.ReviewDate = DateTime.Now;

            bool isNew = _currentReview.ReviewId == 0;
            if (isNew)
                db.Reviews.Add(_currentReview);

            try
            {
                db.SaveChanges();
                MessageHelper.Info(_buyerMode ? "Спасибо за отзыв!" : "Данные успешно сохранены!");
                NavigationService?.GoBack();
            }
            catch (Exception ex)
            {
                if (isNew)
                    Core.Detach(_currentReview);
                MessageHelper.Error("Не удалось сохранить отзыв.\n" + Core.GetErrorMessage(ex));
            }
        }

        private void ButtonClean_Click(object sender, RoutedEventArgs e)
        {
            TBText.Text = "";
            CBRating.SelectedItem = 5;
        }
    }
}
