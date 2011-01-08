namespace Nancy.ViewEngines.NHaml
{
    using System;
    using System.IO;
    using System.Text;
    using global::NHaml.TemplateResolution;

    public class ViewSource : IViewSource
    {
        private readonly IViewLocationResult viewLocationResult;
        private readonly DateTime viewSourceInitialized;

        public ViewSource(IViewLocationResult viewLocationResult)
        {
            this.viewLocationResult = viewLocationResult;
            this.viewSourceInitialized = viewLocationResult.LastModified;
        }

        #region IViewSource Members

        public StreamReader GetStreamReader()
        {
            using (var reader = viewLocationResult.Contents)
            {
                string text = reader.ReadToEnd();
                byte[] bytes = Encoding.UTF8.GetBytes(text);

                var memoryStream = new MemoryStream(bytes);
                return new StreamReader(memoryStream);
            }
        }

        public string Path
        {
            get { return viewLocationResult.Location; }
        }

        public bool IsModified
        {
            get { return viewSourceInitialized < viewLocationResult.LastModified; }
        }

        #endregion
    }
}