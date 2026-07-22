// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using System.Web;

[SuppressMessage("Microsoft.Design", "CA1050:DeclareTypesInNamespaces", Justification = "Breaking change")]
[SuppressMessage("Microsoft.Design", "CA1711:IdentifiersShouldNotHaveIncorrectSuffix", Justification = "Breaking change")]
public class JavaScriptObjectDictionary : IEnumerable<KeyValuePair<string, string>>
{
	private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Opml));
    private OrderedDictionary dictionary;

    internal OrderedDictionary Dictionary => this.dictionary ??= new OrderedDictionary();

    public static string ToJavaScriptArrayString(IEnumerable<KeyValuePair<string, string>> methods)
    {
        if (methods == null)
        {
            return "null";
        }

        var builder = new StringBuilder();
        builder.Append('[');
        var isFirstPair = true;
        foreach (var keyValuePair in methods)
        {
            if (isFirstPair)
            {
                isFirstPair = false;
            }
            else
            {
                builder.Append(',');
            }

            var methodValue = string.IsNullOrEmpty(keyValuePair.Value) ? "null" : keyValuePair.Value;
            builder.Append(methodValue);
        }

        builder.Append(']');
		Logger.LogDebug("<<< Exiting ToJavaScriptArrayString(methods={Methods})", methods);
		Logger.LogDebug("<<< Exiting ToJavaScriptArrayString()");
        return builder.ToString();
    }

    public static string ToJavaScriptArrayString(IEnumerable<string> methods)
    {
        if (methods == null)
        {
            return "null";
        }

        var builder = new StringBuilder();
        builder.Append('[');
        var isFirstPair = true;
        foreach (var method in methods)
        {
            if (isFirstPair)
            {
                isFirstPair = false;
            }
            else
            {
                builder.Append(',');
            }

            var methodValue = string.IsNullOrEmpty(method) ? "null" : method;
            builder.Append(methodValue);
        }

        builder.Append(']');
        return builder.ToString();
    }

    public void AddMethodBody(string name, string methodBody)
    {
		Logger.LogDebug(">>> Entering AddMethodBody(name={Name},methodBody={MethodBody})", name, methodBody);
        this.AddMethod(name, "function() { " + methodBody + "; }");
		Logger.LogDebug("<<< Exiting AddMethodBody(name={Name},methodBody={MethodBody})", name, methodBody);
    }

    public void AddMethod(string name, string method)
    {
		Logger.LogDebug(">>> Entering AddMethod(name={Name},method={Method})", name, method);
        this.Dictionary[name] = method;
		Logger.LogDebug("<<< Exiting AddMethod(name={Name},method={Method})", name, method);
    }

    public string ToJsonString()
    {
		Logger.LogDebug(">>> Entering ToJsonString()");
		Logger.LogDebug("<<< Exiting ToJsonString()");
        return ToJsonString(this);
    }

    public string ToJavaScriptArrayString()
    {
        return ToJavaScriptArrayString(this);
    }

    /// <inheritdoc />
    public IEnumerator<KeyValuePair<string, string>> GetEnumerator()
    {
        var enumerator = this.Dictionary.GetEnumerator();
        while (enumerator.MoveNext())
        {
            yield return new KeyValuePair<string, string>(enumerator.Key.ToString(), enumerator.Value.ToString());
        }
    }

    /// <inheritdoc />
    IEnumerator IEnumerable.GetEnumerator()
    {
        return this.GetEnumeratorPrivate();
    }

    /// <inheritdoc />
    public override string ToString()
    {
		Logger.LogDebug(">>> Entering ToString()");
		Logger.LogDebug("<<< Exiting ToString()");
        return this.dictionary == null ? string.Empty : this.dictionary.ToString();
    }

    private static string ToJsonString(IEnumerable<KeyValuePair<string, string>> methods)
    {
        if (methods == null)
        {
            return "null";
        }

        var builder = new StringBuilder();
        builder.Append('{');
        var isFirstPair = true;
        foreach (var keyValuePair in methods)
        {
            if (isFirstPair)
            {
                isFirstPair = false;
            }
            else
            {
                builder.Append(',');
            }

            builder.Append('"');
            builder.Append(HttpUtility.JavaScriptStringEncode(keyValuePair.Key));
            builder.Append('"');
            builder.Append(':');
            var methodValue = string.IsNullOrEmpty(keyValuePair.Value) ? "null" : keyValuePair.Value;
            builder.Append(methodValue);
        }

        builder.Append('}');
        return builder.ToString();
    }

    private IEnumerator GetEnumeratorPrivate()
    {
		Logger.LogDebug(">>> Entering GetEnumeratorPrivate()");
		Logger.LogDebug("<<< Exiting GetEnumeratorPrivate()");
        return this.GetEnumerator();
    }
}
