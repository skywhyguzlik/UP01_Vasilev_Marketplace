using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using MarketplaceApp.Classes;

namespace MarketplaceApp.Pages
{
    
    public partial class AdminPage : Page
    {
        public AdminPage()
        {
            InitializeComponent();

            bool isAdmin = Session.RoleId == Session.RoleAdmin;
            Title = isAdmin ? "Панель администратора" : "Панель менеджера";
            GreetingText.Text = "Здравствуйте, " + Session.CurrentUser.FirstName + "!";

            if (!isAdmin)
            {
                BtnUsers.Visibility = Visibility.Collapsed;
                BtnSellers.Visibility = Visibility.Collapsed;
            }

            IsVisibleChanged += Page_IsVisibleChanged;
        }

        private void Page_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (IsVisible)
                UpdateStats();
        }

        
        private void UpdateStats()
        {
            try
            {
                var db = Core.Context;
                int products = db.Products.Count();
                int moderation = db.Products.Count(p => p.ModerationStatus == "На модерации");
                int orders = db.Orders.Count();
                int newOrders = db.Orders.Count(o => o.StatusId == 1 || o.StatusId == 2);
                StatsText.Text = "Товаров: " + products + " (на модерации: " + moderation + ")   •   " +
                                 "Заказов: " + orders + " (новых и оплаченных: " + newOrders + ")";
            }
            catch (Exception ex)
            {
                StatsText.Text = Core.GetErrorMessage(ex);
            }
        }

        private void BtnUsers_Click(object sender, RoutedEventArgs e)
        {
            NavigationService?.Navigate(new UsersTabPage());
        }

        private void BtnSellers_Click(object sender, RoutedEventArgs e)
        {
            NavigationService?.Navigate(new SellersTabPage());
        }

        private void BtnCategories_Click(object sender, RoutedEventArgs e)
        {
            NavigationService?.Navigate(new CategoryTabPage());
        }

        private void BtnProducts_Click(object sender, RoutedEventArgs e)
        {
            NavigationService?.Navigate(new ProductsTabPage());
        }

        private void BtnPickupPoints_Click(object sender, RoutedEventArgs e)
        {
            NavigationService?.Navigate(new PickupPointsTabPage());
        }

        private void BtnOrders_Click(object sender, RoutedEventArgs e)
        {
            NavigationService?.Navigate(new OrdersTabPage());
        }

        private void BtnOrderItems_Click(object sender, RoutedEventArgs e)
        {
            NavigationService?.Navigate(new OrderItemsTabPage());
        }

        private void BtnReviews_Click(object sender, RoutedEventArgs e)
        {
            NavigationService?.Navigate(new ReviewsTabPage());
        }

        private void BtnDiagram_Click(object sender, RoutedEventArgs e)
        {
            NavigationService?.Navigate(new DiagrammPage());
        }
    }
}
