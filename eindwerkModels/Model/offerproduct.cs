using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace eindwerkModels.Model
{
    public class OfferProduct
    {

        public OfferProduct() { }

        public int OfferId { get; set; }
        public int ProductId { get; set; }
        public int Quantity { get; set; }

        public OfferProduct(int offerId, int productId, int quantity)
        {
            if (offerId <= 0) throw new DomainException("Offer product offer ID cannot be zero or negative");
            if (productId <= 0) throw new DomainException("Offer product product ID cannot be zero or negative");
            if (quantity <= 0) throw new DomainException("Offer product quantity cannot be zero or negative");
            OfferId = offerId;
            ProductId = productId;
            Quantity = quantity;
        }
    }
}
