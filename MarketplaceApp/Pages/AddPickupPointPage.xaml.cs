using System;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using MarketplaceApp.Classes;

namespace MarketplaceApp.Pages
{
   
    public partial class AddPickupPointPage : Page
    {
        private PickupPoints _currentPoint = PickupPoints.CreateNew();

        public AddPickupPointPage(PickupPoints selectedPoint)
        {
            InitializeComponent();

            if (selectedPoint != null)
                _currentPoint = selectedPoint;

            Title = _currentPoint.PickupPointId == 0 ? "Добавление пункта выдачи" : "Редактирование пункта выдачи";
            HeaderText.Text = Title;
            DataContext = _currentPoint;
        }

        private void ButtonSave_Click(object sender, RoutedEventArgs e)
        {
            var db = Core.Context;
            StringBuilder errors = new StringBuilder();

            _currentPoint.City = (_currentPoint.City ?? "").Trim();
            _currentPoint.Address = (_currentPoint.Address ?? "").Trim();
            _currentPoint.WorkHours = (_currentPoint.WorkHours ?? "").Trim().Replace("-", "–");
            _currentPoint.Phone = (_currentPoint.Phone ?? "").Trim();

            if (!ValidationHelper.IsOnlyLetters(_currentPoint.City))
                errors.AppendLine("Укажите город (только буквы)!");
            if (_currentPoint.Address.Length < 5)
                errors.AppendLine("Укажите адрес (не короче 5 символов)!");
            else if (db.PickupPoints.Any(p => p.City == _currentPoint.City && p.Address == _currentPoint.Address &&
                                              p.PickupPointId != _currentPoint.PickupPointId))
                errors.AppendLine("Пункт выдачи с таким адресом уже есть!");
            if (!ValidationHelper.IsValidWorkHours(_currentPoint.WorkHours))
                errors.AppendLine("Укажите режим работы в формате 09:00–21:00");
            if (!ValidationHelper.IsValidPhone(_currentPoint.Phone))
                errors.AppendLine("Укажите телефон в формате +7 (495) 120-11-01");

            if (errors.Length > 0)
            {
                MessageHelper.Warning(errors.ToString());
                return;
            }

            if (_currentPoint.PickupPointId == 0)
                db.PickupPoints.Add(_currentPoint);

            try
            {
                db.SaveChanges();
                MessageHelper.Info("Данные успешно сохранены!");
                NavigationService?.GoBack();
            }
            catch (Exception ex)
            {
                if (_currentPoint.PickupPointId == 0)
                    Core.Detach(_currentPoint);
                MessageHelper.Error("Не удалось сохранить данные.\n" + Core.GetErrorMessage(ex));
            }
        }

        private void ButtonClean_Click(object sender, RoutedEventArgs e)
        {
            TBCity.Text = "";
            TBAddress.Text = "";
            TBWorkHours.Text = "";
            TBPhone.Text = "";
        }
    }
}
