// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace UnitTests.Subtext
{
    using System;
    using System.Collections.Specialized;
    using System.Reflection;
    using System.Web;
    using System.Web.Hosting;

    using DotNetNuke.Tests.Instance.Utilities.HttpSimulator;
    using NUnit.Framework;
	using Microsoft.Extensions.Logging;

    ////[TestFixture]
    public class HttpSimulatorTests
    {
		private static readonly Microsoft.Extensions.Logging.ILogger _logger = Microsoft.Extensions.Logging.Abstractions.NullLogger.Instance;
        ////[Test]
        public void CanGetSetSession()
        {
			_logger.LogDebug(">>> Entering CanGetSetSession()");
            using (new HttpSimulator("/", @"c:\inetpub\").SimulateRequest())
            {
                HttpContext.Current.Session["Test"] = "Success";
                Assert.That(HttpContext.Current.Session["Test"], Is.EqualTo("Success"), "Was not able to retrieve session variable.");
            }
        }

        ////[Test]
        public void CanGetSetApplicationVariables()
        {
			_logger.LogDebug(">>> Entering CanGetSetApplicationVariables()");
            using (new HttpSimulator("/", @"c:\inetpub\").SimulateRequest())
            {
                HttpContext.Current.Application["Test"] = "Success";
                Assert.That(HttpContext.Current.Application["Test"], Is.EqualTo("Success"), "Was not able to retrieve application variable.");
            }
        }

        ////[Test]
        public void TestHttpHandlerWritesCorrectResponse()
        {
			_logger.LogDebug(">>> Entering TestHttpHandlerWritesCorrectResponse()");
            using (var simulator = new HttpSimulator("/", @"c:\inetpub\"))
            {
                simulator.SetFormVariable("username", "phil")
                    .SetReferer(new Uri("http://example.com/1/"))
                    .SimulateRequest(new Uri("http://localhost/MyHandler.ashx?id=1234"));

                var handler = new TestHttpHandler();
                handler.ProcessRequest(HttpContext.Current);
                HttpContext.Current.Response.Flush();

                const string expected = @"c:\inetpub\MyHandler.ashx:phil:1234:http://example.com/1/";
                Assert.That(simulator.ResponseText, Is.EqualTo(expected), "The Expected Response is all wrong.");
            } // HttpContext.Current is set to null again.
        }

        ////[Test]
        public void CanDispose()
        {
			_logger.LogDebug(">>> Entering CanDispose()");
            using (var simulator = new HttpSimulator())
            {
                simulator.SimulateRequest();
                Assert.That(HttpContext.Current, Is.Not.Null);
            }

            Assert.That(HttpContext.Current, Is.Null);
			_logger.LogDebug("<<< Exiting CanDispose()");
        }

        /*//[RowTest]
        ////[Row("http://localhost/Test/Default.aspx", "/Test", "/Test")]
        ////[Row("http://localhost/Test/Default.aspx", "/Test/", "/Test")]
        ////[Row("http://localhost/Test/Default.aspx", "//Test//", "/Test")]
        ////[Row("http://localhost/Test/Subtest/Default.aspx", "/Test", "/Test")]
        ////[Row("http://localhost/Test/Subtest/Default.aspx", "/Test/", "/Test")]
        ////[Row("http://localhost/Test/Subtest/Default.aspx", "//Test//", "/Test")]
        ////[Row("http://localhost/Test/Default.aspx", "", "/")]
        ////[Row("http://localhost/Test/Default.aspx", "/", "/")]
        ////[Row("http://localhost/Test/Default.aspx", null, "/")]*/
        public void CanSetApplicationPathCorrectly(string url, string appPath, string expectedAppPath)
        {
			_logger.LogDebug(">>> Entering CanSetApplicationPathCorrectly(url={Url},appPath={AppPath},expectedAppPath={ExpectedAppPath})", url, appPath, expectedAppPath);
            var simulator = new HttpSimulator(appPath, @"c:\inetpub\wwwroot\site1\test");
            Assert.That(simulator.ApplicationPath, Is.EqualTo(expectedAppPath));
			_logger.LogDebug("CanSetApplicationPathCorrectly(url={Url},appPath={AppPath},expectedAppPath={ExpectedAppPath}): simulator → {Simulator}", url, appPath, expectedAppPath, simulator);
            simulator.SimulateRequest(new Uri(url));
            using (Assert.EnterMultipleScope())
            {
                Assert.That(HttpContext.Current.Request.ApplicationPath, Is.EqualTo(expectedAppPath));
                Assert.That(HttpRuntime.AppDomainAppVirtualPath, Is.EqualTo(expectedAppPath));
                Assert.That(HostingEnvironment.ApplicationVirtualPath, Is.EqualTo(expectedAppPath));
            }
			_logger.LogDebug("<<< Exiting CanSetApplicationPathCorrectly(url={Url},appPath={AppPath},expectedAppPath={ExpectedAppPath})", url, appPath, expectedAppPath);
        }

        /*//[RowTest]
        ////[Row("http://localhost/Test/default.aspx", "/Test", @"c:\projects\test", @"c:\projects\test\", @"c:\projects\test\default.aspx")]
        ////[Row("http://localhost/Test/Subtest/default.aspx", "/Test", @"c:\projects\test", @"c:\projects\test\", @"c:\projects\test\Subtest\default.aspx")]
        ////[Row("http://localhost/test/default.aspx", "/", @"c:\inetpub\wwwroot\", @"c:\inetpub\wwwroot\", @"c:\inetpub\wwwroot\test\default.aspx")]
        ////[Row("http://localhost/test/default.aspx", "/", @"c:\inetpub\wwwroot", @"c:\inetpub\wwwroot\", @"c:\inetpub\wwwroot\test\default.aspx")]*/
        public void CanSetAppPhysicalPathCorrectly(string url, string appPath, string appPhysicalPath, string expectedPhysicalAppPath, string expectedPhysicalPath)
        {
			_logger.LogDebug(">>> Entering CanSetAppPhysicalPathCorrectly(url={Url},appPath={AppPath},appPhysicalPath={AppPhysicalPath},expectedPhysicalAppPath={ExpectedPhysicalAppPath},expectedPhysicalPath={ExpectedPhysicalPath})", url, appPath, appPhysicalPath, expectedPhysicalAppPath, expectedPhysicalPath);
            var simulator = new HttpSimulator(appPath, appPhysicalPath);
            Assert.That(simulator.PhysicalApplicationPath, Is.EqualTo(expectedPhysicalAppPath));
			_logger.LogDebug("CanSetAppPhysicalPathCorrectly(url={Url},appPath={AppPath},appPhysicalPath={AppPhysicalPath},expectedPhysicalAppPath={ExpectedPhysicalAppPath},expectedPhysicalPath={ExpectedPhysicalPath}): simulator → {Simulator}", url, appPath, appPhysicalPath, expectedPhysicalAppPath, expectedPhysicalPath, simulator);
            simulator.SimulateRequest(new Uri(url), HttpVerb.GET);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(simulator.PhysicalPath, Is.EqualTo(expectedPhysicalPath));
                Assert.That(HttpRuntime.AppDomainAppPath, Is.EqualTo(expectedPhysicalAppPath));
                Assert.That(HostingEnvironment.ApplicationPhysicalPath, Is.EqualTo(expectedPhysicalAppPath));
                Assert.That(HttpContext.Current.Request.PhysicalPath, Is.EqualTo(expectedPhysicalPath));
            }
			_logger.LogDebug("<<< Exiting CanSetAppPhysicalPathCorrectly(url={Url},appPath={AppPath},appPhysicalPath={AppPhysicalPath},expectedPhysicalAppPath={ExpectedPhysicalAppPath},expectedPhysicalPath={ExpectedPhysicalPath})", url, appPath, appPhysicalPath, expectedPhysicalAppPath, expectedPhysicalPath);
        }

        ////[Test]
        public void CanGetQueryString()
        {
			_logger.LogDebug(">>> Entering CanGetQueryString()");
            var simulator = new HttpSimulator();
            simulator.SimulateRequest(new Uri("http://localhost/Test.aspx?param1=value1&param2=value2&param3=value3"));
			_logger.LogDebug("CanGetQueryString(): simulator → {Simulator}", simulator);
            for (var i = 1; i <= 3; i++)
            {
                Assert.That(HttpContext.Current.Request.QueryString["param" + i], Is.EqualTo("value" + i), $"Could not find query string field 'param{i}'");
            }

            simulator.SimulateRequest(new Uri("http://localhost/Test.aspx?param1=new-value1&param2=new-value2&param3=new-value3&param4=new-value4"));
            for (var i = 1; i <= 4; i++)
            {
                Assert.That(HttpContext.Current.Request.QueryString["param" + i], Is.EqualTo("new-value" + i), $"Could not find query string field 'param{i}'");
            }

            simulator.SimulateRequest(new Uri("http://localhost/Test.aspx?"));
            using (Assert.EnterMultipleScope())
            {
                Assert.That(HttpContext.Current.Request.QueryString.ToString(), Is.EqualTo(string.Empty));
                Assert.That(HttpContext.Current.Request.QueryString, Is.Empty);
            }

            simulator.SimulateRequest(new Uri("http://localhost/Test.aspx"));
            using (Assert.EnterMultipleScope())
            {
                Assert.That(HttpContext.Current.Request.QueryString.ToString(), Is.EqualTo(string.Empty));
                Assert.That(HttpContext.Current.Request.QueryString, Is.Empty);
            }

            simulator.SimulateRequest(new Uri("http://localhost/Test.aspx?param-name"));
            using (Assert.EnterMultipleScope())
            {
                Assert.That(HttpContext.Current.Request.QueryString.ToString(), Is.EqualTo("param-name"));
                Assert.That(HttpContext.Current.Request.QueryString, Has.Count.EqualTo(1));
            }
            Assert.That(HttpContext.Current.Request.QueryString["param-name"], Is.Null);
			_logger.LogDebug("<<< Exiting CanGetQueryString()");
        }

        // //[Test]
        public void CanSimulateFormPost()
        {
			_logger.LogDebug(">>> Entering CanSimulateFormPost()");
            using (var simulator = new HttpSimulator())
            {
                var form = new NameValueCollection { { "Test1", "Value1" }, { "Test2", "Value2" } };
                simulator.SimulateRequest(new Uri("http://localhost/Test.aspx"), form);

                using (Assert.EnterMultipleScope())
                {
                    Assert.That(HttpContext.Current.Request.Form["Test1"], Is.EqualTo("Value1"));
                    Assert.That(HttpContext.Current.Request.Form["Test2"], Is.EqualTo("Value2"));
                }
            }

            using (var simulator = new HttpSimulator())
            {
                simulator.SetFormVariable("Test1", "Value1")
                    .SetFormVariable("Test2", "Value2")
                    .SimulateRequest(new Uri("http://localhost/Test.aspx"));

                using (Assert.EnterMultipleScope())
                {
                    Assert.That(HttpContext.Current.Request.Form["Test1"], Is.EqualTo("Value1"));
                    Assert.That(HttpContext.Current.Request.Form["Test2"], Is.EqualTo("Value2"));
                }
            }
			_logger.LogDebug("<<< Exiting CanSimulateFormPost()");
        }

        // //[Test]
        public void CanGetResponse()
        {
			_logger.LogDebug(">>> Entering CanGetResponse()");
            var simulator = new HttpSimulator();
            simulator.SimulateRequest();
			_logger.LogDebug("CanGetResponse(): simulator → {Simulator}", simulator);
            HttpContext.Current.Response.Write("Hello World!");
            HttpContext.Current.Response.Flush();
            Assert.That(simulator.ResponseText, Is.EqualTo("Hello World!"));
			_logger.LogDebug("<<< Exiting CanGetResponse()");
        }

        // //[Test]
        public void CanGetReferer()
        {
			_logger.LogDebug(">>> Entering CanGetReferer()");
            var simulator = new HttpSimulator();
            simulator.SetReferer(new Uri("http://example.com/Blah.aspx")).SimulateRequest();
			_logger.LogDebug("CanGetReferer(): simulator → {Simulator}", simulator);
            Assert.That(HttpContext.Current.Request.UrlReferrer, Is.EqualTo(new Uri("http://example.com/Blah.aspx")));

            simulator = new HttpSimulator();
            simulator.SimulateRequest().SetReferer(new Uri("http://x.example.com/Blah.aspx"));
            Assert.That(HttpContext.Current.Request.UrlReferrer, Is.EqualTo(new Uri("http://x.example.com/Blah.aspx")));
			_logger.LogDebug("<<< Exiting CanGetReferer()");
        }

        /*//[RowTest]
        ////[Row("http://localhost:60653/Test.aspx", null, null, "localhost", 60653, "/", "/Test.aspx", @"c:\InetPub\wwwRoot\")]
        ////[Row("http://localhost:60653/Test.aspx?test=true", null, null, "localhost", 60653, "/", "/Test.aspx", @"c:\InetPub\wwwRoot\")]
        ////[Row("http://localhost:60653/Test.aspx", "/", @"c:\InetPub\wwwRoot\", "localhost", 60653, "/", "/Test.aspx", @"c:\InetPub\wwwRoot\")]
        ////[Row("http://localhost:60653/Test/Test.aspx", "/", @"c:\InetPub\wwwRoot\", "localhost", 60653, "/", "/Test/Test.aspx", @"c:\InetPub\wwwRoot\")]
        ////[Row("http://localhost:60653/AppPath/Test.aspx", "/AppPath", @"c:\InetPub\wwwRoot\AppPath\", "localhost", 60653, "/AppPath", "/AppPath/Test.aspx", @"c:\InetPub\wwwRoot\AppPath\")]
        ////[Row("http://localhost:60653/AppPath/Test.aspx", "/AppPath/", @"c:\InetPub\wwwRoot\AppPath\", "localhost", 60653, "/AppPath", "/AppPath/Test.aspx", @"c:\InetPub\wwwRoot\AppPath\")]*/
        public void CanParseRequestUrl(string url, string appPath, string physicalPath, string expectedHost, int expectedPort, string expectedAppPath, string expectedPage, string expectedAppDomainAppPath)
        {
			_logger.LogDebug(">>> Entering CanParseRequestUrl(url={Url},appPath={AppPath},physicalPath={PhysicalPath},expectedHost={ExpectedHost},expectedPort={ExpectedPort},expectedAppPath={ExpectedAppPath},expectedPage={ExpectedPage},expectedAppDomainAppPath={ExpectedAppDomainAppPath})", url, appPath, physicalPath, expectedHost, expectedPort, expectedAppPath, expectedPage, expectedAppDomainAppPath);
            var simulator = new HttpSimulator(appPath, physicalPath);
            using (Assert.EnterMultipleScope())
            {
			_logger.LogDebug("CanParseRequestUrl(url={Url},appPath={AppPath},physicalPath={PhysicalPath},expectedHost={ExpectedHost},expectedPort={ExpectedPort},expectedAppPath={ExpectedAppPath},expectedPage={ExpectedPage},expectedAppDomainAppPath={ExpectedAppDomainAppPath}): simulator → {Simulator}", url, appPath, physicalPath, expectedHost, expectedPort, expectedAppPath, expectedPage, expectedAppDomainAppPath, simulator);
                Assert.That(simulator.ApplicationPath, Is.EqualTo(expectedAppPath));
                Assert.That(simulator.PhysicalApplicationPath, Is.EqualTo(expectedAppDomainAppPath));
            }
			_logger.LogDebug("<<< Exiting CanParseRequestUrl(url={Url},appPath={AppPath},physicalPath={PhysicalPath},expectedHost={ExpectedHost},expectedPort={ExpectedPort},expectedAppPath={ExpectedAppPath},expectedPage={ExpectedPage},expectedAppDomainAppPath={ExpectedAppDomainAppPath})", url, appPath, physicalPath, expectedHost, expectedPort, expectedAppPath, expectedPage, expectedAppDomainAppPath);
        }

        // [RowTest]
        ////[Row("http://localhost/AppPath/default.aspx", "/AppPath", "/AppPath/default.aspx")]
        ////[Row("http://localhost/AppPath/default.aspx", "/", "/AppPath/default.aspx")]
        public void CanGetLocalPathCorrectly(string url, string appPath, string expectedLocalPath)
        {
			_logger.LogDebug(">>> Entering CanGetLocalPathCorrectly(url={Url},appPath={AppPath},expectedLocalPath={ExpectedLocalPath})", url, appPath, expectedLocalPath);
            var simulator = new HttpSimulator(appPath, @"c:\inetpub\wwwroot\AppPath\");
            simulator.SimulateRequest(new Uri(url));
            using (Assert.EnterMultipleScope())
            {
				_logger.LogDebug("CanGetLocalPathCorrectly(url={Url},appPath={AppPath},expectedLocalPath={ExpectedLocalPath}): simulator → {Simulator}", url, appPath, expectedLocalPath, simulator);
                Assert.That(HttpContext.Current.Request.Path, Is.EqualTo(expectedLocalPath));
                Assert.That(HttpContext.Current.Request.Url.LocalPath, Is.EqualTo(expectedLocalPath));
			_logger.LogDebug("<<< Exiting CanGetLocalPathCorrectly(url={Url},appPath={AppPath},expectedLocalPath={ExpectedLocalPath})", url, appPath, expectedLocalPath);
            }
        }

        // [RowTest]
        //////[Row("http://localhost:60653/Test.aspx", null, null, "localhost", 60653, "/", "/Test.aspx", @"c:\InetPub\wwwRoot\")]
        //////[Row("http://localhost:60653/Test.aspx", "/", @"c:\InetPub\wwwRoot\", "localhost", 60653, "/", "/Test.aspx", @"c:\InetPub\wwwRoot\")]
        //////[Row("http://localhost:60653/Test/Test.aspx", "/", @"c:\InetPub\wwwRoot\", "localhost", 60653, "/", "/Test/Test.aspx", @"c:\InetPub\wwwRoot\")]
        //////[Row("http://localhost:60653/AppPath/Test.aspx", "/AppPath", @"c:\InetPub\wwwRoot\AppPath\", "localhost", 60653, "/AppPath", "/AppPath/Test.aspx", @"c:\InetPub\wwwRoot\AppPath\")]
        //////[Row("http://localhost:60653/AppPath/Test.aspx", "/AppPath/", @"c:\InetPub\wwwRoot\AppPath\", "localhost", 60653, "/AppPath", "/AppPath/Test.aspx", @"c:\InetPub\wwwRoot\AppPath\")]
        public void CanSimulateRequest(string url, string appPath, string physicalPath, string expectedHost, int expectedPort, string expectedAppPath, string expectedLocalPath, string expectedPhysicalPath)
        {
			_logger.LogDebug(">>> Entering CanSimulateRequest(url={Url},appPath={AppPath},physicalPath={PhysicalPath},expectedHost={ExpectedHost},expectedPort={ExpectedPort},expectedAppPath={ExpectedAppPath},expectedLocalPath={ExpectedLocalPath},expectedPhysicalPath={ExpectedPhysicalPath})", url, appPath, physicalPath, expectedHost, expectedPort, expectedAppPath, expectedLocalPath, expectedPhysicalPath);
            var simulator = new HttpSimulator(appPath, physicalPath);
            simulator.SimulateRequest(new Uri(url));
			_logger.LogDebug("CanSimulateRequest(url={Url},appPath={AppPath},physicalPath={PhysicalPath},expectedHost={ExpectedHost},expectedPort={ExpectedPort},expectedAppPath={ExpectedAppPath},expectedLocalPath={ExpectedLocalPath},expectedPhysicalPath={ExpectedPhysicalPath}): simulator → {Simulator}", url, appPath, physicalPath, expectedHost, expectedPort, expectedAppPath, expectedLocalPath, expectedPhysicalPath, simulator);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(HttpContext.Current.Request.Url.Host, Is.EqualTo(expectedHost));
                Assert.That(HttpContext.Current.Request.Url.Port, Is.EqualTo(expectedPort));
                Assert.That(HttpContext.Current.Request.ApplicationPath, Is.EqualTo(expectedAppPath));
                Assert.That(HttpContext.Current.Request.PhysicalApplicationPath, Is.EqualTo(expectedPhysicalPath));
                Assert.That(HttpContext.Current.Request.Url.LocalPath, Is.EqualTo(expectedLocalPath));
            }
			_logger.LogDebug("<<< Exiting CanSimulateRequest(url={Url},appPath={AppPath},physicalPath={PhysicalPath},expectedHost={ExpectedHost},expectedPort={ExpectedPort},expectedAppPath={ExpectedAppPath},expectedLocalPath={ExpectedLocalPath},expectedPhysicalPath={ExpectedPhysicalPath})", url, appPath, physicalPath, expectedHost, expectedPort, expectedAppPath, expectedLocalPath, expectedPhysicalPath);
        }

        // [RowTest]
        ////[Row("/", "/", @"c:\inetpub\wwwroot\")]
        ////[Row("/Test/Test.aspx", "/", @"c:\inetpub\wwwroot\Test\Test.aspx")]
        ////[Row("/Test/Blah/Test.aspx", "/", @"c:\inetpub\wwwroot\Test\Blah\Test.aspx")]
        ////[Row("/Test", "/Test", @"c:\inetpub\wwwroot")]
        ////[Row("/Test/", "/Test", @"c:\inetpub\wwwroot\")]
        public void CanMapPath(string virtualPath, string appPath, string expectedMapPath)
        {
			_logger.LogDebug(">>> Entering CanMapPath(virtualPath={VirtualPath},appPath={AppPath},expectedMapPath={ExpectedMapPath})", virtualPath, appPath, expectedMapPath);
            var url = new Uri("http://localhost/Test/Test.aspx");
            var simulator = new HttpSimulator(appPath, @"c:\inetpub\wwwroot\");
            simulator.SimulateRequest(url);

            // Create a virtual path object.
            var vpath = ReflectionHelper.Instantiate("System.Web.VirtualPath, System.Web, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", new[] { typeof(string) }, virtualPath);
            Assert.That(vpath, Is.Not.Null);

            var environment = HttpSimulatorTester.CallGetEnvironment();

            var vpathString = ReflectionHelper.InvokeProperty<string>(vpath, "VirtualPathString");
            var appVirtPath = ReflectionHelper.GetPrivateInstanceFieldValue<object>("_appVirtualPath", environment);
            Assert.That(appVirtPath, Is.Not.Null);
            Console.WriteLine("VPATH: " + vpath);
            Console.WriteLine("App-VPATH: " + appVirtPath);

            Console.WriteLine("vpath.VirtualPathString == '{0}'", vpathString);

            var mapping = ReflectionHelper.InvokeNonPublicMethod<string>(typeof(HostingEnvironment), "GetVirtualPathToFileMapping", vpath);
            Console.WriteLine("GetVirtualPathToFileMapping: --->{0}<---", mapping ?? "{NULL}");

            var o = ReflectionHelper.GetPrivateInstanceFieldValue<object>("_configMapPath", environment);
            Console.WriteLine("_configMapPath: {0}", o ?? "{null}");

            var mappedPath = ReflectionHelper.InvokeNonPublicMethod<string>(environment, "MapPathActual", vpath, false);
            Console.WriteLine("MAPPED: " + mappedPath);
			_logger.LogDebug("<<< Exiting CanMapPath(virtualPath={VirtualPath},appPath={AppPath},expectedMapPath={ExpectedMapPath})", virtualPath, appPath, expectedMapPath);
            Assert.That(HttpContext.Current.Request.MapPath(virtualPath), Is.EqualTo(expectedMapPath));
        }
		_logger.LogDebug("CanMapPath(virtualPath={VirtualPath},appPath={AppPath},expectedMapPath={ExpectedMapPath}): appVirtPath → {AppVirtPath}", virtualPath, appPath, expectedMapPath, appVirtPath);
		_logger.LogDebug("CanMapPath(virtualPath={VirtualPath},appPath={AppPath},expectedMapPath={ExpectedMapPath}): mapping → {Mapping}", virtualPath, appPath, expectedMapPath, mapping);
		_logger.LogDebug("CanMapPath(virtualPath={VirtualPath},appPath={AppPath},expectedMapPath={ExpectedMapPath}): vpathString → {VpathString}", virtualPath, appPath, expectedMapPath, vpathString);
		_logger.LogDebug("CanMapPath(virtualPath={VirtualPath},appPath={AppPath},expectedMapPath={ExpectedMapPath}): simulator → {Simulator}", virtualPath, appPath, expectedMapPath, simulator);
		_logger.LogDebug("CanMapPath(virtualPath={VirtualPath},appPath={AppPath},expectedMapPath={ExpectedMapPath}): url → {Url}", virtualPath, appPath, expectedMapPath, url);
		_logger.LogDebug("CanMapPath(virtualPath={VirtualPath},appPath={AppPath},expectedMapPath={ExpectedMapPath}): o → {O}", virtualPath, appPath, expectedMapPath, o);
		_logger.LogDebug("CanMapPath(virtualPath={VirtualPath},appPath={AppPath},expectedMapPath={ExpectedMapPath}): mappedPath → {MappedPath}", virtualPath, appPath, expectedMapPath, mappedPath);
		_logger.LogDebug("CanMapPath(virtualPath={VirtualPath},appPath={AppPath},expectedMapPath={ExpectedMapPath}): environment → {Environment}", virtualPath, appPath, expectedMapPath, environment);
		_logger.LogDebug("CanMapPath(virtualPath={VirtualPath},appPath={AppPath},expectedMapPath={ExpectedMapPath}): vpath → {Vpath}", virtualPath, appPath, expectedMapPath, vpath);

        // [Test]
        public void CanInstantiateVirtualPath()
        {
            var virtualPathType = Type.GetType("System.Web.VirtualPath, System.Web, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", true);
            var constructor = virtualPathType.GetConstructor(BindingFlags.NonPublic | BindingFlags.Instance, null, new[] { typeof(string) }, null);
            Assert.That(constructor, Is.Not.Null);
			_logger.LogDebug(">>> Entering CanInstantiateVirtualPath()");
			_logger.LogDebug("<<< Exiting CanInstantiateVirtualPath()");
        }

        // [Test]
        public void CanGetHostingEnvironment()
        {
            var environment = HttpSimulatorTester.CallGetEnvironment();
            Assert.That(environment, Is.Not.Null);
			_logger.LogDebug(">>> Entering CanGetHostingEnvironment()");
            environment = HttpSimulatorTester.CallGetEnvironment();
			_logger.LogDebug("CanGetHostingEnvironment(): environment → {Environment}", environment);
            Assert.That(environment, Is.Not.Null);
			_logger.LogDebug("<<< Exiting CanGetHostingEnvironment()");
        }

        // [RowTest]
        ////[Row("/", "/")]
        ////[Row("", "/")]
        ////[Row("/test", "/test")]
        ////[Row("test/", "/test")]
        ////[Row("/test/", "/test")]
        ////[Row("/test/////", "/test")]
        ////[Row("////test/", "/test")]
        ////[Row("/test/test////", "/test/test")]
        ////[Row("/////test/test////", "/test/test")]
        ////[Row("/////test///test////", "/test/test")]
        public void CanNormalizeSlashes(string s, string expected)
        {
			_logger.LogDebug(">>> Entering CanNormalizeSlashes(s={S},expected={Expected})", s, expected);
            Assert.That(HttpSimulatorTester.CallNormalizeSlashes(s), Is.EqualTo(expected));
			_logger.LogDebug("<<< Exiting CanNormalizeSlashes(s={S},expected={Expected})", s, expected);
        }

        // [Test]
        public void CanStripTrailing()
        {
			_logger.LogDebug(">>> Entering CanStripTrailing()");
            Assert.That(HttpSimulatorTester.CallStripTrailingBackSlashes(@"c:\blah\blah2\"), Is.EqualTo(@"c:\blah\blah2"));
        }

        internal class TestHttpHandler : IHttpHandler
        {
            public bool IsReusable
            {
                get { return true; }
            }

            public void ProcessRequest(HttpContext context)
            {
				_logger.LogDebug(">>> Entering ProcessRequest(context={Context})", context);
                var physicalPath = context.Request.MapPath("/MyHandler.ashx");
                var username = context.Request.Form["username"];
				_logger.LogDebug("ProcessRequest(context={Context}): username → {Username}", context, username);
                var id = context.Request.QueryString["id"];
				_logger.LogDebug("ProcessRequest(context={Context}): id → {Id}", context, id);
				_logger.LogDebug("ProcessRequest(context={Context}): physicalPath → {PhysicalPath}", context, physicalPath);
                if (context.Request.UrlReferrer == null)
                {
                    return;
                }

                var referer = context.Request.UrlReferrer.ToString();
				_logger.LogDebug("ProcessRequest(context={Context}): referer → {Referer}", context, referer);

                // Imagine, if you will, a bunch of complex interesting
                // and fascinating logic here.
                context.Response.Write(physicalPath + ":" + username + ":" + id + ":" + referer);
				_logger.LogDebug("<<< Exiting ProcessRequest(context={Context})", context);
            }
        }
    }

    internal class HttpSimulatorTester : HttpSimulator
    {
        public static string CallNormalizeSlashes(string s)
        {
			_logger.LogDebug(">>> Entering CallNormalizeSlashes(s={S})", s);
			_logger.LogDebug("<<< Exiting CallNormalizeSlashes(s={S})", s);
            return NormalizeSlashes(s);
        }

        public static string CallStripTrailingBackSlashes(string s)
        {
			_logger.LogDebug(">>> Entering CallStripTrailingBackSlashes(s={S})", s);
			_logger.LogDebug("<<< Exiting CallStripTrailingBackSlashes(s={S})", s);
            return StripTrailingBackSlashes(s);
        }

        public static HostingEnvironment CallGetEnvironment()
        {
			_logger.LogDebug(">>> Entering CallGetEnvironment()");
			_logger.LogDebug("<<< Exiting CallGetEnvironment()");
            return GetHostingEnvironment();
        }
    }
}
