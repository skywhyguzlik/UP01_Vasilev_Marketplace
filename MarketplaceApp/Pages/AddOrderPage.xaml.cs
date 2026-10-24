using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using MarketplaceApp.Classes;

namespace MarketplaceApp.Pages
{
    
    public partial class AddOrderPage : Page
    {
        private Orders _currentOrder = Orders.CreateNew();

        public AddOrderPage(Orders selectedOrder)
        {
            InitializeComponent();

            if (selectedOrder != null)
                _currentOrder = selectedOrder;

            Title = _currentOrder.OrderId == 0 ? "Добавление заказа" : "Редактирование заказа №" + _currentOrder.OrderId;
            HeaderText.Text = Title;

            var db = Core.Context;
            CBBuyer.ItemsSource = db.Users.Where(u => u.RoleId == Session.RoleBuyer).OrderBy(u => u.LastName).ToList();
            CBPickupPoint.ItemsSource = db.PickupPoints.OrderBy(p => p.City).ThenBy(p => p.Address).ToList();
            CBStatus.ItemsSource = db.OrderStatuses.OrderBy(s => s.StatusId).ToList();
            CBPayment.ItemsSource = new List<string> { "Банковская карта", "СБП", "Электронный кошелёк" };

            TotalText.Text = _currentOrder.TotalAmount.ToString("N2") + " руб.";
            DataContext = _currentOrder;
        }

        private void ButtonSave_Click(object sender, RoutedEventArgs e)
        {
            var db = Core.Context;
            StringBuilder errors = new StringBuilder();

            _currentOrder.Comment = (_currentOrder.Comment ?? "").Trim();
            if (_currentOrder.Comment.Length == 0)
                _currentOrder.Comment = "Без комментария";

            if (_currentOrder.BuyerId == 0)
                errors.AppendLine("Выберите покупателя!");
            if (_currentOrder.PickupPointId == 0)
                errors.AppendLine("Выберите пункт выдачи!");
            if (_currentOrder.StatusId == 0)
                errors.AppendLine("Выберите статус!");
            if (string.IsNullOrEmpty(_currentOrder.PaymentMethod))
                errors.AppendLine("Выберите способ оплаты!");
            if (_currentOrder.OrderDate.Date > DateTime.Today)
                errors.AppendLine("Дата заказа не может быть в будущем!");
            if (_currentOrder.OrderDate.Year < 2020)
                errors.AppendLine("Укажите корректную дату заказа!");
            if (_currentOrder.DeliveryDate.Date < _currentOrder.OrderDate.Date)
                errors.AppendLine("Дата доставки не может быть раньше даты заказа!");
            if (_currentOrder.DeliveryDate.Date > _currentOrder.OrderDate.Date.AddDays(60))
                errors.AppendLine("Срок доставки не может быть больше 60 дней!");

            if (errors.Length > 0)
            {
                MessageHelper.Warning(errors.ToString());
                return;
            }

            bool isNew = _currentOrder.OrderId == 0;
            if (isNew)
                db.Orders.Add(_currentOrder);

            try
            {
                db.SaveChanges();
                MessageHelper.Info(isNew
                    ? "Заказ №" + _currentOrder.OrderId + " сохранён!\nДобавьте в него товары в таблице «Состав заказов»."
                    : "Данные успешно сохранены!");
                NavigationService?.GoBack();
            }
            catch (Exception ex)
            {
                if (isNew)
                    Core.Detach(_currentOrder);
                MessageHelper.Error("Не удалось сохранить данные.\n" + Core.GetErrorMessage(ex));
            }
        }

        private void ButtonClean_Click(object sender, RoutedEventArgs e)
        {
            TBComment.Text = "";
        }
    }
}
