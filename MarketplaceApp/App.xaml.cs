using System.Globalization;
using System.Windows;
using System.Windows.Markup;
using MarketplaceApp.Classes;

namespace MarketplaceApp
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            // Даты и числа в интерфейсе выводятся в формате Windows (для России: 21.05.2026, 1 234,50)
            FrameworkElement.LanguageProperty.OverrideMetadata(typeof(FrameworkElement),
                new FrameworkPropertyMetadata(XmlLanguage.GetLanguage(CultureInfo.CurrentCulture.IetfLanguageTag)));

            base.OnStartup(e);

            // Тема, выбранная при прошлом запуске
            ThemeManager.LoadSavedTheme();
        }
    }
}
