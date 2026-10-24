using System.Data.Entity;
using System.Linq;

namespace MarketplaceApp.Classes
{
    
    public static class AuthService
    {
        
        public static Users Authenticate(string login, string password, out string error)
        {
            error = null;
            string hashedPassword = PasswordHelper.GetHash(password);

            var user = Core.Context.Users
                .AsNoTracking()
                .Include(u => u.Roles)
                .FirstOrDefault(u => (u.Login == login || u.Email == login) && u.Password == hashedPassword);

            if (user == null)
            {
                error = "Пользователь с такими данными не найден!";
                return null;
            }
            if (!user.IsActive)
            {
                error = "Учётная запись заблокирована. Обратитесь к администратору.";
                return null;
            }
            return user;
        }

        
        public static int GetSellerId(int userId)
        {
            var seller = Core.Context.Sellers.AsNoTracking().FirstOrDefault(s => s.UserId == userId);
            return seller != null ? seller.SellerId : 0;
        }
    }
}
