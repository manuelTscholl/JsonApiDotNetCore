using System;

namespace JsonApiDotNetCore.Serialization
{
    /// <summary>
    /// The error that is thrown when (de)serialization of a JSON:API body fails.
    /// </summary>
    public class JsonApiSerializationException : Exception
    {
        public string GenericMessage { get; }
        public string SpecificMessage { get; }

        public JsonApiSerializationException(string genericMessage, string specificMessage, Exception innerException = null)
            : base(genericMessage, innerException)
        {
            GenericMessage = genericMessage;
            SpecificMessage = specificMessage;
        }
    }
}
