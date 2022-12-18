using LanguageExt;
using System;
using static LanguageExt.Prelude;

namespace Exemple.Domain.Models
{
    public record Order
    {
        public string Value { get; }

        internal Order(string value)
        {
            if (value != "")
            {
                Value = value;
            }
            else
            {
                throw new InvalidOrderException($"{value:0.##} is an invalid order value.");
            }
        }

        //public static Order operator +(Order a, Order b) => new Order((a.Value + b.Value) / 2m);


        // public Order Round()
        // {
        //     var roundedValue = Math.Round(Value);
        //     return new Order(roundedValue);
        // }

        // public override string ToString()
        // {
        //     return $"{Value:0.##}";
        // }

        public static Option<Order> TryParseOrder(string orderString)
        {
            return Some<Order>(new(orderString));
        }

        private static bool IsValid(decimal numericOrder) => numericOrder > 0 && numericOrder <= 10;
    }
}
