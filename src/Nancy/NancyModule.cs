namespace Nancy
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using Nancy.Routing;

    public abstract class NancyModule
    {
        private readonly IDictionary<string, RouteDictionary> routes;

        /// <summary>
        /// Initializes a new instance of the <see cref="NancyModule"/> class.
        /// </summary>
        protected NancyModule() : this(string.Empty)
        {
        }        

        /// <summary>
        /// Initializes a new instance of the <see cref="NancyModule"/> class.
        /// </summary>
        /// <param name="modulePath">A <see cref="string"/> containing the root relative path that all paths in the module will be a subset of.</param>
        protected NancyModule(string modulePath)
        {
            this.ModulePath = modulePath;
            this.routes = new Dictionary<string, RouteDictionary>(StringComparer.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Gets or sets an <see cref="ITemplateEngineSelector"/> which represents the current application context
        /// </summary>
        public ITemplateEngineSelector TemplateEngineSelector { get; set; }

        /// <param name="method">A <see cref="string"/> containing the http request method for which the routes should be returned.</param>
        /// <returns>An <see cref="IDictionary{TKey,TValue}"/> containing the routes.</returns>
        /// <remarks>Valid values are delete, get, post and put. The parameter is not case sensitive.</remarks>
        public RouteDictionary GetRoutes(string method)
        {
            if (method.Equals("HEAD", StringComparison.OrdinalIgnoreCase))
            {
                method = "GET";
            }

            RouteDictionary routesForSpecifiedMethod;

            if (!this.routes.TryGetValue(method, out routesForSpecifiedMethod))
            {
                routesForSpecifiedMethod = new RouteDictionary(this, method);
                this.routes[method] = routesForSpecifiedMethod;
            }

            return routesForSpecifiedMethod;
        }

        /// <summary>
        /// Gets <see cref="RouteDictionary"/> for declaring actions for DELETE requests.
        /// </summary>
        /// <value>A <see cref="RouteDictionary"/> instance.</value>
        public RouteDictionary Delete
        {
            get { return this.GetRoutes("DELETE"); }
        }

        /// <summary>
        /// Gets <see cref="RouteDictionary"/> for declaring actions for GET requests.
        /// </summary>
        /// <value>A <see cref="RouteDictionary"/> instance.</value>
        /// <remarks>These actions will also be used when a HEAD request is recieved.</remarks>
        public RouteDictionary Get
        {
            get { return this.GetRoutes("GET"); }
        }

        public string ModulePath { get; private set; }

        /// <summary>
        /// Gets <see cref="RouteDictionary"/> for declaring actions for POST requests.
        /// </summary>
        /// <value>A <see cref="RouteDictionary"/> instance.</value>
        public RouteDictionary Post
        {
            get { return this.GetRoutes("POST"); }
        }

        /// <summary>
        /// Gets <see cref="RouteDictionary"/> for declaring actions for PUT requests.
        /// </summary>
        /// <value>A <see cref="RouteDictionary"/> instance.</value>
        public RouteDictionary Put
        {
            get { return this.GetRoutes("PUT"); }
        }

        /// <summary>
        /// Gets or sets an <see cref="IRequest"/> instance that represents the current request.
        /// </summary>
        /// <value>An <see cref="IRequest"/> instance.</value>
        public IRequest Request { get; set; }

        /// <summary>
        /// An extension point for adding support for formatting response contents.
        /// </summary>
        /// <value>This property will always return <see langword="null" /> because it acts as an extension point.</value>
        /// <remarks>Extension methods to this property should always return <see cref="Response"/> or one of the types that can implicitly be types into a <see cref="Response"/>.</remarks>
        public IResponseFormatter Response { get; private set; }

        /// <summary>
        /// Renders the view based on the extension without a model.
        /// </summary>
        /// <param name="name">The path to the view</param>        
        public Action<Stream> View(string name)
        {
            return View(name, (object) null);
        }

        /// <summary>
        /// Renders the view based on the extension with a model.
        /// </summary>
        /// <param name="name">The path to the view</param>
        /// <param name="model">The model to pass to the view</param>
        public Action<Stream> View<TModel>(string name, TModel model)
        {
            var extension = Path.GetExtension(name);
            var processor = TemplateEngineSelector.GetTemplateProcessor(extension) ?? TemplateEngineSelector.DefaultProcessor;
            return stream =>
                       {
                           var result = processor.RenderView(name, model);
                           result.Execute(stream);
                       };
        }
    }
}