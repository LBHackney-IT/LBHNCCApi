using System;
using System.Runtime.Serialization;

namespace LbhNCCApi.Controllers
{
    [Serializable]
    internal class NCCInteractionServiceException : Exception
    {
        public NCCInteractionServiceException()
        {
        }

        public NCCInteractionServiceException(string message) : base(message)
        {
        }

        public NCCInteractionServiceException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected NCCInteractionServiceException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}