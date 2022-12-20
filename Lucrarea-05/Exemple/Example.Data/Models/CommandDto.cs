using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Example.Data.Models
{
    public class CommandDto
    {
        public int CommandId { get; set; }
        public int ProductId{get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string Telephone { get; set; }
        public string Address { get; set; }
        public decimal? Subtotal { get; set; }
        public decimal? Total { get; set; }
    }
}
