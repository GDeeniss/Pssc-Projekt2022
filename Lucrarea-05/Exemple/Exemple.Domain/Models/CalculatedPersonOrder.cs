using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Exemple.Domain.Models
{
    public record CalculatedPersonOrder(PersonRegistrationNumber Name, Order Email, Order Telephone, Order Address, Order FinalOrder)
    {
        public int OrderId { get; set; }
        public bool IsUpdated { get; set; } 
    }
}
