using Exemple.Domain.Models;
using static Exemple.Domain.Models.ExamOrdersPublishedEvent;
using static Exemple.Domain.ExamOrdersOperation;
using System;
using static Exemple.Domain.Models.ExamOrders;
using LanguageExt;
using System.Threading.Tasks;
using System.Collections.Generic;
using Exemple.Domain.Repositories;
using System.Linq;
using static LanguageExt.Prelude;
using Microsoft.Extensions.Logging;

namespace Exemple.Domain
{
    public class PublishOrderWorkflow
    {
        private readonly IPersonsRepository personsRepository;
        private readonly IOrdersRepository ordersRepository;
        private readonly ILogger<PublishOrderWorkflow> logger;

        public PublishOrderWorkflow(IPersonsRepository personsRepository, IOrdersRepository ordersRepository, ILogger<PublishOrderWorkflow> logger)
        {
            this.personsRepository = personsRepository;
            this.ordersRepository = ordersRepository;
            this.logger = logger;
        }

        public async Task<IExamOrdersPublishedEvent> ExecuteAsync(PublishOrdersCommand command)
        {
            UnvalidatedExamOrders unvalidatedOrders = new UnvalidatedExamOrders(command.InputExamOrders);

            var result = from persons in personsRepository.TryGetExistingPersons(unvalidatedOrders.OrderList.Select(order => order.Name))
                                          .ToEither(ex => new FailedExamOrders(unvalidatedOrders.OrderList, ex) as IExamOrders)
                         from existingOrders in ordersRepository.TryGetExistingOrders()
                                          .ToEither(ex => new FailedExamOrders(unvalidatedOrders.OrderList, ex) as IExamOrders)
                         let checkPersonExists = (Func<PersonRegistrationNumber, Option<PersonRegistrationNumber>>)(person => CheckPersonExists(persons, person))
                         from publishedOrders in ExecuteWorkflowAsync(unvalidatedOrders, existingOrders, checkPersonExists).ToAsync()
                         from _ in ordersRepository.TrySaveOrders(publishedOrders)
                                          .ToEither(ex => new FailedExamOrders(unvalidatedOrders.OrderList, ex) as IExamOrders)
                         select publishedOrders;

            return await result.Match(
                    Left: examOrders => GenerateFailedEvent(examOrders) as IExamOrdersPublishedEvent,
                    Right: publishedOrders => new ExamOrdersPublishScucceededEvent(publishedOrders.Csv, publishedOrders.PublishedDate)
                );
        }

        private async Task<Either<IExamOrders, PublishedExamOrders>> ExecuteWorkflowAsync(UnvalidatedExamOrders unvalidatedOrders, 
                                                                                          IEnumerable<CalculatedPersonOrder> existingOrders, 
                                                                                          Func<PersonRegistrationNumber, Option<PersonRegistrationNumber>> checkPersonExists)
        {
            
            IExamOrders orders = await ValidateExamOrders(checkPersonExists, unvalidatedOrders);
            orders = CalculateFinalOrders(orders);
            orders = MergeOrders(orders, existingOrders);
            orders = PublishExamOrders(orders);

            return orders.Match<Either<IExamOrders, PublishedExamOrders>>(
                whenUnvalidatedExamOrders: unvalidatedOrders => Left(unvalidatedOrders as IExamOrders),
                whenCalculatedExamOrders: calculatedOrders => Left(calculatedOrders as IExamOrders),
                whenInvalidExamOrders: invalidOrders => Left(invalidOrders as IExamOrders),
                whenFailedExamOrders: failedOrders => Left(failedOrders as IExamOrders),
                whenValidatedExamOrders: validatedOrders => Left(validatedOrders as IExamOrders),
                whenPublishedExamOrders: publishedOrders => Right(publishedOrders)
            );
        }

        private Option<PersonRegistrationNumber> CheckPersonExists(IEnumerable<PersonRegistrationNumber> persons, PersonRegistrationNumber personRegistrationNumber)
        {
            if(persons.Any(s=>s == personRegistrationNumber))
            {
                return Some(personRegistrationNumber);
            }
            else
            {
                return None;
            }
        }

        private ExamOrdersPublishFaildEvent GenerateFailedEvent(IExamOrders examOrders) =>
            examOrders.Match<ExamOrdersPublishFaildEvent>(
                whenUnvalidatedExamOrders: unvalidatedExamOrders => new($"Invalid state {nameof(UnvalidatedExamOrders)}"),
                whenInvalidExamOrders: invalidExamOrders => new(invalidExamOrders.Reason),
                whenValidatedExamOrders: validatedExamOrders => new($"Invalid state {nameof(ValidatedExamOrders)}"),
                whenFailedExamOrders: failedExamOrders =>
                {
                    logger.LogError(failedExamOrders.Exception, failedExamOrders.Exception.Message);
                    return new(failedExamOrders.Exception.Message);
                },
                whenCalculatedExamOrders: calculatedExamOrders => new($"Invalid state {nameof(CalculatedExamOrders)}"),
                whenPublishedExamOrders: publishedExamOrders => new($"Invalid state {nameof(PublishedExamOrders)}"));
    }
}
