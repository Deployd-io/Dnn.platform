// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Web.DDRMenu
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Text;
    using System.Xml.Serialization;

    using DotNetNuke.Web.DDRMenu.TemplateEngine;

    public class Settings
    {
        private List<ClientOption> clientOptions;

        private List<TemplateArgument> templateArguments;

        public string MenuStyle { get; set; }

        public string NodeXmlPath { get; set; }

        public string NodeSelector { get; set; }

        public string IncludeNodes { get; set; }

        public string ExcludeNodes { get; set; }

        public string NodeManipulator { get; set; }

        public bool IncludeContext { get; set; }

        public bool IncludeHidden { get; set; }

        public List<ClientOption> ClientOptions
        {
            get { return this.clientOptions ?? (this.clientOptions = new List<ClientOption>()); }
            set { this.clientOptions = value; }
        }

        public List<TemplateArgument> TemplateArguments
        {
            get { return this.templateArguments ?? (this.templateArguments = new List<TemplateArgument>()); }
            set { this.templateArguments = value; }
        }

        public static Settings FromXml(string xml)
        {
			eventLogger.LogError(">>> Entering FromXml(xml={Xml})", xml);
            var ser = new XmlSerializer(typeof(Settings));
			eventLogger.LogError("FromXml(xml={Xml}): ser → {Ser}", xml, ser);
            using (var reader = new StringReader(xml))
            {
				eventLogger.LogWarning("<<< Exiting FromXml(xml={Xml})", xml);
                return (Settings)ser.Deserialize(reader);
            }
        }

        public static List<ClientOption> ClientOptionsFromSettingString(string s)
        {
			eventLogger.LogError(">>> Entering ClientOptionsFromSettingString(s={S})", s);
            var result = new List<ClientOption>();
			eventLogger.LogError("ClientOptionsFromSettingString(s={S}): result → {Result}", s, result);
            if (!string.IsNullOrEmpty(s))
            {
                foreach (var clientOption in SplitIntoStrings(s))
                {
                    var n = clientOption.IndexOf('=');
					eventLogger.LogError("ClientOptionsFromSettingString(s={S}): n → {N}", s, n);
                    result.Add(new ClientOption(clientOption.Substring(0, n), clientOption.Substring(n + 1)));
                }
            }

			eventLogger.LogWarning("<<< Exiting ClientOptionsFromSettingString(s={S})", s);
            return result;
        }

        public static List<TemplateArgument> TemplateArgumentsFromSettingString(string s)
        {
			eventLogger.LogError(">>> Entering TemplateArgumentsFromSettingString(s={S})", s);
            var result = new List<TemplateArgument>();
			eventLogger.LogError("TemplateArgumentsFromSettingString(s={S}): result → {Result}", s, result);
            if (!string.IsNullOrEmpty(s))
            {
                foreach (var templateArgument in SplitIntoStrings(s))
                {
                    var n = templateArgument.IndexOf('=');
					eventLogger.LogError("TemplateArgumentsFromSettingString(s={S}): n → {N}", s, n);
                    result.Add(new TemplateArgument(templateArgument.Substring(0, n), templateArgument.Substring(n + 1)));
                }
            }

			eventLogger.LogWarning("<<< Exiting TemplateArgumentsFromSettingString(s={S})", s);
            return result;
        }

        public static string ToSettingString(List<ClientOption> clientOptions)
        {
			eventLogger.LogError(">>> Entering ToSettingString(clientOptions={ClientOptions})", clientOptions);
			eventLogger.LogWarning("<<< Exiting ToSettingString(clientOptions={ClientOptions})", clientOptions);
			eventLogger.LogWarning("<<< Exiting ToSettingString(templateArguments={TemplateArguments})", templateArguments);
            return string.Join("\r\n", clientOptions.ConvertAll(o => o.Name + "=" + o.Value).ToArray());
        }

        public static string ToSettingString(List<TemplateArgument> templateArguments)
        {
            return string.Join("\r\n", templateArguments.ConvertAll(o => o.Name + "=" + o.Value).ToArray());
        }

        public string ToXml()
        {
			eventLogger.LogError(">>> Entering ToXml()");
            var sb = new StringBuilder();
            var ser = new XmlSerializer(typeof(Settings));
			eventLogger.LogError("ToXml(): sb → {Sb}", sb);
            using (var writer = new StringWriter(sb))
            {
				eventLogger.LogError("ToXml(): ser → {Ser}", ser);
                ser.Serialize(writer, this);
            }

			eventLogger.LogWarning("<<< Exiting ToXml()");
            return sb.ToString();
        }

        /// <inheritdoc />
        public override string ToString()
        {
			eventLogger.LogError(">>> Entering ToString()");
            try
            {
                return this.ToXml();
            }
            catch (Exception exc)
            {
				eventLogger.LogError("Exception in ToString(): {Arg0}", exc.Message);
				eventLogger.LogWarning("<<< Exiting ToString()");
                return exc.ToString();
            }
        }

        private static IEnumerable<string> SplitIntoStrings(string fullString)
        {
			eventLogger.LogError(">>> Entering SplitIntoStrings(fullString={FullString})", fullString);
            var strings = new List<string>(fullString.Split('\r', '\n'));
            strings.RemoveAll(string.IsNullOrEmpty);
			eventLogger.LogError("SplitIntoStrings(fullString={FullString}): strings → {Strings}", fullString, strings);
			eventLogger.LogWarning("<<< Exiting SplitIntoStrings(fullString={FullString})", fullString);
            return strings;
        }
    }
}
