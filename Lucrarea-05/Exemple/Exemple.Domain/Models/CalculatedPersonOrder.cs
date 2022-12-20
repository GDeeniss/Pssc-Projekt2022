using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Exemple.Domain.Models
{
<<<<<<< HEAD:Lucrarea-05/Exemple/Exemple.Domain/Models/CalculatedSudentGrade.cs
    public record CalculatedSudentGrade(StudentRegistrationNumber Name, Grade Quantity, Grade Subtotal, Grade Total)
    {
        public int CommandId { get; set; }
=======
    public record CalculatedPersonOrder(PersonRegistrationNumber Name, Order Email, Order Telephone, Order Address, Order FinalOrder)
    {
        public int OrderId { get; set; }
>>>>>>> main:Lucrarea-05/Exemple/Exemple.Domain/Models/CalculatedPersonOrder.cs
        public bool IsUpdated { get; set; } 
    }
}
