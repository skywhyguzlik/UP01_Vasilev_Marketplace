using System.Globalization;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace MarketplaceApp.Classes
{
   
    public static class InputMask
    {
        public static readonly DependencyProperty ModeProperty =
            DependencyProperty.RegisterAttached("Mode", typeof(string), typeof(InputMask),
                new PropertyMetadata(null, OnModeChanged));

        public static string GetMode(DependencyObject obj)
        {
            return (string)obj.GetValue(ModeProperty);
        }

        public static void SetMode(DependencyObject obj, string value)
        {
            obj.SetValue(ModeProperty, value);
        }

        private static void OnModeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var textBox = d as TextBox;
            if (textBox == null)
                return;

            textBox.PreviewTextInput -= TextBox_PreviewTextInput;
            textBox.PreviewKeyDown -= TextBox_PreviewKeyDown;
            DataObject.RemovePastingHandler(textBox, TextBox_Pasting);

            textBox.PreviewTextInput += TextBox_PreviewTextInput;
            textBox.PreviewKeyDown += TextBox_PreviewKeyDown;
            DataObject.AddPastingHandler(textBox, TextBox_Pasting);
        }

        
        public static bool IsAllowed(string mode, string text)
        {
            string separator = Regex.Escape(CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator);
            switch (mode)
            {
                case "Digits": return Regex.IsMatch(text, @"^[0-9]*$");
                case "Decimal": return Regex.IsMatch(text, @"^[0-9" + separator + "]*$");
                case "Phone": return Regex.IsMatch(text, @"^[0-9+()\- ]*$");
                case "Letters": return Regex.IsMatch(text, @"^[a-zA-Zа-яА-ЯёЁ\- ]*$");
                case "Latin": return Regex.IsMatch(text, @"^[a-zA-Z0-9_.\-]*$");
                default: return true;
            }
        }

        private static void TextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !IsAllowed(GetMode((DependencyObject)sender), e.Text);
        }

        
        private static void TextBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            string mode = GetMode((DependencyObject)sender);
            if (e.Key == Key.Space && (mode == "Digits" || mode == "Decimal" || mode == "Latin"))
                e.Handled = true;
        }

        
        private static void TextBox_Pasting(object sender, DataObjectPastingEventArgs e)
        {
            if (e.DataObject.GetDataPresent(typeof(string)))
            {
                string text = (string)e.DataObject.GetData(typeof(string));
                if (!IsAllowed(GetMode((DependencyObject)sender), text))
                    e.CancelCommand();
            }
            else
            {
                e.CancelCommand();
            }
        }
    }
}
