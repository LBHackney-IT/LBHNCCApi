using System;
using System.Runtime.Serialization;

namespace LbhNCCApi.Exceptions
{
    [Serializable]
    internal class NCCServiceException : Exception
    {
        public NCCServiceException()
        {
        }

        public NCCServiceException(string message) : base(message)
        {
        }

        public NCCServiceException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected NCCServiceException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}