using System;
using System.Configuration;
using System.Data.Entity;
using System.Linq;

namespace MarketplaceApp.Classes
{
    
    public static class Core
    {
        
        public static MarketplaceContext Context = new MarketplaceContext();

      
        public static void RefreshAll()
        {
            foreach (var entry in Context.ChangeTracker.Entries().ToList())
            {
                if (entry.State.ToString() == "Added")
                {
                    Detach(entry.Entity);                 
                    continue;
                }
                try
                {
                    entry.Reload();                       
                }
                catch (Exception)
                {
                    Detach(entry.Entity);                 
                }
            }
        }

        
        public static void Detach(object entity)
        {
            var entry = Context.Entry(entity);
            entry.State = (dynamic)Enum.Parse(entry.State.GetType(), "Detached");
        }

        
        public static string GetErrorMessage(Exception ex)
        {
            while (ex.InnerException != null)
                ex = ex.InnerException;
            return ex.Message;
        }
    }

    
    public class MarketplaceContext : DbContext
    {
        public MarketplaceContext() : base(FindConnectionName())
        {
        }

        public DbSet<Roles> Roles { get; set; }
        public DbSet<Users> Users { get; set; }
        public DbSet<Sellers> Sellers { get; set; }
        public DbSet<Categories> Categories { get; set; }
        public DbSet<Products> Products { get; set; }
        public DbSet<PickupPoints> PickupPoints { get; set; }
        public DbSet<OrderStatuses> OrderStatuses { get; set; }
        public DbSet<Orders> Orders { get; set; }
        public DbSet<OrderItems> OrderItems { get; set; }
        public DbSet<Reviews> Reviews { get; set; }

        
        private static string FindConnectionName()
        {
            foreach (ConnectionStringSettings item in ConfigurationManager.ConnectionStrings)
            {
                if (item.ConnectionString != null && item.ConnectionString.Contains("metadata="))
                    return "name=" + item.Name;
            }
            throw new InvalidOperationException(
                "В App.config нет строки подключения модели ADO.NET. Добавьте модель Model1.edmx из базы MarketplaceDB.");
        }
    }
}
