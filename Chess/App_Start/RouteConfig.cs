using System.Web.Mvc;
using System.Web.Routing;

namespace Chess
{
	public class RouteConfig
	{
		public static void RegisterRoutes(RouteCollection routes)
		{
			routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            routes.MapRoute(
				name: "Default",
				url: "{controller}/{action}/{id}",
				defaults: new { controller = "Home", action = "Index", id = UrlParameter.Optional }
			);

            routes.MapRoute(
				name: "GameHistory",
				url: "History/ShowMove/{gameId}/{moveNumber}",
				defaults: new { controller = "History", action = "ShowMove", gameId = UrlParameter.Optional, moveNumber = UrlParameter.Optional });
		}
	}
}