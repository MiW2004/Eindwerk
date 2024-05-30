using eindwerkModels.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace eindwerk.model
{
    public class Product
    {
        public Product()
        {

        }

        public int Id { get; set; }
        public string DutchName { get; set; }
        public string ScientificName { get; set; }
        public decimal Price { get; set; }
        public string Description { get; set; }

        public Product(int id, string dutchName, string scientificName, decimal price, string description)
        {
            if (id <= 0) throw new DomainException("Product ID cannot be zero or negative");
            if (string.IsNullOrWhiteSpace(scientificName)) throw new DomainException("Product scientific name cannot be empty");
            if (price <= 0) throw new DomainException("Product price cannot be zero or negative");
            Id = id;
            DutchName = dutchName;
            ScientificName = scientificName;
            Price = price;
            Description = description;
        }
    }
}