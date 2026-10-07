<%@ Control Language="C#" AutoEventWireup="true" %>
<%@ Import Namespace="DotNetNuke.Framework.JavaScriptLibraries" %>
<%@ Import Namespace="DotNetNuke.Services.Localization" %>
<%@ Register TagPrefix="dnn" TagName="ACTIONS" src="~/DesktopModules/DDRMenu/Actions.ascx" %>
<%@ Register TagPrefix="dnn" Namespace="DotNetNuke.Web.UI.WebControls.Internal" Assembly="DotNetNuke.Web" %>
<%@ Register TagPrefix="dnn" Namespace="DotNetNuke.Web.Client.ClientResourceManagement" Assembly="DotNetNuke.Web.Client" %>
<dnn:DnnJsInclude runat="server" FilePath="~/admin/menus/dnnactions/dnnactions.debug.js" />

<dnn:ACTIONS ID="actionMenu" MenuStyle="admin/Menus/DNNActions" runat="server" />
<dnn:DnnScriptBlock ID="confirmScript" runat="server">
	<script type="text/javascript">
	(function($) {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(DDRActionsMenu));
		$(document).ready(function() {
		    //Regular Confirm
			var confirmString = "confirm('<%= Localization.GetSafeJSString("DeleteModule.Confirm")%>')";
			$("a[href*=\"" + confirmString +"\"]").attr("href", function () {
				return $(this).attr("href").replace( /if\(.+?\)\{(.+?)\}/ , "$1");//remove its original confirm function
			}).dnnConfirm({
				text: '<%= Localization.GetSafeJSString("DeleteModule.Confirm", Localization.SharedResourceFile) %>',
				yesText: '<%= Localization.GetSafeJSString("Yes.Text", Localization.SharedResourceFile) %>',
				noText: '<%= Localization.GetSafeJSString("No.Text", Localization.SharedResourceFile) %>',
				title: '<%= Localization.GetSafeJSString("Confirm.Text", Localization.SharedResourceFile) %>'
			});

		    //Module Sharing Confirm
			confirmString = "confirm('<%= Localization.GetSafeJSString("DeleteSharedModule.Confirm")%>')";
			$("a[href*=\"" + confirmString +"\"]").attr("href", function () {
				return $(this).attr("href").replace( /if\(.+?\)\{(.+?)\}/ , "$1");//remove its original confirm function
			}).dnnConfirm({
				text: '<%= Localization.GetSafeJSString("DeleteSharedModule.Confirm", Localization.SharedResourceFile) %>',
				yesText: '<%= Localization.GetSafeJSString("Yes.Text", Localization.SharedResourceFile) %>',
				noText: '<%= Localization.GetSafeJSString("No.Text", Localization.SharedResourceFile) %>',
				title: '<%= Localization.GetSafeJSString("Confirm.Text", Localization.SharedResourceFile) %>'
			});
		});
	}(jQuery));
	</script>
</dnn:DnnScriptBlock>

<script runat="server">
	protected override void OnLoad(EventArgs e)
	{
		Logger.LogError(">>> Entering OnLoad(e={E})", e);
		base.OnLoad(e);
        
		if(!Context.Items.Contains("ActionsMenuLoaded"))
		{
			Context.Items.Add("ActionsMenuLoaded", true);
		}
		else
		{
			confirmScript.Visible = false;
			Logger.LogDebug("OnLoad(e={E}): confirmScript.Visible → {Arg1}", e, confirmScript.Visible);
		}

	    JavaScript.RequestRegistration(CommonJs.jQuery);
	    JavaScript.RequestRegistration(CommonJs.jQueryMigrate);
	    JavaScript.RequestRegistration(CommonJs.jQueryUI);
	    JavaScript.RequestRegistration(CommonJs.HoverIntent);
	    JavaScript.RequestRegistration(CommonJs.DnnPlugins);
		Logger.LogWarning("<<< Exiting OnLoad(e={E})", e);
	}
</script>