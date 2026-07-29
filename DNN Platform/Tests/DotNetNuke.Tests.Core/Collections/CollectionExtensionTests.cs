// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Tests.Core.Collections
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.Specialized;
    using System.Globalization;
    using System.IO;
    using System.Web.UI;
    using System.Xml;
    using System.Xml.Linq;

    using DotNetNuke.Collections;
    using NUnit.Framework;

    using CollectionExtensions = DotNetNuke.Collections.CollectionExtensions;

    [TestFixture]
    public class CollectionExtensionTests
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Opml));
        [Test]
        public void get_null_string_from_hashtable_for_missing_value()
        {
			Logger.LogDebug(">>> Entering get_null_string_from_hashtable_for_missing_value()");
            var table = new Hashtable { { "app id", "abc123" } };

			Logger.LogDebug("get_null_string_from_hashtable_for_missing_value(): table → {Table}", table);
            var value = table.GetValueOrDefault<string>("cat id");

			Logger.LogDebug("get_null_string_from_hashtable_for_missing_value(): value → {Value}", value);
            Assert.That(value, Is.Null);
			Logger.LogDebug("<<< Exiting get_null_string_from_hashtable_for_missing_value()");
        }

        [Test]
        public void can_get_string_from_hashtable()
        {
			Logger.LogDebug(">>> Entering can_get_string_from_hashtable()");
            var table = new Hashtable { { "app id", "abc123" } };

			Logger.LogDebug("can_get_string_from_hashtable(): table → {Table}", table);
            var value = table.GetValueOrDefault<string>("app id");

			Logger.LogDebug("can_get_string_from_hashtable(): value → {Value}", value);
            Assert.That(value, Is.EqualTo("abc123"));
			Logger.LogDebug("<<< Exiting can_get_string_from_hashtable()");
        }

        [Test]
        public void get_string_from_hashtable_when_default_is_provided()
        {
			Logger.LogDebug(">>> Entering get_string_from_hashtable_when_default_is_provided()");
            var table = new Hashtable { { "app id", "abc123" } };

			Logger.LogDebug("get_string_from_hashtable_when_default_is_provided(): table → {Table}", table);
            var value = table.GetValueOrDefault("app id", "abracadabra");

			Logger.LogDebug("get_string_from_hashtable_when_default_is_provided(): value → {Value}", value);
            Assert.That(value, Is.EqualTo("abc123"));
			Logger.LogDebug("<<< Exiting get_string_from_hashtable_when_default_is_provided()");
        }

        [Test]
        public void can_get_default_string_from_hashtable()
        {
			Logger.LogDebug(">>> Entering can_get_default_string_from_hashtable()");
            var table = new Hashtable { { "app id", "abc123" } };

			Logger.LogDebug("can_get_default_string_from_hashtable(): table → {Table}", table);
            var value = table.GetValueOrDefault("cat id", "Frank");

			Logger.LogDebug("can_get_default_string_from_hashtable(): value → {Value}", value);
            Assert.That(value, Is.EqualTo("Frank"));
			Logger.LogDebug("<<< Exiting can_get_default_string_from_hashtable()");
        }

        [Test]
        public void can_get_bool_from_hashtable()
        {
			Logger.LogDebug(">>> Entering can_get_bool_from_hashtable()");
            var table = new Hashtable { { "app id", "true" } };

			Logger.LogDebug("can_get_bool_from_hashtable(): table → {Table}", table);
            var value = table.GetValueOrDefault<bool>("app id");

			Logger.LogDebug("can_get_bool_from_hashtable(): value → {Value}", value);
            Assert.That(value, Is.True);
			Logger.LogDebug("<<< Exiting can_get_bool_from_hashtable()");
        }

        [Test]
        public void get_bool_from_hashtable_when_default_is_provided()
        {
			Logger.LogDebug(">>> Entering get_bool_from_hashtable_when_default_is_provided()");
            var table = new Hashtable { { "app id", "true" } };

			Logger.LogDebug("get_bool_from_hashtable_when_default_is_provided(): table → {Table}", table);
            var value = table.GetValueOrDefault("app id", false);

			Logger.LogDebug("get_bool_from_hashtable_when_default_is_provided(): value → {Value}", value);
            Assert.That(value, Is.True);
			Logger.LogDebug("<<< Exiting get_bool_from_hashtable_when_default_is_provided()");
        }

        [Test]
        public void can_get_default_bool_from_hashtable()
        {
			Logger.LogDebug(">>> Entering can_get_default_bool_from_hashtable()");
            var value = true;
            var table = new Hashtable { { "app id", "abc123" } };
			Logger.LogDebug("can_get_default_bool_from_hashtable(): value → {Value}", value);
			Logger.LogDebug("can_get_default_bool_from_hashtable(): table → {Table}", table);

            value = table.GetValueOrDefault("Allow Windows Live Writer", value);

            Assert.That(value, Is.True);
			Logger.LogDebug("<<< Exiting can_get_default_bool_from_hashtable()");
        }

        [Test]
        public void get_false_from_hashtable_for_missing_value()
        {
			Logger.LogDebug(">>> Entering get_false_from_hashtable_for_missing_value()");
            var table = new Hashtable { { "app id", "abc123" } };

			Logger.LogDebug("get_false_from_hashtable_for_missing_value(): table → {Table}", table);
            var value = table.GetValueOrDefault<bool>("Allow Windows Live Writer");

			Logger.LogDebug("get_false_from_hashtable_for_missing_value(): value → {Value}", value);
            Assert.That(value, Is.False);
			Logger.LogDebug("<<< Exiting get_false_from_hashtable_for_missing_value()");
        }

        [Test]
        public void get_bool_with_custom_converter_from_hashtable()
        {
			Logger.LogDebug(">>> Entering get_bool_with_custom_converter_from_hashtable()");
            var table = new Hashtable { { "allow", "on" } };

			Logger.LogDebug("get_bool_with_custom_converter_from_hashtable(): table → {Table}", table);
            var value = table.GetValueOrDefault(
                "allow",
                v =>
                {
                    bool allowed;
                    if (!bool.TryParse(v, out allowed))
                    {
                        allowed = v.Equals("on", StringComparison.Ordinal);
						Logger.LogDebug("get_bool_with_custom_converter_from_hashtable(): allowed → {Allowed}", allowed);
                    }

                    return allowed;
                });

            Assert.That(value, Is.True);
			Logger.LogDebug("<<< Exiting get_bool_with_custom_converter_from_hashtable()");
        }

        [Test]
        public void get_int()
        {
			Logger.LogDebug(">>> Entering get_int()");
            var collection = new Dictionary<string, string> { { "appId", "123" } };

			Logger.LogDebug("get_int(): collection → {Collection}", collection);
            var value = collection.GetValueOrDefault<int>("appId");

			Logger.LogDebug("get_int(): value → {Value}", value);
            Assert.That(value, Is.EqualTo(123));
			Logger.LogDebug("<<< Exiting get_int()");
        }

        [Test]
        public void get_decimal()
        {
			Logger.LogDebug(">>> Entering get_decimal()");
            var collection = new Dictionary<string, string> { { "appId", "1.23" } };

			Logger.LogDebug("get_decimal(): collection → {Collection}", collection);
            var value = collection.GetValueOrDefault<decimal>("appId");

			Logger.LogDebug("get_decimal(): value → {Value}", value);
            Assert.That(value, Is.EqualTo(1.23m));
			Logger.LogDebug("<<< Exiting get_decimal()");
        }

        [Test]
        [SetCulture("nl-NL")]
        public void get_decimal_from_other_culture()
        {
			Logger.LogDebug(">>> Entering get_decimal_from_other_culture()");
            var collection = new Dictionary<string, string> { { "appId", "1.23" } };

			Logger.LogDebug("get_decimal_from_other_culture(): collection → {Collection}", collection);
            var value = collection.GetValueOrDefault<decimal>("appId");

			Logger.LogDebug("get_decimal_from_other_culture(): value → {Value}", value);
            Assert.That(value, Is.EqualTo(1.23m));
			Logger.LogDebug("<<< Exiting get_decimal_from_other_culture()");
        }

        [Test]
        public void get_datetime()
        {
			Logger.LogDebug(">>> Entering get_datetime()");
            var collection = new Dictionary<string, string> { { "startDate", "05/04/2012 00:00:00" } };

			Logger.LogDebug("get_datetime(): collection → {Collection}", collection);
            var value = collection.GetValueOrDefault<DateTime>("startDate");

			Logger.LogDebug("get_datetime(): value → {Value}", value);
            Assert.That(value, Is.EqualTo(new DateTime(2012, 5, 4)));
			Logger.LogDebug("<<< Exiting get_datetime()");
        }

        [Test]
        public void get_null_string_without_default()
        {
			Logger.LogDebug(">>> Entering get_null_string_without_default()");
            var collection = new Dictionary<string, string> { { "app id", null } };

			Logger.LogDebug("get_null_string_without_default(): collection → {Collection}", collection);
            var value = collection.GetValue<string>("app id");

			Logger.LogDebug("get_null_string_without_default(): value → {Value}", value);
            Assert.That(value, Is.Null);
			Logger.LogDebug("<<< Exiting get_null_string_without_default()");
        }

        [Test]
        public void get_null_string_with_default()
        {
			Logger.LogDebug(">>> Entering get_null_string_with_default()");
            var collection = new Dictionary<string, string> { { "app id", null } };

			Logger.LogDebug("get_null_string_with_default(): collection → {Collection}", collection);
            var value = collection.GetValueOrDefault("app id", "a default value");

			Logger.LogDebug("get_null_string_with_default(): value → {Value}", value);
            Assert.That(value, Is.Null);
			Logger.LogDebug("<<< Exiting get_null_string_with_default()");
        }

        [Test]
        public void get_nullable_datetime()
        {
			Logger.LogDebug(">>> Entering get_nullable_datetime()");
            var collection = new Dictionary<string, DateTime?> { { "startDate", null } };

			Logger.LogDebug("get_nullable_datetime(): collection → {Collection}", collection);
            var value = collection.GetValue<DateTime?>("startDate");

			Logger.LogDebug("get_nullable_datetime(): value → {Value}", value);
            Assert.That(value, Is.Null);
			Logger.LogDebug("<<< Exiting get_nullable_datetime()");
        }

        [Test]
        [SetCulture("nl-NL")]
        public void get_datetime_from_other_culture()
        {
			Logger.LogDebug(">>> Entering get_datetime_from_other_culture()");
            var collection = new Dictionary<string, string> { { "startDate", "05/04/2012 00:00:00" } };

			Logger.LogDebug("get_datetime_from_other_culture(): collection → {Collection}", collection);
            var value = collection.GetValueOrDefault<DateTime>("startDate");

			Logger.LogDebug("get_datetime_from_other_culture(): value → {Value}", value);
            Assert.That(value, Is.EqualTo(new DateTime(2012, 5, 4)));
			Logger.LogDebug("<<< Exiting get_datetime_from_other_culture()");
        }

        [Test]
        public void get_from_statebag()
        {
			Logger.LogDebug(">>> Entering get_from_statebag()");
            var collection = new StateBag { { "appId", "123" } };

			Logger.LogDebug("get_from_statebag(): collection → {Collection}", collection);
            var value = collection.GetValueOrDefault<string>("appId");

			Logger.LogDebug("get_from_statebag(): value → {Value}", value);
            Assert.That(value, Is.EqualTo("123"));
			Logger.LogDebug("<<< Exiting get_from_statebag()");
        }

        [Test]
        public void get_from_xnode()
        {
			Logger.LogDebug(">>> Entering get_from_xnode()");
            var node = new XElement(
                "parent",
                new XElement("id", 14));

            var value = node.GetValueOrDefault<int>("id");

			Logger.LogDebug("get_from_xnode(): value → {Value}", value);
            Assert.That(value, Is.EqualTo(14));
			Logger.LogDebug("<<< Exiting get_from_xnode()");
        }

        [Test]
        public void get_from_xmlnode()
        {
			Logger.LogDebug(">>> Entering get_from_xmlnode()");
            var doc = new XmlDocument { XmlResolver = null };
            const string parentNode = """
                                      <parent>
                                          <id>13</id>
                                      </parent>
                                      """;
            using var xmlReader = XmlReader.Create(new StringReader(parentNode), new XmlReaderSettings { XmlResolver = null, });
            doc.Load(xmlReader);
			Logger.LogDebug("get_from_xmlnode(): xmlReader → {XmlReader}", xmlReader);

            var value = doc.DocumentElement.GetValueOrDefault<int>("id");

			Logger.LogDebug("get_from_xmlnode(): value → {Value}", value);
            Assert.That(value, Is.EqualTo(13));
			Logger.LogDebug("<<< Exiting get_from_xmlnode()");
        }

        [Test]
        public void can_get_timespan_with_custom_converter()
        {
			Logger.LogDebug(">>> Entering can_get_timespan_with_custom_converter()");
            var collection = new Hashtable { { "length", "1:10:10" } };

			Logger.LogDebug("can_get_timespan_with_custom_converter(): collection → {Collection}", collection);
            var value = collection.GetValueOrDefault("length", TimeSpan.Parse);

			Logger.LogDebug("can_get_timespan_with_custom_converter(): value → {Value}", value);
            Assert.That(value, Is.EqualTo(TimeSpan.FromSeconds(4210)));
			Logger.LogDebug("<<< Exiting can_get_timespan_with_custom_converter()");
        }

        [Test]
        public void can_get_empty_boolean_from_form()
        {
			Logger.LogDebug(">>> Entering can_get_empty_boolean_from_form()");
            var collection = new NameValueCollection { { "text", "blah" } };

			Logger.LogDebug("can_get_empty_boolean_from_form(): collection → {Collection}", collection);
            var value = collection.GetValueOrDefault("radio", CollectionExtensions.GetFlexibleBooleanParsingFunction());

			Logger.LogDebug("can_get_empty_boolean_from_form(): value → {Value}", value);
            Assert.That(value, Is.False);
			Logger.LogDebug("<<< Exiting can_get_empty_boolean_from_form()");
        }

        [Test]
        public void can_get_boolean_from_form()
        {
			Logger.LogDebug(">>> Entering can_get_boolean_from_form()");
            var collection = new NameValueCollection { { "radio", "on" } };

			Logger.LogDebug("can_get_boolean_from_form(): collection → {Collection}", collection);
            var value = collection.GetValueOrDefault("radio", CollectionExtensions.GetFlexibleBooleanParsingFunction());

			Logger.LogDebug("can_get_boolean_from_form(): value → {Value}", value);
            Assert.That(value, Is.True);
			Logger.LogDebug("<<< Exiting can_get_boolean_from_form()");
        }

        [Test]
        public void flexible_boolean_parsing_is_case_insensitive()
        {
			Logger.LogDebug(">>> Entering flexible_boolean_parsing_is_case_insensitive()");
            var collection = new NameValueCollection { { "question", "YES" } };

			Logger.LogDebug("flexible_boolean_parsing_is_case_insensitive(): collection → {Collection}", collection);
            var value = collection.GetValueOrDefault("question", CollectionExtensions.GetFlexibleBooleanParsingFunction("yes"));

			Logger.LogDebug("flexible_boolean_parsing_is_case_insensitive(): value → {Value}", value);
            Assert.That(value, Is.True);
			Logger.LogDebug("<<< Exiting flexible_boolean_parsing_is_case_insensitive()");
        }

        [Test]
        public void can_convert_namevaluecollection_to_lookup()
        {
			Logger.LogDebug(">>> Entering can_convert_namevaluecollection_to_lookup()");
            var collection = new NameValueCollection { { "question", "YES" } };

			Logger.LogDebug("can_convert_namevaluecollection_to_lookup(): collection → {Collection}", collection);
            var lookup = collection.ToLookup();

			Logger.LogDebug("can_convert_namevaluecollection_to_lookup(): lookup → {Lookup}", lookup);
            Assert.That(lookup["question"], Is.EquivalentTo(new[] { "YES" }));
			Logger.LogDebug("<<< Exiting can_convert_namevaluecollection_to_lookup()");
        }

        [Test]
        public void can_convert_namevaluecollection_with_multiple_values_to_lookup()
        {
			Logger.LogDebug(">>> Entering can_convert_namevaluecollection_with_multiple_values_to_lookup()");
            var collection = new NameValueCollection { { "question", "A" }, { "question", "B" }, { "question", "C" }, };

			Logger.LogDebug("can_convert_namevaluecollection_with_multiple_values_to_lookup(): collection → {Collection}", collection);
            var lookup = collection.ToLookup();

			Logger.LogDebug("can_convert_namevaluecollection_with_multiple_values_to_lookup(): lookup → {Lookup}", lookup);
            Assert.That(lookup["question"], Is.EquivalentTo(new[] { "A", "B", "C", }));
			Logger.LogDebug("<<< Exiting can_convert_namevaluecollection_with_multiple_values_to_lookup()");
        }

        [Test]
        public void can_get_null_value_rather_than_default()
        {
			Logger.LogDebug(">>> Entering can_get_null_value_rather_than_default()");
            var dictionary = new Hashtable { { "question", null } };

			Logger.LogDebug("can_get_null_value_rather_than_default(): dictionary → {Dictionary}", dictionary);
            var value = dictionary.GetValueOrDefault("question", "yes");

			Logger.LogDebug("can_get_null_value_rather_than_default(): value → {Value}", value);
            Assert.That(value, Is.Null);
			Logger.LogDebug("<<< Exiting can_get_null_value_rather_than_default()");
        }

        [Test]
        public void can_get_empty_string_rather_than_default()
        {
			Logger.LogDebug(">>> Entering can_get_empty_string_rather_than_default()");
            var dictionary = new Hashtable { { "question", string.Empty } };

			Logger.LogDebug("can_get_empty_string_rather_than_default(): dictionary → {Dictionary}", dictionary);
            var value = dictionary.GetValueOrDefault("question", "yes");

			Logger.LogDebug("can_get_empty_string_rather_than_default(): value → {Value}", value);
            Assert.That(value, Is.Empty);
			Logger.LogDebug("<<< Exiting can_get_empty_string_rather_than_default()");
        }

        [Test]
        public void can_get_value_without_default()
        {
			Logger.LogDebug(">>> Entering can_get_value_without_default()");
            var dictionary = new Hashtable { { "question", "what is it" } };

			Logger.LogDebug("can_get_value_without_default(): dictionary → {Dictionary}", dictionary);
            var value = dictionary.GetValue<string>("question");

			Logger.LogDebug("can_get_value_without_default(): value → {Value}", value);
            Assert.That(value, Is.EqualTo("what is it"));
			Logger.LogDebug("<<< Exiting can_get_value_without_default()");
        }

        [Test]
        public void can_get_value_without_default_with_custom_converter()
        {
			Logger.LogDebug(">>> Entering can_get_value_without_default_with_custom_converter()");
            var dictionary = new Hashtable { { "question", "what is it" } };

			Logger.LogDebug("can_get_value_without_default_with_custom_converter(): dictionary → {Dictionary}", dictionary);
            var value = dictionary.GetValue("question", (object v) => v is string ? 10 : 20);

			Logger.LogDebug("can_get_value_without_default_with_custom_converter(): value → {Value}", value);
            Assert.That(value, Is.EqualTo(10));
			Logger.LogDebug("<<< Exiting can_get_value_without_default_with_custom_converter()");
        }

        [Test]
        public void can_get_value_without_default_from_namevaluecollection()
        {
			Logger.LogDebug(">>> Entering can_get_value_without_default_from_namevaluecollection()");
            var collection = new NameValueCollection { { "question", "what is it" } };

			Logger.LogDebug("can_get_value_without_default_from_namevaluecollection(): collection → {Collection}", collection);
            var value = collection.GetValue<string>("question");

			Logger.LogDebug("can_get_value_without_default_from_namevaluecollection(): value → {Value}", value);
            Assert.That(value, Is.EqualTo("what is it"));
			Logger.LogDebug("<<< Exiting can_get_value_without_default_from_namevaluecollection()");
        }

        [Test]
        public void can_get_value_containing_comma_from_namevaluecollection()
        {
			Logger.LogDebug(">>> Entering can_get_value_containing_comma_from_namevaluecollection()");
            var collection = new NameValueCollection { { "question", "what, is it?" } };

			Logger.LogDebug("can_get_value_containing_comma_from_namevaluecollection(): collection → {Collection}", collection);
            var value = collection.GetValue<string>("question");

			Logger.LogDebug("can_get_value_containing_comma_from_namevaluecollection(): value → {Value}", value);
            Assert.That(value, Is.EqualTo("what, is it?"));
			Logger.LogDebug("<<< Exiting can_get_value_containing_comma_from_namevaluecollection()");
        }

        [Test]
        public void can_get_multiple_values_from_namevaluecollection()
        {
			Logger.LogDebug(">>> Entering can_get_multiple_values_from_namevaluecollection()");
            var collection = new NameValueCollection { { "state", "CA" }, { "state", "BC" }, };

			Logger.LogDebug("can_get_multiple_values_from_namevaluecollection(): collection → {Collection}", collection);
            var value = collection.GetValues<string>("state");

			Logger.LogDebug("can_get_multiple_values_from_namevaluecollection(): value → {Value}", value);
            Assert.That(value, Is.EquivalentTo(new[] { "CA", "BC", }));
			Logger.LogDebug("<<< Exiting can_get_multiple_values_from_namevaluecollection()");
        }

        [Test]
        public void can_get_sequence_with_single_value_from_namevaluecollection()
        {
			Logger.LogDebug(">>> Entering can_get_sequence_with_single_value_from_namevaluecollection()");
            var collection = new NameValueCollection { { "state", "CA" } };

			Logger.LogDebug("can_get_sequence_with_single_value_from_namevaluecollection(): collection → {Collection}", collection);
            var value = collection.GetValues<string>("state");

			Logger.LogDebug("can_get_sequence_with_single_value_from_namevaluecollection(): value → {Value}", value);
            Assert.That(value, Is.EquivalentTo(new[] { "CA" }));
			Logger.LogDebug("<<< Exiting can_get_sequence_with_single_value_from_namevaluecollection()");
        }

        [Test]
        public void can_get_sequence_with_no_value_from_namevaluecollection()
        {
			Logger.LogDebug(">>> Entering can_get_sequence_with_no_value_from_namevaluecollection()");
            var collection = new NameValueCollection { { "state", "CA" } };

			Logger.LogDebug("can_get_sequence_with_no_value_from_namevaluecollection(): collection → {Collection}", collection);
            var value = collection.GetValues<string>("cat");

			Logger.LogDebug("can_get_sequence_with_no_value_from_namevaluecollection(): value → {Value}", value);
            Assert.That(value, Is.Empty);
			Logger.LogDebug("<<< Exiting can_get_sequence_with_no_value_from_namevaluecollection()");
        }

        [Test]
        public void can_get_multiple_values_from_namevaluecollection_with_custom_converter()
        {
			Logger.LogDebug(">>> Entering can_get_multiple_values_from_namevaluecollection_with_custom_converter()");
            var collection = new NameValueCollection { { "state", "12" }, { "state", "1" } };

			Logger.LogDebug("can_get_multiple_values_from_namevaluecollection_with_custom_converter(): collection → {Collection}", collection);
            var value = collection.GetValues("state", v => int.Parse(v, CultureInfo.InvariantCulture) + 10);

			Logger.LogDebug("can_get_multiple_values_from_namevaluecollection_with_custom_converter(): value → {Value}", value);
            Assert.That(value, Is.EquivalentTo(new[] { 22, 11 }));
			Logger.LogDebug("<<< Exiting can_get_multiple_values_from_namevaluecollection_with_custom_converter()");
        }

        [Test]
        public void can_get_value_without_default_from_statebag()
        {
			Logger.LogDebug(">>> Entering can_get_value_without_default_from_statebag()");
            var dictionary = new StateBag { { "question", "what is it" } };

			Logger.LogDebug("can_get_value_without_default_from_statebag(): dictionary → {Dictionary}", dictionary);
            var value = dictionary.GetValue<string>("question");

			Logger.LogDebug("can_get_value_without_default_from_statebag(): value → {Value}", value);
            Assert.That(value, Is.EqualTo("what is it"));
			Logger.LogDebug("<<< Exiting can_get_value_without_default_from_statebag()");
        }

        [Test]
        public void can_get_value_without_default_from_xnode()
        {
			Logger.LogDebug(">>> Entering can_get_value_without_default_from_xnode()");
            var node = new XElement(
                "parent",
                new XElement("id", 21));

            var value = node.GetValue<int>("id");

			Logger.LogDebug("can_get_value_without_default_from_xnode(): value → {Value}", value);
            Assert.That(value, Is.EqualTo(21));
			Logger.LogDebug("<<< Exiting can_get_value_without_default_from_xnode()");
        }

        [Test]
        public void can_get_value_without_default_from_xmlnode()
        {
			Logger.LogDebug(">>> Entering can_get_value_without_default_from_xmlnode()");
            var doc = new XmlDocument { XmlResolver = null };
            const string ParentWithIdXml = """
                                           <parent>
                                               <id>123</id>
                                           </parent>
                                           """;
            using var xmlReader = XmlReader.Create(new StringReader(ParentWithIdXml), new XmlReaderSettings { XmlResolver = null, });
            doc.Load(xmlReader);
			Logger.LogDebug("can_get_value_without_default_from_xmlnode(): xmlReader → {XmlReader}", xmlReader);

            var value = doc.DocumentElement.GetValue<int>("id");

			Logger.LogDebug("can_get_value_without_default_from_xmlnode(): value → {Value}", value);
            Assert.That(value, Is.EqualTo(123));
			Logger.LogDebug("<<< Exiting can_get_value_without_default_from_xmlnode()");
        }

        [Test]
        public void getvalue_throws_argumentexception_when_value_is_not_present()
        {
			Logger.LogDebug(">>> Entering getvalue_throws_argumentexception_when_value_is_not_present()");
            var dictionary = new Hashtable { { "question", "what is it" } };

			Logger.LogDebug("getvalue_throws_argumentexception_when_value_is_not_present(): dictionary → {Dictionary}", dictionary);
            Assert.That(() => dictionary.GetValue<string>("answer"), Throws.ArgumentException.With.Property("ParamName").EqualTo("key"));
			Logger.LogDebug("<<< Exiting getvalue_throws_argumentexception_when_value_is_not_present()");
        }

        [Test]
        public void throws_invalidoperationexception_when_lookup_has_multiple_values()
        {
			Logger.LogDebug(">>> Entering throws_invalidoperationexception_when_lookup_has_multiple_values()");
            var collection = new NameValueCollection { { "state", "CA" }, { "state", "BC" } };

			Logger.LogDebug("throws_invalidoperationexception_when_lookup_has_multiple_values(): collection → {Collection}", collection);
            Assert.That(() => collection.GetValueOrDefault<string>("state"), Throws.InvalidOperationException);
			Logger.LogDebug("<<< Exiting throws_invalidoperationexception_when_lookup_has_multiple_values()");
        }

        [Test]
        public void throws_argumentnullexception_when_dictionary_is_null()
        {
			Logger.LogDebug(">>> Entering throws_argumentnullexception_when_dictionary_is_null()");
            IDictionary dictionary = null;

            Assert.That(() => dictionary.GetValueOrDefault<int>("value ID"), Throws.TypeOf<ArgumentNullException>().With.Property("ParamName").EqualTo("dictionary"));
			Logger.LogDebug("<<< Exiting throws_argumentnullexception_when_dictionary_is_null()");
        }

        [Test]
        public void throws_argumentnullexception_when_xelement_is_null()
        {
			Logger.LogDebug(">>> Entering throws_argumentnullexception_when_xelement_is_null()");
            XElement node = null;

            Assert.That(() => node.GetValueOrDefault<int>("value ID"), Throws.TypeOf<ArgumentNullException>().With.Property("ParamName").EqualTo("node"));
			Logger.LogDebug("<<< Exiting throws_argumentnullexception_when_xelement_is_null()");
        }

        [Test]
        public void throws_argumentnullexception_when_xmlnode_is_null()
        {
			Logger.LogDebug(">>> Entering throws_argumentnullexception_when_xmlnode_is_null()");
            XmlNode node = null;

            Assert.That(() => node.GetValueOrDefault<int>("value ID"), Throws.TypeOf<ArgumentNullException>().With.Property("ParamName").EqualTo("node"));
			Logger.LogDebug("<<< Exiting throws_argumentnullexception_when_xmlnode_is_null()");
        }

        [Test]
        public void does_not_throw_invalidcastexception_when_value_is_null_for_reference_type()
        {
			Logger.LogDebug(">>> Entering does_not_throw_invalidcastexception_when_value_is_null_for_reference_type()");
            var dictionary = new Dictionary<string, string> { { "length", null } };

			Logger.LogDebug("does_not_throw_invalidcastexception_when_value_is_null_for_reference_type(): dictionary → {Dictionary}", dictionary);
            var value = dictionary.GetValueOrDefault<ApplicationException>("length");

			Logger.LogDebug("does_not_throw_invalidcastexception_when_value_is_null_for_reference_type(): value → {Value}", value);
            Assert.That(value, Is.Null);
			Logger.LogDebug("<<< Exiting does_not_throw_invalidcastexception_when_value_is_null_for_reference_type()");
        }

        [Test]
        public void tolookup_throws_argumentnullexception_when_namevaluecollection_is_null()
        {
			Logger.LogDebug(">>> Entering tolookup_throws_argumentnullexception_when_namevaluecollection_is_null()");
            NameValueCollection col = null;

            Assert.That(() => col.ToLookup(), Throws.TypeOf<ArgumentNullException>().With.Property("ParamName").EqualTo("collection"));
			Logger.LogDebug("<<< Exiting tolookup_throws_argumentnullexception_when_namevaluecollection_is_null()");
        }
    }
}
