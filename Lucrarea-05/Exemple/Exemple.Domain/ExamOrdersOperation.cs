using Exemple.Domain.Models;
using static LanguageExt.Prelude;
using LanguageExt;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using static Exemple.Domain.Models.ExamOrders;
using System.Threading.Tasks;

namespace Exemple.Domain
{
    public static class ExamOrdersOperation
    {
        public static Task<IExamOrders> ValidateExamOrders(Func<PersonRegistrationNumber, Option<PersonRegistrationNumber>> checkPersonExists, UnvalidatedExamOrders examOrders) =>
            examOrders.OrderList
                      .Select(ValidatePersonOrder(checkPersonExists))
                      .Aggregate(CreateEmptyValatedOrdersList().ToAsync(), ReduceValidOrders)
                      .MatchAsync(
                            Right: validatedOrders => new ValidatedExamOrders(validatedOrders),
                            LeftAsync: errorMessage => Task.FromResult((IExamOrders)new InvalidExamOrders(examOrders.OrderList, errorMessage))
                      );

        private static Func<UnvalidatedPersonOrder, EitherAsync<string, ValidatedPersonOrder>> ValidatePersonOrder(Func<PersonRegistrationNumber, Option<PersonRegistrationNumber>> checkPersonExists) =>
            unvalidatedPersonOrder => ValidatePersonOrder(checkPersonExists, unvalidatedPersonOrder);

        private static EitherAsync<string, ValidatedPersonOrder> ValidatePersonOrder(Func<PersonRegistrationNumber, Option<PersonRegistrationNumber>> checkPersonExists, UnvalidatedPersonOrder unvalidatedOrder)=>
            from email in Order.TryParseOrder(unvalidatedOrder.Email)
                                   .ToEitherAsync($"Invalid email ({unvalidatedOrder.Name}, {unvalidatedOrder.Email})")
            from telephone in Order.TryParseOrder(unvalidatedOrder.Telephone)
                                   .ToEitherAsync($"Invalid telephone ({unvalidatedOrder.Name}, {unvalidatedOrder.Telephone})")
            from name in PersonRegistrationNumber.TryParse(unvalidatedOrder.Name)
                                   .ToEitherAsync($"Invalid person name ({unvalidatedOrder.Name})")
            from address in Order.TryParseOrder(unvalidatedOrder.Address)
                                   .ToEitherAsync($"Invalid address ({unvalidatedOrder.Name}, {unvalidatedOrder.Address})")
            from personExists in checkPersonExists(name)
                                   .ToEitherAsync($"Person {name.Value} does not exist.")
            select new ValidatedPersonOrder(name, email, telephone, address);

        private static Either<string, List<ValidatedPersonOrder>> CreateEmptyValatedOrdersList() =>
            Right(new List<ValidatedPersonOrder>());

        private static EitherAsync<string, List<ValidatedPersonOrder>> ReduceValidOrders(EitherAsync<string, List<ValidatedPersonOrder>> acc, EitherAsync<string, ValidatedPersonOrder> next) =>
            from list in acc
            from nextOrder in next
            select list.AppendValidOrder(nextOrder);

        private static List<ValidatedPersonOrder> AppendValidOrder(this List<ValidatedPersonOrder> list, ValidatedPersonOrder validOrder)
        {
            list.Add(validOrder);
            return list;
        }

        public static IExamOrders CalculateFinalOrders(IExamOrders examOrders) => examOrders.Match(
            whenUnvalidatedExamOrders: unvalidaTedExam => unvalidaTedExam,
            whenInvalidExamOrders: invalidExam => invalidExam,
            whenFailedExamOrders: failedExam => failedExam, 
            whenCalculatedExamOrders: calculatedExam => calculatedExam,
            whenPublishedExamOrders: publishedExam => publishedExam,
            whenValidatedExamOrders: CalculateFinalOrder
        );

        private static IExamOrders CalculateFinalOrder(ValidatedExamOrders validExamOrders) =>
            new CalculatedExamOrders(validExamOrders.OrderList
                                                    .Select(CalculatePersonFinalOrder)
                                                    .ToList()
                                                    .AsReadOnly());

        private static CalculatedPersonOrder CalculatePersonFinalOrder(ValidatedPersonOrder validOrder) => 
            new CalculatedPersonOrder(validOrder.Name,
                                      validOrder.Email,
                                      validOrder.Telephone,
                                      validOrder.Address,
                                      validOrder.Address);

        public static IExamOrders MergeOrders(IExamOrders examOrders, IEnumerable<CalculatedPersonOrder> existingOrders) => examOrders.Match(
            whenUnvalidatedExamOrders: unvalidaTedExam => unvalidaTedExam,
            whenInvalidExamOrders: invalidExam => invalidExam,
            whenFailedExamOrders: failedExam => failedExam,
            whenValidatedExamOrders: validatedExam => validatedExam,
            whenPublishedExamOrders: publishedExam => publishedExam,
            whenCalculatedExamOrders: calculatedExam => MergeOrders(calculatedExam.OrderList, existingOrders));

        private static CalculatedExamOrders MergeOrders(IEnumerable<CalculatedPersonOrder> newList, IEnumerable<CalculatedPersonOrder> existingList)
        {
            var updatedAndNewOrders = newList.Select(order => order with { OrderId = existingList.FirstOrDefault(g => g.Name == order.Name)?.OrderId ?? 0, IsUpdated = true });
            var oldOrders = existingList.Where(order => !newList.Any(g => g.Name == order.Name));
            var allOrders = updatedAndNewOrders.Union(oldOrders)
                                               .ToList()
                                               .AsReadOnly();
            return new CalculatedExamOrders(allOrders);
        }

        public static IExamOrders PublishExamOrders(IExamOrders examOrders) => examOrders.Match(
            whenUnvalidatedExamOrders: unvalidaTedExam => unvalidaTedExam,
            whenInvalidExamOrders: invalidExam => invalidExam,
            whenFailedExamOrders: failedExam => failedExam,
            whenValidatedExamOrders: validatedExam => validatedExam,
            whenPublishedExamOrders: publishedExam => publishedExam,
            whenCalculatedExamOrders: GenerateExport);

        private static IExamOrders GenerateExport(CalculatedExamOrders calculatedExam) => 
            new PublishedExamOrders(calculatedExam.OrderList, 
                                    calculatedExam.OrderList.Aggregate(new StringBuilder(), CreateCsvLine).ToString(), 
                                    DateTime.Now);

        private static StringBuilder CreateCsvLine(StringBuilder export, CalculatedPersonOrder order) =>
            export.AppendLine($"{order.Name.Value}, {order.Email}, {order.Telephone}, {order.Address}, {order.FinalOrder}");
    }
}
