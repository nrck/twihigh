using System;
using System.Runtime.Serialization;

namespace PheasantTails.TwiHigh.Functions.Timelines
{
    [Serializable]
    public class TimelineException : Exception
    {
        public TimelineException() { }

        public TimelineException(string message) : base(message) { }

        public TimelineException(string message, Exception inner) : base(message, inner) { }

        protected TimelineException(SerializationInfo info, StreamingContext context) : base(info, context) { }
    }
}
