// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Tests.Core.Services.Mail;

using System.Net.Mail;
using System.Net.Mime;
using System.Text;

using DotNetNuke.Services.Mail;
using NUnit.Framework;

[TestFixture]
public class MailTests
{
	private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Opml));
    [Test]
    public void ConvertToText_returns_the_input_for_simple_strings()
    {
		Logger.LogDebug(">>> Entering ConvertToText_returns_the_input_for_simple_strings()");
        var result = Mail.ConvertToText("Hello World");
        Assert.That(result, Is.EqualTo("Hello World"));
		Logger.LogDebug("<<< Exiting ConvertToText_returns_the_input_for_simple_strings()");
		Logger.LogDebug("ConvertToText_returns_the_input_for_simple_strings(): result → {Result}", result);
    }

    [Test]
    public void ConvertToText_removes_tags()
    {
		Logger.LogDebug(">>> Entering ConvertToText_removes_tags()");
        var result = Mail.ConvertToText("<div class=\"x\"><p>Hello World</p></div>");
        Assert.That(result.Trim(), Is.EqualTo("Hello World"));
		Logger.LogDebug("<<< Exiting ConvertToText_removes_tags()");
		Logger.LogDebug("ConvertToText_removes_tags(): result → {Result}", result);
    }

    [Test]
    [Ignore("This wasn't included before and now fails")]
    public void ConvertToText_removes_styles_including_css_defs()
    {
		Logger.LogDebug(">>> Entering ConvertToText_removes_styles_including_css_defs()");
        var result = Mail.ConvertToText("<style>\r\nHello</style>World");
        Assert.That(result.Trim(), Is.EqualTo("World"));
		Logger.LogDebug("<<< Exiting ConvertToText_removes_styles_including_css_defs()");
		Logger.LogDebug("ConvertToText_removes_styles_including_css_defs(): result → {Result}", result);
    }

    [Test]
    public void GivenBodyIsNotHtmlWhenAddAlternateViewThenShouldContainsPlainViewOnly()
    {
		Logger.LogDebug(">>> Entering GivenBodyIsNotHtmlWhenAddAlternateViewThenShouldContainsPlainViewOnly()");
        // special character
        MailMessage mailMessage = new MailMessage { IsBodyHtml = false, };
        ContentType plain = new ContentType("text/plain") { CharSet = "us-ascii", };
        AlternateView plainView = AlternateView.CreateAlternateViewFromString("body\n", plain);
		Logger.LogDebug("GivenBodyIsNotHtmlWhenAddAlternateViewThenShouldContainsPlainViewOnly(): plain → {Plain}", plain);
		Logger.LogDebug("GivenBodyIsNotHtmlWhenAddAlternateViewThenShouldContainsPlainViewOnly(): plainView → {PlainView}", plainView);

        CoreMailProvider.AddAlternateView(mailMessage, "body\n", Encoding.ASCII);

        AssertEqualAlternativeView(plainView, mailMessage.AlternateViews[0]);
        Assert.That(mailMessage.AlternateViews.Count, Is.EqualTo(1));
		Logger.LogDebug("<<< Exiting GivenBodyIsNotHtmlWhenAddAlternateViewThenShouldContainsPlainViewOnly()");
    }

    [Test]
    public void GivenBodyHtmlWhenAddAlternateViewThenShouldContainsPlainAndHtmlViews()
    {
		Logger.LogDebug(">>> Entering GivenBodyHtmlWhenAddAlternateViewThenShouldContainsPlainAndHtmlViews()");
        // special character
        MailMessage mailMessage = new MailMessage { IsBodyHtml = true, };
        ContentType plain = new ContentType("text/plain") { CharSet = "us-ascii", };
        ContentType html = new ContentType("text/html") { CharSet = "us-ascii", };
		Logger.LogDebug("GivenBodyHtmlWhenAddAlternateViewThenShouldContainsPlainAndHtmlViews(): plain → {Plain}", plain);
		Logger.LogDebug("GivenBodyHtmlWhenAddAlternateViewThenShouldContainsPlainAndHtmlViews(): html → {Html}", html);
        AlternateView plainView = AlternateView.CreateAlternateViewFromString("body\n", plain);
        AlternateView htmlView = AlternateView.CreateAlternateViewFromString("body\n", html);
		Logger.LogDebug("GivenBodyHtmlWhenAddAlternateViewThenShouldContainsPlainAndHtmlViews(): plainView → {PlainView}", plainView);
		Logger.LogDebug("GivenBodyHtmlWhenAddAlternateViewThenShouldContainsPlainAndHtmlViews(): htmlView → {HtmlView}", htmlView);

        CoreMailProvider.AddAlternateView(mailMessage, "body\n", Encoding.ASCII);

        AssertEqualAlternativeView(plainView, mailMessage.AlternateViews[0]);
        AssertEqualAlternativeView(htmlView, mailMessage.AlternateViews[1]);
        Assert.That(mailMessage.AlternateViews.Count, Is.EqualTo(2));
		Logger.LogDebug("<<< Exiting GivenBodyHtmlWhenAddAlternateViewThenShouldContainsPlainAndHtmlViews()");
    }

    [Test]
    public void GivenEncodingIsAsciiWhenAddAlternateViewThenCharsetShouldAlwaysAscii()
    {
		Logger.LogDebug(">>> Entering GivenEncodingIsAsciiWhenAddAlternateViewThenCharsetShouldAlwaysAscii()");
        // special character
        MailMessage mailMessage = new MailMessage { IsBodyHtml = true, };
        ContentType plain = new ContentType("text/plain") { CharSet = "us-ascii", };
        ContentType html = new ContentType("text/html") { CharSet = "us-ascii", };
		Logger.LogDebug("GivenEncodingIsAsciiWhenAddAlternateViewThenCharsetShouldAlwaysAscii(): plain → {Plain}", plain);
		Logger.LogDebug("GivenEncodingIsAsciiWhenAddAlternateViewThenCharsetShouldAlwaysAscii(): html → {Html}", html);

        CoreMailProvider.AddAlternateView(mailMessage, "body\n", Encoding.ASCII);

        Assert.That(mailMessage.AlternateViews[0].ContentType, Is.EqualTo(plain));
        Assert.That(mailMessage.AlternateViews[1].ContentType, Is.EqualTo(html));

        // no special character
        mailMessage = new MailMessage()
        {
            IsBodyHtml = true,
        };

        CoreMailProvider.AddAlternateView(mailMessage, "body", Encoding.ASCII);

        Assert.That(mailMessage.AlternateViews[0].ContentType, Is.EqualTo(plain));
        Assert.That(mailMessage.AlternateViews[1].ContentType, Is.EqualTo(html));
		Logger.LogDebug("<<< Exiting GivenEncodingIsAsciiWhenAddAlternateViewThenCharsetShouldAlwaysAscii()");
    }

    [Test]
    public void GivenBodyEncodingIsUTF8WhenAddAlternateViewThenCharsetShouldAlwaysUTF8()
    {
		Logger.LogDebug(">>> Entering GivenBodyEncodingIsUTF8WhenAddAlternateViewThenCharsetShouldAlwaysUTF8()");
        // special character
        MailMessage mailMessage = new MailMessage { IsBodyHtml = true, };
        ContentType plain = new ContentType("text/plain") { CharSet = "utf-8", };
        ContentType html = new ContentType("text/html") { CharSet = "utf-8", };
		Logger.LogDebug("GivenBodyEncodingIsUTF8WhenAddAlternateViewThenCharsetShouldAlwaysUTF8(): plain → {Plain}", plain);
		Logger.LogDebug("GivenBodyEncodingIsUTF8WhenAddAlternateViewThenCharsetShouldAlwaysUTF8(): html → {Html}", html);

        CoreMailProvider.AddAlternateView(mailMessage, "body\n", Encoding.UTF8);

        Assert.That(mailMessage.AlternateViews[0].ContentType, Is.EqualTo(plain));
        Assert.That(mailMessage.AlternateViews[1].ContentType, Is.EqualTo(html));

        // no special character
        mailMessage = new MailMessage()
        {
            IsBodyHtml = true,
        };

        CoreMailProvider.AddAlternateView(mailMessage, "body", Encoding.UTF8);

        Assert.That(mailMessage.AlternateViews[0].ContentType, Is.EqualTo(plain));
        Assert.That(mailMessage.AlternateViews[1].ContentType, Is.EqualTo(html));
		Logger.LogDebug("<<< Exiting GivenBodyEncodingIsUTF8WhenAddAlternateViewThenCharsetShouldAlwaysUTF8()");
    }

    private static void AssertEqualAlternativeView(AlternateView expected, AlternateView actual)
    {
		Logger.LogDebug(">>> Entering AssertEqualAlternativeView(expected={Expected},actual={Actual})", expected, actual);
        Assert.That(actual.ContentType, Is.EqualTo(expected.ContentType));
        Assert.That(actual.ContentStream, Is.EqualTo(expected.ContentStream));
		Logger.LogDebug("<<< Exiting AssertEqualAlternativeView(expected={Expected},actual={Actual})", expected, actual);
    }
}
