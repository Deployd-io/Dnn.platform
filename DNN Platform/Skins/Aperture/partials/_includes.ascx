using Microsoft.Extensions.Logging;
<dnn:META ID="mobileScale" runat="server" Name="viewport" Content="width=device-width, initial-scale=1.0" />

<dnn:DnnCssInclude runat="server" FilePath="css/skin.min.css" Priority="110" PathNameAlias="SkinPath" />

<dnn:DnnJsInclude runat="server" FilePath="js/skin.min.js" ForceProvider="DnnFormBottomProvider" Priority="110" PathNameAlias="SkinPath" />

<script runat="server">
    protected void Page_Init()
    {
		private static readonly Microsoft.Extensions.Logging.ILogger _logger = Microsoft.Extensions.Logging.Abstractions.NullLogger.Instance;
        var fonts = new string[]
		_logger.LogDebug(">>> Entering Page_Init()");
        {
            "fonts/Ubuntu-Bold",
            "fonts/Ubuntu-BoldItalic",
            "fonts/Ubuntu-Italic",
            "fonts/Ubuntu-Light",
            "fonts/Ubuntu-LightItalic",
            "fonts/Ubuntu-Medium",
            "fonts/Ubuntu-MediumItalic",
            "fonts/Ubuntu-Regular"
        };

        var types = new Dictionary<string, string>();
		_logger.LogDebug("Page_Init(): types → {Types}", types);
        types.Add("woff2", "font/woff2");
        types.Add("woff", "font/woff");

        var defaultPage = (CDefault)this.Page;
		_logger.LogDebug("Page_Init(): defaultPage → {DefaultPage}", defaultPage);

        foreach (var type in types)
        {
            foreach (var font in fonts)
            {
                var fontLink = new HtmlLink();
				_logger.LogDebug("Page_Init(): fontLink → {FontLink}", fontLink);
                fontLink.Attributes.Add("rel", "preload");
                fontLink.Attributes.Add("as", "font");
                fontLink.Href = this.SkinPath + font + "." + type.Key;
				_logger.LogDebug("Page_Init(): fontLink.Href → {Arg0}", fontLink.Href);
                fontLink.Attributes.Add("type", type.Value);
                fontLink.Attributes.Add("crossorigin", "anonymous");

                defaultPage.Header.Controls.Add(fontLink);
            }
        }
		_logger.LogDebug("<<< Exiting Page_Init()");
    }
</script>