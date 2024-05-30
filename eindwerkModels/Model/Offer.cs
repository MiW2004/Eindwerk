using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace eindwerkModels.Model
{
    public class Offer
    {

        public Offer() { }

        public int Id { get; set; }
        public DateTime Date { get; set; }
        public int CustomerId { get; set; }
        public bool Pickup { get; set; }
        public bool GardenLayout { get; set; }

        public int AantalPRperOffer { get; set; }
        public decimal TotalPrice { get; set; }
        public List<OfferProduct> OfferProducts { get; set; }

        public Offer(int id, DateTime date, int customerId, bool pickup, bool gardenLayout, int aantalprperoffer)
        {
            if (id <= 0) throw new DomainException("Offer ID cannot be zero or negative");
            if (customerId <= 0) throw new DomainException("Offer customer ID cannot be zero or negative");
            Id = id;
            Date = date;
            CustomerId = customerId;
            Pickup = pickup;
            GardenLayout = gardenLayout;
            TotalPrice = 0;
            OfferProducts = new List<OfferProduct>();
            AantalPRperOffer = aantalprperoffer;
        }
    }
}
