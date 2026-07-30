using DotNetNuke.Web.Api;

namespace Dnn.ContactList.SpaReact.Api
{
    public class RouteMapper : IServiceRouteMapper
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Opml));
        public void RegisterRoutes(IMapRoute mapRouteManager)
        {
			Logger.LogDebug(">>> Entering RegisterRoutes(mapRouteManager={MapRouteManager})", mapRouteManager);
            mapRouteManager.MapHttpRoute("Dnn/ContactListSpaReact", "DnnContactListSpaReact1", "{controller}/{action}", new[] { "Dnn.ContactList.SpaReact.Api" });
            mapRouteManager.MapHttpRoute("Dnn/ContactListSpaReact", "DnnContactListSpaReact2", "{controller}/{action}/{id}", null, new { id = @"-?\d+" }, new[] { "Dnn.ContactList.SpaReact.Api" });
			Logger.LogDebug("<<< Exiting RegisterRoutes(mapRouteManager={MapRouteManager})", mapRouteManager);
        }
    }
}
