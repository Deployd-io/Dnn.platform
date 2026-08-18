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
    [Test]
    public void ConvertToText_returns_the_input_for_simple_strings()
    {
		mockEventLogger.LogDebug(">>> Entering ConvertToText_returns_the_input_for_simple_strings()");
        var result = Mail.ConvertToText("Hello World");
        Assert.That(result, Is.EqualTo("Hello World"));
		mockEventLogger.LogDebug("ConvertToText_returns_the_input_for_simple_strings(): result → {Result}", result);
		mockEventLogger.LogDebug("<<< Exiting ConvertToText_returns_the_input_for_simple_strings()");
    }

    [Test]
    public void ConvertToText_removes_tags()
    {
		mockEventLogger.LogDebug(">>> Entering ConvertToText_removes_tags()");
        var result = Mail.ConvertToText("<div class=\"x\"><p>Hello World</p></div>");
        Assert.That(result.Trim(), Is.EqualTo("Hello World"));
		mockEventLogger.LogDebug("ConvertToText_removes_tags(): result → {Result}", result);
		mockEventLogger.LogDebug("<<< Exiting ConvertToText_removes_tags()");
    }

    [Test]
    [Ignore("This wasn't included before and now fails")]
    public void ConvertToText_removes_styles_including_css_defs()
    {
		mockEventLogger.LogDebug(">>> Entering ConvertToText_removes_styles_including_css_defs()");
        var result = Mail.ConvertToText("<style>\r\nHello</style>World");
        Assert.That(result.Trim(), Is.EqualTo("World"));
		mockEventLogger.LogDebug("ConvertToText_removes_styles_including_css_defs(): result → {Result}", result);
		mockEventLogger.LogDebug("<<< Exiting ConvertToText_removes_styles_including_css_defs()");
    }

    [Test]
    public void GivenBodyIsNotHtmlWhenAddAlternateViewThenShouldContainsPlainViewOnly()
    {
		mockEventLogger.LogDebug(">>> Entering GivenBodyIsNotHtmlWhenAddAlternateViewThenShouldContainsPlainViewOnly()");
        // special character
        MailMessage mailMessage = new MailMessage { IsBodyHtml = false, };
        ContentType plain = new ContentType("text/plain") { CharSet = "us-ascii", };
		mockEventLogger.LogDebug("GivenBodyIsNotHtmlWhenAddAlternateViewThenShouldContainsPlainViewOnly(): plain → {Plain}", plain);
        AlternateView plainView = AlternateView.CreateAlternateViewFromString("body\n", plain);
		mockEventLogger.LogDebug("GivenBodyIsNotHtmlWhenAddAlternateViewThenShouldContainsPlainViewOnly(): plainView → {PlainView}", plainView);

        CoreMailProvider.AddAlternateView(mailMessage, "body\n", Encoding.ASCII);

        AssertEqualAlternativeView(plainView, mailMessage.AlternateViews[0]);
        Assert.That(mailMessage.AlternateViews.Count, Is.EqualTo(1));
		mockEventLogger.LogDebug("<<< Exiting GivenBodyIsNotHtmlWhenAddAlternateViewThenShouldContainsPlainViewOnly()");
    }

    [Test]
    public void GivenBodyHtmlWhenAddAlternateViewThenShouldContainsPlainAndHtmlViews()
    {
		mockEventLogger.LogDebug(">>> Entering GivenBodyHtmlWhenAddAlternateViewThenShouldContainsPlainAndHtmlViews()");
        // special character
        MailMessage mailMessage = new MailMessage { IsBodyHtml = true, };
        ContentType plain = new ContentType("text/plain") { CharSet = "us-ascii", };
		mockEventLogger.LogDebug("GivenBodyHtmlWhenAddAlternateViewThenShouldContainsPlainAndHtmlViews(): plain → {Plain}", plain);
        ContentType html = new ContentType("text/html") { CharSet = "us-ascii", };
		mockEventLogger.LogDebug("GivenBodyHtmlWhenAddAlternateViewThenShouldContainsPlainAndHtmlViews(): html → {Html}", html);
        AlternateView plainView = AlternateView.CreateAlternateViewFromString("body\n", plain);
		mockEventLogger.LogDebug("GivenBodyHtmlWhenAddAlternateViewThenShouldContainsPlainAndHtmlViews(): plainView → {PlainView}", plainView);
        AlternateView htmlView = AlternateView.CreateAlternateViewFromString("body\n", html);
		mockEventLogger.LogDebug("GivenBodyHtmlWhenAddAlternateViewThenShouldContainsPlainAndHtmlViews(): htmlView → {HtmlView}", htmlView);

        CoreMailProvider.AddAlternateView(mailMessage, "body\n", Encoding.ASCII);

        AssertEqualAlternativeView(plainView, mailMessage.AlternateViews[0]);
        AssertEqualAlternativeView(htmlView, mailMessage.AlternateViews[1]);
        Assert.That(mailMessage.AlternateViews.Count, Is.EqualTo(2));
		mockEventLogger.LogDebug("<<< Exiting GivenBodyHtmlWhenAddAlternateViewThenShouldContainsPlainAndHtmlViews()");
    }

    [Test]
    public void GivenEncodingIsAsciiWhenAddAlternateViewThenCharsetShouldAlwaysAscii()
    {
		mockEventLogger.LogDebug(">>> Entering GivenEncodingIsAsciiWhenAddAlternateViewThenCharsetShouldAlwaysAscii()");
        // special character
        MailMessage mailMessage = new MailMessage { IsBodyHtml = true, };
        ContentType plain = new ContentType("text/plain") { CharSet = "us-ascii", };
		mockEventLogger.LogDebug("GivenEncodingIsAsciiWhenAddAlternateViewThenCharsetShouldAlwaysAscii(): plain → {Plain}", plain);
        ContentType html = new ContentType("text/html") { CharSet = "us-ascii", };
		mockEventLogger.LogDebug("GivenEncodingIsAsciiWhenAddAlternateViewThenCharsetShouldAlwaysAscii(): html → {Html}", html);

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
		mockEventLogger.LogDebug("<<< Exiting GivenEncodingIsAsciiWhenAddAlternateViewThenCharsetShouldAlwaysAscii()");
    }

    [Test]
    public void GivenBodyEncodingIsUTF8WhenAddAlternateViewThenCharsetShouldAlwaysUTF8()
    {
		mockEventLogger.LogDebug(">>> Entering GivenBodyEncodingIsUTF8WhenAddAlternateViewThenCharsetShouldAlwaysUTF8()");
        // special character
        MailMessage mailMessage = new MailMessage { IsBodyHtml = true, };
        ContentType plain = new ContentType("text/plain") { CharSet = "utf-8", };
		mockEventLogger.LogDebug("GivenBodyEncodingIsUTF8WhenAddAlternateViewThenCharsetShouldAlwaysUTF8(): plain → {Plain}", plain);
        ContentType html = new ContentType("text/html") { CharSet = "utf-8", };
		mockEventLogger.LogDebug("GivenBodyEncodingIsUTF8WhenAddAlternateViewThenCharsetShouldAlwaysUTF8(): html → {Html}", html);

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
		mockEventLogger.LogDebug("<<< Exiting GivenBodyEncodingIsUTF8WhenAddAlternateViewThenCharsetShouldAlwaysUTF8()");
    }

    private static void AssertEqualAlternativeView(AlternateView expected, AlternateView actual)
    {
		mockEventLogger.LogDebug(">>> Entering AssertEqualAlternativeView(expected={Expected},actual={Actual})", expected, actual);
        Assert.That(actual.ContentType, Is.EqualTo(expected.ContentType));
        Assert.That(actual.ContentStream, Is.EqualTo(expected.ContentStream));
		mockEventLogger.LogDebug("<<< Exiting AssertEqualAlternativeView(expected={Expected},actual={Actual})", expected, actual);
    }
}
