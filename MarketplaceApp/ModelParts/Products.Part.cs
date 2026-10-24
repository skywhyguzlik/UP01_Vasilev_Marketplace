using System;

namespace MarketplaceApp
{
    
    public partial class Products
    {
        
        public decimal FinalPrice
        {
            get { return Math.Round(Price * (100 - DiscountPercent) / 100m, 2); }
        }

        
        public bool HasDiscount
        {
            get { return DiscountPercent > 0; }
        }

        
        public bool InStock
        {
            get { return StockQuantity > 0; }
        }

        
        public static Products CreateNew()
        {
            return new Products
            {
                ImagePath = "images/no_photo.png",
                ModerationStatus = "На модерации",
                CreatedAt = DateTime.Now,
                IsActive = true
            };
        }
    }
}
