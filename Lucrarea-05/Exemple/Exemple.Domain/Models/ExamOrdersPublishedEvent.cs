using CSharp.Choices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Exemple.Domain.Models
{
    [AsChoice]
    public static partial class ExamOrdersPublishedEvent
    {
        public interface IExamOrdersPublishedEvent { }

        public record ExamOrdersPublishScucceededEvent : IExamOrdersPublishedEvent 
        {
            public string Csv{ get;}
            public DateTime PublishedDate { get; }

            internal ExamOrdersPublishScucceededEvent(string csv, DateTime publishedDate)
            {
                Csv = csv;
                PublishedDate = publishedDate;
            }
        }

        public record ExamOrdersPublishFaildEvent : IExamOrdersPublishedEvent 
        {
            public string Reason { get; }

            internal ExamOrdersPublishFaildEvent(string reason)
            {
                Reason = reason;
            }
        }
    }
}
