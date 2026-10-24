namespace MarketplaceApp.Classes
{
    
    public static class Session
    {
        public const int RoleAdmin = 1;
        public const int RoleManager = 2;
        public const int RoleSeller = 3;
        public const int RoleBuyer = 4;

        public static Users CurrentUser { get; private set; }

       
        public static int SellerId { get; private set; }

        
        public static bool ClearHistory { get; set; }

        public static int RoleId
        {
            get { return CurrentUser != null ? CurrentUser.RoleId : 0; }
        }

        public static void Login(Users user, int sellerId)
        {
            CurrentUser = user;
            SellerId = sellerId;
        }

        public static void Logout()
        {
            CurrentUser = null;
            SellerId = 0;
        }
    }
}
