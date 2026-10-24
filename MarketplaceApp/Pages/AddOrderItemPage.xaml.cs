using System;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using MarketplaceApp.Classes;

namespace MarketplaceApp.Pages
{
    
    public partial class AddOrderItemPage : Page
    {
        private OrderItems _currentItem = OrderItems.CreateNew();
        private readonly bool _isNew;

        public AddOrderItemPage(OrderItems selectedItem, int orderId)
        {
            InitializeComponent();

            if (selectedItem != null)
                _currentItem = selectedItem;
            _isNew = _currentItem.OrderItemId == 0;
            if (_isNew && orderId != 0)
                _currentItem.OrderId = orderId;

            Title = _isNew ? "Добавление товара в заказ" : "Редактирование позиции заказа";
            HeaderText.Text = Title;

            var db = Core.Context;
            CBOrder.ItemsSource = db.Orders.OrderByDescending(o => o.OrderId).ToList();
            CBProduct.ItemsSource = db.Products.OrderBy(p => p.ProductName).ToList();

            
            if (!_isNew)
            {
                CBOrder.IsEnabled = false;
                CBProduct.IsEnabled = false;
            }

            DataContext = _currentItem;
        }

        
        private void CBProduct_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var product = CBProduct.SelectedItem as Products;
            if (product == null)
                return;
            StockText.Text = "На складе: " + product.StockQuantity + " шт.   Цена со скидкой: " +
                             product.FinalPrice.ToString("N2") + " руб.";
            if (_isNew)
                TBPrice.Text = product.FinalPrice.ToString();
        }

        private void ButtonSave_Click(object sender, RoutedEventArgs e)
        {
            var db = Core.Context;
            StringBuilder errors = new StringBuilder();

            if (_currentItem.OrderId == 0)
                errors.AppendLine("Выберите заказ!");
            if (_currentItem.ProductId == 0)
                errors.AppendLine("Выберите товар!");
            else if (_isNew && db.OrderItems.Any(i => i.OrderId == _currentItem.OrderId && i.ProductId == _currentItem.ProductId))
                errors.AppendLine("Этот товар уже есть в заказе — измените количество в существующей позиции.");
            if (_currentItem.Quantity < 1 || _currentItem.Quantity > 1000)
                errors.AppendLine("Количество должно быть от 1 до 1000 шт.!");
            if (_currentItem.UnitPrice < 0 || _currentItem.UnitPrice > 10000000)
                errors.AppendLine("Цена должна быть от 0 до 10 000 000 руб.!");

            var product = CBProduct.SelectedItem as Products;
            if (_isNew && product != null && product.StockQuantity < _currentItem.Quantity)
                errors.AppendLine("На складе только " + product.StockQuantity + " шт. этого товара!");

            if (errors.Length > 0)
            {
                MessageHelper.Warning(errors.ToString());
                return;
            }

            _currentItem.UnitPrice = Math.Round(_currentItem.UnitPrice, 2);
            if (_isNew)
                db.OrderItems.Add(_currentItem);

            try
            {
                db.SaveChanges();   
                MessageHelper.Info("Данные успешно сохранены!");
                NavigationService?.GoBack();
            }
            catch (Exception ex)
            {
                if (_isNew)
                    Core.Detach(_currentItem);
                MessageHelper.Error("Не удалось сохранить данные.\n" + Core.GetErrorMessage(ex));
            }
        }

        private void ButtonClean_Click(object sender, RoutedEventArgs e)
        {
            TBQuantity.Text = "1";
            TBPrice.Text = "";
        }
    }
}
