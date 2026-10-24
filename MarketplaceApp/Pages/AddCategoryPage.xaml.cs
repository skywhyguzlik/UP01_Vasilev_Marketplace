using System;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using MarketplaceApp.Classes;

namespace MarketplaceApp.Pages
{
    
    public partial class AddCategoryPage : Page
    {
        private Categories _currentCategory = Categories.CreateNew();

        public AddCategoryPage(Categories selectedCategory)
        {
            InitializeComponent();

            if (selectedCategory != null)
                _currentCategory = selectedCategory;

            Title = _currentCategory.CategoryId == 0 ? "Добавление категории" : "Редактирование категории";
            HeaderText.Text = Title;
            DataContext = _currentCategory;
        }

        private void ButtonSaveCategory_Click(object sender, RoutedEventArgs e)
        {
            var db = Core.Context;
            StringBuilder errors = new StringBuilder();

            _currentCategory.CategoryName = (_currentCategory.CategoryName ?? "").Trim();
            _currentCategory.Description = (_currentCategory.Description ?? "").Trim();

            if (_currentCategory.CategoryName.Length < 2)
                errors.AppendLine("Укажите название категории (не короче 2 символов)!");
            else if (db.Categories.Any(c => c.CategoryName == _currentCategory.CategoryName &&
                                            c.CategoryId != _currentCategory.CategoryId))
                errors.AppendLine("Категория с таким названием уже существует!");
            if (_currentCategory.Description.Length == 0)
                errors.AppendLine("Укажите описание категории!");

            if (errors.Length > 0)
            {
                MessageHelper.Warning(errors.ToString());
                return;
            }

            if (_currentCategory.CategoryId == 0)
                db.Categories.Add(_currentCategory);

            try
            {
                db.SaveChanges();
                MessageHelper.Info("Данные успешно сохранены!");
                NavigationService?.GoBack();
            }
            catch (Exception ex)
            {
                if (_currentCategory.CategoryId == 0)
                    Core.Detach(_currentCategory);
                MessageHelper.Error("Не удалось сохранить данные.\n" + Core.GetErrorMessage(ex));
            }
        }

        private void ButtonClean_Click(object sender, RoutedEventArgs e)
        {
            TBCategoryName.Text = "";
            TBDescription.Text = "";
        }
    }
}
