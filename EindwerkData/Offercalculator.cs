using eindwerkDL;
using eindwerkModels.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace eindwerk.model
{
    public class OfferCalculator
    {
        public decimal CalculateTotalPrice(Offer offer, GardenCenterDbContext context)
        {
            decimal totalPrice = 0;
            foreach (var offerProduct in offer.OfferProducts)
            {
                var productid = offerProduct.ProductId;
                var product = context.GetProductById(productid);
                totalPrice += product.Price * offerProduct.Quantity;
            }
            return totalPrice;
        }
    }
}

   