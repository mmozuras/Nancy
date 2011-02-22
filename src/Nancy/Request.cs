namespace Nancy
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using Nancy.Extensions;

    /// <summary>
    /// Encapsulates HTTP-request information to an Nancy application.
    /// </summary>
    public class Request
    {
        private dynamic form;

        /// <summary>
        /// Initializes a new instance of the <see cref="Request"/> class.
        /// </summary>
        /// <param name="method">The HTTP data transfer method used by the client.</param>
        /// <param name="uri">The absolute path of the requested resource. This shold not not include the scheme, host name, or query portion of the URI.</param>
        /// <param name="protocol">The HTTP protocol that was used by the client.</param>
        public Request(string method, string uri, string protocol)
            : this(method, uri, new Dictionary<string, IEnumerable<string>>(), new MemoryStream(), protocol)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Request"/> class.
        /// </summary>
        /// <param name="method">The HTTP data transfer method used by the client.</param>
        /// <param name="uri">The absolute path of the requested resource. This shold not not include the scheme, host name, or query portion of the URI</param>
        /// <param name="headers">The headers that was passed in by the client.</param>
        /// <param name="body">The <see cref="Stream"/> that represents the incoming HTTP body.</param>
        /// <param name="protocol">The HTTP protocol that was used by the client.</param>
        /// <param name="query">The querystring data that was sent by the client.</param>
        public Request(string method, string uri, IDictionary<string, IEnumerable<string>> headers, Stream body, string protocol, string query = "")
        {
            if (method == null)
                throw new ArgumentNullException("method", "The value of the method parameter cannot be null.");
            
            if (method.Length == 0)
                throw new ArgumentOutOfRangeException("method", method, "The value of the method parameter cannot be empty.");

            if (uri == null)
                throw new ArgumentNullException("uri", "The value of the uri parameter cannot be null.");

            if (uri.Length == 0)
                throw new ArgumentOutOfRangeException("uri", uri, "The value of the uri parameter cannot be empty.");

            if (headers == null)
                throw new ArgumentNullException("headers", "The value of the headers parameter cannot be null.");

            if (body == null)
                throw new ArgumentNullException("body", "The value of the body parameter cannot be null.");

            if (protocol == null)
                throw new ArgumentNullException("protocol", "The value of the protocol parameter cannot be null.");

            if (protocol.Length == 0)
                throw new ArgumentOutOfRangeException("protocol", protocol, "The value of the protocol parameter cannot be empty.");

            this.Body = body;
            this.Headers = new Dictionary<string, IEnumerable<string>>(headers, StringComparer.OrdinalIgnoreCase);
            this.Method = method;
            this.Uri = uri;
            this.Protocol = protocol;
            this.Query = query.AsQueryDictionary();
        }

        /// <summary>
        /// Gets a <see cref="Stream"/> that can be used to read the incoming HTTP body
        /// </summary>
        /// <value>A <see cref="Stream"/> object representing the incoming HTTP body.</value>
        public Stream Body { get; private set; }

        /// <summary>
        /// Gets the form data of the request.
        /// </summary>
        /// <value>A <see cref="DynamicDictionary"/>instance, containing the key/value pairs of form data.</value>
        /// <remarks>Currently Nancy will only parse form data sent using the application/x-www-url-encoded mime-type.</remarks>
        public dynamic Form
        {
            get { return this.form ?? (this.form = this.GetFormData()); }
        }

        /// <summary>
        /// Gets the HTTP headers sent by the client.
        /// </summary>
        /// <value>An <see cref="IDictionary{TKey,TValue}"/> containing the name and values of the headers.</value>
        /// <remarks>The values are stored in an <see cref="IEnumerable{T}"/> of string to be compliant with multi-value headers.</remarks>
        public IDictionary<string, IEnumerable<string>> Headers { get; private set; }

        /// <summary>
        /// Gets or sets the HTTP data transfer method used by the client.
        /// </summary>
        /// <value>The method.</value>
        public string Method { get; private set; }

        /// <summary>
        /// Gets or sets the HTTP protocol used by the client.
        /// </summary>
        /// <value>The protocol.</value>
        public string Protocol { get; private set; }

        /// <summary>
        /// Gets the querystring data of the requested resource.
        /// </summary>
        /// <value>A <see cref="DynamicDictionary"/>instance, containing the key/value pairs of querystring data.</value>
        public dynamic Query { get; private set; }

        /// <summary>
        /// Gets the absolute path of the requested resource. 
        /// </summary>
        /// <value>A <see cref="string"/> containing the absolute path of the requested resource.</value>
        /// <remarks>This does not include the scheme, host name, or query portion of the URI.</remarks>
        public string Uri { get; private set; }
        
        private dynamic GetFormData()
        {
            if (this.Headers.Keys.Any(x => x.Equals("content-type", StringComparison.OrdinalIgnoreCase)))
            {
                var contentType = this.Headers["content-type"].First();
                if (contentType.Equals("application/x-www-form-urlencoded", StringComparison.OrdinalIgnoreCase))
                {
                    var reader = new StreamReader(this.Body);
                    return reader.ReadToEnd().AsQueryDictionary();
                }
            }
            return new DynamicDictionary();
        }
    }
}