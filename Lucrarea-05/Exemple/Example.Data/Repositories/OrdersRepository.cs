using Exemple.Domain.Models;
using Exemple.Domain.Repositories;
using LanguageExt;
using Example.Data.Models;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using static Exemple.Domain.Models.ExamOrders;
using static LanguageExt.Prelude;

namespace Example.Data.Repositories
{
    public class OrdersRepository : IOrdersRepository
    {
        private readonly OrdersContext dbContext;

        public OrdersRepository(OrdersContext dbContext)
        {
            this.dbContext = dbContext;
        }

        public TryAsync<List<CalculatedPersonOrder>> TryGetExistingOrders() => async () => (await (
                          from g in dbContext.Command
                          join s in dbContext.Products on g.ProductId equals s.ProductId
                          select new { s.ProductName, g.CommandId, g.Name, g.Email, g.Telephone, g.Address, g.Subtotal, g.Total })
                          .AsNoTracking()
                          .ToListAsync())
                          .Select(result => new CalculatedPersonOrder(
                                                    Name: new(result.Name),
                                                    Email: new(result.Email),
                                                    Telephone: new(result.Telephone),
                                                    Address: new(result.Address),
                                                    FinalOrder: new(result.Total.ToString()))
                          {
                            
                          })
                          .ToList();

        public TryAsync<Unit> TrySaveOrders(PublishedExamOrders orders) => async () =>
        {
            var persons = (await dbContext.Command.ToListAsync()).ToLookup(person=>person.Name);
            var newOrders = orders.OrderList
                                    .Where(g => g.IsUpdated && g.OrderId == 0)
                                    .Select(g => new CommandDto()
                                    {
                                        CommandId = persons[g.OrderId.ToString()].Single().CommandId,
                                        Email = g.Email.Value,
                                        Telephone = g.Telephone.Value,
                                        Address = g.Address.Value,
                                    });
            var updatedOrders = orders.OrderList.Where(g => g.IsUpdated && g.OrderId > 0)
                                    .Select(g => new ProductDto()
                                    {
                                        ProductId = g.OrderId,
                                        ProductName = persons[g.Name.Value].Single().ProductId.ToString(),
                                        Price = decimal.Parse(g.Telephone.Value.ToString()),
                                        Stock = int.Parse(g.OrderId.ToString()),
                                        //Stock = int.Parse(g.FinalOrder.ToString()),
                                    });

            dbContext.AddRange(newOrders);
            foreach (var entity in updatedOrders)
            {
                dbContext.Entry(entity).State = EntityState.Modified;
            }

            await dbContext.SaveChangesAsync();

            return unit;
        };
    }
}
