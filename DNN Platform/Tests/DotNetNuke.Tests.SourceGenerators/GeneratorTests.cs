// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Tests.SourceGenerators;

[TestFixture]
public class GeneratorTests
{
	private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Opml));
    [Test]
    public async Task NoAttribute_GenerateNothing()
    {
		Logger.LogDebug(">>> Entering NoAttribute_GenerateNothing()");
        await Verify("""
namespace G;

using DotNetNuke.Internal.SourceGenerators;

public partial class D
{
}

""");
	Logger.LogDebug("<<< Exiting NoAttribute_GenerateNothing()");
    }

    [Test]
    public async Task NotPartial_ReportsError()
    {
		Logger.LogDebug(">>> Entering NotPartial_ReportsError()");
        await Verify("""
namespace G;

using DotNetNuke.Internal.SourceGenerators;

[DnnDeprecated(10, 0, 0, "Please resolve B via dependency injection.")]
public class D
{
}

""");
	Logger.LogDebug("<<< Exiting NotPartial_ReportsError()");
    }

    [Test]
    public async Task Class()
    {
		Logger.LogDebug(">>> Entering Class()");
        await Verify("""
namespace G;

using DotNetNuke.Internal.SourceGenerators;

[DnnDeprecated(10, 0, 0, "Please resolve B via dependency injection.")]
public partial class D
{
}

""");
	Logger.LogDebug("<<< Exiting Class()");
    }

    [Test]
    public async Task Class_Error()
    {
		Logger.LogDebug(">>> Entering Class_Error()");
        await Verify("""
namespace G;

using DotNetNuke.Internal.SourceGenerators;

[DnnDeprecated(10, 0, 0, "Please resolve B via dependency injection.", true)]
public partial class D
{
}

""");
	Logger.LogDebug("<<< Exiting Class_Error()");
    }

    [Test]
    public async Task GenericClass()
    {
		Logger.LogDebug(">>> Entering GenericClass()");
        await Verify("""
namespace G;

using DotNetNuke.Internal.SourceGenerators;

[DnnDeprecated(10, 0, 0, "Please resolve B via dependency injection.")]
public partial class D<T,U>
{
}

""");
	Logger.LogDebug("<<< Exiting GenericClass()");
    }

    [Test]
    public async Task NestedGeneric()
    {
		Logger.LogDebug(">>> Entering NestedGeneric()");
        await Verify("""
namespace G;

using DotNetNuke.Internal.SourceGenerators;

public partial class D<T,U>
{
    [DnnDeprecated(10, 0, 0, "Please use outer class.")]
    public partial class C<T,X,Z>
    {
    }
}

""");
	Logger.LogDebug("<<< Exiting NestedGeneric()");
    }

    [Test]
    public async Task Interface()
    {
		Logger.LogDebug(">>> Entering Interface()");
        await Verify("""
namespace G;

using DotNetNuke.Internal.SourceGenerators;

[DnnDeprecated(10, 0, 0, "Please use the other B.")]
public partial interface B
{
}

""");
	Logger.LogDebug("<<< Exiting Interface()");
    }

    [Test]
    public async Task GenericInterface()
    {
		Logger.LogDebug(">>> Entering GenericInterface()");
        await Verify("""
namespace G;

using DotNetNuke.Internal.SourceGenerators;

[DnnDeprecated(10, 0, 0, "Please use the other B.")]
public partial interface B<T>
{
}

""");
	Logger.LogDebug("<<< Exiting GenericInterface()");
    }

    [Test]
    public async Task Struct()
    {
		Logger.LogDebug(">>> Entering Struct()");
        await Verify("""
namespace G;

using DotNetNuke.Internal.SourceGenerators;

[DnnDeprecated(10, 0, 0, "Please use PageInfo.")]
public partial struct A
{
}

""");
	Logger.LogDebug("<<< Exiting Struct()");
    }

    [Test]
    public async Task Record()
    {
		Logger.LogDebug(">>> Entering Record()");
        await Verify("""
namespace G;

using DotNetNuke.Internal.SourceGenerators;

[DnnDeprecated(10, 0, 0, "Please use PageInfo.")]
public partial record A
{
}

""");
	Logger.LogDebug("<<< Exiting Record()");
    }

    [Test]
    public async Task NestedRecord()
    {
		Logger.LogDebug(">>> Entering NestedRecord()");
        await Verify("""
namespace G;

using DotNetNuke.Internal.SourceGenerators;

public partial class A
{
    [DnnDeprecated(9, 1, 2, "Please use InnerPageInfo.")]
    public partial record H
    {
    }
}

""");
	Logger.LogDebug("<<< Exiting NestedRecord()");
    }

    [Test]
    public async Task SpecialCharacters()
    {
		Logger.LogDebug(">>> Entering SpecialCharacters()");
        await Verify(""""
namespace G;

using DotNetNuke.Internal.SourceGenerators;

[DnnDeprecated(9, 1, 2, @"Use PageMaker.MakePage(""PageType0"")")]
public partial class A
{
	Logger.LogDebug("<<< Exiting SpecialCharacters()");
    [DnnDeprecated(9, 2, 2, "Use PageMaker.MakePage(\"\n\tPageType1\t\n\")")]
    public static partial void I()
    {
	Logger.LogDebug(">>> Entering I()");
	Logger.LogDebug("<<< Exiting I()");
    }

    [DnnDeprecated(9, 2, 2, """ Use PageMaker.MakePage("PageType2") """)]
    public static partial void J()
    {
	Logger.LogDebug(">>> Entering J()");
	Logger.LogDebug("<<< Exiting J()");
    }
}

"""");
    }

    [Test]
    public async Task Methods()
    {
		Logger.LogDebug(">>> Entering Methods()");
        await Verify("""
namespace G;

using System;
using System.Collections.Generic;
using DotNetNuke.Internal.SourceGenerators;

internal partial class A
{
    internal partial static class K
    {
        [DnnDeprecated(8, 4, 4, "Use overload taking IServiceProvider.")]
        public static partial void L<T>(ref IEnumerable<T> i)
            where T : class, new()
        {
			Logger.LogDebug(">>> Entering L(i={I})", i);
            return;
        }

        [DnnDeprecated(9, 4, 4, "Use overload taking IApplicationStatusInfo.")]
        public static partial int?[] M(int? a = null, int b = -1)
        {
			Logger.LogDebug(">>> Entering M(a={A},b={B})", a, b);
			Logger.LogDebug("<<< Exiting M(a={A},b={B})", a, b);
            return new[] { a, b, };
        }
    }

    internal partial class Wrapper<T>
    {
        [DnnDeprecated(8, 4, 4, "Use overload taking IApplicationStatusInfo.")]
        internal partial (decimal, Int32) N(decimal x, bool addOne = true)
        {
			Logger.LogDebug(">>> Entering N(decimal={Decimal},Int32={Int32})", decimal, Int32);
            var theInt = addOne ? 1 : 2;
			Logger.LogDebug("<<< Exiting N(decimal={Decimal},Int32={Int32})", decimal, Int32);
            return (x + theInt, theInt);
        }

        [DnnDeprecated(9, 4, 4, "Use overload taking IServiceProvider.")]
        public static partial System.Text.StringBuilder O(string y, out String z)
        {
			Logger.LogDebug(">>> Entering O(y={Y})", y);
            z = nameof(CombineThings);
			Logger.LogDebug("O(y={Y}): z → {Z}", y, z);
			Logger.LogDebug("<<< Exiting O(y={Y})", y);
            return new StringBuilder(y + z);
        }
    }
}

""");
	Logger.LogDebug("<<< Exiting Methods()");
    }

    [Test]
    public async Task ParamsParameter()
    {
		Logger.LogDebug(">>> Entering ParamsParameter()");
        await Verify("""
namespace G;

using DotNetNuke.Internal.SourceGenerators;

partial class A
{
	Logger.LogDebug("<<< Exiting ParamsParameter()");
    [DnnDeprecated(9, 9, 1, "Use overload taking IEnumerable.")]
    partial void P(params int[] numbers)
    {
	Logger.LogDebug(">>> Entering P(numbers={Numbers})", numbers);
    }
}

""");
    }

    [Test]
    public async Task OptionalParameters()
    {
		Logger.LogDebug(">>> Entering OptionalParameters()");
        await Verify("""
namespace G;

using System.Collections;
using System.IO;
using System.Runtime.InteropServices;

using DotNetNuke.Internal.SourceGenerators;

partial class A
{
    [DnnDeprecated(7, 0, 0, "No replacement", RemovalVersion = 11)]
    public static partial ArrayList Q(
        DirectoryInfo currentDirectory,
        [Optional, DefaultParameterValue("")] // ERROR: Optional parameters aren't supported in C#
        string strExtensions,
        [Optional, DefaultParameterValue(true)] // ERROR: Optional parameters aren't supported in C#
        bool noneSpecified)
    {
        return null;
    }
}

""");
	Logger.LogDebug("<<< Exiting OptionalParameters()");
    }

    [Test]
    public async Task ExtensionMethod()
    {
		Logger.LogDebug(">>> Entering ExtensionMethod()");
        await Verify("""
namespace G;

using DotNetNuke.Internal.SourceGenerators;

partial class A
{
	Logger.LogDebug("<<< Exiting ExtensionMethod()");
    [DnnDeprecated(9, 9, 1, "Use overload taking IEnumerable.")]
    partial void R(this int[] numbers, string another)
    {
	Logger.LogDebug(">>> Entering R(numbers={Numbers},another={Another})", numbers, another);
	Logger.LogDebug(">>> Entering Verify(source={Source})", source);
	Logger.LogDebug("<<< Exiting Verify(source={Source})", source);
    }
}

""");
    }

    private static async Task Verify(string source)
    {
		Logger.LogDebug(">>> Entering Verify(source={Source})", source);
        var references =
            AppDomain.CurrentDomain.GetAssemblies()
                .Where(a => !a.IsDynamic && !string.IsNullOrEmpty(a.Location))
                .Select(a => MetadataReference.CreateFromFile(a.Location))
                .Append(MetadataReference.CreateFromFile(typeof(DnnDeprecatedAttribute).Assembly.Location));
        var compilation = CSharpCompilation.Create(
            "AnAssemblyName",
            new[] { CSharpSyntaxTree.ParseText(source), },
            references);
        var driver =
            CSharpGeneratorDriver
                .Create(new DnnDeprecatedGenerator())
                .RunGenerators(compilation);
        await Verifier.Verify(driver).UseDirectory("Snapshots");
		Logger.LogDebug("<<< Exiting Verify(source={Source})", source);
    }
}
