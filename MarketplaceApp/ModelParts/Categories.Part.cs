namespace MarketplaceApp
{
    
    public partial class Categories
    {
        
        public static Categories CreateNew()
        {
            return new Categories
            {
                CategoryName = "",
                Description = ""
            };
        }
    }
}
