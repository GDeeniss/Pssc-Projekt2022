using Exemple.Domain.Models;
using Exemple.Domain.Repositories;
using LanguageExt;
using System.Linq;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace Example.Data.Repositories
{
    public class PersonsRepository: IPersonsRepository
    {
        private readonly OrdersContext ordersContext;

        public PersonsRepository(OrdersContext ordersContext)
        {
            this.ordersContext = ordersContext;  
        }

        public TryAsync<List<PersonRegistrationNumber>> TryGetExistingPersons(IEnumerable<string> personsToCheck) => async () =>
        {
            var persons = await ordersContext.Persons
                                              .Where(person => personsToCheck.Contains(person.RegistrationNumber))
                                              .AsNoTracking()
                                              .ToListAsync();
            return persons.Select(person => new PersonRegistrationNumber(person.RegistrationNumber))
                           .ToList();
        };
    }
}
