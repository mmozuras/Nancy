namespace Nancy.Tests.Fakes
{
    using System;

    using Nancy.Session;

    public class FakeSessionObjectFormatter : ISessionObjectFormatter
    {
        /// <summary>
        /// Serialize an object
        /// </summary>
        /// <param name="sourceObject">Source object</param>
        /// <returns>Serialised object string</returns>
        public string Serialize(object sourceObject)
        {
            return sourceObject.ToString();
        }

        /// <summary>
        /// Deserialize an object string
        /// </summary>
        /// <param name="sourceString">Source object string</param>
        /// <returns>Deserialized object</returns>
        public object Deserialize(string sourceString)
        {
            return sourceString;
        }
    }
}