using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace DatabaseLayer.Exceptions
{
    [Serializable]
    public class BatchNumberNotFoundException : Exception
    {
        public BatchNumberNotFoundException()
        {
        }

        public BatchNumberNotFoundException(string message)
          : base(message)
        {
        }

        public BatchNumberNotFoundException(int batchNumber)
          : base(string.Format("Could not find batch number type {0}.", (object)batchNumber))
        {
        }

        public BatchNumberNotFoundException(string message, Exception innerException)
          : base(message, innerException)
        {
        }

        protected BatchNumberNotFoundException(SerializationInfo info, StreamingContext context)
          : base(info, context)
        {
        }
    }
}