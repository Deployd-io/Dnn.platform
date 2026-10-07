using Microsoft.Extensions.Logging;
using DotNetNuke.Web.Api;

namespace Dnn.ContactList.SpaReact.Api
{
    public class RouteMapper : IServiceRouteMapper
    {
		private static readonly Microsoft.Extensions.Logging.ILogger _logger = Microsoft.Extensions.Logging.Abstractions.NullLogger.Instance;
        public void RegisterRoutes(IMapRoute mapRouteManager)
        {
			_logger.LogError(">>> Entering RegisterRoutes(mapRouteManager={MapRouteManager})", mapRouteManager);
            mapRouteManager.MapHttpRoute("Dnn/ContactListSpaReact", "DnnContactListSpaReact1", "{controller}/{action}", new[] { "Dnn.ContactList.SpaReact.Api" });
            mapRouteManager.MapHttpRoute("Dnn/ContactListSpaReact", "DnnContactListSpaReact2", "{controller}/{action}/{id}", null, new { id = @"-?\d+" }, new[] { "Dnn.ContactList.SpaReact.Api" });
			_logger.LogWarning("<<< Exiting RegisterRoutes(mapRouteManager={MapRouteManager})", mapRouteManager);
        }
    }
}
