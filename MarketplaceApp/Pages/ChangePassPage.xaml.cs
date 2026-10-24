using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using MarketplaceApp.Classes;

namespace MarketplaceApp.Pages
{
    
    public partial class ChangePassPage : Page
    {
        public ChangePassPage()
        {
            InitializeComponent();
        }

        private void PasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            HintHelper.UpdatePasswordHint(sender);
        }

        private void ButtonSave_Click(object sender, RoutedEventArgs e)
        {
            
            if (string.IsNullOrEmpty(CurrentPasswordBox.Password) ||
                string.IsNullOrEmpty(NewPasswordBox.Password) ||
                string.IsNullOrEmpty(ConfirmPasswordBox.Password) ||
                string.IsNullOrWhiteSpace(TbLogin.Text))
            {
                MessageHelper.Warning("Все поля обязательны к заполнению!");
                return;
            }

            
            string login = TbLogin.Text.Trim();
            string hashedPass = PasswordHelper.GetHash(CurrentPasswordBox.Password);
            var user = Core.Context.Users
                .FirstOrDefault(u => u.Login == login && u.Password == hashedPass);
            if (user == null)
            {
                MessageHelper.Error("Текущий пароль/Логин неверный!");
                return;
            }

            
            string passwordError = ValidationHelper.CheckPassword(NewPasswordBox.Password);
            if (passwordError != null)
            {
                MessageHelper.Warning(passwordError);
                return;
            }
            if (NewPasswordBox.Password != ConfirmPasswordBox.Password)
            {
                MessageHelper.Warning("Пароли не совпадают!");
                return;
            }
            if (NewPasswordBox.Password == CurrentPasswordBox.Password)
            {
                MessageHelper.Warning("Новый пароль должен отличаться от текущего!");
                return;
            }

            try
            {
                user.Password = PasswordHelper.GetHash(NewPasswordBox.Password);
                Core.Context.SaveChanges();
                MessageHelper.Info("Пароль успешно изменен!");
                if (NavigationService != null && NavigationService.CanGoBack)
                    NavigationService.GoBack();
                else
                    NavigationService?.Navigate(new AuthPage());
            }
            catch (Exception ex)
            {
                Core.RefreshAll();
                MessageHelper.Error("Не удалось изменить пароль.\n" + Core.GetErrorMessage(ex));
            }
        }
    }
}
