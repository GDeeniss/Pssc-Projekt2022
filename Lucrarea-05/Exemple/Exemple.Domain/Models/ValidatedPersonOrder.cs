using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Exemple.Domain.Models
{
    public record ValidatedPersonOrder(PersonRegistrationNumber Name, Order Email, Order Telephone, Order Address);
}
