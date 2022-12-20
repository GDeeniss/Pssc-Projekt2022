using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Exemple.Domain.Models.ExamOrders;

namespace Exemple.Domain.Models
{
    public record PublishOrdersCommand
    {
        public PublishOrdersCommand(IReadOnlyCollection<UnvalidatedPersonOrder> inputExamOrders)
        {
            InputExamOrders = inputExamOrders;
        }

        public IReadOnlyCollection<UnvalidatedPersonOrder> InputExamOrders { get; }
    }
}
