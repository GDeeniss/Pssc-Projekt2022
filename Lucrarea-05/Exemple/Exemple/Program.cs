using Exemple.Domain.Models;
using System;
using System.Collections.Generic;
using static Exemple.Domain.Models.ExamOrders;
using static Exemple.Domain.ExamOrdersOperation;
using Exemple.Domain;
using System.Threading.Tasks;
using LanguageExt;
using static LanguageExt.Prelude;
using System.Net.Http;
using Example.Data.Repositories;
using Example.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Data.SqlClient;

namespace Exemple
{
    class Program
    {
        static async Task Main(string[] args)
        {
            SqlConnectionStringBuilder builder = new SqlConnectionStringBuilder();

            builder.DataSource = "tcp:pssc2022.database.windows.net"; 
            builder.UserID = "denis";            
            builder.Password = "Pssc2022@";     
            builder.InitialCatalog = "Students";

            try 
            {
                using (SqlConnection connection = new SqlConnection(builder.ConnectionString))
                {                    
                    connection.Open();

                    // var name = ReadValue("Name: ");
                    // var telephone = ReadValue("Telephone: ");
                    // var email = ReadValue("Email: ");

                    // var inserting = new SqlCommand("INSERT INTO dbo.People (Name, Email, Telephone) VALUES (@name, @email, @telephone);", connection);

                    // inserting.Parameters.AddWithValue("@name", name);
                    // inserting.Parameters.AddWithValue("@email", email);
                    // inserting.Parameters.AddWithValue("@telephone", telephone);

                    // inserting.ExecuteNonQuery();

                    Console.WriteLine("Available products:");
                    String sql = "SELECT * FROM dbo.Products";

                    using (SqlCommand commanding = new SqlCommand(sql, connection))
                    {
                        using (SqlDataReader reader = commanding.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                Console.WriteLine("Product name: {0}, available units: {1}, product price: {2}", reader.GetString(1), reader.GetDecimal(2), reader.GetDecimal(3));
                                Console.WriteLine("----------");
                            }
                        }
                    }                    
                }
            }

            catch (SqlException e)
            {
                Console.WriteLine(e.ToString());
            }
            
            using ILoggerFactory loggerFactory = ConfigureLoggerFactory();
            ILogger<PublishOrderWorkflow> logger = loggerFactory.CreateLogger<PublishOrderWorkflow>();

            var listOfOrders = ReadListOfOrders().ToArray();
            PublishOrdersCommand command = new(listOfOrders);
            var dbContextBuilder = new DbContextOptionsBuilder<OrdersContext>().UseSqlServer(builder.ConnectionString).UseLoggerFactory(loggerFactory);
            OrdersContext ordersContext = new OrdersContext(dbContextBuilder.Options);
            PersonsRepository personsRepository = new(ordersContext);
            OrdersRepository ordersRepository = new(ordersContext);
            PublishOrderWorkflow workflow = new(personsRepository, ordersRepository, logger);
            var result = await workflow.ExecuteAsync(command);

            result.Match(
                    whenExamOrdersPublishFaildEvent: @event =>
                    {
                        Console.WriteLine($"Publish failed: {@event.Reason}");
                        return @event;
                    },
                    whenExamOrdersPublishScucceededEvent: @event =>
                    {
                        Console.WriteLine($"Publish succeeded.");
                        Console.WriteLine(@event.Csv);
                        return @event;
                    }
                );
        }

        private static ILoggerFactory ConfigureLoggerFactory()
        {
            return LoggerFactory.Create(builder =>
                                builder.AddSimpleConsole(options =>
                                {
                                    options.IncludeScopes = true;
                                    options.SingleLine = true;
                                    options.TimestampFormat = "hh:mm:ss ";
                                })
                                .AddProvider(new Microsoft.Extensions.Logging.Debug.DebugLoggerProvider()));
        }

        private static List<UnvalidatedPersonOrder> ReadListOfOrders()
        {
            List<UnvalidatedPersonOrder> listOfOrders = new();
            do
            {
                //read registration number and order and create a list of greads
                var name = ReadValue("Please enter your data to complete the purchase:\nName: ");
                if (string.IsNullOrEmpty(name))
                {
                    break;
                }

                var email = ReadValue("Email: ");
                if (string.IsNullOrEmpty(email))
                {
                    break;
                }

                var telephone = ReadValue("Telephone number: ");
                if (string.IsNullOrEmpty(telephone))
                {
                    break;
                }

                var address = ReadValue("Billing address: ");
                if (string.IsNullOrEmpty(address))
                {
                    break;
                }

                listOfOrders.Add(new(name, email, telephone, address));
            } while (true);
            return listOfOrders;
        }

        private static string? ReadValue(string prompt)
        {
            Console.Write(prompt);
            return Console.ReadLine();
        }

    }
}
