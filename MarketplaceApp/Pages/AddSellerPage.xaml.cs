using System;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using MarketplaceApp.Classes;

namespace MarketplaceApp.Pages
{
    
    public partial class AddSellerPage : Page
    {
        private Sellers _currentSeller = Sellers.CreateNew();

        public AddSellerPage(Sellers selectedSeller)
        {
            InitializeComponent();

            if (selectedSeller != null)
                _currentSeller = selectedSeller;

            Title = _currentSeller.SellerId == 0 ? "Добавление продавца" : "Редактирование продавца";
            HeaderText.Text = Title;

            
            int currentUserId = _currentSeller.UserId;
            CBUser.ItemsSource = Core.Context.Users
                .Where(u => u.RoleId == Session.RoleSeller &&
                            (!u.Sellers.Any() || u.UserId == currentUserId))
                .OrderBy(u => u.LastName)
                .ToList();

            DPDate.DisplayDateEnd = DateTime.Today;
            DataContext = _currentSeller;
        }

        private void ButtonSave_Click(object sender, RoutedEventArgs e)
        {
            var db = Core.Context;
            StringBuilder errors = new StringBuilder();

            _currentSeller.ShopName = (_currentSeller.ShopName ?? "").Trim();
            _currentSeller.CompanyName = (_currentSeller.CompanyName ?? "").Trim();
            _currentSeller.INN = (_currentSeller.INN ?? "").Trim();
            _currentSeller.LegalAddress = (_currentSeller.LegalAddress ?? "").Trim();

            if (_currentSeller.UserId == 0)
                errors.AppendLine("Выберите владельца магазина!");
            if (_currentSeller.ShopName.Length == 0)
                errors.AppendLine("Укажите название магазина!");
            else if (db.Sellers.Any(s => s.ShopName == _currentSeller.ShopName && s.SellerId != _currentSeller.SellerId))
                errors.AppendLine("Магазин с таким названием уже есть!");
            if (_currentSeller.CompanyName.Length == 0)
                errors.AppendLine("Укажите название компании!");
            if (!ValidationHelper.IsValidInn(_currentSeller.INN))
                errors.AppendLine("ИНН должен состоять из 10 или 12 цифр!");
            else if (db.Sellers.Any(s => s.INN == _currentSeller.INN && s.SellerId != _currentSeller.SellerId))
                errors.AppendLine("Продавец с таким ИНН уже есть!");
            if (_currentSeller.LegalAddress.Length == 0)
                errors.AppendLine("Укажите юридический адрес!");
            if (_currentSeller.CommissionPercent < 0 || _currentSeller.CommissionPercent > 50)
                errors.AppendLine("Комиссия должна быть от 0 до 50 %!");
            if (_currentSeller.Rating < 0 || _currentSeller.Rating > 5)
                errors.AppendLine("Рейтинг должен быть от 0 до 5!");
            if (_currentSeller.RegistrationDate.Date > DateTime.Today)
                errors.AppendLine("Дата регистрации не может быть в будущем!");

            if (errors.Length > 0)
            {
                MessageHelper.Warning(errors.ToString());
                return;
            }

            _currentSeller.Rating = Math.Round(_currentSeller.Rating, 2);
            if (_currentSeller.SellerId == 0)
                db.Sellers.Add(_currentSeller);

            try
            {
                db.SaveChanges();
                MessageHelper.Info("Данные успешно сохранены!");
                NavigationService?.GoBack();
            }
            catch (Exception ex)
            {
                if (_currentSeller.SellerId == 0)
                    Core.Detach(_currentSeller);
                MessageHelper.Error("Не удалось сохранить данные.\n" + Core.GetErrorMessage(ex));
            }
        }

        private void ButtonClean_Click(object sender, RoutedEventArgs e)
        {
            TBShopName.Text = "";
            TBCompany.Text = "";
            TBInn.Text = "";
            TBAddress.Text = "";
            TBCommission.Text = "10";
            TBRating.Text = "0";
        }
    }
}
