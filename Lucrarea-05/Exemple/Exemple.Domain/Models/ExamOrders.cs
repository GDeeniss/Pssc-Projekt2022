using CSharp.Choices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Exemple.Domain.Models
{
    [AsChoice]
    public static partial class ExamOrders
    {
        public interface IExamOrders { }

        public record UnvalidatedExamOrders: IExamOrders
        {
            public UnvalidatedExamOrders(IReadOnlyCollection<UnvalidatedPersonOrder> orderList)
            {
                OrderList = orderList;
            }

            public IReadOnlyCollection<UnvalidatedPersonOrder> OrderList { get; }
        }

        public record InvalidExamOrders: IExamOrders
        {
            internal InvalidExamOrders(IReadOnlyCollection<UnvalidatedPersonOrder> orderList, string reason)
            {
                OrderList = orderList;
                Reason = reason;
            }

            public IReadOnlyCollection<UnvalidatedPersonOrder> OrderList { get; }
            public string Reason { get; }
        }

        public record FailedExamOrders : IExamOrders
        {
            internal FailedExamOrders(IReadOnlyCollection<UnvalidatedPersonOrder> orderList, Exception exception)
            {
                OrderList = orderList;
                Exception = exception;
            }

            public IReadOnlyCollection<UnvalidatedPersonOrder> OrderList { get; }
            public Exception Exception { get; }
        }

        public record ValidatedExamOrders: IExamOrders
        {
            internal ValidatedExamOrders(IReadOnlyCollection<ValidatedPersonOrder> ordersList)
            {
                OrderList = ordersList;
            }

            public IReadOnlyCollection<ValidatedPersonOrder> OrderList { get; }
        }

        public record CalculatedExamOrders : IExamOrders
        {
            internal CalculatedExamOrders(IReadOnlyCollection<CalculatedPersonOrder> ordersList)
            {
                OrderList = ordersList;
            }

            public IReadOnlyCollection<CalculatedPersonOrder> OrderList { get; }
        }

        public record PublishedExamOrders : IExamOrders
        {
            internal PublishedExamOrders(IReadOnlyCollection<CalculatedPersonOrder> ordersList, string csv, DateTime publishedDate)
            {
                OrderList = ordersList;
                PublishedDate = publishedDate;
                Csv = csv;
            }

            public IReadOnlyCollection<CalculatedPersonOrder> OrderList { get; }
            public DateTime PublishedDate { get; }
            public string Csv { get; }
        }
    }
}
