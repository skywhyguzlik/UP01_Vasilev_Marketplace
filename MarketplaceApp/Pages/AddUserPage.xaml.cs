using System;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using MarketplaceApp.Classes;

namespace MarketplaceApp.Pages
{
    
    public partial class AddUserPage : Page
    {
        private Users _currentUser = Users.CreateNew();
        private readonly bool _isNew;

        public AddUserPage(Users selectedUser)
        {
            InitializeComponent();

            if (selectedUser != null)
                _currentUser = selectedUser;
            _isNew = _currentUser.UserId == 0;

            Title = _isNew ? "Добавление пользователя" : "Редактирование пользователя";
            HeaderText.Text = Title;
            if (!_isNew)
            {
                PassLabel.Text = "Новый пароль";
                PassHint.Text = "Оставьте пустым, чтобы не менять";
            }

            cmbRole.ItemsSource = Core.Context.Roles.OrderBy(r => r.RoleId).ToList();
            DPBirthDate.DisplayDateEnd = DateTime.Today.AddYears(-14);
            DataContext = _currentUser;
        }

        private void PasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            HintHelper.UpdatePasswordHint(sender);
        }

        private void ButtonSave_Click(object sender, RoutedEventArgs e)
        {
            var db = Core.Context;
            StringBuilder errors = new StringBuilder();

            _currentUser.Login = (_currentUser.Login ?? "").Trim();
            _currentUser.LastName = (_currentUser.LastName ?? "").Trim();
            _currentUser.FirstName = (_currentUser.FirstName ?? "").Trim();
            _currentUser.MiddleName = (_currentUser.MiddleName ?? "").Trim();
            _currentUser.Email = (_currentUser.Email ?? "").Trim();
            _currentUser.Phone = ValidationHelper.NormalizePhone(_currentUser.Phone);
            TBPhone.Text = _currentUser.Phone;

            if (!ValidationHelper.IsValidLogin(_currentUser.Login))
                errors.AppendLine("Укажите логин: от 3 символов, латинские буквы, цифры, . _ -");
            else if (db.Users.Any(u => u.Login == _currentUser.Login && u.UserId != _currentUser.UserId))
                errors.AppendLine("Пользователь с таким логином уже существует!");

            if (_isNew && string.IsNullOrEmpty(TBPass.Password))
                errors.AppendLine("Укажите пароль!");
            else if (TBPass.Password.Length > 0)
            {
                string passwordError = ValidationHelper.CheckPassword(TBPass.Password);
                if (passwordError != null)
                    errors.AppendLine(passwordError);
            }

            if (_currentUser.RoleId == 0)
                errors.AppendLine("Выберите роль!");

            if (!ValidationHelper.IsOnlyLetters(_currentUser.LastName))
                errors.AppendLine("Укажите фамилию (только буквы)!");
            if (!ValidationHelper.IsOnlyLetters(_currentUser.FirstName))
                errors.AppendLine("Укажите имя (только буквы)!");
            if (_currentUser.MiddleName.Length > 0 && !ValidationHelper.IsOnlyLetters(_currentUser.MiddleName))
                errors.AppendLine("Отчество может содержать только буквы!");

            if (!ValidationHelper.IsValidEmail(_currentUser.Email))
                errors.AppendLine("Укажите e-mail в формате name@mail.ru");
            else if (db.Users.Any(u => u.Email == _currentUser.Email && u.UserId != _currentUser.UserId))
                errors.AppendLine("Пользователь с таким e-mail уже существует!");

            if (!ValidationHelper.IsValidPhone(_currentUser.Phone))
                errors.AppendLine("Укажите телефон в формате +7 (999) 123-45-67");

            int age = ValidationHelper.GetAge(_currentUser.BirthDate);
            if (age < 14 || age > 100)
                errors.AppendLine("Возраст пользователя должен быть от 14 до 100 лет!");

            if (errors.Length > 0)
            {
                MessageHelper.Warning(errors.ToString());
                return;
            }

            if (TBPass.Password.Length > 0)
                _currentUser.Password = PasswordHelper.GetHash(TBPass.Password);

            if (_currentUser.UserId == 0)
                db.Users.Add(_currentUser);

            try
            {
                db.SaveChanges();
                string text = "Данные успешно сохранены!";
                if (_currentUser.RoleId == Session.RoleSeller && !db.Sellers.Any(s => s.UserId == _currentUser.UserId))
                    text += "\n\nНе забудьте добавить магазин этого продавца в таблице «Продавцы».";
                MessageHelper.Info(text);
                NavigationService?.GoBack();
            }
            catch (Exception ex)
            {
                if (_currentUser.UserId == 0)
                    Core.Detach(_currentUser);
                MessageHelper.Error("Не удалось сохранить данные.\n" + Core.GetErrorMessage(ex));
            }
        }

        private void ButtonClean_Click(object sender, RoutedEventArgs e)
        {
            TBLogin.Text = "";
            TBPass.Clear();
            cmbRole.SelectedIndex = -1;
            TBLastName.Text = "";
            TBFirstName.Text = "";
            TBMiddleName.Text = "";
            TBEmail.Text = "";
            TBPhone.Text = "";
        }
    }
}
