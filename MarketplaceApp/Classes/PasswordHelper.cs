using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace MarketplaceApp.Classes
{
    
    public static class PasswordHelper
    {
        
        public static string GetHash(String password)
        {
            using (var hash = SHA1.Create())
            {
                return string.Concat(hash.ComputeHash(Encoding.UTF8.GetBytes(password)).Select(x => x.ToString("X2")));
            }
        }
    }
}
