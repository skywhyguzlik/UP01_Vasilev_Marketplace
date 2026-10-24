namespace MarketplaceApp
{
    
    public partial class OrderItems
    {
       
        public decimal LineTotal
        {
            get { return Quantity * UnitPrice; }
        }

        
        public static OrderItems CreateNew()
        {
            return new OrderItems
            {
                Quantity = 1
            };
        }
    }
}
