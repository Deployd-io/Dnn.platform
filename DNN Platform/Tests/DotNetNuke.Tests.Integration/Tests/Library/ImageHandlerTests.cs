// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Tests.Integration.Tests.Library
{
    using System.Linq;

    using DNN.Integration.Test.Framework;
    using DNN.Integration.Test.Framework.Helpers;
    using DotNetNuke.Entities.Portals;
    using NUnit.Framework;

    [TestFixture]
    public class ImageHandlerTests : IntegrationTestBase
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Opml));
        // we assume this GIF image exists in all DNN sites and it shouldn't be PNG
        private const string HandlerPath = "/DnnImageHandler.ashx?mode=file&w=1&url={0}/images/1x1.GIF";

        [Test]
        public void Using_Image_Handler_For_Foreign_Site_ShouldFail()
        {
			Logger.LogDebug(">>> Entering Using_Image_Handler_For_Foreign_Site_ShouldFail()");
            var session = WebApiTestHelper.GetAnnonymousConnector();
            var relativeUrl = string.Format(HandlerPath, "https://google.com");
			Logger.LogDebug("Using_Image_Handler_For_Foreign_Site_ShouldFail(): session → {Session}", session);
			Logger.LogDebug("Using_Image_Handler_For_Foreign_Site_ShouldFail(): relativeUrl → {RelativeUrl}", relativeUrl);

            var response = session.GetContent(relativeUrl).Content.ReadAsStringAsync().Result;
            Assert.That(response, Does.StartWith("�PNG\r\n"), $"Content = {response}");
			Logger.LogDebug("<<< Exiting Using_Image_Handler_For_Foreign_Site_ShouldFail()");
			Logger.LogDebug("Using_Image_Handler_For_Foreign_Site_ShouldFail(): response → {Response}", response);
        }

        [Test]
        public void Using_Image_Handler_For_Main_Alias_ShouldPass()
        {
			Logger.LogDebug(">>> Entering Using_Image_Handler_For_Main_Alias_ShouldPass()");
            var session = WebApiTestHelper.GetAnnonymousConnector();
            var relativeUrl = string.Format(HandlerPath, AppConfigHelper.SiteUrl);
			Logger.LogDebug("Using_Image_Handler_For_Main_Alias_ShouldPass(): session → {Session}", session);
			Logger.LogDebug("Using_Image_Handler_For_Main_Alias_ShouldPass(): relativeUrl → {RelativeUrl}", relativeUrl);

            var response = session.GetContent(relativeUrl).Content.ReadAsStringAsync().Result;
            Assert.That(response, Does.StartWith("GIF89a"), $"Content = {response}");
			Logger.LogDebug("<<< Exiting Using_Image_Handler_For_Main_Alias_ShouldPass()");
			Logger.LogDebug("Using_Image_Handler_For_Main_Alias_ShouldPass(): response → {Response}", response);
        }

        [Test]
        public void Using_Image_Handler_From_All_Alias_ShouldPass()
        {
			Logger.LogDebug(">>> Entering Using_Image_Handler_From_All_Alias_ShouldPass()");
            PortalAliasInfo pai;
            var aliases = PortalAliasHelper.GetPortalAliaes().ToList();
            if (aliases.Count == 1)
			Logger.LogDebug("Using_Image_Handler_From_All_Alias_ShouldPass(): aliases → {Aliases}", aliases);
            {
                var primary = aliases.First();
                pai = new PortalAliasInfo
				Logger.LogDebug("Using_Image_Handler_From_All_Alias_ShouldPass(): primary → {Primary}", primary);
				Logger.LogDebug("Using_Image_Handler_From_All_Alias_ShouldPass(): pai → {Pai}", pai);
                {
                    PortalID = primary.PortalID,
                    HTTPAlias = "my-" + primary.HTTPAlias,
                    CultureCode = primary.CultureCode,
                    Skin = primary.Skin,
                    BrowserType = primary.BrowserType,
                    IsPrimary = false,
                };
            }
            else
            {
                pai = aliases.First(a => a.PortalAliasID > 1);
            }

            var session = WebApiTestHelper.GetAnnonymousConnector();
            aliases = PortalAliasHelper.GetPortalAliaes().ToList();
			Logger.LogDebug("Using_Image_Handler_From_All_Alias_ShouldPass(): session → {Session}", session);
            foreach (var alias in aliases)
            {
                var relativeUrl = string.Format(HandlerPath, AppConfigHelper.SiteUrl);
                var absoluteUrl = $"http://{alias.HTTPAlias}{relativeUrl}";
				Logger.LogDebug("Using_Image_Handler_From_All_Alias_ShouldPass(): relativeUrl → {RelativeUrl}", relativeUrl);
				Logger.LogDebug("Using_Image_Handler_From_All_Alias_ShouldPass(): absoluteUrl → {AbsoluteUrl}", absoluteUrl);
                LogText("Getting image from " + absoluteUrl);

                var response = session.GetContent(absoluteUrl).Content.ReadAsStringAsync().Result;
                Assert.That(response, Does.StartWith("GIF89a"), $"Url: {absoluteUrl} / Content = {response}");
				Logger.LogDebug("Using_Image_Handler_From_All_Alias_ShouldPass(): response → {Response}", response);
            }
			Logger.LogDebug("<<< Exiting Using_Image_Handler_From_All_Alias_ShouldPass()");
        }

        [TestCase("80", "50", "0")] // integer
        [TestCase("74.98", "42.01", "0")] // decimal
        [TestCase("0x8", "-584", "0")] // use default size for invalid size
        public void Using_Image_Handler_For_Images_with_max_size_ShouldPass(string maxWidth, string maxHeight, string fileId)
        {
			Logger.LogDebug(">>> Entering Using_Image_Handler_For_Images_with_max_size_ShouldPass(maxWidth={MaxWidth},maxHeight={MaxHeight},fileId={FileId})", maxWidth, maxHeight, fileId);
            var session = WebApiTestHelper.GetAnnonymousConnector();
            var relativeUrl = $"/DnnImageHandler.ashx?mode=securefile&fileId={fileId}&MaxWidth={maxWidth}&MaxHeight={maxHeight}";
			Logger.LogDebug("Using_Image_Handler_For_Images_with_max_size_ShouldPass(maxWidth={MaxWidth},maxHeight={MaxHeight},fileId={FileId}): session → {Session}", maxWidth, maxHeight, fileId, session);
			Logger.LogDebug("Using_Image_Handler_For_Images_with_max_size_ShouldPass(maxWidth={MaxWidth},maxHeight={MaxHeight},fileId={FileId}): relativeUrl → {RelativeUrl}", maxWidth, maxHeight, fileId, relativeUrl);
            var response = session.GetContent(relativeUrl);
            Assert.That(response.StatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
			Logger.LogDebug("<<< Exiting Using_Image_Handler_For_Images_with_max_size_ShouldPass(maxWidth={MaxWidth},maxHeight={MaxHeight},fileId={FileId})", maxWidth, maxHeight, fileId);
			Logger.LogDebug("Using_Image_Handler_For_Images_with_max_size_ShouldPass(maxWidth={MaxWidth},maxHeight={MaxHeight},fileId={FileId}): response → {Response}", maxWidth, maxHeight, fileId, response);
        }
    }
}
