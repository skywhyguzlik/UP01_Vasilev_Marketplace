using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using MarketplaceApp.Classes;

namespace MarketplaceApp.Pages
{
    
    public partial class UsersTabPage : Page
    {
        public UsersTabPage()
        {
            InitializeComponent();

            var roles = Core.Context.Roles.OrderBy(r => r.RoleId).ToList();
            roles.Insert(0, new Roles { RoleId = 0, RoleName = "Все роли" });
            RoleBox.ItemsSource = roles;
            RoleBox.SelectedIndex = 0;

            IsVisibleChanged += Page_IsVisibleChanged;
            UpdateData();
        }

        private void Page_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (IsVisible)
            {
                Core.RefreshAll();
                UpdateData();
            }
        }

        private void UpdateData()
        {
            if (!IsInitialized)
                return;

            List<Users> all = Core.Context.Users.ToList();
            List<Users> list = all;

            string search = SearchBox.Text.Trim().ToLower();
            if (search.Length > 0)
                list = list.Where(u => u.FIO.ToLower().Contains(search) ||
                                       u.Login.ToLower().Contains(search) ||
                                       u.Email.ToLower().Contains(search)).ToList();

            var role = RoleBox.SelectedItem as Roles;
            if (role != null && role.RoleId != 0)
                list = list.Where(u => u.RoleId == role.RoleId).ToList();

            switch (SortBox.SelectedIndex)
            {
                case 1: list = list.OrderByDescending(u => u.FIO).ToList(); break;
                case 2: list = list.OrderByDescending(u => u.RegistrationDate).ToList(); break;
                case 3: list = list.OrderBy(u => u.RegistrationDate).ToList(); break;
                default: list = list.OrderBy(u => u.FIO).ToList(); break;
            }

            DataGridUser.ItemsSource = list;
            CountText.Text = "Записей: " + list.Count + " из " + all.Count;
        }

        private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            UpdateData();
        }

        private void Filter_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateData();
        }

        private void ClearFiltersButton_Click(object sender, RoutedEventArgs e)
        {
            SearchBox.Text = "";
            RoleBox.SelectedIndex = 0;
            SortBox.SelectedIndex = 0;
        }

        private void ButtonAdd_Click(object sender, RoutedEventArgs e)
        {
            NavigationService?.Navigate(new AddUserPage(null));
        }

        private void ButtonEdit_Click(object sender, RoutedEventArgs e)
        {
            NavigationService?.Navigate(new AddUserPage((sender as Button).DataContext as Users));
        }

        
        private void ButtonDel_Click(object sender, RoutedEventArgs e)
        {
            var usersForRemoving = DataGridUser.SelectedItems.Cast<Users>().ToList();
            if (usersForRemoving.Count == 0)
            {
                MessageHelper.Warning("Выделите записи, которые нужно удалить.");
                return;
            }
            if (usersForRemoving.Any(u => u.UserId == Session.CurrentUser.UserId))
            {
                MessageHelper.Warning("Нельзя удалить свою учётную запись.");
                return;
            }
            if (!MessageHelper.Confirm("Вы точно хотите удалить записи в количестве " + usersForRemoving.Count + " элементов?\n\n" +
                                       "Вместе с пользователями будут удалены их заказы, отзывы, " +
                                       "а у продавцов — магазины и товары."))
                return;

            try
            {
                var db = Core.Context;
                var ids = usersForRemoving.Select(u => u.UserId).ToList();
                var sellers = db.Sellers.Where(s => ids.Contains(s.UserId)).ToList();
                foreach (var entity in sellers)
                    db.Sellers.Remove(entity);
                foreach (var entity in usersForRemoving)
                    db.Users.Remove(entity);
                db.SaveChanges();
                MessageHelper.Info("Данные успешно удалены!");
            }
            catch (Exception ex)
            {
                Core.RefreshAll();
                MessageHelper.Error("Не удалось удалить данные.\n" + Core.GetErrorMessage(ex));
            }
            UpdateData();
        }
    }
}
