using System.Web.Mvc;
using System.Web.Routing;

namespace HTMLSnippetShare
{
    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");
            
            // User Controller
            routes.MapRoute(
                name: "Register",
                url: "register",
                defaults: new { controller = "User", action = "Register", id = UrlParameter.Optional }
            );

            routes.MapRoute(
                name: "Login",
                url: "login",
                defaults: new { controller = "User", action = "Login", id = UrlParameter.Optional }
            );

            routes.MapRoute(
                name: "Logout",
                url: "logout",
                defaults: new { controller = "User", action = "Logout", id = UrlParameter.Optional }
            );

            // Admin Controller
            routes.MapRoute(
                name: "Admin Panel",
                url: "admin",
                defaults: new { controller = "Admin", action = "AdminPanel", id = UrlParameter.Optional }
            );

            routes.MapRoute(
                name: "User Snippets",
                url: "admin/user-snippets/{id}",
                defaults: new { controller = "Admin", action = "UserSnippets", id = UrlParameter.Optional }
            );

            routes.MapRoute(
                name: "Delete User Snippet",
                url: "admin/delete-user-snippet/{id}/{username}",
                defaults: new { controller = "Admin", action = "DeleteUserSnippet", id = UrlParameter.Optional, username = UrlParameter.Optional }
            );

            // Home Controller
            routes.MapRoute(
                name: "My Snippets",
                url: "my-snippets",
                defaults: new { controller = "Home", action = "MySnippets", id = UrlParameter.Optional }
            );

            routes.MapRoute(
                name: "Delete Snippet",
                url: "delete-snippet",
                defaults: new { controller = "Home", action = "DeleteSnippet", id = UrlParameter.Optional }
            );

            routes.MapRoute(
                name: "Not Found",
                url: "not-found",
                defaults: new { controller = "Home", action = "NotFound", id = UrlParameter.Optional }
            );

            routes.MapRoute(
                name: "View Saved Snippet",
                url: "{id}",
                defaults: new { controller = "Home", action = "Index", id = UrlParameter.Optional }
            );

            routes.MapRoute(
                name: "Default",
                url: "{controller}/{action}/{id}",
                defaults: new { controller = "Home", action = "Index", id = UrlParameter.Optional }
            );
        }
    }
}