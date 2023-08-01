using System;
using System.Runtime.Serialization;

namespace PheasantTails.TwiHigh.Functions.Feeds
{
    [Serializable]
    public class FeedException : Exception
    {
        public FeedException() { }

        public FeedException(string message) : base(message) { }

        public FeedException(string message, Exception inner) : base(message, inner) { }

        protected FeedException(SerializationInfo info, StreamingContext context) : base(info, context) { }
    }
}
