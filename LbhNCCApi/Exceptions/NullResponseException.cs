using System;
using System.Runtime.Serialization;

namespace LbhNCCApi.Exceptions
{
    [Serializable]
    internal class NullResponseException : Exception
    {
        public NullResponseException()
        {
        }

        public NullResponseException(string message) : base(message)
        {
        }

        public NullResponseException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected NullResponseException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}