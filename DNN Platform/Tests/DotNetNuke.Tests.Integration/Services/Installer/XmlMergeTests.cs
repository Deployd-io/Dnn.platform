// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Tests.Integration.Services.Installer
{
    using System;
    using System.Diagnostics;
    using System.IO;
    using System.Reflection;
    using System.Xml;

    using DotNetNuke.Instrumentation;
    using DotNetNuke.Services.Installer;
    using DotNetNuke.Tests.Utilities;
    using NUnit.Framework;
	using Microsoft.Extensions.Logging;

    [TestFixture]
    public class XmlMergeTests : DnnUnitTest
    {
		private static readonly Microsoft.Extensions.Logging.ILogger _logger = Microsoft.Extensions.Logging.Abstractions.NullLogger.Instance;
        private const bool OutputXml = true;
        private readonly Assembly _assembly = typeof(XmlMergeTests).Assembly;

        [OneTimeSetUp]
        public override void OneTimeSetUp()
        {
			_logger.LogDebug(">>> Entering OneTimeSetUp()");
            base.OneTimeSetUp();
			_logger.LogDebug("<<< Exiting OneTimeSetUp()");
        }

        [SetUp]
        public void SetUp()
        {
			_logger.LogDebug(">>> Entering SetUp()");
            AppDomain.CurrentDomain.SetData("APPBASE", this.WebsitePhysicalAppPath);

            LoggerSource.SetTestableInstance(new TestLogSource());
			_logger.LogDebug("<<< Exiting SetUp()");
        }

        // ReSharper disable PossibleNullReferenceException
        [Test]
        public void SimpleUpdate()
        {
			_logger.LogDebug(">>> Entering SimpleUpdate()");
            XmlDocument targetDoc = this.ExecuteMerge();

			_logger.LogDebug("SimpleUpdate(): targetDoc → {TargetDoc}", targetDoc);
            // children are in correct location
            XmlNodeList nodes = targetDoc.SelectNodes("/configuration/updateme/children/child");
			_logger.LogDebug("SimpleUpdate(): nodes → {Nodes}", nodes);
            Assert.That(nodes, Has.Count.EqualTo(2));

            // children only inserted once
            nodes = targetDoc.SelectNodes("//child");
            Assert.That(nodes, Has.Count.EqualTo(2));
			_logger.LogDebug("<<< Exiting SimpleUpdate()");
        }

        [Test]
        public void SimpleUpdateInLocation()
        {
			_logger.LogDebug(">>> Entering SimpleUpdateInLocation()");
            XmlDocument targetDoc = this.ExecuteMerge("SimpleUpdate");

			_logger.LogDebug("SimpleUpdateInLocation(): targetDoc → {TargetDoc}", targetDoc);
            // children are in correct location
            XmlNodeList nodes = targetDoc.SelectNodes("/configuration/location/updateme/children/child");
			_logger.LogDebug("SimpleUpdateInLocation(): nodes → {Nodes}", nodes);
            Assert.That(nodes, Has.Count.EqualTo(2));

            // children only inserted once
            nodes = targetDoc.SelectNodes("//child");
            Assert.That(nodes, Has.Count.EqualTo(2));
			_logger.LogDebug("<<< Exiting SimpleUpdateInLocation()");
        }

        [Test]
        public void SimpleUpdateInLocationWithDistractingLocations()
        {
			_logger.LogDebug(">>> Entering SimpleUpdateInLocationWithDistractingLocations()");
            XmlDocument targetDoc = this.ExecuteMerge("SimpleUpdate");

			_logger.LogDebug("SimpleUpdateInLocationWithDistractingLocations(): targetDoc → {TargetDoc}", targetDoc);
            // children are in correct location
            XmlNodeList nodes = targetDoc.SelectNodes("/configuration/location/updateme/children/child");
			_logger.LogDebug("SimpleUpdateInLocationWithDistractingLocations(): nodes → {Nodes}", nodes);
            Assert.That(nodes, Has.Count.EqualTo(2));

            // children only inserted once
            nodes = targetDoc.SelectNodes("//child");
            Assert.That(nodes, Has.Count.EqualTo(2));
			_logger.LogDebug("<<< Exiting SimpleUpdateInLocationWithDistractingLocations()");
        }

        [Test]
        public void UpdateWithTargetPath()
        {
			_logger.LogDebug(">>> Entering UpdateWithTargetPath()");
            XmlDocument targetDoc = this.ExecuteMerge();

			_logger.LogDebug("UpdateWithTargetPath(): targetDoc → {TargetDoc}", targetDoc);
            // children are in correct location
            XmlNodeList nodes = targetDoc.SelectNodes("/configuration/updateme/children/child");
			_logger.LogDebug("UpdateWithTargetPath(): nodes → {Nodes}", nodes);
            Assert.That(nodes, Has.Count.EqualTo(2));

            // children only inserted once
            nodes = targetDoc.SelectNodes("//child");
            Assert.That(nodes, Has.Count.EqualTo(2));
			_logger.LogDebug("<<< Exiting UpdateWithTargetPath()");
        }

        [Test]
        public void UpdateInLocationWithTargetPath()
        {
			_logger.LogDebug(">>> Entering UpdateInLocationWithTargetPath()");
            XmlDocument targetDoc = this.ExecuteMerge("UpdateWithTargetPath");

			_logger.LogDebug("UpdateInLocationWithTargetPath(): targetDoc → {TargetDoc}", targetDoc);
            // children are in correct location
            XmlNodeList nodes = targetDoc.SelectNodes("/configuration/location/updateme/children/child");
			_logger.LogDebug("UpdateInLocationWithTargetPath(): nodes → {Nodes}", nodes);
            Assert.That(nodes, Has.Count.EqualTo(2));

            // children only inserted once
            nodes = targetDoc.SelectNodes("//child");
            Assert.That(nodes, Has.Count.EqualTo(2));
			_logger.LogDebug("<<< Exiting UpdateInLocationWithTargetPath()");
        }

        [Test]
        public void UpdateWithDistractingLocationAndTargetPath()
        {
			_logger.LogDebug(">>> Entering UpdateWithDistractingLocationAndTargetPath()");
            XmlDocument targetDoc = this.ExecuteMerge("UpdateWithTargetPath");

			_logger.LogDebug("UpdateWithDistractingLocationAndTargetPath(): targetDoc → {TargetDoc}", targetDoc);
            // children are in correct location
            XmlNodeList nodes = targetDoc.SelectNodes("/configuration/updateme/children/child");
			_logger.LogDebug("UpdateWithDistractingLocationAndTargetPath(): nodes → {Nodes}", nodes);
            Assert.That(nodes, Has.Count.EqualTo(2));

            // children only inserted once
            nodes = targetDoc.SelectNodes("//child");
            Assert.That(nodes, Has.Count.EqualTo(2));
			_logger.LogDebug("<<< Exiting UpdateWithDistractingLocationAndTargetPath()");
        }

        [Test]
        public void UpdateInLocationWithDistractingLocationAndTargetPath()
        {
			_logger.LogDebug(">>> Entering UpdateInLocationWithDistractingLocationAndTargetPath()");
            XmlDocument targetDoc = this.ExecuteMerge("UpdateWithTargetPath");

			_logger.LogDebug("UpdateInLocationWithDistractingLocationAndTargetPath(): targetDoc → {TargetDoc}", targetDoc);
            // children are in correct location
            XmlNodeList nodes = targetDoc.SelectNodes("/configuration/location/updateme/children/child");
			_logger.LogDebug("UpdateInLocationWithDistractingLocationAndTargetPath(): nodes → {Nodes}", nodes);
            Assert.That(nodes, Has.Count.EqualTo(2));

            // children only inserted once
            nodes = targetDoc.SelectNodes("//child");
            Assert.That(nodes, Has.Count.EqualTo(2));
			_logger.LogDebug("<<< Exiting UpdateInLocationWithDistractingLocationAndTargetPath()");
        }

        [Test]
        public void UpdateInFirstLocationWithDistractingLocationAndTargetPath()
        {
			_logger.LogDebug(">>> Entering UpdateInFirstLocationWithDistractingLocationAndTargetPath()");
            XmlDocument targetDoc = this.ExecuteMerge("UpdateWithTargetPath");

			_logger.LogDebug("UpdateInFirstLocationWithDistractingLocationAndTargetPath(): targetDoc → {TargetDoc}", targetDoc);
            // children are in correct location
            // first location/updateme has updated node
            XmlNode root = targetDoc.SelectSingleNode("/configuration/location[1]");
			_logger.LogWarning("UpdateInFirstLocationWithDistractingLocationAndTargetPath(): root → {Root}", root);
            XmlNodeList nodes = root.SelectNodes("updateme/children/child");
			_logger.LogDebug("UpdateInFirstLocationWithDistractingLocationAndTargetPath(): nodes → {Nodes}", nodes);
            Assert.That(nodes, Has.Count.EqualTo(2));

            // second location/updateme still empty
            root = targetDoc.SelectSingleNode("/configuration/location[2]");
            nodes = root.SelectNodes("updateme/children/child");
            Assert.That(nodes, Is.Empty);

            // children only inserted once
            nodes = targetDoc.SelectNodes("//child");
            Assert.That(nodes, Has.Count.EqualTo(2));

            // two instances of location/updateme exist
            nodes = targetDoc.SelectNodes("//configuration/location/updateme");
            Assert.That(nodes, Has.Count.EqualTo(2));
			_logger.LogDebug("<<< Exiting UpdateInFirstLocationWithDistractingLocationAndTargetPath()");
        }

        [Test]
        public void SimpleAdd()
        {
			_logger.LogDebug(">>> Entering SimpleAdd()");
            XmlDocument targetDoc = this.ExecuteMerge();

			_logger.LogDebug("SimpleAdd(): targetDoc → {TargetDoc}", targetDoc);
            // children are in correct location
            XmlNodeList nodes = targetDoc.SelectNodes("/configuration/updateme/children/child");
			_logger.LogDebug("SimpleAdd(): nodes → {Nodes}", nodes);
            Assert.That(nodes, Has.Count.EqualTo(2));
			_logger.LogDebug("<<< Exiting SimpleAdd()");
        }

        [Test]
        public void AddWithLocation()
        {
			_logger.LogDebug(">>> Entering AddWithLocation()");
            XmlDocument targetDoc = this.ExecuteMerge("SimpleAdd");

			_logger.LogDebug("AddWithLocation(): targetDoc → {TargetDoc}", targetDoc);
            // children are in correct location
            XmlNodeList nodes = targetDoc.SelectNodes("/configuration/updateme/children/child");
			_logger.LogDebug("AddWithLocation(): nodes → {Nodes}", nodes);
            Assert.That(nodes, Has.Count.EqualTo(2));

            // second location/updateme still empty
            var root = targetDoc.SelectSingleNode("/configuration/location[2]");
			_logger.LogWarning("AddWithLocation(): root → {Root}", root);
            nodes = root.SelectNodes("updateme/children/child");
            Assert.That(nodes, Is.Empty);

            // children only inserted once
            nodes = targetDoc.SelectNodes("//child");
            Assert.That(nodes, Has.Count.EqualTo(2));

            // 1 instance of location/updateme exist
            nodes = targetDoc.SelectNodes("//configuration/location/updateme");
            Assert.That(nodes, Has.Count.EqualTo(1));
			_logger.LogDebug("<<< Exiting AddWithLocation()");
        }

        [Test]
        public void SimpleInsertBefore()
        {
			_logger.LogDebug(">>> Entering SimpleInsertBefore()");
            XmlDocument targetDoc = this.ExecuteMerge();

			_logger.LogDebug("SimpleInsertBefore(): targetDoc → {TargetDoc}", targetDoc);
            // children are in correct location
            XmlNodeList nodes = targetDoc.SelectNodes("/configuration/updateme/children/child");
			_logger.LogDebug("SimpleInsertBefore(): nodes → {Nodes}", nodes);
            Assert.That(nodes, Has.Count.EqualTo(2));

            // inserted before node2
            XmlNode node = targetDoc.SelectSingleNode("/configuration/updateme");
			_logger.LogDebug("SimpleInsertBefore(): node → {Node}", node);
            Assert.That(node.NextSibling.Name, Is.EqualTo("node2"));
			_logger.LogDebug("<<< Exiting SimpleInsertBefore()");
        }

        [Test]
        public void InsertBeforeInLocation()
        {
			_logger.LogDebug(">>> Entering InsertBeforeInLocation()");
            XmlDocument targetDoc = this.ExecuteMerge("SimpleInsertBefore");

			_logger.LogDebug("InsertBeforeInLocation(): targetDoc → {TargetDoc}", targetDoc);
            // children are in correct location
            XmlNodeList nodes = targetDoc.SelectNodes("/configuration/location/updateme/children/child");
			_logger.LogDebug("InsertBeforeInLocation(): nodes → {Nodes}", nodes);
            Assert.That(nodes, Has.Count.EqualTo(2));

            // inserted before node2
            XmlNode node = targetDoc.SelectSingleNode("/configuration/location/updateme");
			_logger.LogDebug("InsertBeforeInLocation(): node → {Node}", node);
            Assert.That(node.NextSibling.Name, Is.EqualTo("node2"));
			_logger.LogDebug("<<< Exiting InsertBeforeInLocation()");
        }

        [Test]
        public void SimpleInsertAfter()
        {
			_logger.LogDebug(">>> Entering SimpleInsertAfter()");
            XmlDocument targetDoc = this.ExecuteMerge();

			_logger.LogDebug("SimpleInsertAfter(): targetDoc → {TargetDoc}", targetDoc);
            // children are in correct location
            XmlNodeList nodes = targetDoc.SelectNodes("/configuration/updateme/children/child");
			_logger.LogDebug("SimpleInsertAfter(): nodes → {Nodes}", nodes);
            Assert.That(nodes, Has.Count.EqualTo(2));

            // inserted before node2
            XmlNode node = targetDoc.SelectSingleNode("/configuration/updateme");
			_logger.LogDebug("SimpleInsertAfter(): node → {Node}", node);
            Assert.That(node.PreviousSibling.Name, Is.EqualTo("node1"));
			_logger.LogDebug("<<< Exiting SimpleInsertAfter()");
        }

        [Test]
        public void InsertAfterInLocation()
        {
			_logger.LogDebug(">>> Entering InsertAfterInLocation()");
            XmlDocument targetDoc = this.ExecuteMerge("SimpleInsertAfter");

			_logger.LogDebug("InsertAfterInLocation(): targetDoc → {TargetDoc}", targetDoc);
            // children are in correct location
            XmlNodeList nodes = targetDoc.SelectNodes("/configuration/location/updateme/children/child");
			_logger.LogDebug("InsertAfterInLocation(): nodes → {Nodes}", nodes);
            Assert.That(nodes, Has.Count.EqualTo(2));

            // inserted before node2
            XmlNode node = targetDoc.SelectSingleNode("/configuration/location/updateme");
			_logger.LogDebug("InsertAfterInLocation(): node → {Node}", node);
            Assert.That(node.NextSibling.Name, Is.EqualTo("node2"));
			_logger.LogDebug("<<< Exiting InsertAfterInLocation()");
        }

        [Test]
        public void SimpleRemove()
        {
			_logger.LogDebug(">>> Entering SimpleRemove()");
            XmlDocument targetDoc = this.ExecuteMerge();

			_logger.LogDebug("SimpleRemove(): targetDoc → {TargetDoc}", targetDoc);
            // node is gone
            var nodes = targetDoc.SelectNodes("//removeme");
			_logger.LogDebug("SimpleRemove(): nodes → {Nodes}", nodes);
            Assert.That(nodes, Is.Empty);

            // other nodes still present
            nodes = targetDoc.SelectNodes("/configuration/distraction");
            Assert.That(nodes, Has.Count.EqualTo(1));
			_logger.LogDebug("<<< Exiting SimpleRemove()");
        }

        [Test]
        public void RemoveFromLocation()
        {
			_logger.LogDebug(">>> Entering RemoveFromLocation()");
            XmlDocument targetDoc = this.ExecuteMerge("SimpleRemove");

			_logger.LogDebug("RemoveFromLocation(): targetDoc → {TargetDoc}", targetDoc);
            // node is gone
            var nodes = targetDoc.SelectNodes("//removeme");
			_logger.LogDebug("RemoveFromLocation(): nodes → {Nodes}", nodes);
            Assert.That(nodes, Is.Empty);

            // other nodes still present
            nodes = targetDoc.SelectNodes("/configuration/distraction");
            Assert.That(nodes, Has.Count.EqualTo(1));
			_logger.LogDebug("<<< Exiting RemoveFromLocation()");
        }

        [Test]
        public void SimpleRemoveAttribute()
        {
			_logger.LogDebug(">>> Entering SimpleRemoveAttribute()");
            var targetDoc = this.ExecuteMerge();

			_logger.LogDebug("SimpleRemoveAttribute(): targetDoc → {TargetDoc}", targetDoc);
            var node = targetDoc.SelectSingleNode("/configuration/updateme");
			_logger.LogDebug("SimpleRemoveAttribute(): node → {Node}", node);
            Assert.That(node.Attributes, Is.Empty);
			_logger.LogDebug("<<< Exiting SimpleRemoveAttribute()");
        }

        [Test]
        public void RemoveAttributeFromLocation()
        {
			_logger.LogDebug(">>> Entering RemoveAttributeFromLocation()");
            var targetDoc = this.ExecuteMerge("SimpleRemoveAttribute");

			_logger.LogDebug("RemoveAttributeFromLocation(): targetDoc → {TargetDoc}", targetDoc);
            var node = targetDoc.SelectSingleNode("/configuration/location/updateme");
			_logger.LogDebug("RemoveAttributeFromLocation(): node → {Node}", node);
            Assert.That(node.Attributes, Is.Empty);
			_logger.LogDebug("<<< Exiting RemoveAttributeFromLocation()");
        }

        [Test]
        public void SimpleInsertAttribute()
        {
			_logger.LogDebug(">>> Entering SimpleInsertAttribute()");
            var targetDoc = this.ExecuteMerge();

			_logger.LogDebug("SimpleInsertAttribute(): targetDoc → {TargetDoc}", targetDoc);
            var node = targetDoc.SelectSingleNode("/configuration/updateme");
			_logger.LogDebug("SimpleInsertAttribute(): node → {Node}", node);
            Assert.That(node.Attributes, Has.Count.EqualTo(2));
            Assert.That(node.Attributes["attrib2"].Value, Is.EqualTo("fee"));
			_logger.LogDebug("<<< Exiting SimpleInsertAttribute()");
        }

        [Test]
        public void InsertAttributeInLocation()
        {
			_logger.LogDebug(">>> Entering InsertAttributeInLocation()");
            var targetDoc = this.ExecuteMerge("SimpleInsertAttribute");

			_logger.LogDebug("InsertAttributeInLocation(): targetDoc → {TargetDoc}", targetDoc);
            var node = targetDoc.SelectSingleNode("/configuration/location/updateme");
			_logger.LogDebug("InsertAttributeInLocation(): node → {Node}", node);
            Assert.That(node.Attributes, Has.Count.EqualTo(2));
            Assert.That(node.Attributes["attrib2"].Value, Is.EqualTo("fee"));
			_logger.LogDebug("<<< Exiting InsertAttributeInLocation()");
        }

        [Test]
        public void UpdateAttributeInLocation()
        {
			_logger.LogDebug(">>> Entering UpdateAttributeInLocation()");
            var targetDoc = this.ExecuteMerge("SimpleInsertAttribute");

			_logger.LogDebug("UpdateAttributeInLocation(): targetDoc → {TargetDoc}", targetDoc);
            var node = targetDoc.SelectSingleNode("/configuration/location/updateme");
			_logger.LogDebug("UpdateAttributeInLocation(): node → {Node}", node);
            Assert.That(node.Attributes, Has.Count.EqualTo(2));
            Assert.That(node.Attributes["attrib2"].Value, Is.EqualTo("fee"));
			_logger.LogDebug("<<< Exiting UpdateAttributeInLocation()");
        }

        [Test]
        public void SimpleUpdateWithKey()
        {
			_logger.LogDebug(">>> Entering SimpleUpdateWithKey()");
            var targetDoc = this.ExecuteMerge();

			_logger.LogDebug("SimpleUpdateWithKey(): targetDoc → {TargetDoc}", targetDoc);
            // a key was added
            var nodes = targetDoc.SelectNodes("/configuration/updateme/add");
			_logger.LogDebug("SimpleUpdateWithKey(): nodes → {Nodes}", nodes);
            Assert.That(nodes, Has.Count.EqualTo(1));

            // test attribute is set
            var node = nodes[0];
            Assert.That(node.Attributes["test"].Value, Is.EqualTo("foo"));
			_logger.LogDebug("<<< Exiting SimpleUpdateWithKey()");
        }

        [Test]
        public void UpdateWithKeyInLocation()
        {
			_logger.LogDebug(">>> Entering UpdateWithKeyInLocation()");
            var targetDoc = this.ExecuteMerge("SimpleUpdateWithKey");

			_logger.LogDebug("UpdateWithKeyInLocation(): targetDoc → {TargetDoc}", targetDoc);
            // a key was added
            var nodes = targetDoc.SelectNodes("/configuration/location/updateme/add");
			_logger.LogDebug("UpdateWithKeyInLocation(): nodes → {Nodes}", nodes);
            Assert.That(nodes, Has.Count.EqualTo(1));

            // test attribute is set
            var node = nodes[0];
            Assert.That(node.Attributes["test"].Value, Is.EqualTo("foo"));
			_logger.LogDebug("<<< Exiting UpdateWithKeyInLocation()");
        }

        [Test]
        public void NoChangeOnOverwrite()
        {
			_logger.LogDebug(">>> Entering NoChangeOnOverwrite()");
            XmlMerge merge = this.GetXmlMerge(nameof(this.NoChangeOnOverwrite));
            XmlDocument targetDoc = this.LoadTargetDoc(nameof(this.NoChangeOnOverwrite));
			_logger.LogDebug("NoChangeOnOverwrite(): targetDoc → {TargetDoc}", targetDoc);
			_logger.LogDebug("NoChangeOnOverwrite(): merge → {Merge}", merge);

            merge.UpdateConfig(targetDoc);

            this.WriteToDebug(targetDoc);

            var nodes = targetDoc.SelectNodes("/configuration/appSettings/add");
			_logger.LogDebug("NoChangeOnOverwrite(): nodes → {Nodes}", nodes);
            using (Assert.EnterMultipleScope())
            {
                Assert.That(nodes, Has.Count.EqualTo(3));

                Assert.That(merge.ConfigUpdateChangedNodes, Is.False);
            }
			_logger.LogDebug("<<< Exiting NoChangeOnOverwrite()");
        }

        [Test]
        public void ShouldChangeOnOverwrite()
        {
			_logger.LogDebug(">>> Entering ShouldChangeOnOverwrite()");
            XmlMerge merge = this.GetXmlMerge(nameof(this.ShouldChangeOnOverwrite));
            XmlDocument targetDoc = this.LoadTargetDoc(nameof(this.ShouldChangeOnOverwrite));
			_logger.LogDebug("ShouldChangeOnOverwrite(): targetDoc → {TargetDoc}", targetDoc);
			_logger.LogDebug("ShouldChangeOnOverwrite(): merge → {Merge}", merge);

            merge.UpdateConfig(targetDoc);

            this.WriteToDebug(targetDoc);

            var nodes = targetDoc.SelectNodes("/configuration/appSettings/add");
			_logger.LogDebug("ShouldChangeOnOverwrite(): nodes → {Nodes}", nodes);
            using (Assert.EnterMultipleScope())
            {
                Assert.That(nodes, Has.Count.EqualTo(3));

                Assert.That(merge.ConfigUpdateChangedNodes, Is.True);
            }
			_logger.LogDebug("<<< Exiting ShouldChangeOnOverwrite()");
        }

        [Test]
        public void ShouldPreserveEmptyNamespaceOnSave()
        {
			_logger.LogDebug(">>> Entering ShouldPreserveEmptyNamespaceOnSave()");
            var targetDoc = this.ExecuteMerge();

			_logger.LogDebug("ShouldPreserveEmptyNamespaceOnSave(): targetDoc → {TargetDoc}", targetDoc);
            var ns = new XmlNamespaceManager(targetDoc.NameTable);
			_logger.LogDebug("ShouldPreserveEmptyNamespaceOnSave(): ns → {Ns}", ns);
            ns.AddNamespace("ab", "urn:schemas-microsoft-com:asm.v1");

            // removed the existing node, since it matched targetpath attribute
            var nodesWithNamespace = targetDoc.SelectNodes("/configuration/runtime/ab:assemblyBinding/ab:dependentAssembly", ns);
			_logger.LogDebug("ShouldPreserveEmptyNamespaceOnSave(): nodesWithNamespace → {NodesWithNamespace}", nodesWithNamespace);
            Assert.That(nodesWithNamespace, Is.Empty);

            // added a new node with xmlns=""
            var nodesWithoutNamespace = targetDoc.SelectNodes("/configuration/runtime/ab:assemblyBinding/dependentAssembly", ns);
			_logger.LogDebug("ShouldPreserveEmptyNamespaceOnSave(): nodesWithoutNamespace → {NodesWithoutNamespace}", nodesWithoutNamespace);
            Assert.That(nodesWithoutNamespace, Has.Count.EqualTo(1));

            // non-namespaced node has newVersion from merge
            var dependentAssembly = nodesWithoutNamespace[0];
            var bindingRedirect = dependentAssembly.SelectSingleNode("bindingRedirect", ns);
			_logger.LogDebug("ShouldPreserveEmptyNamespaceOnSave(): bindingRedirect → {BindingRedirect}", bindingRedirect);
            Assert.That(bindingRedirect.Attributes["newVersion"].Value, Is.EqualTo("4.1.0.0"));
			_logger.LogDebug("<<< Exiting ShouldPreserveEmptyNamespaceOnSave()");
        }

        /// <summary>Merges the Merge and Target files based on the name of the calling method.</summary>
        /// <remarks>xml files must be embedded resources in the MergeFiles folder named {method}Merge.xml and {method}Target.xml.</remarks>
        /// <returns>XmlDocument with the result of the merge operation.</returns>
        private XmlDocument ExecuteMerge()
        {
			_logger.LogDebug(">>> Entering ExecuteMerge()");
			_logger.LogDebug("<<< Exiting ExecuteMerge()");
			_logger.LogDebug("<<< Exiting ExecuteMerge(mergeName={MergeName})", mergeName);
            return this.ExecuteMerge(null);
        }

        /// <summary>As ExecuteMerge but allows the merge file prefix to be specified.</summary>
        private XmlDocument ExecuteMerge(string mergeName)
        {
            string testMethodName = this.GetTestMethodName();

            XmlMerge merge = this.GetXmlMerge(mergeName ?? testMethodName);
            XmlDocument targetDoc = this.LoadTargetDoc(testMethodName);

            merge.UpdateConfig(targetDoc);

            this.WriteToDebug(targetDoc);

            return targetDoc;
        }

        private string GetTestMethodName()
        {
			_logger.LogDebug(">>> Entering GetTestMethodName()");
            var st = new StackTrace(2);

			_logger.LogDebug("GetTestMethodName(): st → {St}", st);
            string name;
            int i = 0;
            do
            {
                name = st.GetFrame(i).GetMethod().Name;
                i++;
				_logger.LogDebug("GetTestMethodName(): name → {Name}", name);
            }
            while (name == "ExecuteMerge");

			_logger.LogDebug("<<< Exiting GetTestMethodName()");
            return name;
        }

        private XmlDocument LoadTargetDoc(string testMethodName)
        {
			_logger.LogDebug(">>> Entering LoadTargetDoc(testMethodName={TestMethodName})", testMethodName);
            using (Stream targetStream =
                this._assembly.GetManifestResourceStream(string.Format(
                    "DotNetNuke.Tests.Integration.Services.Installer.MergeFiles.{0}Target.xml",
                    testMethodName)))
            {
                Debug.Assert(
                    targetStream != null,
                    string.Format("Unable to location embedded resource for {0}Target.xml", testMethodName));
                var targetDoc = new XmlDocument { XmlResolver = null };
                using var xmlReader = XmlReader.Create(targetStream, new XmlReaderSettings { XmlResolver = null, });
				_logger.LogDebug("LoadTargetDoc(testMethodName={TestMethodName}): xmlReader → {XmlReader}", testMethodName, xmlReader);
                targetDoc.Load(xmlReader);
				_logger.LogDebug("<<< Exiting LoadTargetDoc(testMethodName={TestMethodName})", testMethodName);
                return targetDoc;
            }
        }

        private XmlMerge GetXmlMerge(string fileName)
        {
			_logger.LogDebug(">>> Entering GetXmlMerge(fileName={FileName})", fileName);
            using (Stream mergeStream =
                this._assembly.GetManifestResourceStream(string.Format(
                    "DotNetNuke.Tests.Integration.Services.Installer.MergeFiles.{0}Merge.xml",
                    fileName)))
            {
                Debug.Assert(
                    mergeStream != null,
                    string.Format("Unable to location embedded resource for {0}Merge.xml", fileName));
                var merge = new XmlMerge(mergeStream, "version", "sender");
				_logger.LogDebug("GetXmlMerge(fileName={FileName}): merge → {Merge}", fileName, merge);
				_logger.LogDebug("<<< Exiting GetXmlMerge(fileName={FileName})", fileName);
                return merge;
            }
        }

        private void WriteToDebug(XmlDocument targetDoc)
        {
            // ReSharper disable ConditionIsAlwaysTrueOrFalse
            if (OutputXml)

            // ReSharper restore ConditionIsAlwaysTrueOrFalse
            {
                using (var writer = new StreamWriter(new MemoryStream()))
                {
                    targetDoc.Save(writer);
                    writer.BaseStream.Seek(0, SeekOrigin.Begin);
                    using (var sr = new StreamReader(writer.BaseStream))
                    {
                        Debug.WriteLine("{0}", sr.ReadToEnd());
                    }
                }
            }
			_logger.LogDebug("<<< Exiting WriteToDebug(targetDoc={TargetDoc})", targetDoc);
        }

        // ReSharper restore PossibleNullReferenceException
    }

    internal class TestLogger : ILog
    {
        public bool IsDebugEnabled
        {
            get { return false; }
        }

        public bool IsErrorEnabled
        {
            get { return false; }
        }

        public bool IsFatalEnabled
        {
            get { return false; }
        }

        public bool IsInfoEnabled
        {
            get { return false; }
        }

        public bool IsTraceEnabled
        {
            get { return false; }
        }

        public bool IsWarnEnabled
        {
            get { return false; }
        }

        public void Debug(object message, Exception exception)
        {
		_logger.LogDebug(">>> Entering Debug(message={Message},exception={Exception})", message, exception);
		_logger.LogDebug("<<< Exiting Debug(message={Message},exception={Exception})", message, exception);
		_logger.LogDebug(">>> Entering Debug(message={Message})", message);
        }

        public void Debug(object message)
        {
		_logger.LogDebug("<<< Exiting Debug(message={Message})", message);
        }

        public void DebugFormat(IFormatProvider provider, string format, params object[] args)
        {
		_logger.LogDebug(">>> Entering DebugFormat(provider={Provider},format={Format},args={Args})", provider, format, args);
		_logger.LogDebug("<<< Exiting DebugFormat(provider={Provider},format={Format},args={Args})", provider, format, args);
		_logger.LogDebug(">>> Entering DebugFormat(format={Format},args={Args})", format, args);
        }

        public void DebugFormat(string format, params object[] args)
        {
		_logger.LogDebug("<<< Exiting DebugFormat(format={Format},args={Args})", format, args);
        }

        public void Error(object message, Exception exception)
        {
		_logger.LogDebug(">>> Entering Error(message={Message},exception={Exception})", message, exception);
		_logger.LogDebug("<<< Exiting Error(message={Message},exception={Exception})", message, exception);
		_logger.LogDebug(">>> Entering Error(message={Message})", message);
        }

        public void Error(object message)
        {
		_logger.LogDebug("<<< Exiting Error(message={Message})", message);
        }

        public void ErrorFormat(IFormatProvider provider, string format, params object[] args)
        {
		_logger.LogDebug(">>> Entering ErrorFormat(provider={Provider},format={Format},args={Args})", provider, format, args);
		_logger.LogDebug("<<< Exiting ErrorFormat(provider={Provider},format={Format},args={Args})", provider, format, args);
		_logger.LogDebug(">>> Entering ErrorFormat(format={Format},args={Args})", format, args);
        }

        public void ErrorFormat(string format, params object[] args)
        {
		_logger.LogDebug("<<< Exiting ErrorFormat(format={Format},args={Args})", format, args);
        }

        public void Fatal(object message, Exception exception)
        {
		_logger.LogDebug(">>> Entering Fatal(message={Message},exception={Exception})", message, exception);
		_logger.LogDebug("<<< Exiting Fatal(message={Message},exception={Exception})", message, exception);
		_logger.LogDebug(">>> Entering Fatal(message={Message})", message);
        }

        public void Fatal(object message)
        {
		_logger.LogDebug("<<< Exiting Fatal(message={Message})", message);
        }

        public void FatalFormat(IFormatProvider provider, string format, params object[] args)
        {
		_logger.LogDebug(">>> Entering FatalFormat(provider={Provider},format={Format},args={Args})", provider, format, args);
		_logger.LogDebug("<<< Exiting FatalFormat(provider={Provider},format={Format},args={Args})", provider, format, args);
		_logger.LogDebug(">>> Entering FatalFormat(format={Format},args={Args})", format, args);
        }

        public void FatalFormat(string format, params object[] args)
        {
		_logger.LogDebug("<<< Exiting FatalFormat(format={Format},args={Args})", format, args);
        }

        public void Info(object message, Exception exception)
        {
		_logger.LogDebug(">>> Entering Info(message={Message},exception={Exception})", message, exception);
		_logger.LogDebug("<<< Exiting Info(message={Message},exception={Exception})", message, exception);
		_logger.LogDebug(">>> Entering Info(message={Message})", message);
        }

        public void Info(object message)
        {
		_logger.LogDebug("<<< Exiting Info(message={Message})", message);
        }

        public void InfoFormat(IFormatProvider provider, string format, params object[] args)
        {
		_logger.LogDebug(">>> Entering InfoFormat(provider={Provider},format={Format},args={Args})", provider, format, args);
		_logger.LogDebug("<<< Exiting InfoFormat(provider={Provider},format={Format},args={Args})", provider, format, args);
		_logger.LogDebug(">>> Entering InfoFormat(format={Format},args={Args})", format, args);
        }

        public void InfoFormat(string format, params object[] args)
        {
		_logger.LogDebug("<<< Exiting InfoFormat(format={Format},args={Args})", format, args);
        }

        public void Trace(object message, Exception exception)
        {
		_logger.LogDebug(">>> Entering Trace(message={Message},exception={Exception})", message, exception);
		_logger.LogDebug("<<< Exiting Trace(message={Message},exception={Exception})", message, exception);
		_logger.LogDebug(">>> Entering Trace(message={Message})", message);
        }

        public void Trace(object message)
        {
		_logger.LogDebug("<<< Exiting Trace(message={Message})", message);
        }

        public void TraceFormat(IFormatProvider provider, string format, params object[] args)
        {
		_logger.LogDebug(">>> Entering TraceFormat(provider={Provider},format={Format},args={Args})", provider, format, args);
		_logger.LogDebug("<<< Exiting TraceFormat(provider={Provider},format={Format},args={Args})", provider, format, args);
		_logger.LogDebug(">>> Entering TraceFormat(format={Format},args={Args})", format, args);
        }

        public void TraceFormat(string format, params object[] args)
        {
		_logger.LogDebug("<<< Exiting TraceFormat(format={Format},args={Args})", format, args);
        }

        public void Warn(object message, Exception exception)
        {
		_logger.LogDebug(">>> Entering Warn(message={Message},exception={Exception})", message, exception);
		_logger.LogDebug("<<< Exiting Warn(message={Message},exception={Exception})", message, exception);
		_logger.LogDebug(">>> Entering Warn(message={Message})", message);
        }

        public void Warn(object message)
        {
		_logger.LogDebug("<<< Exiting Warn(message={Message})", message);
        }

        public void WarnFormat(IFormatProvider provider, string format, params object[] args)
        {
		_logger.LogDebug(">>> Entering WarnFormat(provider={Provider},format={Format},args={Args})", provider, format, args);
		_logger.LogDebug("<<< Exiting WarnFormat(provider={Provider},format={Format},args={Args})", provider, format, args);
		_logger.LogDebug(">>> Entering WarnFormat(format={Format},args={Args})", format, args);
        }

        public void WarnFormat(string format, params object[] args)
        {
		_logger.LogDebug("<<< Exiting WarnFormat(format={Format},args={Args})", format, args);
        }
    }
}
