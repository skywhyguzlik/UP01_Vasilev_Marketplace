using System;

namespace MarketplaceApp
{
    
    public partial class Reviews
    {
        
        public static Reviews CreateNew()
        {
            return new Reviews
            {
                Rating = 5,
                ReviewDate = DateTime.Now,
                IsPublished = true
            };
        }
    }
}
