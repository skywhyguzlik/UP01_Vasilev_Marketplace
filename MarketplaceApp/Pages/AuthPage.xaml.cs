using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using MarketplaceApp.Classes;

namespace MarketplaceApp.Pages
{
    
    public partial class AuthPage : Page
    {
        
        private int failedAttempts = 0;

        public AuthPage()
        {
            InitializeComponent();
        }

        private void ButtonEnter_OnClick(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(TextBoxLogin.Text) || string.IsNullOrEmpty(PasswordBox.Password))
            {
                MessageHelper.Warning("Введите логин и пароль!");
                return;
            }

            Users user;
            string error;
            try
            {
                user = AuthService.Authenticate(TextBoxLogin.Text.Trim(), PasswordBox.Password, out error);
            }
            catch (Exception ex)
            {
                MessageHelper.Error("Не удалось подключиться к базе данных.\n" + Core.GetErrorMessage(ex));
                return;
            }

            if (user == null)
            {
                MessageHelper.Error(error);
                failedAttempts++;
                if (failedAttempts >= 3)
                {
                    if (captcha.Visibility != Visibility.Visible)
                        CaptchaSwitch();
                    CaptchaChange();
                }
                else
                {
                    AttemptsText.Text = "Осталось попыток до появления капчи: " + (3 - failedAttempts);
                }
                return;
            }

            failedAttempts = 0;
            int sellerId = user.RoleId == Session.RoleSeller ? AuthService.GetSellerId(user.UserId) : 0;
            if (user.RoleId == Session.RoleSeller && sellerId == 0)
            {
                MessageHelper.Warning("Для этого продавца ещё не создан магазин. Обратитесь к администратору.");
                return;
            }

            Session.Login(user, sellerId);
            MessageHelper.Info("Добро пожаловать, " + user.FirstName + "!\nВаша роль: " + user.Roles.RoleName);
            OpenHomePage();
        }

        
        private void OpenHomePage()
        {
            Session.ClearHistory = true;   
            switch (Session.RoleId)
            {
                case Session.RoleAdmin:
                case Session.RoleManager:
                    NavigationService?.Navigate(new AdminPage());
                    break;
                case Session.RoleSeller:
                    NavigationService?.Navigate(new ProductsTabPage());
                    break;
                default:
                    NavigationService?.Navigate(new UserPage());
                    break;
            }
        }

        
        public void CaptchaSwitch()
        {
            if (captcha.Visibility == Visibility.Visible)
            {
                TextBoxLogin.Clear();
                PasswordBox.Clear();

                captcha.Visibility = Visibility.Collapsed;
                captchaInput.Visibility = Visibility.Collapsed;
                labelCaptcha.Visibility = Visibility.Collapsed;
                submitCaptcha.Visibility = Visibility.Collapsed;
                submitCaptcha.IsDefault = false;

                labelLogin.Visibility = Visibility.Visible;
                labelPass.Visibility = Visibility.Visible;
                TextBoxLogin.Visibility = Visibility.Visible;
                PassGrid.Visibility = Visibility.Visible;
                ButtonChangePassword.Visibility = Visibility.Visible;
                ButtonEnter.Visibility = Visibility.Visible;
                ButtonEnter.IsDefault = true;
                ButtonReg.Visibility = Visibility.Visible;
                AttemptsText.Text = "";
            }
            else
            {
                captcha.Visibility = Visibility.Visible;
                captchaInput.Visibility = Visibility.Visible;
                labelCaptcha.Visibility = Visibility.Visible;
                submitCaptcha.Visibility = Visibility.Visible;
                submitCaptcha.IsDefault = true;

                labelLogin.Visibility = Visibility.Collapsed;
                labelPass.Visibility = Visibility.Collapsed;
                TextBoxLogin.Visibility = Visibility.Collapsed;
                PassGrid.Visibility = Visibility.Collapsed;
                ButtonChangePassword.Visibility = Visibility.Collapsed;
                ButtonEnter.Visibility = Visibility.Collapsed;
                ButtonEnter.IsDefault = false;
                ButtonReg.Visibility = Visibility.Collapsed;
                AttemptsText.Text = "Три неверные попытки входа. Подтвердите, что вы не робот.";
            }
        }

        
        public void CaptchaChange()
        {
            captcha.Text = CaptchaHelper.Generate();
            captchaInput.Clear();
        }

        private void submitCaptcha_Click(object sender, RoutedEventArgs e)
        {
            if (!CaptchaHelper.Check(captchaInput.Text, captcha.Text))
            {
                MessageHelper.Error("Неверно введена капча");
                CaptchaChange();
            }
            else
            {
                MessageHelper.Info("Капча введена успешно, можете продолжить авторизацию");
                CaptchaSwitch();
                failedAttempts = 0;
            }
        }

        
        private void textBox_PreviewExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            if (e.Command == ApplicationCommands.Copy ||
                e.Command == ApplicationCommands.Cut ||
                e.Command == ApplicationCommands.Paste)
            {
                e.Handled = true;
            }
        }

        private void PasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            HintHelper.UpdatePasswordHint(sender);
        }

        private void ButtonReg_Click(object sender, RoutedEventArgs e)
        {
            NavigationService?.Navigate(new RegPage());
        }

        private void ButtonChangePassword_Click(object sender, RoutedEventArgs e)
        {
            NavigationService?.Navigate(new ChangePassPage());
        }
    }
}
