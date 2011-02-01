namespace Nancy.Tests.Unit
{
    using FakeItEasy;
    using Fakes;
    using Nancy.ViewEngines;
    using Xunit;

    public class NancyModuleFixture
    {
        [Fact]
        public void Should_execute_the_default_processor_unregistered_extension()
        {
            var application = A.Fake<ITemplateEngineSelector>();
            var viewEngine = A.Fake<IViewEngine>();
            var module = new FakeNancyModuleWithoutBasePath {TemplateEngineSelector = application};

            A.CallTo(() => application.GetTemplateProcessor(".txt")).Returns(null);
            A.CallTo(() => application.DefaultProcessor).Returns(viewEngine);

            module.View("file.txt");

            A.CallTo(() => application.DefaultProcessor).MustHaveHappened();
        }

        [Fact]
        public void Should_execute_the_processor_associated_with_the_extension()
        {
            var application = A.Fake<ITemplateEngineSelector>();
            var module = new FakeNancyModuleWithoutBasePath { TemplateEngineSelector = application };
            var viewEngine = new FakeViewEngine();

            A.CallTo(() => application.GetTemplateProcessor(".razor")).Returns(viewEngine);

            module.View("file2.razor");

            A.CallTo(() => application.GetTemplateProcessor(".razor")).MustHaveHappened();
            A.CallTo(() => application.DefaultProcessor).MustNotHaveHappened();
        }
    }
}