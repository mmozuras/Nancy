namespace Nancy.Demo
{
    using Nancy.Demo.Models;
    using Nancy.Formatters;
	using Nancy.Extensions;
    using Nancy.ViewEngines;
    using Nancy.ViewEngines.NDjango;
	//Compiles but does not execute as expected under Mono 2.8
//    using Nancy.ViewEngines.Razor;
    using Nancy.ViewEngines.Spark;
    using Nancy.Routing;
	using System.Linq;

    public class MainModule : NancyModule
    {
        public MainModule(IRouteCacheProvider routeCacheProvider)
        {
			//Compiles but does not execute as expected under Mono 2.8
//            Get["/"] = x => {
//                return View.Razor("~/views/routes.cshtml", routeCacheProvider.GetCache());
//            };
			
			Get["/"] = x => {
				var model = routeCacheProvider.GetCache().ToList();
                return View.Spark("~/views/routes.spark", model);
            };

            // TODO - implement filtering at the RouteDictionary GetRoute level
            Get["/filtered", r => true] = x => {
                return "This is a route with a filter that always returns true.";
            };

            Get["/filtered", r => false] = x => {
                return "This is also a route, but filtered out so should never be hit.";
            };
			
			Get["/redirect"] = x => {
				return Response.AsRedirect("http://www.google.com"); 
			};
			
            Get["/test"] = x => {
                return "Test";
            };

            Get["/static"] = x => {
                return View.Static("~/views/static.htm");
            };
			
			//Compiles but does not execute as expected under Mono 2.8
//            Get["/razor"] = x => {
//                var model = new RatPack { FirstName = "Frank" };
//                return View.Razor("~/views/razor.cshtml", model);
//            };

            Get["/ndjango"] = x => {
                var model = new RatPack { FirstName = "Michael" };
                return View.Django("~/views/ndjango.django", model);
            };

            Get["/spark"] = x => {
                var model = new RatPack { FirstName = "Bright" };
                return View.Spark("~/views/spark.spark", model);
            };

            Get["/json"] = x => {
                var model = new RatPack { FirstName = "Andy" };
                return Response.AsJson(model);
            };

            Get["/xml"] = x => {
                var model = new RatPack { FirstName = "Andy" };
                return Response.AsXml(model);
            };
			
			//Call the following url to test
			//http://127.0.0.1:8080/access?oauth_token=11111111111111&oauth_verifier=2222222222222222
			//Dynamic cast is for Mono 2.8 only - Fixed in Mono 2.10 Preview
			Get["/access"] = x => {
				try{
					return "Success: " + ((dynamic)Request.Query).oauth_token + "; " + ((dynamic)Request.Query).oauth_verifier;
				}
				catch {
					return "Call as: /access?oauth_token=11111111111111&oauth_verifier=2222222222222222";
				}
			};
        }
    }
}