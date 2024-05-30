using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SqlClient;
using System.Numerics;
using eindwerk;
using eindwerk.model;
using eindwerkDL;

namespace eindwerk
{
    public class Program
    {
        static void Main(string[] args)
        {
            var connectionString = "Data Source=MW\\sqlexpress;Initial Catalog=Product;Integrated Security=True";
            var dbContext = new GardenCenterDbContext(connectionString);
            var calculator = new OfferCalculator();

           

            var offers = dbContext.GetOffers();
            foreach (var offer in offers)
            {
                offer.TotalPrice = calculator.CalculateTotalPrice(offer, dbContext);
                Console.WriteLine($"Offer {offer.Id} for customer {offer.CustomerId} costs {offer.TotalPrice}");
            }
        }
    }
}






