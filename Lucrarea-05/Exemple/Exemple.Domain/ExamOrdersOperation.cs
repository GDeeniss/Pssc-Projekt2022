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
            from examOrder in Order.TryParseOrder(unvalidatedOrder.ExamOrder)
                                   .ToEitherAsync($"Invalid exam order ({unvalidatedOrder.PersonRegistrationNumber}, {unvalidatedOrder.ExamOrder})")
            from activityOrder in Order.TryParseOrder(unvalidatedOrder.ActivityOrder)
                                   .ToEitherAsync($"Invalid activity order ({unvalidatedOrder.PersonRegistrationNumber}, {unvalidatedOrder.ActivityOrder})")
            from personRegistrationNumber in PersonRegistrationNumber.TryParse(unvalidatedOrder.PersonRegistrationNumber)
                                   .ToEitherAsync($"Invalid person registration number ({unvalidatedOrder.PersonRegistrationNumber})")
            from personExists in checkPersonExists(personRegistrationNumber)
                                   .ToEitherAsync($"Person {personRegistrationNumber.Value} does not exist.")
            select new ValidatedPersonOrder(personRegistrationNumber, examOrder, activityOrder);

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
            new CalculatedPersonOrder(validOrder.PersonRegistrationNumber,
                                      validOrder.ExamOrder,
                                      validOrder.ActivityOrder,
                                      validOrder.ExamOrder + validOrder.ActivityOrder);

        public static IExamOrders MergeOrders(IExamOrders examOrders, IEnumerable<CalculatedPersonOrder> existingOrders) => examOrders.Match(
            whenUnvalidatedExamOrders: unvalidaTedExam => unvalidaTedExam,
            whenInvalidExamOrders: invalidExam => invalidExam,
            whenFailedExamOrders: failedExam => failedExam,
            whenValidatedExamOrders: validatedExam => validatedExam,
            whenPublishedExamOrders: publishedExam => publishedExam,
            whenCalculatedExamOrders: calculatedExam => MergeOrders(calculatedExam.OrderList, existingOrders));

        private static CalculatedExamOrders MergeOrders(IEnumerable<CalculatedPersonOrder> newList, IEnumerable<CalculatedPersonOrder> existingList)
        {
            var updatedAndNewOrders = newList.Select(order => order with { OrderId = existingList.FirstOrDefault(g => g.PersonRegistrationNumber == order.PersonRegistrationNumber)?.OrderId ?? 0, IsUpdated = true });
            var oldOrders = existingList.Where(order => !newList.Any(g => g.PersonRegistrationNumber == order.PersonRegistrationNumber));
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
            export.AppendLine($"{order.PersonRegistrationNumber.Value}, {order.ExamOrder}, {order.ActivityOrder}, {order.FinalOrder}");
    }
}
