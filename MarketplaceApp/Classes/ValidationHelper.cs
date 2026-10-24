using System;
using System.Text.RegularExpressions;

namespace MarketplaceApp.Classes
{
    
    public static class ValidationHelper
    {
        
        public static string CheckPassword(string password)
        {
            if (password.Length < 6)
                return "Пароль слишком короткий, должно быть минимум 6 символов!";

            bool en = true;
            bool number = false;
            for (int i = 0; i < password.Length; i++)
            {
                if (password[i] >= '0' && password[i] <= '9')
                    number = true;
                else if (!((password[i] >= 'A' && password[i] <= 'Z') || (password[i] >= 'a' && password[i] <= 'z')))
                    en = false;
            }

            if (!en)
                return "Используйте только английскую раскладку (латинские буквы и цифры)!";
            if (!number)
                return "Добавьте в пароль хотя бы одну цифру!";
            return null;
        }

        public static bool IsValidLogin(string login)
        {
            return Regex.IsMatch(login ?? "", @"^[a-zA-Z0-9_.\-]{3,50}$");
        }

        public static bool IsValidEmail(string email)
        {
            return Regex.IsMatch(email ?? "", @"^[a-zA-Z0-9._\-]+@[a-zA-Z0-9.\-]+\.[a-zA-Z]{2,}$");
        }

        
        public static bool IsValidPhone(string phone)
        {
            return Regex.IsMatch(phone ?? "", @"^\+7 \(\d{3,5}\) \d{1,3}-\d{2}-\d{2}$");
        }

        
        public static bool IsOnlyLetters(string text)
        {
            return Regex.IsMatch(text ?? "", @"^[a-zA-Zа-яА-ЯёЁ\- ]+$");
        }

        
        public static bool IsValidInn(string inn)
        {
            return Regex.IsMatch(inn ?? "", @"^(\d{10}|\d{12})$");
        }

        
        public static bool IsValidWorkHours(string text)
        {
            return Regex.IsMatch(text ?? "", @"^([01]\d|2[0-3]):[0-5]\d[–-]([01]\d|2[0-3]):[0-5]\d$");
        }

        
        public static string NormalizePhone(string phone)
        {
            string digits = Regex.Replace(phone ?? "", @"\D", "");
            if (digits.Length == 11 && (digits[0] == '7' || digits[0] == '8'))
                digits = digits.Substring(1);
            if (digits.Length != 10)
                return (phone ?? "").Trim();
            return "+7 (" + digits.Substring(0, 3) + ") " + digits.Substring(3, 3) + "-" + digits.Substring(6, 2) + "-" + digits.Substring(8, 2);
        }

        
        public static int GetAge(DateTime birthDate)
        {
            DateTime today = DateTime.Today;
            int age = today.Year - birthDate.Year;
            if (birthDate.Date > today.AddYears(-age))
                age--;
            return age;
        }
    }
}
