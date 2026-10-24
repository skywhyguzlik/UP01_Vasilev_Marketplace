using System;

namespace MarketplaceApp
{
   
    public partial class Users
    {
        
        public string FIO
        {
            get { return (LastName + " " + FirstName + " " + MiddleName).Trim(); }
        }

        public static Users CreateNew()
        {
            return new Users
            {
                MiddleName = "",
                BirthDate = new DateTime(2000, 1, 1),
                RegistrationDate = DateTime.Now,
                IsActive = true
            };
        }
    }
}
