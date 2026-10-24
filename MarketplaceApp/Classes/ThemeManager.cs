using System;
using System.IO;
using System.Windows;

namespace MarketplaceApp.Classes
{
    
    public static class ThemeManager
    {
        public const string BlueTheme = "Dictionary.xaml";        
        public const string GreenTheme = "GreenDictionary.xaml";  

        public static string CurrentTheme { get; private set; } = BlueTheme;

        private static string SettingsFile
        {
            get
            {
                string folder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "MarketplaceApp");
                Directory.CreateDirectory(folder);
                return Path.Combine(folder, "theme.txt");
            }
        }

       
        public static void ApplyTheme(string themeFile)
        {
            
            var uri = new Uri(themeFile, UriKind.Relative);
            
            ResourceDictionary resourceDict = Application.LoadComponent(uri) as ResourceDictionary;
            
            Application.Current.Resources.Clear();
            
            Application.Current.Resources.MergedDictionaries.Add(resourceDict);
            CurrentTheme = themeFile;
        }

        
        public static void ToggleTheme()
        {
            ApplyTheme(CurrentTheme == BlueTheme ? GreenTheme : BlueTheme);
            SaveTheme();
        }

        public static void SaveTheme()
        {
            try
            {
                File.WriteAllText(SettingsFile, CurrentTheme);
            }
            catch
            {
                
            }
        }

        
        public static void LoadSavedTheme()
        {
            string theme = BlueTheme;
            try
            {
                if (File.Exists(SettingsFile))
                {
                    string saved = File.ReadAllText(SettingsFile).Trim();
                    if (saved == GreenTheme || saved == BlueTheme)
                        theme = saved;
                }
            }
            catch
            {
                theme = BlueTheme;
            }
            ApplyTheme(theme);
        }
    }
}
