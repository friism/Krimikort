using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace EB.Crime.Map
{
    // Note: For instructions on enabling IIS6 or IIS7 classic mode, 
    // visit http://go.microsoft.com/?LinkId=9394801

    public class MvcApplication : System.Web.HttpApplication
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            routes.MapRoute(
                "Sitemap",                                              // Route name
                "sitemap.xml",                           // URL with parameters
            new { controller = "Sitemap", action = "Sitemap", id = "" }  // Parameter defaults
            );

            routes.MapRoute(
                "Data",                                              // Route name
                "data",                           // URL with parameters
                new { controller = "Data", action = "Data", id = "" }  // Parameter defaults
            );

            routes.MapRoute(
                "GoogleEarth",                                              // Route name
                "gearth.kmz",                           // URL with parameters
                new { controller = "KML", action = "GE", id = "" }  // Parameter defaults
            );

            routes.MapRoute(
                "KML",                                              // Route name
                "kml",                           // URL with parameters
                new { controller = "KML", action = "KML", id = "" }  // Parameter defaults
            );

            routes.MapRoute(
                "KMZ",                                              // Route name
                "kmz",                           // URL with parameters
                new { controller = "KML", action = "KMZ", id = "" }  // Parameter defaults
            );

            routes.MapRoute(
                "Layar",                                              // Route name
                "layar",                           // URL with parameters
                new { controller = "Layar", action = "GetPOIs", id = "" }  // Parameter defaults
            );

            routes.MapRoute(
                "ArticleGenerator",                                              // Route name
                "article",                           // URL with parameters
                new { controller = "Article", action = "GetArticle" }  // Parameter defaults
            );

            routes.MapRoute(
                "HeatTile",                                              // Route name
                "heattile",                           // URL with parameters
                new { controller = "HeatTile", action = "GetTile" }  // Parameter defaults
            );

            routes.MapRoute(
                "Eventdata",                                              // Route name
                "eventdata",                           // URL with parameters
                new { controller = "Data", action = "EventData", id = "" }  // Parameter defaults
            );

            routes.MapRoute(
                "EventdataMulti",                                              // Route name
                "eventdatamulti",                           // URL with parameters
                new { controller = "Data", action = "EventdataMulti", id = "" }  // Parameter defaults
            );

            routes.MapRoute(
                "Event",                                              // Route name
                "haendelse/{slug}/{eventid}",                           // URL with parameters
                new { controller = "Event", action = "EventDetails", id = "" }  // Parameter defaults
            );

            routes.MapRoute(
                "Index",                                              // Route name
                "",                           // URL with parameters
                new { controller = "Home", action = "Index", id = "" }  // Parameter defaults
            );


            //routes.MapRoute(
            //    "Default",                                              // Route name
            //    "{controller}/{action}/{id}",                           // URL with parameters
            //    new { controller = "Home", action = "Index", id = "" }  // Parameter defaults
            //);
            
        }

        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();

            RegisterRoutes(RouteTable.Routes);
        }
    }
}