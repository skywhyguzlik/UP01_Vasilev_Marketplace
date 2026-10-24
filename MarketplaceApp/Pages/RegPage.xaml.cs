using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using MarketplaceApp.Classes;

namespace MarketplaceApp.Pages
{
    
    public partial class RegPage : Page
    {
        public RegPage()
        {
            InitializeComponent();

            
            try
            {
                comboBxRole.ItemsSource = Core.Context.Roles
                    .Where(r => r.RoleId != Session.RoleSeller)
                    .OrderByDescending(r => r.RoleId)
                    .ToList();
                comboBxRole.SelectedValue = Session.RoleBuyer;   
            }
            catch (Exception ex)
            {
                MessageHelper.Error("Не удалось подключиться к базе данных.\n" + Core.GetErrorMessage(ex));
            }

            dpBirthDate.DisplayDateEnd = DateTime.Today.AddYears(-14);
            dpBirthDate.DisplayDate = new DateTime(2000, 1, 1);
        }

        private void PasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            HintHelper.UpdatePasswordHint(sender);
        }

        private void regButton_Click(object sender, RoutedEventArgs e)
        {
           
            if (string.IsNullOrWhiteSpace(txtbxLog.Text) ||
                string.IsNullOrEmpty(passBxFrst.Password) ||
                string.IsNullOrEmpty(passBxScnd.Password) ||
                string.IsNullOrWhiteSpace(txtbxLastName.Text) ||
                string.IsNullOrWhiteSpace(txtbxFirstName.Text) ||
                string.IsNullOrWhiteSpace(txtbxEmail.Text) ||
                string.IsNullOrWhiteSpace(txtbxPhone.Text) ||
                dpBirthDate.SelectedDate == null ||
                comboBxRole.SelectedValue == null)
            {
                MessageHelper.Warning("Заполните все обязательные поля!");
                return;
            }

            string login = txtbxLog.Text.Trim();
            string email = txtbxEmail.Text.Trim();
            string phone = ValidationHelper.NormalizePhone(txtbxPhone.Text);
            txtbxPhone.Text = phone;

            
            if (!ValidationHelper.IsValidLogin(login))
            {
                MessageHelper.Warning("Логин должен содержать от 3 символов: латинские буквы, цифры, точку, дефис или подчёркивание.");
                return;
            }

            var db = Core.Context;
            if (db.Users.Any(u => u.Login == login))
            {
                MessageHelper.Warning("Пользователь с таким логином уже существует!");
                return;
            }

            
            string passwordError = ValidationHelper.CheckPassword(passBxFrst.Password);
            if (passwordError != null)
            {
                MessageHelper.Warning(passwordError);
                return;
            }

            
            if (passBxFrst.Password != passBxScnd.Password)
            {
                MessageHelper.Warning("Пароли не совпадают!");
                return;
            }

            
            if (!ValidationHelper.IsOnlyLetters(txtbxLastName.Text.Trim()) ||
                !ValidationHelper.IsOnlyLetters(txtbxFirstName.Text.Trim()) ||
                (txtbxMiddleName.Text.Trim().Length > 0 && !ValidationHelper.IsOnlyLetters(txtbxMiddleName.Text.Trim())))
            {
                MessageHelper.Warning("Фамилия, имя и отчество могут содержать только буквы, пробел и дефис.");
                return;
            }
            if (!ValidationHelper.IsValidEmail(email))
            {
                MessageHelper.Warning("Введите e-mail в формате name@mail.ru");
                return;
            }
            if (db.Users.Any(u => u.Email == email))
            {
                MessageHelper.Warning("Пользователь с таким e-mail уже существует!");
                return;
            }
            if (!ValidationHelper.IsValidPhone(phone))
            {
                MessageHelper.Warning("Введите телефон в формате +7 (999) 123-45-67");
                return;
            }
            int age = ValidationHelper.GetAge(dpBirthDate.SelectedDate.Value);
            if (age < 14 || age > 100)
            {
                MessageHelper.Warning("Возраст пользователя должен быть от 14 до 100 лет.");
                return;
            }

            
            var userObject = new Users
            {
                RoleId = (int)comboBxRole.SelectedValue,
                Login = login,
                Password = PasswordHelper.GetHash(passBxFrst.Password),
                LastName = txtbxLastName.Text.Trim(),
                FirstName = txtbxFirstName.Text.Trim(),
                MiddleName = txtbxMiddleName.Text.Trim(),
                Email = email,
                Phone = phone,
                BirthDate = dpBirthDate.SelectedDate.Value,
                RegistrationDate = DateTime.Now,
                IsActive = true
            };

            try
            {
                db.Users.Add(userObject);
                db.SaveChanges();
                MessageHelper.Info("Пользователь успешно зарегистрирован!\nТеперь вы можете войти в систему.");
                if (NavigationService != null && NavigationService.CanGoBack)
                    NavigationService.GoBack();
            }
            catch (Exception ex)
            {
                Core.RefreshAll();
                MessageHelper.Error("Не удалось сохранить пользователя.\n" + Core.GetErrorMessage(ex));
            }
        }
    }
}
