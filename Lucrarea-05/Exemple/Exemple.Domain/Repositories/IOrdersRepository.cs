using Exemple.Domain.Models;
using LanguageExt;
using System.Collections.Generic;
using static Exemple.Domain.Models.ExamOrders;

namespace Exemple.Domain.Repositories
{
    public interface IOrdersRepository
    {
        TryAsync<List<CalculatedPersonOrder>> TryGetExistingOrders();

        TryAsync<Unit> TrySaveOrders(PublishedExamOrders orders);
    }
}
