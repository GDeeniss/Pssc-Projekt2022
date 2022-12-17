using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Example.Data.Models
{
    public class OrderDto
    {
        public int OrderId { get; set; }
        public int PersonId { get; set; }
        public decimal? Exam { get; set; }
        public decimal? Activity { get; set; }
        public decimal? Final { get; set; }
    }
}
