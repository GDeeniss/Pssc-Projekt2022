using System;
using System.Runtime.Serialization;

namespace Exemple.Domain.Models
{
    [Serializable]
    internal class InvalidPersonRegistrationNumberException : Exception
    {
        public InvalidPersonRegistrationNumberException()
        {
        }

        public InvalidPersonRegistrationNumberException(string? message) : base(message)
        {
        }

        public InvalidPersonRegistrationNumberException(string? message, Exception? innerException) : base(message, innerException)
        {
        }

        protected InvalidPersonRegistrationNumberException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}