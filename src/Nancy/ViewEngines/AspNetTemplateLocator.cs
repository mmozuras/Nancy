namespace Nancy.ViewEngines 
{
    using System.IO;
    using System.Web.Hosting;

    public class AspNetTemplateLocator : IViewLocator
    {
        public IViewLocationResult GetTemplateContents(string viewTemplate)
        {
            var path = HostingEnvironment.MapPath(viewTemplate);
            return new FileViewLocationResult(new FileInfo(path));
        }
    }
}