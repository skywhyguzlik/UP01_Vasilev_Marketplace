using System.Windows;

namespace MarketplaceApp.Classes
{
    
    public static class MessageHelper
    {
        public static void Info(string text)
        {
            MessageBox.Show(text, "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        public static void Warning(string text)
        {
            MessageBox.Show(text, "Внимание", MessageBoxButton.OK, MessageBoxImage.Warning);
        }

        public static void Error(string text)
        {
            MessageBox.Show(text, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        
        public static bool Confirm(string text)
        {
            return MessageBox.Show(text, "Подтверждение", MessageBoxButton.YesNo, MessageBoxImage.Question)
                   == MessageBoxResult.Yes;
        }
    }
}
