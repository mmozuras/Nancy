namespace Nancy.ViewEngines.NHaml.Tests
{
    using System;
    using System.IO;
    using Nancy.Tests;
    using Nancy.Tests.Fakes;
    using NHaml;
    using Xunit;

    public class NHamlViewCompilerFixture
    {
        private readonly NHamlViewCompiler compiler;

        public NHamlViewCompilerFixture()
        {
            compiler = new NHamlViewCompiler();
        }

        [Fact]
        public void GetCompiledView_should_render_to_stream()
        {
            // Given            
            var viewLocationResult = new FakeViewLocationResult("- var x = \"test\"\n%h1= \"Hello Mr. \" + @x");

            var view = compiler.GetCompiledView<object>(viewLocationResult);
            view.Writer = new StringWriter();

            // When
            view.Execute();

            // Then
            view.Writer.ToString().ShouldMatch(s => s.Contains("Hello Mr. test"));
        }

        [Fact]
        public void GetCompiledView_should_render_the_cached_one()
        {
            // Given
            DateTime now = DateTime.Now;
            var viewLocationResult = new FakeViewLocationResult("%h1 cached", now);
            compiler.GetCompiledView<object>(viewLocationResult);

            viewLocationResult.ChangeContents("%h1 new", now);

            var view = compiler.GetCompiledView<object>(viewLocationResult);
            view.Writer = new StringWriter();

            // When
            view.Execute();

            // Then
            view.Writer.ToString().ShouldEqual("<h1>cached</h1>" + Environment.NewLine);
        }

        [Fact]
        public void GetCompiledView_should_render_the_new_one()
        {
            // Given
            var viewLocationResult = new FakeViewLocationResult("%h1 cached", DateTime.MinValue);
            compiler.GetCompiledView<object>(viewLocationResult);

            viewLocationResult.ChangeContents("%h1 new", DateTime.Now);

            var view = compiler.GetCompiledView<object>(viewLocationResult);
            view.Writer = new StringWriter();

            // When
            view.Execute();

            // Then
            view.Writer.ToString().ShouldEqual("<h1>new</h1>" + Environment.NewLine);
        }
    }
}
