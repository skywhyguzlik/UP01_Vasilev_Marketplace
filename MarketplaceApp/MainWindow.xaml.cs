using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using MarketplaceApp.Classes;
using MarketplaceApp.Pages;

namespace MarketplaceApp
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            MainFrame.Navigate(new AuthPage());
        }

        
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            var timer = new System.Windows.Threading.DispatcherTimer();
            timer.Interval = new TimeSpan(0, 0, 1);
            timer.IsEnabled = true;
            timer.Tick += (o, t) => { DateTimeNow.Text = DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss"); };
            DateTimeNow.Text = DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss");
            timer.Start();
        }

        
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (MessageHelper.Confirm("Вы действительно хотите выйти из приложения?"))
            {
                ThemeManager.SaveTheme();
                e.Cancel = false;
            }
            else
            {
                e.Cancel = true;
            }
        }

        
        private void MainFrame_Navigated(object sender, NavigationEventArgs e)
        {
            if (Session.ClearHistory)
            {
                while (MainFrame.CanGoBack)
                    MainFrame.RemoveBackEntry();
                Session.ClearHistory = false;
            }

            var page = e.Content as Page;
            string pageTitle = page != null && !string.IsNullOrEmpty(page.Title) ? page.Title : "Маркетплейс товаров";
            TitleText.Text = pageTitle;
            Title = "Маркетплейс товаров — " + pageTitle;

            BtnBack.Visibility = MainFrame.CanGoBack ? Visibility.Visible : Visibility.Hidden;

            if (Session.CurrentUser != null)
            {
                BtnLogout.Visibility = Visibility.Visible;
                UserInfo.Text = "Вы вошли как: " + Session.CurrentUser.FIO + " (" + Session.CurrentUser.Roles.RoleName + ")";
            }
            else
            {
                BtnLogout.Visibility = Visibility.Collapsed;
                UserInfo.Text = "Вход не выполнен";
            }
        }

        private void BtnBack_Click(object sender, RoutedEventArgs e)
        {
            if (MainFrame.CanGoBack)
                MainFrame.GoBack();
        }

        private void BtnTheme_Click(object sender, RoutedEventArgs e)
        {
            ThemeManager.ToggleTheme();
        }

        
        private void BtnLogout_Click(object sender, RoutedEventArgs e)
        {
            if (!MessageHelper.Confirm("Вы действительно хотите выйти из учётной записи?"))
                return;

            ThemeManager.SaveTheme();
            Session.Logout();
            Session.ClearHistory = true;
            MainFrame.Navigate(new AuthPage());
        }
    }
}
