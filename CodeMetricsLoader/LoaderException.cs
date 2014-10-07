using System;
using System.Runtime.Serialization;

namespace CodeMetricsLoader
{
    [Serializable]
    public class LoaderException : Exception, ISerializable
    {
        const string errorMessagePrefix = "Could not load metrics.";
        public LoaderException()
            : base(errorMessagePrefix) {}

        public LoaderException(string message) : 
            this(message, null){}

        public LoaderException(string message, Exception inner) :
            base(string.Format("{0} {1}", errorMessagePrefix, message), inner) { }
    }
}
