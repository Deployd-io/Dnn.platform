<dnn:META ID="mobileScale" runat="server" Name="viewport" Content="width=device-width, initial-scale=1.0" />

<dnn:DnnCssInclude runat="server" FilePath="css/skin.min.css" Priority="110" PathNameAlias="SkinPath" />

<dnn:DnnJsInclude runat="server" FilePath="js/skin.min.js" ForceProvider="DnnFormBottomProvider" Priority="110" PathNameAlias="SkinPath" />

<script runat="server">
    protected void Page_Init()
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Opml));
        var fonts = new string[]
		Logger.LogDebug(">>> Entering Page_Init()");
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
        types.Add("woff2", "font/woff2");
		Logger.LogDebug("Page_Init(): types → {Types}", types);
        types.Add("woff", "font/woff");

        var defaultPage = (CDefault)this.Page;

		Logger.LogDebug("Page_Init(): defaultPage → {DefaultPage}", defaultPage);
        foreach (var type in types)
        {
            foreach (var font in fonts)
            {
                var fontLink = new HtmlLink();
                fontLink.Attributes.Add("rel", "preload");
				Logger.LogDebug("Page_Init(): fontLink → {FontLink}", fontLink);
                fontLink.Attributes.Add("as", "font");
                fontLink.Href = this.SkinPath + font + "." + type.Key;
				Logger.LogDebug("Page_Init(): fontLink.Href → {Arg0}", fontLink.Href);
                fontLink.Attributes.Add("type", type.Value);
                fontLink.Attributes.Add("crossorigin", "anonymous");

                defaultPage.Header.Controls.Add(fontLink);
            }
        }
		Logger.LogDebug("<<< Exiting Page_Init()");
    }
</script>