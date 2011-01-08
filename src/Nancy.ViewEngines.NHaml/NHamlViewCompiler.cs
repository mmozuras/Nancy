namespace Nancy.ViewEngines.NHaml
{
    using System.Collections.Generic;
    using global::NHaml;
    using global::NHaml.TemplateResolution;

    public class NHamlViewCompiler : IViewCompiler
    {
        private readonly IDictionary<string, ViewSource> viewSources;
        private readonly TemplateEngine templateEngine;

        public NHamlViewCompiler()
        {
            this.viewSources = new Dictionary<string, ViewSource>();
            this.templateEngine = new TemplateEngine(new TemplateOptions {AutoRecompile = true});
        }

        public IView GetCompiledView<TModel>(IViewLocationResult viewLocationResult)
        {
            string location = viewLocationResult.Location;

            ViewSource viewSource;
            if (viewSources.ContainsKey(location))
            {
                viewSource = viewSources[location];
            }
            else
            {
                viewSource = new ViewSource(viewLocationResult);
                viewSources.Add(location, viewSource);
            }

            var compiledTemplate = templateEngine.Compile(new List<IViewSource> {viewSource}, typeof (NHamlView<TModel>));
            return (NHamlView<TModel>)compiledTemplate.CreateInstance();
        }
    }
}