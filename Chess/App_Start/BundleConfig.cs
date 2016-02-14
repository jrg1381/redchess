using System.Web.Optimization;

namespace Chess
{
	public class BundleConfig
	{
		// For more information on Bundling, visit http://go.microsoft.com/fwlink/?LinkId=254725
		public static void RegisterBundles(BundleCollection bundles)
		{
			bundles.Add(new ScriptBundle("~/bundles/jquerykobs").Include(
                        "~/Scripts/knockout-{version}.js",
                        "~/Scripts/bootstrap.js",
                        "~/Scripts/jquery-{version}.js"));

			bundles.Add(new ScriptBundle("~/bundles/jqueryui").Include(
						"~/Scripts/jquery-ui-{version}.js"));

			bundles.Add(new ScriptBundle("~/bundles/jqueryval").Include(
						"~/Scripts/jquery.unobtrusive*",
						"~/Scripts/jquery.validate*",
                        "~/Scripts/jquery.cookie.js"));

			bundles.Add(new ScriptBundle("~/bundles/ajaxlogin").Include(
				"~/Scripts/app/ajaxlogin.js"));

            bundles.Add(new ScriptBundle("~/bundles/appscripts").Include(
                "~/Scripts/spin.min.js",
                "~/Scripts/app/spinner.js",
                "~/Scripts/app/chessboard-0.3.0.js",
                "~/Scripts/app/clock.js",
                "~/Scripts/app/chess.js"));

            bundles.Add(new ScriptBundle("~/bundles/timeago").Include("~/Scripts/jquery.timeago.js"));

            bundles.Add(new ScriptBundle("~/bundles/stats").Include(
                "~/Scripts/d3/d3.min.js",
                "~/Scripts/app/stats.js"));

			bundles.Add(new StyleBundle("~/Content/css").Include(
				"~/Content/Site.css",
				"~/Scripts/app/chessboard-0.3.0.css"));

            bundles.Add(new StyleBundle("~/Content/bootstrap").Include(
                "~/Content/bootstrap.css",
                "~/Content/bootstrap-theme.css"));

			bundles.Add(new StyleBundle("~/Content/themes/base/css").Include(
						"~/Content/themes/base/jquery.ui.core.css",
						"~/Content/themes/base/jquery.ui.resizable.css",
						"~/Content/themes/base/jquery.ui.selectable.css",
						"~/Content/themes/base/jquery.ui.accordion.css",
						"~/Content/themes/base/jquery.ui.autocomplete.css",
						"~/Content/themes/base/jquery.ui.button.css",
						"~/Content/themes/base/jquery.ui.dialog.css",
						"~/Content/themes/base/jquery.ui.slider.css",
						"~/Content/themes/base/jquery.ui.tabs.css",
						"~/Content/themes/base/jquery.ui.datepicker.css",
						"~/Content/themes/base/jquery.ui.progressbar.css",
						"~/Content/themes/base/jquery.ui.theme.css"));

#if !DEBUG
            BundleTable.EnableOptimizations = true;
#endif
		}
	}
}