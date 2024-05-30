using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using eindwerk;

namespace eindwerkModels.Model
{
    public class Customer
    {
        public Customer() { }


        public int Id { get; set; }
        public string Name { get; set; }
        public string Address { get; set; }
        public List<Offer> Offers { get; set; }

        public Customer(int id, string name, string address)
        {
            if (id < 0) throw new DomainException("Customer ID cannot be lower than zero or negative");
            if (string.IsNullOrWhiteSpace(name)) throw new DomainException("Customer name cannot be empty");
            if (string.IsNullOrWhiteSpace(address)) throw new DomainException("Customer address cannot be empty");
            Id = id;
            Name = name;
            Address = address;
            Offers = new List<Offer>();
        }

    }

}