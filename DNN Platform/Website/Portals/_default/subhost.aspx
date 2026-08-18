<%@ Page Language="C#" %>

<script runat="server">
    protected override void OnInit(EventArgs e)
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(subhost));
        base.OnInit(e);
		Logger.LogDebug(">>> Entering OnInit(e={E})", e);

        PortalSettings settings = PortalController.Instance.GetCurrentPortalSettings();
		Logger.LogDebug("OnInit(e={E}): settings → {Settings}", e, settings);
        CultureInfo pageLocale = Localization.GetPageLocale(settings);
		Logger.LogDebug("OnInit(e={E}): pageLocale → {PageLocale}", e, pageLocale);
        if ((settings != null) && (pageLocale != null))
        {
            Localization.SetThreadCultures(pageLocale, settings);
        }
		Logger.LogDebug("<<< Exiting OnInit(e={E})", e);
    }

    protected override void OnLoad(EventArgs e)
	{
		Logger.LogDebug(">>> Entering OnLoad(e={E})", e);
		base.OnLoad(e);

	    var domainName = "";
		Logger.LogDebug("OnLoad(e={E}): domainName → {DomainName}", e, domainName);
	    int urlIndex;

		// parse the Request URL into a Domain Name token 
		string[] url = Request.Url.ToString().Split('/');
		for (urlIndex = 2; urlIndex <= url.GetUpperBound(0); urlIndex++)
		{
			bool willExit = false;
			Logger.LogDebug("OnLoad(e={E}): willExit → {WillExit}", e, willExit);
			switch (url[urlIndex].ToUpperInvariant())
			{
				case "ADMIN":
				case "DESKTOPMODULES":
				case "MOBILEMODULES":
				case "PREMIUMMODULES":
					willExit = true;
					break;
				default:
					// check if filename
					if (url[urlIndex].IndexOf(".aspx", StringComparison.Ordinal) == -1)
					{
						domainName = domainName + (!string.IsNullOrEmpty(domainName) ? "/" : "") + url[urlIndex];
					}
					else
					{
						willExit = true;
					}

					break;
			}
			if (willExit)
				break;
		}

		// format the Request.ApplicationPath
		string serverPath = Request.ApplicationPath;
		Logger.LogDebug("OnLoad(e={E}): serverPath → {ServerPath}", e, serverPath);
		if (serverPath != null && serverPath.Substring(serverPath.Length - 1, 1) != "/")
		{
			serverPath = serverPath + "/";
		}

        PortalSettings portal = PortalController.Instance.GetCurrentPortalSettings();
		Logger.LogDebug("OnLoad(e={E}): portal → {Portal}", e, portal);

		var queryString = Request.Url.Query.TrimStart('?');
		Logger.LogDebug("OnLoad(e={E}): queryString → {QueryString}", e, queryString);

        if (Request.Url.Query.Length == 0 && portal.HomeTabId > Null.NullInteger)
        {
			Response.Redirect(DotNetNuke.Common.Globals.NavigateURL(portal.HomeTabId, portal, string.Empty, queryString), true);
        }
        else
        {
			domainName = string.Format("{0}Default.aspx?alias={1}&{2}", serverPath, domainName, queryString);

            Response.Redirect(domainName, true);
		Logger.LogDebug("<<< Exiting OnLoad(e={E})", e);
        }

	}

</script>

