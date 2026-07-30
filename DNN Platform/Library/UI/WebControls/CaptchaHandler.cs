// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.UI.WebControls
{
    using System.Collections.Specialized;
    using System.Drawing;
    using System.Drawing.Imaging;
    using System.Net.Mime;
    using System.Web;

    /// <summary>The CaptchaHandler control provides a validator to validate a CAPTCHA Challenge.</summary>
    public class CaptchaHandler : IHttpHandler
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Opml));
        private const int MAXIMAGEWIDTH = 600;
        private const int MAXIMAGEHEIGHT = 600;

        /// <inheritdoc />
        public bool IsReusable
        {
            get
            {
                return false;
            }
        }

        /// <inheritdoc />
        public void ProcessRequest(HttpContext context)
        {
			Logger.LogDebug(">>> Entering ProcessRequest(context={Context})", context);
            NameValueCollection queryString = context.Request.QueryString;
            string text = queryString[CaptchaControl.KEY];
            HttpResponse response = context.Response;
            response.ContentType = MediaTypeNames.Image.Jpeg;
			Logger.LogDebug("ProcessRequest(context={Context}): response → {Response}", context, response);
			Logger.LogDebug("ProcessRequest(context={Context}): response.ContentType → {Arg1}", context, response.ContentType);
            Bitmap bmp = CaptchaControl.GenerateImage(text);
            if (bmp != null)
			Logger.LogDebug("ProcessRequest(context={Context}): bmp → {Bmp}", context, bmp);
            {
                bmp.Save(response.OutputStream, ImageFormat.Jpeg);
            }
			Logger.LogDebug("<<< Exiting ProcessRequest(context={Context})", context);
        }
    }
}
