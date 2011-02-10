﻿namespace Nancy.ViewEngines.Spark
{
    using System;
    using System.Collections.Generic;
    using System.Configuration;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Threading;
    using System.Web;
    using System.Web.Hosting;
    using global::Spark;
    using global::Spark.Compiler;
    using global::Spark.FileSystem;
    using Nancy.ViewEngines.Spark.Caching;
    using Nancy.ViewEngines.Spark.Descriptors;

    public class ViewFactory : ISparkServiceInitialize, IViewEngine
    {
        private readonly Dictionary<BuildDescriptorParams, ISparkViewEntry> cache = new Dictionary<BuildDescriptorParams, ISparkViewEntry>();
        private readonly ViewEngineResult cacheMissResult = new ViewEngineResult(new List<string>());
        
        private ICacheServiceProvider cacheServiceProvider;
        private IDescriptorBuilder descriptorBuilder;
        private ISparkViewEngine engine;

        /// <summary>
        /// Initializes a new instance of the <see cref="ViewFactory"/> class.
        /// </summary>
        public ViewFactory()
            : this(null)
        {
        }

        public ViewFactory(ISparkSettings settings)
        {
            this.Settings = settings ?? (ISparkSettings)ConfigurationManager.GetSection("spark") ?? new SparkSettings();
        }

        public ISparkSettings Settings { get; set; }

        public ISparkViewEngine Engine
        {
            get
            {
                if (engine == null)
                {
                    this.SetEngine(new SparkViewEngine(Settings));
                }

                return engine;
            }
            set { this.SetEngine(value); }
        }

        public IViewActivatorFactory ViewActivatorFactory
        {
            get { return this.Engine.ViewActivatorFactory; }
            set { this.Engine.ViewActivatorFactory = value; }
        }

        public IViewFolder ViewFolder
        {
            get { return this.Engine.ViewFolder; }
            set { this.Engine.ViewFolder = value; }
        }

        public IDescriptorBuilder DescriptorBuilder
        {
            get
            {
                return this.descriptorBuilder ??
                       Interlocked.CompareExchange(ref descriptorBuilder, new DefaultDescriptorBuilder(Engine), null) ??
                       descriptorBuilder;
            }
            set { this.descriptorBuilder = value; }
        }

        public ICacheServiceProvider CacheServiceProvider
        {
            get
            {
                return this.cacheServiceProvider ??
                       Interlocked.CompareExchange(ref cacheServiceProvider, new DefaultCacheServiceProvider(), null) ??
                       cacheServiceProvider;
            }
            set { this.cacheServiceProvider = value; }
        }

        void ISparkServiceInitialize.Initialize(ISparkServiceContainer container)
        {
            this.Initialize(container);
        }

        public virtual void Initialize(ISparkServiceContainer container)
        {
            this.Settings = container.GetService<ISparkSettings>();
            this.Engine = container.GetService<ISparkViewEngine>();
            this.DescriptorBuilder = container.GetService<IDescriptorBuilder>();
            this.CacheServiceProvider = container.GetService<ICacheServiceProvider>();
        }

        public void SetEngine(ISparkViewEngine viewEngine)
        {
            descriptorBuilder = null;
            this.engine = viewEngine;

            if (this.engine != null)
            {
                this.engine.DefaultPageBaseType = typeof(SparkView).FullName;
            }
        }

        public virtual ViewEngineResult FindView(ActionContext actionContext, string viewName, string masterName)
        {
            return this.FindViewInternal(actionContext, viewName, masterName, true, false);
        }

        public virtual ViewEngineResult FindView(ActionContext actionContext, string viewName, string masterName, bool useCache)
        {
            return this.FindViewInternal(actionContext, viewName, masterName, true, useCache);
        }

        public virtual ViewEngineResult FindPartialView(ActionContext actionContext, string partialViewName)
        {
            return this.FindViewInternal(actionContext, partialViewName, null /*masterName*/, false, false);
        }

        public virtual ViewEngineResult FindPartialView(ActionContext actionContext, string partialViewName, bool useCache)
        {
            return this.FindViewInternal(actionContext, partialViewName, null /*masterName*/, false, useCache);
        }

        public virtual void ReleaseView(ActionContext actionContext, ISparkView view)
        {
            this.Engine.ReleaseInstance(view);
        }

        private ViewEngineResult FindViewInternal(ActionContext actionContext, string viewName, string masterName, bool findDefaultMaster, bool useCache)
        {
            var searchedLocations = new List<string>();
            var viewPath = actionContext.ViewPath;

            var descriptorParams = new BuildDescriptorParams(
                viewPath,
                viewName,
                masterName,
                findDefaultMaster,
                DescriptorBuilder.GetExtraParameters(actionContext));

            ISparkViewEntry entry;
            if (useCache)
            {
                if (TryGetCacheValue(descriptorParams, out entry) && entry.IsCurrent())
                {
                    return BuildResult(actionContext.HttpContext, entry);
                }

                return cacheMissResult;
            }

            var descriptor = DescriptorBuilder.BuildDescriptor(
                descriptorParams,
                searchedLocations);

            if (descriptor == null)
            {
                return new ViewEngineResult(searchedLocations);
            }

            entry = Engine.CreateEntry(descriptor);
            this.SetCacheValue(descriptorParams, entry);
            return BuildResult(actionContext.HttpContext, entry);
        }

        private bool TryGetCacheValue(BuildDescriptorParams descriptorParams, out ISparkViewEntry entry)
        {
            lock (this.cache)
            {
                return this.cache.TryGetValue(descriptorParams, out entry);
            }
        }

        private void SetCacheValue(BuildDescriptorParams descriptorParams, ISparkViewEntry entry)
        {
            lock (this.cache)
            {
                this.cache[descriptorParams] = entry;
            }
        }

        private ViewEngineResult BuildResult(HttpContextBase httpContext, ISparkViewEntry entry)
        {
            var view = entry.CreateInstance();
            if (view is SparkView)
            {
                ((SparkView)view).CacheService = this.CacheServiceProvider.GetCacheService(httpContext);
            }

            return new ViewEngineResult(view, this);
        }

        public SparkViewDescriptor CreateDescriptor(
            ActionContext actionContext,
            string viewName,
            string masterName,
            bool findDefaultMaster,
            ICollection<string> searchedLocations)
        {
            var viewPath = actionContext.ViewPath;

            return this.DescriptorBuilder.BuildDescriptor(
                new BuildDescriptorParams(
                    viewPath,
                    viewName,
                    masterName,
                    findDefaultMaster,
                    DescriptorBuilder.GetExtraParameters(actionContext)),
                searchedLocations);
        }

        public SparkViewDescriptor CreateDescriptor(string viewPath, string controllerName, string viewName,
                                                    string masterName, bool findDefaultMaster)
        {
            var searchedLocations = new List<string>();
            var descriptor = this.DescriptorBuilder.BuildDescriptor(
                new BuildDescriptorParams(
                    viewPath,
                    viewName,
                    masterName,
                    findDefaultMaster, null),
                searchedLocations);

            if (descriptor == null)
            {
                throw new CompilerException("Unable to find templates at " + string.Join(", ", searchedLocations.ToArray()));
            }

            return descriptor;
        }


        public Assembly Precompile(SparkBatchDescriptor batch)
        {
            return this.Engine.BatchCompilation(batch.OutputAssembly, this.CreateDescriptors(batch));
        }

        public List<SparkViewDescriptor> CreateDescriptors(SparkBatchDescriptor batch)
        {
            var descriptors = new List<SparkViewDescriptor>();
            foreach (var entry in batch.Entries)
            {
                descriptors.AddRange(this.CreateDescriptors(entry));
            }

            return descriptors;
        }

        public IList<SparkViewDescriptor> CreateDescriptors(SparkBatchEntry entry)
        {
            var descriptors = new List<SparkViewDescriptor>();

            var controllerName = RemoveSuffix(entry.ControllerType.Name, "Controller");

            var viewNames = new List<string>();
            var includeViews = entry.IncludeViews;

            if (includeViews.Count == 0)
            {
                includeViews = new[] { "*" };
            }

            foreach (var include in includeViews)
            {
                if (include.EndsWith("*"))
                {
                    foreach (var fileName in ViewFolder.ListViews(controllerName))
                    {
                        if (!string.Equals(Path.GetExtension(fileName), ".spark", StringComparison.InvariantCultureIgnoreCase))
                            continue;

                        var potentialMatch = Path.GetFileNameWithoutExtension(fileName);

                        if (!TestMatch(potentialMatch, include))
                        {
                            continue;
                        }

                        var isExcluded = false;
                        foreach (var exclude in entry.ExcludeViews)
                        {
                            if (!TestMatch(potentialMatch, RemoveSuffix(exclude, ".spark")))
                                continue;

                            isExcluded = true;
                            break;
                        }

                        if (!isExcluded)
                        {
                            viewNames.Add(potentialMatch);
                        }
                    }
                }
                else
                {
                    // explicitly included views don't test for exclusion
                    viewNames.Add(RemoveSuffix(include, ".spark"));
                }
            }

            foreach (var viewName in viewNames)
            {
                if (entry.LayoutNames.Count == 0)
                {
                    descriptors.Add(CreateDescriptor(
                        entry.ControllerType.Namespace,
                        controllerName,
                        viewName,
                        null /*masterName*/,
                        true));
                }
                else
                {
                    descriptors.AddRange(entry.LayoutNames.Select(masterName => CreateDescriptor(entry.ControllerType.Namespace, controllerName, viewName, string.Join(" ", masterName.ToArray()), false)));
                }
            }

            return descriptors;
        }

        private static bool TestMatch(string potentialMatch, string pattern)
        {
            if (!pattern.EndsWith("*"))
            {
                return string.Equals(potentialMatch, pattern, StringComparison.InvariantCultureIgnoreCase);
            }

            // raw wildcard matches anything that's not a partial
            if (pattern == "*")
            {
                return !potentialMatch.StartsWith("_");
            }

            // otherwise the only thing that's supported is "starts with"
            return potentialMatch.StartsWith(pattern.Substring(0, pattern.Length - 1), StringComparison.InvariantCultureIgnoreCase);
        }

        private static string RemoveSuffix(string value, string suffix)
        {
            if (value.EndsWith(suffix, StringComparison.InvariantCultureIgnoreCase))
            {
                return value.Substring(0, value.Length - suffix.Length);
            }

            return value;
        }

        public class ViewEngineResult
        {
            public ViewEngineResult(ISparkView view, ViewFactory factory)
            {
                View = view;
                Factory = factory;
            }

            public ViewEngineResult(List<string> searchedLocations)
            {
                var locations = string.Empty;
                searchedLocations.ForEach(loc => locations += string.Format("{0} ", loc));

                if (!string.IsNullOrEmpty(locations))
                {
                    throw new CompilerException(string.Format("The view could not be in any of the following locations: {0}", locations));
                }
            }

            public ISparkView View { get; set; }

            public ViewFactory Factory { get; set; }
        }

        public ViewResult RenderView<TModel>(string path, TModel model)
        {
            var viewName = path.Substring(path.LastIndexOf('/') + 1).Replace(".spark", string.Empty);
            var viewPath = path.Substring(0, path.LastIndexOf('/'));
            var targetNamespace = string.Empty; //TODO Rob G: This can be used to support things like areas or features
            ViewFolder = new FileSystemViewFolder(HostingEnvironment.MapPath(viewPath));
            HttpContextBase httpContext = null; //TODO Rob G: figure out how to get httpcontext passed in so that we can support view and partial caching.
            var actionContext = new ActionContext(httpContext, targetNamespace);
            var result = FindView(actionContext, viewName, null);
            var viewWithModel = result.View as SparkView<TModel>;

            if (viewWithModel != null)
            {
                viewWithModel.SetModel(model);
            }

            return new ViewResult(result.View as SparkView, HostingEnvironment.MapPath(path));
        }
    }
}