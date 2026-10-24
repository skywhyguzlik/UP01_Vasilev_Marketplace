using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace MarketplaceApp.Classes
{
    
    public static class HintHelper
    {
        public static void UpdatePasswordHint(object sender)
        {
            var passwordBox = sender as PasswordBox;
            var grid = passwordBox != null ? passwordBox.Parent as Grid : null;
            if (grid == null)
                return;

            var hint = grid.Children.OfType<TextBlock>().FirstOrDefault();
            if (hint != null)
                hint.Visibility = passwordBox.Password.Length > 0 ? Visibility.Collapsed : Visibility.Visible;
        }
    }
}
