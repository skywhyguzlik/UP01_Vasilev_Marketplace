using System;

namespace MarketplaceApp
{
    
    public partial class Sellers
    {
        
        public static Sellers CreateNew()
        {
            return new Sellers
            {
                CommissionPercent = 10,
                Rating = 0,
                RegistrationDate = DateTime.Today
            };
        }
    }
}
