// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Tests.Modules.Html
{
    using DotNetNuke.Modules.Html;
    using NUnit.Framework;
	using Microsoft.Extensions.Logging;

    [TestFixture]
    public class HtmlTextControllerTests
    {
		private static readonly Microsoft.Extensions.Logging.ILogger _logger = Microsoft.Extensions.Logging.Abstractions.NullLogger.Instance;
        [Test]
        public void ManageRelativePaths_DoesNotChangePlainHtml()
        {
			_logger.LogDebug(">>> Entering ManageRelativePaths_DoesNotChangePlainHtml()");
            var actual = HtmlTextController.ManageRelativePaths(
                "<p>Hello</p>",
                "/portals/0/",
                "src");
            Assert.That(actual, Is.EqualTo("<p>Hello</p>"));
			_logger.LogDebug("<<< Exiting ManageRelativePaths_DoesNotChangePlainHtml()");
        }

        [Test]
        public void ManageRelativePaths_AdjustsRelativeImgSrc()
        {
			_logger.LogDebug(">>> Entering ManageRelativePaths_AdjustsRelativeImgSrc()");
            var actual = HtmlTextController.ManageRelativePaths(
                "<img src=\"image.jpg\"/>",
                "/portals/0/",
                "src");
            Assert.That(actual, Is.EqualTo("<img src=\"/portals/0/image.jpg\"/>"));
			_logger.LogDebug("<<< Exiting ManageRelativePaths_AdjustsRelativeImgSrc()");
        }

        [Test]
        public void ManageRelativePaths_DoesNotAdjustImgSrcWithCorrectPathCaseInsensitive()
        {
			_logger.LogDebug(">>> Entering ManageRelativePaths_DoesNotAdjustImgSrcWithCorrectPathCaseInsensitive()");
            var actual = HtmlTextController.ManageRelativePaths(
                "<img src=\"/Portals/0/image.jpg\"/>",
                "/portals/0/",
                "src");
            Assert.That(actual, Is.EqualTo("<img src=\"/portals/0/image.jpg\"/>"));
			_logger.LogDebug("<<< Exiting ManageRelativePaths_DoesNotAdjustImgSrcWithCorrectPathCaseInsensitive()");
        }

        [Test]
        public void ManageRelativePaths_DoesNotAdjustImgSrcWithAbsoluteUrl()
        {
			_logger.LogDebug(">>> Entering ManageRelativePaths_DoesNotAdjustImgSrcWithAbsoluteUrl()");
            var actual = HtmlTextController.ManageRelativePaths(
                "<img src=\"https://example.com/image.jpg\"/>",
                "/portals/0/",
                "src");
            Assert.That(actual, Is.EqualTo("<img src=\"https://example.com/image.jpg\"/>"));
			_logger.LogDebug("<<< Exiting ManageRelativePaths_DoesNotAdjustImgSrcWithAbsoluteUrl()");
        }

        [Test]
        public void ManageRelativePaths_DoesNotAdjustImgSrcWithAbsoluteUrlInContent()
        {
			_logger.LogDebug(">>> Entering ManageRelativePaths_DoesNotAdjustImgSrcWithAbsoluteUrlInContent()");
            var actual = HtmlTextController.ManageRelativePaths(
                "src=\"https://example.com/image.jpg\" is how you indicate a URL",
                "/portals/0/",
                "src");
            Assert.That(actual, Is.EqualTo("src=\"https://example.com/image.jpg\" is how you indicate a URL"));
			_logger.LogDebug("<<< Exiting ManageRelativePaths_DoesNotAdjustImgSrcWithAbsoluteUrlInContent()");
        }

        [Test]
        public void ManageRelativePaths_DoesNotAdjustContentEndingInImgSrc()
        {
			_logger.LogDebug(">>> Entering ManageRelativePaths_DoesNotAdjustContentEndingInImgSrc()");
            var actual = HtmlTextController.ManageRelativePaths(
                "src=\"image.jpg\"",
                "/portals/0/",
                "src");
            Assert.That(actual, Is.EqualTo("src=\"image.jpg\""));
			_logger.LogDebug("<<< Exiting ManageRelativePaths_DoesNotAdjustContentEndingInImgSrc()");
        }

        [Test]
        public void ManageRelativePaths_DoesNotAdjustContentEndingInUnclosedImgSrc()
        {
			_logger.LogDebug(">>> Entering ManageRelativePaths_DoesNotAdjustContentEndingInUnclosedImgSrc()");
            var actual = HtmlTextController.ManageRelativePaths(
                "src=\"image.jpg",
                "/portals/0/",
                "src");
            Assert.That(actual, Is.EqualTo("src=\"image.jpg"));
			_logger.LogDebug("<<< Exiting ManageRelativePaths_DoesNotAdjustContentEndingInUnclosedImgSrc()");
        }

        [Test]
        public void ManageRelativePaths_DoesAdjustImgSrcWithRelativeUrlInContent()
        {
			_logger.LogDebug(">>> Entering ManageRelativePaths_DoesAdjustImgSrcWithRelativeUrlInContent()");
            // TODO: should we attempt to avoid making this change?
            var actual = HtmlTextController.ManageRelativePaths(
                "src=\"image.jpg\" is how you indicate a URL",
                "/portals/0/",
                "src");
            Assert.That(actual, Is.EqualTo("src=\"/portals/0/image.jpg\" is how you indicate a URL"));
			_logger.LogDebug("<<< Exiting ManageRelativePaths_DoesAdjustImgSrcWithRelativeUrlInContent()");
        }

        [Test]
        public void ManageRelativePaths_DoesNotAdjustImgSrcWithDataUrl()
        {
			_logger.LogDebug(">>> Entering ManageRelativePaths_DoesNotAdjustImgSrcWithDataUrl()");
            var actual = HtmlTextController.ManageRelativePaths(
                "<img src=\"data:image/gif;base64,R0lGODlhEAAQAMQAAORHHOVSKudfOulrSOp3WOyDZu6QdvCchPGolfO0o/XBs/fNwfjZ0frl3/zy7////wAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAACH5BAkAABAALAAAAAAQABAAAAVVICSOZGlCQAosJ6mu7fiyZeKqNKToQGDsM8hBADgUXoGAiqhSvp5QAnQKGIgUhwFUYLCVDFCrKUE1lBavAViFIDlTImbKC5Gm2hB0SlBCBMQiB0UjIQA7\"/>",
                "/portals/0/",
                "src");
            Assert.That(actual, Is.EqualTo("<img src=\"data:image/gif;base64,R0lGODlhEAAQAMQAAORHHOVSKudfOulrSOp3WOyDZu6QdvCchPGolfO0o/XBs/fNwfjZ0frl3/zy7////wAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAACH5BAkAABAALAAAAAAQABAAAAVVICSOZGlCQAosJ6mu7fiyZeKqNKToQGDsM8hBADgUXoGAiqhSvp5QAnQKGIgUhwFUYLCVDFCrKUE1lBavAViFIDlTImbKC5Gm2hB0SlBCBMQiB0UjIQA7\"/>"));
			_logger.LogDebug("<<< Exiting ManageRelativePaths_DoesNotAdjustImgSrcWithDataUrl()");
        }

        [Test]
        public void ManageRelativePaths_AdjustsNonRootedRelativePathsAndDoesNotAdjustOtherPaths()
        {
			_logger.LogDebug(">>> Entering ManageRelativePaths_AdjustsNonRootedRelativePathsAndDoesNotAdjustOtherPaths()");
			_logger.LogDebug("<<< Exiting ManageRelativePaths_AdjustsNonRootedRelativePathsAndDoesNotAdjustOtherPaths()");
            const string HtmlContent = @"
<img alt=""a data URI"" src=""data:image/gif;base64,R0lGODlhEAAQAMQAAORHHOVSKudfOulrSOp3WOyDZu6QdvCchPGolfO0o/XBs/fNwfjZ0frl3/zy7////wAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAACH5BAkAABAALAAAAAAQABAAAAVVICSOZGlCQAosJ6mu7fiyZeKqNKToQGDsM8hBADgUXoGAiqhSvp5QAnQKGIgUhwFUYLCVDFCrKUE1lBavAViFIDlTImbKC5Gm2hB0SlBCBMQiB0UjIQA7""/>
<img alt=""a non-rooted relative path"" src=""images/image.png"" />
<img alt=""a rooted relative path"" src=""/image.png"" />
<img alt=""an absolute path"" src=""https://dnncommunity.org/Portals/0/DNN_White_Logo_lg.png"" />
<img alt=""another data URI"" src=""data:image/gif;base64,R0lGODlhDgAOAOYAAOBxJvGKIfOEIu5+ItlaIt1XD/zu5uNzNvSJItxXD99cFumEIe2/lOGfb/nNoN9/Huy+lNxgIuqHHeOAH/PQsNZ+SfvRpN+NNficPu2DLt1WDfueOvCfV+WldviSLfHJoeGYR9+AH+l3It1hHvbfxu/MqeSLT+OcUedxIuFwIfaQMeate+qTNeWkfuGib/fcwvufPvjn1uuCKNxbFe2QTOOLNvKHGfmdOt95F9uGRuSjdeiHH+KMQ999H/ubOei0jO6LKuFoIvjZuuFuMeSZSt58P+CFNvCZSPfDivjIm9NgIN5rJfOLLttaH/KNIPKvavSMIfeMJdZUDu+MMvvz6uV4KeR2J+qUXtlTDPLXv+l9KvSMG+y7jPrTqPaWNu2EHdlWD/CELPufPOOJWvfl0tlkI91lIvuaMuadcOONSuqTSeOQU95XD+aIM+SBGu2sivCFHfucOdp5F+etc91cF92CRvHLotxyKPzu5+SLQviSL/rQpN9dF+NvMdxNAP///yH5BAAAAAAALAAAAAAOAA4AAAe7gHgHIzN0fIeHCgp9BgdmSi1oJlZaYUwqeh59CWUVf59/FEdePhtnfhpLdTodP1mfDjdicX4FAHdreW01czF/XTA+tQA5ny9PLCBkfxhRtVU8ECWfSDsnf0kCfmwZU0ALRCR/HCEMfwN+CTJfNlsSF39CPSt/NH5gOB9/Fk4P00Yu/lzxI0UOlU9QJnD5k6bBnzd+sLix82cPggVqOKQoMoaAnyFwAgRAIGCACBRBIhBowqiPn5cwYzIKBAA7"">
<img alt=""another non-rooted relative path"" src=""images2/image2.png"" />
<img alt=""another rooted relative path"" src=""/images/image2.png"" />
<img alt=""another absolute path"" src=""https://dnncommunity.org/DesktopModules/ActiveForums/images/feedicon.gif"" />
";
            var actual = HtmlTextController.ManageRelativePaths(
                HtmlContent,
                "/portals/0/",
                "src");

            const string Expected = @"
<img alt=""a data URI"" src=""data:image/gif;base64,R0lGODlhEAAQAMQAAORHHOVSKudfOulrSOp3WOyDZu6QdvCchPGolfO0o/XBs/fNwfjZ0frl3/zy7////wAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAACH5BAkAABAALAAAAAAQABAAAAVVICSOZGlCQAosJ6mu7fiyZeKqNKToQGDsM8hBADgUXoGAiqhSvp5QAnQKGIgUhwFUYLCVDFCrKUE1lBavAViFIDlTImbKC5Gm2hB0SlBCBMQiB0UjIQA7""/>
<img alt=""a non-rooted relative path"" src=""/portals/0/images/image.png"" />
<img alt=""a rooted relative path"" src=""/image.png"" />
<img alt=""an absolute path"" src=""https://dnncommunity.org/Portals/0/DNN_White_Logo_lg.png"" />
<img alt=""another data URI"" src=""data:image/gif;base64,R0lGODlhDgAOAOYAAOBxJvGKIfOEIu5+ItlaIt1XD/zu5uNzNvSJItxXD99cFumEIe2/lOGfb/nNoN9/Huy+lNxgIuqHHeOAH/PQsNZ+SfvRpN+NNficPu2DLt1WDfueOvCfV+WldviSLfHJoeGYR9+AH+l3It1hHvbfxu/MqeSLT+OcUedxIuFwIfaQMeate+qTNeWkfuGib/fcwvufPvjn1uuCKNxbFe2QTOOLNvKHGfmdOt95F9uGRuSjdeiHH+KMQ999H/ubOei0jO6LKuFoIvjZuuFuMeSZSt58P+CFNvCZSPfDivjIm9NgIN5rJfOLLttaH/KNIPKvavSMIfeMJdZUDu+MMvvz6uV4KeR2J+qUXtlTDPLXv+l9KvSMG+y7jPrTqPaWNu2EHdlWD/CELPufPOOJWvfl0tlkI91lIvuaMuadcOONSuqTSeOQU95XD+aIM+SBGu2sivCFHfucOdp5F+etc91cF92CRvHLotxyKPzu5+SLQviSL/rQpN9dF+NvMdxNAP///yH5BAAAAAAALAAAAAAOAA4AAAe7gHgHIzN0fIeHCgp9BgdmSi1oJlZaYUwqeh59CWUVf59/FEdePhtnfhpLdTodP1mfDjdicX4FAHdreW01czF/XTA+tQA5ny9PLCBkfxhRtVU8ECWfSDsnf0kCfmwZU0ALRCR/HCEMfwN+CTJfNlsSF39CPSt/NH5gOB9/Fk4P00Yu/lzxI0UOlU9QJnD5k6bBnzd+sLix82cPggVqOKQoMoaAnyFwAgRAIGCACBRBIhBowqiPn5cwYzIKBAA7"">
<img alt=""another non-rooted relative path"" src=""/portals/0/images2/image2.png"" />
<img alt=""another rooted relative path"" src=""/images/image2.png"" />
<img alt=""another absolute path"" src=""https://dnncommunity.org/DesktopModules/ActiveForums/images/feedicon.gif"" />
";
            Assert.That(actual, Is.EqualTo(Expected));
        }
    }
}
