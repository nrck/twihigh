using System;
using System.Runtime.Serialization;

namespace PheasantTails.TwiHigh.Functions.Tweets
{
    [Serializable]
    internal class TweetException : Exception
    {
        public TweetException() { }

        public TweetException(string message) : base(message) { }

        public TweetException(string message, Exception innerException) : base(message, innerException) { }

        protected TweetException(SerializationInfo info, StreamingContext context) : base(info, context) { }
    }
}
