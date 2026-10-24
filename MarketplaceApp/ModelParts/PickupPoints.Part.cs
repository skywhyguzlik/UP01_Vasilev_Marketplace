namespace MarketplaceApp
{
    
    public partial class PickupPoints
    {
       
        public string FullAddress
        {
            get { return City + ", " + Address; }
        }

        
        public static PickupPoints CreateNew()
        {
            return new PickupPoints
            {
                City = "",
                Address = "",
                WorkHours = "09:00–21:00",
                Phone = ""
            };
        }
    }
}
