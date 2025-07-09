using System;
using System.Runtime.Serialization;

namespace DatabaseLayer.Exceptions
{
    [Serializable]
    public class InvalidBatchNumberException : Exception
    {
        public InvalidBatchNumberException()
        {
        }

        public InvalidBatchNumberException(string batchNumber)
          : base("Batch number " + batchNumber + " has invalid characters.")
        {
        }

        public InvalidBatchNumberException(string message, Exception innerException)
          : base(message, innerException)
        {
        }

        protected InvalidBatchNumberException(SerializationInfo info, StreamingContext context)
          : base(info, context)
        {
        }
    }
}