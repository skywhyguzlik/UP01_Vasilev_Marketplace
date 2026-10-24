using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using MarketplaceApp.Classes;
using Microsoft.Win32;

namespace MarketplaceApp.Pages
{
    
    public partial class AddProductPage : Page
    {
        private Products _currentProduct = Products.CreateNew();
        private readonly bool _sellerMode;

        public AddProductPage(Products selectedProduct)
        {
            InitializeComponent();

            if (selectedProduct != null)
                _currentProduct = selectedProduct;

            Title = _currentProduct.ProductId == 0 ? "Добавление товара" : "Редактирование товара";
            HeaderText.Text = Title;

            var db = Core.Context;
            CBSeller.ItemsSource = db.Sellers.OrderBy(s => s.ShopName).ToList();
            CBCategory.ItemsSource = db.Categories.OrderBy(c => c.CategoryName).ToList();
            CBModeration.ItemsSource = new List<string> { "На модерации", "Одобрен", "Отклонён" };

            
            _sellerMode = Session.RoleId == Session.RoleSeller;
            if (_sellerMode)
            {
                if (_currentProduct.ProductId == 0)
                    _currentProduct.SellerId = Session.SellerId;
                CBSeller.IsEnabled = false;
                CBModeration.IsEnabled = false;
                NoteText.Text = "* — обязательные поля. После сохранения товар отправляется на модерацию.";
            }

            DataContext = _currentProduct;
            ShowPhoto(_currentProduct.ImagePath);
        }

        
        private void ShowPhoto(string path)
        {
            PhotoImage.Source = new ImagePathConverter().Convert(path, typeof(ImageSource),
                                                                 null, System.Globalization.CultureInfo.CurrentCulture) as ImageSource;
        }

        
        private void ButtonPhoto_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog
            {
                Title = "Выберите фото товара",
                Filter = "Изображения (*.png;*.jpg;*.jpeg)|*.png;*.jpg;*.jpeg"
            };
            if (dialog.ShowDialog() != true)
                return;

            try
            {
                var info = new FileInfo(dialog.FileName);
                if (info.Length > 5 * 1024 * 1024)
                {
                    MessageHelper.Warning("Размер файла не должен превышать 5 МБ.");
                    return;
                }

                string folder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "images");
                Directory.CreateDirectory(folder);
                string fileName = "photo_" + DateTime.Now.ToString("yyyyMMddHHmmss") + info.Extension.ToLower();
                File.Copy(dialog.FileName, Path.Combine(folder, fileName), true);

                _currentProduct.ImagePath = "images/" + fileName;
                ShowPhoto(_currentProduct.ImagePath);
            }
            catch (Exception ex)
            {
                MessageHelper.Error("Не удалось загрузить фото.\n" + ex.Message);
            }
        }

        private void ButtonSave_Click(object sender, RoutedEventArgs e)
        {
            var db = Core.Context;
            StringBuilder errors = new StringBuilder();

            _currentProduct.Article = (_currentProduct.Article ?? "").Trim();
            _currentProduct.ProductName = (_currentProduct.ProductName ?? "").Trim();
            _currentProduct.Brand = (_currentProduct.Brand ?? "").Trim();
            _currentProduct.Description = (_currentProduct.Description ?? "").Trim();

            if (_currentProduct.SellerId == 0)
                errors.AppendLine("Выберите продавца!");
            if (_currentProduct.CategoryId == 0)
                errors.AppendLine("Выберите категорию!");
            if (_currentProduct.Article.Length < 3)
                errors.AppendLine("Укажите артикул (не короче 3 символов)!");
            else if (db.Products.Any(p => p.Article == _currentProduct.Article && p.ProductId != _currentProduct.ProductId))
                errors.AppendLine("Товар с таким артикулом уже существует!");
            if (_currentProduct.ProductName.Length < 2)
                errors.AppendLine("Укажите название товара!");
            if (_currentProduct.Brand.Length == 0)
                errors.AppendLine("Укажите бренд!");
            if (_currentProduct.Price < 1 || _currentProduct.Price > 10000000)
                errors.AppendLine("Цена должна быть от 1 до 10 000 000 руб.!");
            if (_currentProduct.DiscountPercent < 0 || _currentProduct.DiscountPercent > 90)
                errors.AppendLine("Скидка должна быть от 0 до 90 %!");
            if (_currentProduct.StockQuantity < 0 || _currentProduct.StockQuantity > 100000)
                errors.AppendLine("Остаток должен быть от 0 до 100 000 шт.!");

            if (errors.Length > 0)
            {
                MessageHelper.Warning(errors.ToString());
                return;
            }

            _currentProduct.Price = Math.Round(_currentProduct.Price, 2);
            if (_sellerMode)
                _currentProduct.ModerationStatus = "На модерации";   
            if (string.IsNullOrEmpty(_currentProduct.ModerationStatus))
                _currentProduct.ModerationStatus = "На модерации";

            if (_currentProduct.ProductId == 0)
                db.Products.Add(_currentProduct);

            try
            {
                db.SaveChanges();
                MessageHelper.Info(_sellerMode ? "Данные успешно сохранены! Товар отправлен на модерацию."
                                               : "Данные успешно сохранены!");
                NavigationService?.GoBack();
            }
            catch (Exception ex)
            {
                if (_currentProduct.ProductId == 0)
                    Core.Detach(_currentProduct);
                MessageHelper.Error("Не удалось сохранить данные.\n" + Core.GetErrorMessage(ex));
            }
        }

        private void ButtonClean_Click(object sender, RoutedEventArgs e)
        {
            TBArticle.Text = "";
            TBName.Text = "";
            TBBrand.Text = "";
            TBDescription.Text = "";
            TBPrice.Text = "";
            TBDiscount.Text = "0";
            TBStock.Text = "0";
        }
    }
}
