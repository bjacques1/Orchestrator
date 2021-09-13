using System;
using System.Runtime.Serialization;

namespace TeamFoundationServerIntegrationPack
{
    [Serializable]
    public class ActivityExecutionException : Exception
    {
        public ActivityExecutionException()
            : base()
        {
        }

        public ActivityExecutionException(string message)
            : base(message)
        {
        }

        public ActivityExecutionException(string message, Exception exception)
            : base(message, exception)
        {
        }

        protected ActivityExecutionException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}
