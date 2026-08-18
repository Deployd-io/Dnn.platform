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
        [Test]
        public void get_null_string_from_hashtable_for_missing_value()
        {
			mockEventLogger.LogDebug(">>> Entering get_null_string_from_hashtable_for_missing_value()");
            var table = new Hashtable { { "app id", "abc123" } };

			mockEventLogger.LogDebug("get_null_string_from_hashtable_for_missing_value(): table → {Table}", table);
            var value = table.GetValueOrDefault<string>("cat id");
			mockEventLogger.LogDebug("get_null_string_from_hashtable_for_missing_value(): value → {Value}", value);

            Assert.That(value, Is.Null);
			mockEventLogger.LogDebug("<<< Exiting get_null_string_from_hashtable_for_missing_value()");
        }

        [Test]
        public void can_get_string_from_hashtable()
        {
			mockEventLogger.LogDebug(">>> Entering can_get_string_from_hashtable()");
            var table = new Hashtable { { "app id", "abc123" } };

			mockEventLogger.LogDebug("can_get_string_from_hashtable(): table → {Table}", table);
            var value = table.GetValueOrDefault<string>("app id");
			mockEventLogger.LogDebug("can_get_string_from_hashtable(): value → {Value}", value);

            Assert.That(value, Is.EqualTo("abc123"));
			mockEventLogger.LogDebug("<<< Exiting can_get_string_from_hashtable()");
        }

        [Test]
        public void get_string_from_hashtable_when_default_is_provided()
        {
			mockEventLogger.LogDebug(">>> Entering get_string_from_hashtable_when_default_is_provided()");
            var table = new Hashtable { { "app id", "abc123" } };

			mockEventLogger.LogDebug("get_string_from_hashtable_when_default_is_provided(): table → {Table}", table);
            var value = table.GetValueOrDefault("app id", "abracadabra");
			mockEventLogger.LogDebug("get_string_from_hashtable_when_default_is_provided(): value → {Value}", value);

            Assert.That(value, Is.EqualTo("abc123"));
			mockEventLogger.LogDebug("<<< Exiting get_string_from_hashtable_when_default_is_provided()");
        }

        [Test]
        public void can_get_default_string_from_hashtable()
        {
			mockEventLogger.LogDebug(">>> Entering can_get_default_string_from_hashtable()");
            var table = new Hashtable { { "app id", "abc123" } };

			mockEventLogger.LogDebug("can_get_default_string_from_hashtable(): table → {Table}", table);
            var value = table.GetValueOrDefault("cat id", "Frank");
			mockEventLogger.LogDebug("can_get_default_string_from_hashtable(): value → {Value}", value);

            Assert.That(value, Is.EqualTo("Frank"));
			mockEventLogger.LogDebug("<<< Exiting can_get_default_string_from_hashtable()");
        }

        [Test]
        public void can_get_bool_from_hashtable()
        {
			mockEventLogger.LogDebug(">>> Entering can_get_bool_from_hashtable()");
            var table = new Hashtable { { "app id", "true" } };

			mockEventLogger.LogDebug("can_get_bool_from_hashtable(): table → {Table}", table);
            var value = table.GetValueOrDefault<bool>("app id");
			mockEventLogger.LogDebug("can_get_bool_from_hashtable(): value → {Value}", value);

            Assert.That(value, Is.True);
			mockEventLogger.LogDebug("<<< Exiting can_get_bool_from_hashtable()");
        }

        [Test]
        public void get_bool_from_hashtable_when_default_is_provided()
        {
			mockEventLogger.LogDebug(">>> Entering get_bool_from_hashtable_when_default_is_provided()");
            var table = new Hashtable { { "app id", "true" } };

			mockEventLogger.LogDebug("get_bool_from_hashtable_when_default_is_provided(): table → {Table}", table);
            var value = table.GetValueOrDefault("app id", false);
			mockEventLogger.LogDebug("get_bool_from_hashtable_when_default_is_provided(): value → {Value}", value);

            Assert.That(value, Is.True);
			mockEventLogger.LogDebug("<<< Exiting get_bool_from_hashtable_when_default_is_provided()");
        }

        [Test]
        public void can_get_default_bool_from_hashtable()
        {
			mockEventLogger.LogDebug(">>> Entering can_get_default_bool_from_hashtable()");
            var value = true;
            var table = new Hashtable { { "app id", "abc123" } };
			mockEventLogger.LogDebug("can_get_default_bool_from_hashtable(): table → {Table}", table);
			mockEventLogger.LogDebug("can_get_default_bool_from_hashtable(): value → {Value}", value);

            value = table.GetValueOrDefault("Allow Windows Live Writer", value);

            Assert.That(value, Is.True);
			mockEventLogger.LogDebug("<<< Exiting can_get_default_bool_from_hashtable()");
        }

        [Test]
        public void get_false_from_hashtable_for_missing_value()
        {
			mockEventLogger.LogDebug(">>> Entering get_false_from_hashtable_for_missing_value()");
            var table = new Hashtable { { "app id", "abc123" } };

			mockEventLogger.LogDebug("get_false_from_hashtable_for_missing_value(): table → {Table}", table);
            var value = table.GetValueOrDefault<bool>("Allow Windows Live Writer");
			mockEventLogger.LogDebug("get_false_from_hashtable_for_missing_value(): value → {Value}", value);

            Assert.That(value, Is.False);
			mockEventLogger.LogDebug("<<< Exiting get_false_from_hashtable_for_missing_value()");
        }

        [Test]
        public void get_bool_with_custom_converter_from_hashtable()
        {
			mockEventLogger.LogDebug(">>> Entering get_bool_with_custom_converter_from_hashtable()");
            var table = new Hashtable { { "allow", "on" } };

			mockEventLogger.LogDebug("get_bool_with_custom_converter_from_hashtable(): table → {Table}", table);
            var value = table.GetValueOrDefault(
                "allow",
                v =>
                {
                    bool allowed;
                    if (!bool.TryParse(v, out allowed))
                    {
                        allowed = v.Equals("on", StringComparison.Ordinal);
                    }
					mockEventLogger.LogDebug("get_bool_with_custom_converter_from_hashtable(): allowed → {Allowed}", allowed);

                    return allowed;
                });

            Assert.That(value, Is.True);
			mockEventLogger.LogDebug("<<< Exiting get_bool_with_custom_converter_from_hashtable()");
        }

        [Test]
        public void get_int()
        {
			mockEventLogger.LogDebug(">>> Entering get_int()");
            var collection = new Dictionary<string, string> { { "appId", "123" } };

			mockEventLogger.LogDebug("get_int(): collection → {Collection}", collection);
            var value = collection.GetValueOrDefault<int>("appId");
			mockEventLogger.LogDebug("get_int(): value → {Value}", value);

            Assert.That(value, Is.EqualTo(123));
			mockEventLogger.LogDebug("<<< Exiting get_int()");
        }

        [Test]
        public void get_decimal()
        {
			mockEventLogger.LogDebug(">>> Entering get_decimal()");
            var collection = new Dictionary<string, string> { { "appId", "1.23" } };

			mockEventLogger.LogDebug("get_decimal(): collection → {Collection}", collection);
            var value = collection.GetValueOrDefault<decimal>("appId");
			mockEventLogger.LogDebug("get_decimal(): value → {Value}", value);

            Assert.That(value, Is.EqualTo(1.23m));
			mockEventLogger.LogDebug("<<< Exiting get_decimal()");
        }

        [Test]
        [SetCulture("nl-NL")]
        public void get_decimal_from_other_culture()
        {
			mockEventLogger.LogDebug(">>> Entering get_decimal_from_other_culture()");
            var collection = new Dictionary<string, string> { { "appId", "1.23" } };

			mockEventLogger.LogDebug("get_decimal_from_other_culture(): collection → {Collection}", collection);
            var value = collection.GetValueOrDefault<decimal>("appId");
			mockEventLogger.LogDebug("get_decimal_from_other_culture(): value → {Value}", value);

            Assert.That(value, Is.EqualTo(1.23m));
			mockEventLogger.LogDebug("<<< Exiting get_decimal_from_other_culture()");
        }

        [Test]
        public void get_datetime()
        {
			mockEventLogger.LogDebug(">>> Entering get_datetime()");
            var collection = new Dictionary<string, string> { { "startDate", "05/04/2012 00:00:00" } };

			mockEventLogger.LogDebug("get_datetime(): collection → {Collection}", collection);
            var value = collection.GetValueOrDefault<DateTime>("startDate");
			mockEventLogger.LogDebug("get_datetime(): value → {Value}", value);

            Assert.That(value, Is.EqualTo(new DateTime(2012, 5, 4)));
			mockEventLogger.LogDebug("<<< Exiting get_datetime()");
        }

        [Test]
        public void get_null_string_without_default()
        {
			mockEventLogger.LogDebug(">>> Entering get_null_string_without_default()");
            var collection = new Dictionary<string, string> { { "app id", null } };

			mockEventLogger.LogDebug("get_null_string_without_default(): collection → {Collection}", collection);
            var value = collection.GetValue<string>("app id");
			mockEventLogger.LogDebug("get_null_string_without_default(): value → {Value}", value);

            Assert.That(value, Is.Null);
			mockEventLogger.LogDebug("<<< Exiting get_null_string_without_default()");
        }

        [Test]
        public void get_null_string_with_default()
        {
			mockEventLogger.LogDebug(">>> Entering get_null_string_with_default()");
            var collection = new Dictionary<string, string> { { "app id", null } };

			mockEventLogger.LogDebug("get_null_string_with_default(): collection → {Collection}", collection);
            var value = collection.GetValueOrDefault("app id", "a default value");
			mockEventLogger.LogDebug("get_null_string_with_default(): value → {Value}", value);

            Assert.That(value, Is.Null);
			mockEventLogger.LogDebug("<<< Exiting get_null_string_with_default()");
        }

        [Test]
        public void get_nullable_datetime()
        {
			mockEventLogger.LogDebug(">>> Entering get_nullable_datetime()");
            var collection = new Dictionary<string, DateTime?> { { "startDate", null } };

			mockEventLogger.LogDebug("get_nullable_datetime(): collection → {Collection}", collection);
            var value = collection.GetValue<DateTime?>("startDate");
			mockEventLogger.LogDebug("get_nullable_datetime(): value → {Value}", value);

            Assert.That(value, Is.Null);
			mockEventLogger.LogDebug("<<< Exiting get_nullable_datetime()");
        }

        [Test]
        [SetCulture("nl-NL")]
        public void get_datetime_from_other_culture()
        {
			mockEventLogger.LogDebug(">>> Entering get_datetime_from_other_culture()");
            var collection = new Dictionary<string, string> { { "startDate", "05/04/2012 00:00:00" } };

			mockEventLogger.LogDebug("get_datetime_from_other_culture(): collection → {Collection}", collection);
            var value = collection.GetValueOrDefault<DateTime>("startDate");
			mockEventLogger.LogDebug("get_datetime_from_other_culture(): value → {Value}", value);

            Assert.That(value, Is.EqualTo(new DateTime(2012, 5, 4)));
			mockEventLogger.LogDebug("<<< Exiting get_datetime_from_other_culture()");
        }

        [Test]
        public void get_from_statebag()
        {
			mockEventLogger.LogDebug(">>> Entering get_from_statebag()");
            var collection = new StateBag { { "appId", "123" } };

			mockEventLogger.LogDebug("get_from_statebag(): collection → {Collection}", collection);
            var value = collection.GetValueOrDefault<string>("appId");
			mockEventLogger.LogDebug("get_from_statebag(): value → {Value}", value);

            Assert.That(value, Is.EqualTo("123"));
			mockEventLogger.LogDebug("<<< Exiting get_from_statebag()");
        }

        [Test]
        public void get_from_xnode()
        {
			mockEventLogger.LogDebug(">>> Entering get_from_xnode()");
            var node = new XElement(
                "parent",
                new XElement("id", 14));

            var value = node.GetValueOrDefault<int>("id");
			mockEventLogger.LogDebug("get_from_xnode(): value → {Value}", value);

            Assert.That(value, Is.EqualTo(14));
			mockEventLogger.LogDebug("<<< Exiting get_from_xnode()");
        }

        [Test]
        public void get_from_xmlnode()
        {
			mockEventLogger.LogDebug(">>> Entering get_from_xmlnode()");
            var doc = new XmlDocument { XmlResolver = null };
            const string parentNode = """
                                      <parent>
                                          <id>13</id>
                                      </parent>
                                      """;
            using var xmlReader = XmlReader.Create(new StringReader(parentNode), new XmlReaderSettings { XmlResolver = null, });
			mockEventLogger.LogDebug("get_from_xmlnode(): xmlReader → {XmlReader}", xmlReader);
            doc.Load(xmlReader);

            var value = doc.DocumentElement.GetValueOrDefault<int>("id");
			mockEventLogger.LogDebug("get_from_xmlnode(): value → {Value}", value);

            Assert.That(value, Is.EqualTo(13));
			mockEventLogger.LogDebug("<<< Exiting get_from_xmlnode()");
        }

        [Test]
        public void can_get_timespan_with_custom_converter()
        {
			mockEventLogger.LogDebug(">>> Entering can_get_timespan_with_custom_converter()");
            var collection = new Hashtable { { "length", "1:10:10" } };

			mockEventLogger.LogDebug("can_get_timespan_with_custom_converter(): collection → {Collection}", collection);
            var value = collection.GetValueOrDefault("length", TimeSpan.Parse);
			mockEventLogger.LogDebug("can_get_timespan_with_custom_converter(): value → {Value}", value);

            Assert.That(value, Is.EqualTo(TimeSpan.FromSeconds(4210)));
			mockEventLogger.LogDebug("<<< Exiting can_get_timespan_with_custom_converter()");
        }

        [Test]
        public void can_get_empty_boolean_from_form()
        {
			mockEventLogger.LogDebug(">>> Entering can_get_empty_boolean_from_form()");
            var collection = new NameValueCollection { { "text", "blah" } };

			mockEventLogger.LogDebug("can_get_empty_boolean_from_form(): collection → {Collection}", collection);
            var value = collection.GetValueOrDefault("radio", CollectionExtensions.GetFlexibleBooleanParsingFunction());
			mockEventLogger.LogDebug("can_get_empty_boolean_from_form(): value → {Value}", value);

            Assert.That(value, Is.False);
			mockEventLogger.LogDebug("<<< Exiting can_get_empty_boolean_from_form()");
        }

        [Test]
        public void can_get_boolean_from_form()
        {
			mockEventLogger.LogDebug(">>> Entering can_get_boolean_from_form()");
            var collection = new NameValueCollection { { "radio", "on" } };

			mockEventLogger.LogDebug("can_get_boolean_from_form(): collection → {Collection}", collection);
            var value = collection.GetValueOrDefault("radio", CollectionExtensions.GetFlexibleBooleanParsingFunction());
			mockEventLogger.LogDebug("can_get_boolean_from_form(): value → {Value}", value);

            Assert.That(value, Is.True);
			mockEventLogger.LogDebug("<<< Exiting can_get_boolean_from_form()");
        }

        [Test]
        public void flexible_boolean_parsing_is_case_insensitive()
        {
			mockEventLogger.LogDebug(">>> Entering flexible_boolean_parsing_is_case_insensitive()");
            var collection = new NameValueCollection { { "question", "YES" } };

			mockEventLogger.LogDebug("flexible_boolean_parsing_is_case_insensitive(): collection → {Collection}", collection);
            var value = collection.GetValueOrDefault("question", CollectionExtensions.GetFlexibleBooleanParsingFunction("yes"));
			mockEventLogger.LogDebug("flexible_boolean_parsing_is_case_insensitive(): value → {Value}", value);

            Assert.That(value, Is.True);
			mockEventLogger.LogDebug("<<< Exiting flexible_boolean_parsing_is_case_insensitive()");
        }

        [Test]
        public void can_convert_namevaluecollection_to_lookup()
        {
			mockEventLogger.LogDebug(">>> Entering can_convert_namevaluecollection_to_lookup()");
            var collection = new NameValueCollection { { "question", "YES" } };

			mockEventLogger.LogDebug("can_convert_namevaluecollection_to_lookup(): collection → {Collection}", collection);
            var lookup = collection.ToLookup();
			mockEventLogger.LogDebug("can_convert_namevaluecollection_to_lookup(): lookup → {Lookup}", lookup);

            Assert.That(lookup["question"], Is.EquivalentTo(new[] { "YES" }));
			mockEventLogger.LogDebug("<<< Exiting can_convert_namevaluecollection_to_lookup()");
        }

        [Test]
        public void can_convert_namevaluecollection_with_multiple_values_to_lookup()
        {
			mockEventLogger.LogDebug(">>> Entering can_convert_namevaluecollection_with_multiple_values_to_lookup()");
            var collection = new NameValueCollection { { "question", "A" }, { "question", "B" }, { "question", "C" }, };

			mockEventLogger.LogDebug("can_convert_namevaluecollection_with_multiple_values_to_lookup(): collection → {Collection}", collection);
            var lookup = collection.ToLookup();
			mockEventLogger.LogDebug("can_convert_namevaluecollection_with_multiple_values_to_lookup(): lookup → {Lookup}", lookup);

            Assert.That(lookup["question"], Is.EquivalentTo(new[] { "A", "B", "C", }));
			mockEventLogger.LogDebug("<<< Exiting can_convert_namevaluecollection_with_multiple_values_to_lookup()");
        }

        [Test]
        public void can_get_null_value_rather_than_default()
        {
			mockEventLogger.LogDebug(">>> Entering can_get_null_value_rather_than_default()");
            var dictionary = new Hashtable { { "question", null } };

			mockEventLogger.LogDebug("can_get_null_value_rather_than_default(): dictionary → {Dictionary}", dictionary);
            var value = dictionary.GetValueOrDefault("question", "yes");
			mockEventLogger.LogDebug("can_get_null_value_rather_than_default(): value → {Value}", value);

            Assert.That(value, Is.Null);
			mockEventLogger.LogDebug("<<< Exiting can_get_null_value_rather_than_default()");
        }

        [Test]
        public void can_get_empty_string_rather_than_default()
        {
			mockEventLogger.LogDebug(">>> Entering can_get_empty_string_rather_than_default()");
            var dictionary = new Hashtable { { "question", string.Empty } };

			mockEventLogger.LogDebug("can_get_empty_string_rather_than_default(): dictionary → {Dictionary}", dictionary);
            var value = dictionary.GetValueOrDefault("question", "yes");
			mockEventLogger.LogDebug("can_get_empty_string_rather_than_default(): value → {Value}", value);

            Assert.That(value, Is.Empty);
			mockEventLogger.LogDebug("<<< Exiting can_get_empty_string_rather_than_default()");
        }

        [Test]
        public void can_get_value_without_default()
        {
			mockEventLogger.LogDebug(">>> Entering can_get_value_without_default()");
            var dictionary = new Hashtable { { "question", "what is it" } };

			mockEventLogger.LogDebug("can_get_value_without_default(): dictionary → {Dictionary}", dictionary);
            var value = dictionary.GetValue<string>("question");
			mockEventLogger.LogDebug("can_get_value_without_default(): value → {Value}", value);

            Assert.That(value, Is.EqualTo("what is it"));
			mockEventLogger.LogDebug("<<< Exiting can_get_value_without_default()");
        }

        [Test]
        public void can_get_value_without_default_with_custom_converter()
        {
			mockEventLogger.LogDebug(">>> Entering can_get_value_without_default_with_custom_converter()");
            var dictionary = new Hashtable { { "question", "what is it" } };

			mockEventLogger.LogDebug("can_get_value_without_default_with_custom_converter(): dictionary → {Dictionary}", dictionary);
            var value = dictionary.GetValue("question", (object v) => v is string ? 10 : 20);
			mockEventLogger.LogDebug("can_get_value_without_default_with_custom_converter(): value → {Value}", value);

            Assert.That(value, Is.EqualTo(10));
			mockEventLogger.LogDebug("<<< Exiting can_get_value_without_default_with_custom_converter()");
        }

        [Test]
        public void can_get_value_without_default_from_namevaluecollection()
        {
			mockEventLogger.LogDebug(">>> Entering can_get_value_without_default_from_namevaluecollection()");
            var collection = new NameValueCollection { { "question", "what is it" } };

			mockEventLogger.LogDebug("can_get_value_without_default_from_namevaluecollection(): collection → {Collection}", collection);
            var value = collection.GetValue<string>("question");
			mockEventLogger.LogDebug("can_get_value_without_default_from_namevaluecollection(): value → {Value}", value);

            Assert.That(value, Is.EqualTo("what is it"));
			mockEventLogger.LogDebug("<<< Exiting can_get_value_without_default_from_namevaluecollection()");
        }

        [Test]
        public void can_get_value_containing_comma_from_namevaluecollection()
        {
			mockEventLogger.LogDebug(">>> Entering can_get_value_containing_comma_from_namevaluecollection()");
            var collection = new NameValueCollection { { "question", "what, is it?" } };

			mockEventLogger.LogDebug("can_get_value_containing_comma_from_namevaluecollection(): collection → {Collection}", collection);
            var value = collection.GetValue<string>("question");
			mockEventLogger.LogDebug("can_get_value_containing_comma_from_namevaluecollection(): value → {Value}", value);

            Assert.That(value, Is.EqualTo("what, is it?"));
			mockEventLogger.LogDebug("<<< Exiting can_get_value_containing_comma_from_namevaluecollection()");
        }

        [Test]
        public void can_get_multiple_values_from_namevaluecollection()
        {
			mockEventLogger.LogDebug(">>> Entering can_get_multiple_values_from_namevaluecollection()");
            var collection = new NameValueCollection { { "state", "CA" }, { "state", "BC" }, };

			mockEventLogger.LogDebug("can_get_multiple_values_from_namevaluecollection(): collection → {Collection}", collection);
            var value = collection.GetValues<string>("state");
			mockEventLogger.LogDebug("can_get_multiple_values_from_namevaluecollection(): value → {Value}", value);

            Assert.That(value, Is.EquivalentTo(new[] { "CA", "BC", }));
			mockEventLogger.LogDebug("<<< Exiting can_get_multiple_values_from_namevaluecollection()");
        }

        [Test]
        public void can_get_sequence_with_single_value_from_namevaluecollection()
        {
			mockEventLogger.LogDebug(">>> Entering can_get_sequence_with_single_value_from_namevaluecollection()");
            var collection = new NameValueCollection { { "state", "CA" } };

			mockEventLogger.LogDebug("can_get_sequence_with_single_value_from_namevaluecollection(): collection → {Collection}", collection);
            var value = collection.GetValues<string>("state");
			mockEventLogger.LogDebug("can_get_sequence_with_single_value_from_namevaluecollection(): value → {Value}", value);

            Assert.That(value, Is.EquivalentTo(new[] { "CA" }));
			mockEventLogger.LogDebug("<<< Exiting can_get_sequence_with_single_value_from_namevaluecollection()");
        }

        [Test]
        public void can_get_sequence_with_no_value_from_namevaluecollection()
        {
			mockEventLogger.LogDebug(">>> Entering can_get_sequence_with_no_value_from_namevaluecollection()");
            var collection = new NameValueCollection { { "state", "CA" } };

			mockEventLogger.LogDebug("can_get_sequence_with_no_value_from_namevaluecollection(): collection → {Collection}", collection);
            var value = collection.GetValues<string>("cat");
			mockEventLogger.LogDebug("can_get_sequence_with_no_value_from_namevaluecollection(): value → {Value}", value);

            Assert.That(value, Is.Empty);
			mockEventLogger.LogDebug("<<< Exiting can_get_sequence_with_no_value_from_namevaluecollection()");
        }

        [Test]
        public void can_get_multiple_values_from_namevaluecollection_with_custom_converter()
        {
			mockEventLogger.LogDebug(">>> Entering can_get_multiple_values_from_namevaluecollection_with_custom_converter()");
            var collection = new NameValueCollection { { "state", "12" }, { "state", "1" } };

			mockEventLogger.LogDebug("can_get_multiple_values_from_namevaluecollection_with_custom_converter(): collection → {Collection}", collection);
            var value = collection.GetValues("state", v => int.Parse(v, CultureInfo.InvariantCulture) + 10);
			mockEventLogger.LogDebug("can_get_multiple_values_from_namevaluecollection_with_custom_converter(): value → {Value}", value);

            Assert.That(value, Is.EquivalentTo(new[] { 22, 11 }));
			mockEventLogger.LogDebug("<<< Exiting can_get_multiple_values_from_namevaluecollection_with_custom_converter()");
        }

        [Test]
        public void can_get_value_without_default_from_statebag()
        {
			mockEventLogger.LogDebug(">>> Entering can_get_value_without_default_from_statebag()");
            var dictionary = new StateBag { { "question", "what is it" } };

			mockEventLogger.LogDebug("can_get_value_without_default_from_statebag(): dictionary → {Dictionary}", dictionary);
            var value = dictionary.GetValue<string>("question");
			mockEventLogger.LogDebug("can_get_value_without_default_from_statebag(): value → {Value}", value);

            Assert.That(value, Is.EqualTo("what is it"));
			mockEventLogger.LogDebug("<<< Exiting can_get_value_without_default_from_statebag()");
        }

        [Test]
        public void can_get_value_without_default_from_xnode()
        {
			mockEventLogger.LogDebug(">>> Entering can_get_value_without_default_from_xnode()");
            var node = new XElement(
                "parent",
                new XElement("id", 21));

            var value = node.GetValue<int>("id");
			mockEventLogger.LogDebug("can_get_value_without_default_from_xnode(): value → {Value}", value);

            Assert.That(value, Is.EqualTo(21));
			mockEventLogger.LogDebug("<<< Exiting can_get_value_without_default_from_xnode()");
        }

        [Test]
        public void can_get_value_without_default_from_xmlnode()
        {
			mockEventLogger.LogDebug(">>> Entering can_get_value_without_default_from_xmlnode()");
            var doc = new XmlDocument { XmlResolver = null };
            const string ParentWithIdXml = """
                                           <parent>
                                               <id>123</id>
                                           </parent>
                                           """;
            using var xmlReader = XmlReader.Create(new StringReader(ParentWithIdXml), new XmlReaderSettings { XmlResolver = null, });
			mockEventLogger.LogDebug("can_get_value_without_default_from_xmlnode(): xmlReader → {XmlReader}", xmlReader);
            doc.Load(xmlReader);

            var value = doc.DocumentElement.GetValue<int>("id");
			mockEventLogger.LogDebug("can_get_value_without_default_from_xmlnode(): value → {Value}", value);

            Assert.That(value, Is.EqualTo(123));
			mockEventLogger.LogDebug("<<< Exiting can_get_value_without_default_from_xmlnode()");
        }

        [Test]
        public void getvalue_throws_argumentexception_when_value_is_not_present()
        {
			mockEventLogger.LogDebug(">>> Entering getvalue_throws_argumentexception_when_value_is_not_present()");
            var dictionary = new Hashtable { { "question", "what is it" } };

			mockEventLogger.LogDebug("getvalue_throws_argumentexception_when_value_is_not_present(): dictionary → {Dictionary}", dictionary);
            Assert.That(() => dictionary.GetValue<string>("answer"), Throws.ArgumentException.With.Property("ParamName").EqualTo("key"));
			mockEventLogger.LogDebug("<<< Exiting getvalue_throws_argumentexception_when_value_is_not_present()");
        }

        [Test]
        public void throws_invalidoperationexception_when_lookup_has_multiple_values()
        {
			mockEventLogger.LogDebug(">>> Entering throws_invalidoperationexception_when_lookup_has_multiple_values()");
            var collection = new NameValueCollection { { "state", "CA" }, { "state", "BC" } };

			mockEventLogger.LogDebug("throws_invalidoperationexception_when_lookup_has_multiple_values(): collection → {Collection}", collection);
            Assert.That(() => collection.GetValueOrDefault<string>("state"), Throws.InvalidOperationException);
			mockEventLogger.LogDebug("<<< Exiting throws_invalidoperationexception_when_lookup_has_multiple_values()");
        }

        [Test]
        public void throws_argumentnullexception_when_dictionary_is_null()
        {
			mockEventLogger.LogDebug(">>> Entering throws_argumentnullexception_when_dictionary_is_null()");
            IDictionary dictionary = null;

            Assert.That(() => dictionary.GetValueOrDefault<int>("value ID"), Throws.TypeOf<ArgumentNullException>().With.Property("ParamName").EqualTo("dictionary"));
			mockEventLogger.LogDebug("<<< Exiting throws_argumentnullexception_when_dictionary_is_null()");
        }

        [Test]
        public void throws_argumentnullexception_when_xelement_is_null()
        {
			mockEventLogger.LogDebug(">>> Entering throws_argumentnullexception_when_xelement_is_null()");
            XElement node = null;

            Assert.That(() => node.GetValueOrDefault<int>("value ID"), Throws.TypeOf<ArgumentNullException>().With.Property("ParamName").EqualTo("node"));
			mockEventLogger.LogDebug("<<< Exiting throws_argumentnullexception_when_xelement_is_null()");
        }

        [Test]
        public void throws_argumentnullexception_when_xmlnode_is_null()
        {
			mockEventLogger.LogDebug(">>> Entering throws_argumentnullexception_when_xmlnode_is_null()");
            XmlNode node = null;

            Assert.That(() => node.GetValueOrDefault<int>("value ID"), Throws.TypeOf<ArgumentNullException>().With.Property("ParamName").EqualTo("node"));
			mockEventLogger.LogDebug("<<< Exiting throws_argumentnullexception_when_xmlnode_is_null()");
        }

        [Test]
        public void does_not_throw_invalidcastexception_when_value_is_null_for_reference_type()
        {
			mockEventLogger.LogDebug(">>> Entering does_not_throw_invalidcastexception_when_value_is_null_for_reference_type()");
            var dictionary = new Dictionary<string, string> { { "length", null } };

			mockEventLogger.LogDebug("does_not_throw_invalidcastexception_when_value_is_null_for_reference_type(): dictionary → {Dictionary}", dictionary);
            var value = dictionary.GetValueOrDefault<ApplicationException>("length");
			mockEventLogger.LogDebug("does_not_throw_invalidcastexception_when_value_is_null_for_reference_type(): value → {Value}", value);

            Assert.That(value, Is.Null);
			mockEventLogger.LogDebug("<<< Exiting does_not_throw_invalidcastexception_when_value_is_null_for_reference_type()");
        }

        [Test]
        public void tolookup_throws_argumentnullexception_when_namevaluecollection_is_null()
        {
			mockEventLogger.LogDebug(">>> Entering tolookup_throws_argumentnullexception_when_namevaluecollection_is_null()");
            NameValueCollection col = null;

            Assert.That(() => col.ToLookup(), Throws.TypeOf<ArgumentNullException>().With.Property("ParamName").EqualTo("collection"));
			mockEventLogger.LogDebug("<<< Exiting tolookup_throws_argumentnullexception_when_namevaluecollection_is_null()");
        }
    }
}
