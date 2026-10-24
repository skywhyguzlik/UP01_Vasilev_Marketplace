using System;

namespace MarketplaceApp
{
    
    public partial class Orders
    {
       
        public string DisplayName
        {
            get
            {
                if (OrderId == 0)
                    return "Все заказы";
                string buyer = Users != null ? Users.LastName : "";
                return "№" + OrderId + " от " + OrderDate.ToString("dd.MM.yyyy") + " — " + buyer;
            }
        }

        
        public static Orders CreateNew()
        {
            return new Orders
            {
                StatusId = 1,
                PaymentMethod = "Банковская карта",
                OrderDate = DateTime.Now,
                DeliveryDate = DateTime.Today.AddDays(4),
                Comment = "Без комментария"
            };
        }
    }
}
