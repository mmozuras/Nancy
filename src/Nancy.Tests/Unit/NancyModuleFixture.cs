namespace Nancy.Tests.Unit
{
    using System;
    using System.IO;
    using FakeItEasy;
    using Fakes;
    using Xunit;

    public class NancyModuleFixture
    {
        [Fact]
        public void Should_execute_the_default_processor_unregistered_extension()
        {
            var application = A.Fake<ITemplateEngineSelector>();
            var module = new FakeNancyModuleWithoutBasePath {TemplateEngineSelector = application};
            var action = new Action<Stream>(s => { });
            var processor = new Func<string, object, Action<Stream>>((a, b) => action);

            A.CallTo(() => application.GetTemplateProcessor<object>(".txt")).Returns(null);
            A.CallTo(() => application.DefaultProcessor<object>()).Returns(processor);

            module.View("file.txt").ShouldBeSameAs(action);
        }

        [Fact]
        public void Should_execute_the_processor_associated_with_the_extension()
        {
            var application = A.Fake<ITemplateEngineSelector>();
            var module = new FakeNancyModuleWithoutBasePath { TemplateEngineSelector = application };
            var action = new Action<Stream>((s) => { });
            var processor = new Func<string, object, Action<Stream>>((a, b) => action);

            A.CallTo(() => application.GetTemplateProcessor<object>(".razor")).Returns(processor);            

            module.View("file2.razor").ShouldBeSameAs(action);
        }
    }
}