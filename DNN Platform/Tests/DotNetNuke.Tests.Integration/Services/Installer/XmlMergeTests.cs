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

    [TestFixture]
    public class XmlMergeTests : DnnUnitTest
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Opml));
        private const bool OutputXml = true;
        private readonly Assembly _assembly = typeof(XmlMergeTests).Assembly;

        [OneTimeSetUp]
        public override void OneTimeSetUp()
        {
			Logger.LogDebug(">>> Entering OneTimeSetUp()");
            base.OneTimeSetUp();
			Logger.LogDebug("<<< Exiting OneTimeSetUp()");
        }

        [SetUp]
        public void SetUp()
        {
			Logger.LogDebug(">>> Entering SetUp()");
            AppDomain.CurrentDomain.SetData("APPBASE", this.WebsitePhysicalAppPath);

            LoggerSource.SetTestableInstance(new TestLogSource());
			Logger.LogDebug("<<< Exiting SetUp()");
        }

        // ReSharper disable PossibleNullReferenceException
        [Test]
        public void SimpleUpdate()
        {
			Logger.LogDebug(">>> Entering SimpleUpdate()");
            XmlDocument targetDoc = this.ExecuteMerge();

			Logger.LogDebug("SimpleUpdate(): targetDoc → {TargetDoc}", targetDoc);
            // children are in correct location
            XmlNodeList nodes = targetDoc.SelectNodes("/configuration/updateme/children/child");
            Assert.That(nodes, Has.Count.EqualTo(2));
			Logger.LogDebug("SimpleUpdate(): nodes → {Nodes}", nodes);

            // children only inserted once
            nodes = targetDoc.SelectNodes("//child");
            Assert.That(nodes, Has.Count.EqualTo(2));
			Logger.LogDebug("<<< Exiting SimpleUpdate()");
        }

        [Test]
        public void SimpleUpdateInLocation()
        {
			Logger.LogDebug(">>> Entering SimpleUpdateInLocation()");
            XmlDocument targetDoc = this.ExecuteMerge("SimpleUpdate");

			Logger.LogDebug("SimpleUpdateInLocation(): targetDoc → {TargetDoc}", targetDoc);
            // children are in correct location
            XmlNodeList nodes = targetDoc.SelectNodes("/configuration/location/updateme/children/child");
            Assert.That(nodes, Has.Count.EqualTo(2));
			Logger.LogDebug("SimpleUpdateInLocation(): nodes → {Nodes}", nodes);

            // children only inserted once
            nodes = targetDoc.SelectNodes("//child");
            Assert.That(nodes, Has.Count.EqualTo(2));
			Logger.LogDebug("<<< Exiting SimpleUpdateInLocation()");
        }

        [Test]
        public void SimpleUpdateInLocationWithDistractingLocations()
        {
			Logger.LogDebug(">>> Entering SimpleUpdateInLocationWithDistractingLocations()");
            XmlDocument targetDoc = this.ExecuteMerge("SimpleUpdate");

			Logger.LogDebug("SimpleUpdateInLocationWithDistractingLocations(): targetDoc → {TargetDoc}", targetDoc);
            // children are in correct location
            XmlNodeList nodes = targetDoc.SelectNodes("/configuration/location/updateme/children/child");
            Assert.That(nodes, Has.Count.EqualTo(2));
			Logger.LogDebug("SimpleUpdateInLocationWithDistractingLocations(): nodes → {Nodes}", nodes);

            // children only inserted once
            nodes = targetDoc.SelectNodes("//child");
            Assert.That(nodes, Has.Count.EqualTo(2));
			Logger.LogDebug("<<< Exiting SimpleUpdateInLocationWithDistractingLocations()");
        }

        [Test]
        public void UpdateWithTargetPath()
        {
			Logger.LogDebug(">>> Entering UpdateWithTargetPath()");
            XmlDocument targetDoc = this.ExecuteMerge();

			Logger.LogDebug("UpdateWithTargetPath(): targetDoc → {TargetDoc}", targetDoc);
            // children are in correct location
            XmlNodeList nodes = targetDoc.SelectNodes("/configuration/updateme/children/child");
            Assert.That(nodes, Has.Count.EqualTo(2));
			Logger.LogDebug("UpdateWithTargetPath(): nodes → {Nodes}", nodes);

            // children only inserted once
            nodes = targetDoc.SelectNodes("//child");
            Assert.That(nodes, Has.Count.EqualTo(2));
			Logger.LogDebug("<<< Exiting UpdateWithTargetPath()");
        }

        [Test]
        public void UpdateInLocationWithTargetPath()
        {
			Logger.LogDebug(">>> Entering UpdateInLocationWithTargetPath()");
            XmlDocument targetDoc = this.ExecuteMerge("UpdateWithTargetPath");

			Logger.LogDebug("UpdateInLocationWithTargetPath(): targetDoc → {TargetDoc}", targetDoc);
            // children are in correct location
            XmlNodeList nodes = targetDoc.SelectNodes("/configuration/location/updateme/children/child");
            Assert.That(nodes, Has.Count.EqualTo(2));
			Logger.LogDebug("UpdateInLocationWithTargetPath(): nodes → {Nodes}", nodes);

            // children only inserted once
            nodes = targetDoc.SelectNodes("//child");
            Assert.That(nodes, Has.Count.EqualTo(2));
			Logger.LogDebug("<<< Exiting UpdateInLocationWithTargetPath()");
        }

        [Test]
        public void UpdateWithDistractingLocationAndTargetPath()
        {
			Logger.LogDebug(">>> Entering UpdateWithDistractingLocationAndTargetPath()");
            XmlDocument targetDoc = this.ExecuteMerge("UpdateWithTargetPath");

			Logger.LogDebug("UpdateWithDistractingLocationAndTargetPath(): targetDoc → {TargetDoc}", targetDoc);
            // children are in correct location
            XmlNodeList nodes = targetDoc.SelectNodes("/configuration/updateme/children/child");
            Assert.That(nodes, Has.Count.EqualTo(2));
			Logger.LogDebug("UpdateWithDistractingLocationAndTargetPath(): nodes → {Nodes}", nodes);

            // children only inserted once
            nodes = targetDoc.SelectNodes("//child");
            Assert.That(nodes, Has.Count.EqualTo(2));
			Logger.LogDebug("<<< Exiting UpdateWithDistractingLocationAndTargetPath()");
        }

        [Test]
        public void UpdateInLocationWithDistractingLocationAndTargetPath()
        {
			Logger.LogDebug(">>> Entering UpdateInLocationWithDistractingLocationAndTargetPath()");
            XmlDocument targetDoc = this.ExecuteMerge("UpdateWithTargetPath");

			Logger.LogDebug("UpdateInLocationWithDistractingLocationAndTargetPath(): targetDoc → {TargetDoc}", targetDoc);
            // children are in correct location
            XmlNodeList nodes = targetDoc.SelectNodes("/configuration/location/updateme/children/child");
            Assert.That(nodes, Has.Count.EqualTo(2));
			Logger.LogDebug("UpdateInLocationWithDistractingLocationAndTargetPath(): nodes → {Nodes}", nodes);

            // children only inserted once
            nodes = targetDoc.SelectNodes("//child");
            Assert.That(nodes, Has.Count.EqualTo(2));
			Logger.LogDebug("<<< Exiting UpdateInLocationWithDistractingLocationAndTargetPath()");
        }

        [Test]
        public void UpdateInFirstLocationWithDistractingLocationAndTargetPath()
        {
			Logger.LogDebug(">>> Entering UpdateInFirstLocationWithDistractingLocationAndTargetPath()");
            XmlDocument targetDoc = this.ExecuteMerge("UpdateWithTargetPath");

			Logger.LogDebug("UpdateInFirstLocationWithDistractingLocationAndTargetPath(): targetDoc → {TargetDoc}", targetDoc);
            // children are in correct location
            // first location/updateme has updated node
            XmlNode root = targetDoc.SelectSingleNode("/configuration/location[1]");
            XmlNodeList nodes = root.SelectNodes("updateme/children/child");
			Logger.LogWarning("UpdateInFirstLocationWithDistractingLocationAndTargetPath(): root → {Root}", root);
			Logger.LogDebug("UpdateInFirstLocationWithDistractingLocationAndTargetPath(): nodes → {Nodes}", nodes);
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
			Logger.LogDebug("<<< Exiting UpdateInFirstLocationWithDistractingLocationAndTargetPath()");
        }

        [Test]
        public void SimpleAdd()
        {
			Logger.LogDebug(">>> Entering SimpleAdd()");
            XmlDocument targetDoc = this.ExecuteMerge();

			Logger.LogDebug("SimpleAdd(): targetDoc → {TargetDoc}", targetDoc);
            // children are in correct location
            XmlNodeList nodes = targetDoc.SelectNodes("/configuration/updateme/children/child");
            Assert.That(nodes, Has.Count.EqualTo(2));
			Logger.LogDebug("<<< Exiting SimpleAdd()");
			Logger.LogDebug("SimpleAdd(): nodes → {Nodes}", nodes);
        }

        [Test]
        public void AddWithLocation()
        {
			Logger.LogDebug(">>> Entering AddWithLocation()");
            XmlDocument targetDoc = this.ExecuteMerge("SimpleAdd");

			Logger.LogDebug("AddWithLocation(): targetDoc → {TargetDoc}", targetDoc);
            // children are in correct location
            XmlNodeList nodes = targetDoc.SelectNodes("/configuration/updateme/children/child");
            Assert.That(nodes, Has.Count.EqualTo(2));
			Logger.LogDebug("AddWithLocation(): nodes → {Nodes}", nodes);

            // second location/updateme still empty
            var root = targetDoc.SelectSingleNode("/configuration/location[2]");
            nodes = root.SelectNodes("updateme/children/child");
			Logger.LogWarning("AddWithLocation(): root → {Root}", root);
            Assert.That(nodes, Is.Empty);

            // children only inserted once
            nodes = targetDoc.SelectNodes("//child");
            Assert.That(nodes, Has.Count.EqualTo(2));

            // 1 instance of location/updateme exist
            nodes = targetDoc.SelectNodes("//configuration/location/updateme");
            Assert.That(nodes, Has.Count.EqualTo(1));
			Logger.LogDebug("<<< Exiting AddWithLocation()");
        }

        [Test]
        public void SimpleInsertBefore()
        {
			Logger.LogDebug(">>> Entering SimpleInsertBefore()");
            XmlDocument targetDoc = this.ExecuteMerge();

			Logger.LogDebug("SimpleInsertBefore(): targetDoc → {TargetDoc}", targetDoc);
            // children are in correct location
            XmlNodeList nodes = targetDoc.SelectNodes("/configuration/updateme/children/child");
            Assert.That(nodes, Has.Count.EqualTo(2));
			Logger.LogDebug("SimpleInsertBefore(): nodes → {Nodes}", nodes);

            // inserted before node2
            XmlNode node = targetDoc.SelectSingleNode("/configuration/updateme");
            Assert.That(node.NextSibling.Name, Is.EqualTo("node2"));
			Logger.LogDebug("<<< Exiting SimpleInsertBefore()");
			Logger.LogDebug("SimpleInsertBefore(): node → {Node}", node);
        }

        [Test]
        public void InsertBeforeInLocation()
        {
			Logger.LogDebug(">>> Entering InsertBeforeInLocation()");
            XmlDocument targetDoc = this.ExecuteMerge("SimpleInsertBefore");

			Logger.LogDebug("InsertBeforeInLocation(): targetDoc → {TargetDoc}", targetDoc);
            // children are in correct location
            XmlNodeList nodes = targetDoc.SelectNodes("/configuration/location/updateme/children/child");
            Assert.That(nodes, Has.Count.EqualTo(2));
			Logger.LogDebug("InsertBeforeInLocation(): nodes → {Nodes}", nodes);

            // inserted before node2
            XmlNode node = targetDoc.SelectSingleNode("/configuration/location/updateme");
            Assert.That(node.NextSibling.Name, Is.EqualTo("node2"));
			Logger.LogDebug("<<< Exiting InsertBeforeInLocation()");
			Logger.LogDebug("InsertBeforeInLocation(): node → {Node}", node);
        }

        [Test]
        public void SimpleInsertAfter()
        {
			Logger.LogDebug(">>> Entering SimpleInsertAfter()");
            XmlDocument targetDoc = this.ExecuteMerge();

			Logger.LogDebug("SimpleInsertAfter(): targetDoc → {TargetDoc}", targetDoc);
            // children are in correct location
            XmlNodeList nodes = targetDoc.SelectNodes("/configuration/updateme/children/child");
            Assert.That(nodes, Has.Count.EqualTo(2));
			Logger.LogDebug("SimpleInsertAfter(): nodes → {Nodes}", nodes);

            // inserted before node2
            XmlNode node = targetDoc.SelectSingleNode("/configuration/updateme");
            Assert.That(node.PreviousSibling.Name, Is.EqualTo("node1"));
			Logger.LogDebug("<<< Exiting SimpleInsertAfter()");
			Logger.LogDebug("SimpleInsertAfter(): node → {Node}", node);
        }

        [Test]
        public void InsertAfterInLocation()
        {
			Logger.LogDebug(">>> Entering InsertAfterInLocation()");
            XmlDocument targetDoc = this.ExecuteMerge("SimpleInsertAfter");

			Logger.LogDebug("InsertAfterInLocation(): targetDoc → {TargetDoc}", targetDoc);
            // children are in correct location
            XmlNodeList nodes = targetDoc.SelectNodes("/configuration/location/updateme/children/child");
            Assert.That(nodes, Has.Count.EqualTo(2));
			Logger.LogDebug("InsertAfterInLocation(): nodes → {Nodes}", nodes);

            // inserted before node2
            XmlNode node = targetDoc.SelectSingleNode("/configuration/location/updateme");
            Assert.That(node.NextSibling.Name, Is.EqualTo("node2"));
			Logger.LogDebug("<<< Exiting InsertAfterInLocation()");
			Logger.LogDebug("InsertAfterInLocation(): node → {Node}", node);
        }

        [Test]
        public void SimpleRemove()
        {
			Logger.LogDebug(">>> Entering SimpleRemove()");
            XmlDocument targetDoc = this.ExecuteMerge();

			Logger.LogDebug("SimpleRemove(): targetDoc → {TargetDoc}", targetDoc);
            // node is gone
            var nodes = targetDoc.SelectNodes("//removeme");
            Assert.That(nodes, Is.Empty);
			Logger.LogDebug("SimpleRemove(): nodes → {Nodes}", nodes);

            // other nodes still present
            nodes = targetDoc.SelectNodes("/configuration/distraction");
            Assert.That(nodes, Has.Count.EqualTo(1));
			Logger.LogDebug("<<< Exiting SimpleRemove()");
        }

        [Test]
        public void RemoveFromLocation()
        {
			Logger.LogDebug(">>> Entering RemoveFromLocation()");
            XmlDocument targetDoc = this.ExecuteMerge("SimpleRemove");

			Logger.LogDebug("RemoveFromLocation(): targetDoc → {TargetDoc}", targetDoc);
            // node is gone
            var nodes = targetDoc.SelectNodes("//removeme");
            Assert.That(nodes, Is.Empty);
			Logger.LogDebug("RemoveFromLocation(): nodes → {Nodes}", nodes);

            // other nodes still present
            nodes = targetDoc.SelectNodes("/configuration/distraction");
            Assert.That(nodes, Has.Count.EqualTo(1));
			Logger.LogDebug("<<< Exiting RemoveFromLocation()");
        }

        [Test]
        public void SimpleRemoveAttribute()
        {
			Logger.LogDebug(">>> Entering SimpleRemoveAttribute()");
            var targetDoc = this.ExecuteMerge();

			Logger.LogDebug("SimpleRemoveAttribute(): targetDoc → {TargetDoc}", targetDoc);
            var node = targetDoc.SelectSingleNode("/configuration/updateme");
            Assert.That(node.Attributes, Is.Empty);
			Logger.LogDebug("<<< Exiting SimpleRemoveAttribute()");
			Logger.LogDebug("SimpleRemoveAttribute(): node → {Node}", node);
        }

        [Test]
        public void RemoveAttributeFromLocation()
        {
			Logger.LogDebug(">>> Entering RemoveAttributeFromLocation()");
            var targetDoc = this.ExecuteMerge("SimpleRemoveAttribute");

			Logger.LogDebug("RemoveAttributeFromLocation(): targetDoc → {TargetDoc}", targetDoc);
            var node = targetDoc.SelectSingleNode("/configuration/location/updateme");
            Assert.That(node.Attributes, Is.Empty);
			Logger.LogDebug("<<< Exiting RemoveAttributeFromLocation()");
			Logger.LogDebug("RemoveAttributeFromLocation(): node → {Node}", node);
        }

        [Test]
        public void SimpleInsertAttribute()
        {
			Logger.LogDebug(">>> Entering SimpleInsertAttribute()");
            var targetDoc = this.ExecuteMerge();

			Logger.LogDebug("SimpleInsertAttribute(): targetDoc → {TargetDoc}", targetDoc);
            var node = targetDoc.SelectSingleNode("/configuration/updateme");
            Assert.That(node.Attributes, Has.Count.EqualTo(2));
			Logger.LogDebug("SimpleInsertAttribute(): node → {Node}", node);
            Assert.That(node.Attributes["attrib2"].Value, Is.EqualTo("fee"));
			Logger.LogDebug("<<< Exiting SimpleInsertAttribute()");
        }

        [Test]
        public void InsertAttributeInLocation()
        {
			Logger.LogDebug(">>> Entering InsertAttributeInLocation()");
            var targetDoc = this.ExecuteMerge("SimpleInsertAttribute");

			Logger.LogDebug("InsertAttributeInLocation(): targetDoc → {TargetDoc}", targetDoc);
            var node = targetDoc.SelectSingleNode("/configuration/location/updateme");
            Assert.That(node.Attributes, Has.Count.EqualTo(2));
			Logger.LogDebug("InsertAttributeInLocation(): node → {Node}", node);
            Assert.That(node.Attributes["attrib2"].Value, Is.EqualTo("fee"));
			Logger.LogDebug("<<< Exiting InsertAttributeInLocation()");
        }

        [Test]
        public void UpdateAttributeInLocation()
        {
			Logger.LogDebug(">>> Entering UpdateAttributeInLocation()");
            var targetDoc = this.ExecuteMerge("SimpleInsertAttribute");

			Logger.LogDebug("UpdateAttributeInLocation(): targetDoc → {TargetDoc}", targetDoc);
            var node = targetDoc.SelectSingleNode("/configuration/location/updateme");
            Assert.That(node.Attributes, Has.Count.EqualTo(2));
			Logger.LogDebug("UpdateAttributeInLocation(): node → {Node}", node);
            Assert.That(node.Attributes["attrib2"].Value, Is.EqualTo("fee"));
			Logger.LogDebug("<<< Exiting UpdateAttributeInLocation()");
        }

        [Test]
        public void SimpleUpdateWithKey()
        {
			Logger.LogDebug(">>> Entering SimpleUpdateWithKey()");
            var targetDoc = this.ExecuteMerge();

			Logger.LogDebug("SimpleUpdateWithKey(): targetDoc → {TargetDoc}", targetDoc);
            // a key was added
            var nodes = targetDoc.SelectNodes("/configuration/updateme/add");
            Assert.That(nodes, Has.Count.EqualTo(1));
			Logger.LogDebug("SimpleUpdateWithKey(): nodes → {Nodes}", nodes);

            // test attribute is set
            var node = nodes[0];
            Assert.That(node.Attributes["test"].Value, Is.EqualTo("foo"));
			Logger.LogDebug("<<< Exiting SimpleUpdateWithKey()");
        }

        [Test]
        public void UpdateWithKeyInLocation()
        {
			Logger.LogDebug(">>> Entering UpdateWithKeyInLocation()");
            var targetDoc = this.ExecuteMerge("SimpleUpdateWithKey");

			Logger.LogDebug("UpdateWithKeyInLocation(): targetDoc → {TargetDoc}", targetDoc);
            // a key was added
            var nodes = targetDoc.SelectNodes("/configuration/location/updateme/add");
            Assert.That(nodes, Has.Count.EqualTo(1));
			Logger.LogDebug("UpdateWithKeyInLocation(): nodes → {Nodes}", nodes);

            // test attribute is set
            var node = nodes[0];
            Assert.That(node.Attributes["test"].Value, Is.EqualTo("foo"));
			Logger.LogDebug("<<< Exiting UpdateWithKeyInLocation()");
        }

        [Test]
        public void NoChangeOnOverwrite()
        {
			Logger.LogDebug(">>> Entering NoChangeOnOverwrite()");
            XmlMerge merge = this.GetXmlMerge(nameof(this.NoChangeOnOverwrite));
            XmlDocument targetDoc = this.LoadTargetDoc(nameof(this.NoChangeOnOverwrite));
			Logger.LogDebug("NoChangeOnOverwrite(): merge → {Merge}", merge);
			Logger.LogDebug("NoChangeOnOverwrite(): targetDoc → {TargetDoc}", targetDoc);

            merge.UpdateConfig(targetDoc);

            this.WriteToDebug(targetDoc);

            var nodes = targetDoc.SelectNodes("/configuration/appSettings/add");
            using (Assert.EnterMultipleScope())
            {
			Logger.LogDebug("NoChangeOnOverwrite(): nodes → {Nodes}", nodes);
                Assert.That(nodes, Has.Count.EqualTo(3));

                Assert.That(merge.ConfigUpdateChangedNodes, Is.False);
            }
			Logger.LogDebug("<<< Exiting NoChangeOnOverwrite()");
        }

        [Test]
        public void ShouldChangeOnOverwrite()
        {
			Logger.LogDebug(">>> Entering ShouldChangeOnOverwrite()");
            XmlMerge merge = this.GetXmlMerge(nameof(this.ShouldChangeOnOverwrite));
            XmlDocument targetDoc = this.LoadTargetDoc(nameof(this.ShouldChangeOnOverwrite));
			Logger.LogDebug("ShouldChangeOnOverwrite(): merge → {Merge}", merge);
			Logger.LogDebug("ShouldChangeOnOverwrite(): targetDoc → {TargetDoc}", targetDoc);

            merge.UpdateConfig(targetDoc);

            this.WriteToDebug(targetDoc);

            var nodes = targetDoc.SelectNodes("/configuration/appSettings/add");
            using (Assert.EnterMultipleScope())
            {
			Logger.LogDebug("ShouldChangeOnOverwrite(): nodes → {Nodes}", nodes);
                Assert.That(nodes, Has.Count.EqualTo(3));

                Assert.That(merge.ConfigUpdateChangedNodes, Is.True);
            }
			Logger.LogDebug("<<< Exiting ShouldChangeOnOverwrite()");
        }

        [Test]
        public void ShouldPreserveEmptyNamespaceOnSave()
        {
			Logger.LogDebug(">>> Entering ShouldPreserveEmptyNamespaceOnSave()");
            var targetDoc = this.ExecuteMerge();

			Logger.LogDebug("ShouldPreserveEmptyNamespaceOnSave(): targetDoc → {TargetDoc}", targetDoc);
            var ns = new XmlNamespaceManager(targetDoc.NameTable);
            ns.AddNamespace("ab", "urn:schemas-microsoft-com:asm.v1");
			Logger.LogDebug("ShouldPreserveEmptyNamespaceOnSave(): ns → {Ns}", ns);

            // removed the existing node, since it matched targetpath attribute
            var nodesWithNamespace = targetDoc.SelectNodes("/configuration/runtime/ab:assemblyBinding/ab:dependentAssembly", ns);
            Assert.That(nodesWithNamespace, Is.Empty);
			Logger.LogDebug("ShouldPreserveEmptyNamespaceOnSave(): nodesWithNamespace → {NodesWithNamespace}", nodesWithNamespace);

            // added a new node with xmlns=""
            var nodesWithoutNamespace = targetDoc.SelectNodes("/configuration/runtime/ab:assemblyBinding/dependentAssembly", ns);
            Assert.That(nodesWithoutNamespace, Has.Count.EqualTo(1));
			Logger.LogDebug("ShouldPreserveEmptyNamespaceOnSave(): nodesWithoutNamespace → {NodesWithoutNamespace}", nodesWithoutNamespace);

            // non-namespaced node has newVersion from merge
            var dependentAssembly = nodesWithoutNamespace[0];
            var bindingRedirect = dependentAssembly.SelectSingleNode("bindingRedirect", ns);
            Assert.That(bindingRedirect.Attributes["newVersion"].Value, Is.EqualTo("4.1.0.0"));
			Logger.LogDebug("<<< Exiting ShouldPreserveEmptyNamespaceOnSave()");
			Logger.LogDebug("ShouldPreserveEmptyNamespaceOnSave(): bindingRedirect → {BindingRedirect}", bindingRedirect);
        }

        /// <summary>Merges the Merge and Target files based on the name of the calling method.</summary>
        /// <remarks>xml files must be embedded resources in the MergeFiles folder named {method}Merge.xml and {method}Target.xml.</remarks>
        /// <returns>XmlDocument with the result of the merge operation.</returns>
        private XmlDocument ExecuteMerge()
        {
			Logger.LogDebug(">>> Entering ExecuteMerge()");
			Logger.LogDebug("<<< Exiting ExecuteMerge()");
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

			Logger.LogDebug("<<< Exiting ExecuteMerge(mergeName={MergeName})", mergeName);
            return targetDoc;
        }

        private string GetTestMethodName()
        {
			Logger.LogDebug(">>> Entering GetTestMethodName()");
            var st = new StackTrace(2);

			Logger.LogDebug("GetTestMethodName(): st → {St}", st);
            string name;
            int i = 0;
            do
            {
                name = st.GetFrame(i).GetMethod().Name;
				Logger.LogDebug("GetTestMethodName(): name → {Name}", name);
                i++;
            }
            while (name == "ExecuteMerge");

			Logger.LogDebug("<<< Exiting GetTestMethodName()");
            return name;
        }

        private XmlDocument LoadTargetDoc(string testMethodName)
        {
			Logger.LogDebug(">>> Entering LoadTargetDoc(testMethodName={TestMethodName})", testMethodName);
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
                targetDoc.Load(xmlReader);
				Logger.LogDebug("LoadTargetDoc(testMethodName={TestMethodName}): xmlReader → {XmlReader}", testMethodName, xmlReader);
				Logger.LogDebug("<<< Exiting LoadTargetDoc(testMethodName={TestMethodName})", testMethodName);
                return targetDoc;
            }
        }

        private XmlMerge GetXmlMerge(string fileName)
        {
			Logger.LogDebug(">>> Entering GetXmlMerge(fileName={FileName})", fileName);
            using (Stream mergeStream =
                this._assembly.GetManifestResourceStream(string.Format(
                    "DotNetNuke.Tests.Integration.Services.Installer.MergeFiles.{0}Merge.xml",
                    fileName)))
            {
                Debug.Assert(
                    mergeStream != null,
                    string.Format("Unable to location embedded resource for {0}Merge.xml", fileName));
                var merge = new XmlMerge(mergeStream, "version", "sender");
				Logger.LogDebug("GetXmlMerge(fileName={FileName}): merge → {Merge}", fileName, merge);
				Logger.LogDebug("<<< Exiting GetXmlMerge(fileName={FileName})", fileName);
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
		Logger.LogDebug(">>> Entering Debug(message={Message},exception={Exception})", message, exception);
		Logger.LogDebug("<<< Exiting Debug(message={Message},exception={Exception})", message, exception);
		Logger.LogDebug(">>> Entering Debug(message={Message})", message);
        }

        public void Debug(object message)
        {
		Logger.LogDebug("<<< Exiting Debug(message={Message})", message);
        }

        public void DebugFormat(IFormatProvider provider, string format, params object[] args)
        {
		Logger.LogDebug(">>> Entering DebugFormat(provider={Provider},format={Format},args={Args})", provider, format, args);
		Logger.LogDebug("<<< Exiting DebugFormat(provider={Provider},format={Format},args={Args})", provider, format, args);
		Logger.LogDebug(">>> Entering DebugFormat(format={Format},args={Args})", format, args);
        }

        public void DebugFormat(string format, params object[] args)
        {
		Logger.LogDebug("<<< Exiting DebugFormat(format={Format},args={Args})", format, args);
        }

        public void Error(object message, Exception exception)
        {
		Logger.LogDebug(">>> Entering Error(message={Message},exception={Exception})", message, exception);
		Logger.LogDebug("<<< Exiting Error(message={Message},exception={Exception})", message, exception);
		Logger.LogDebug(">>> Entering Error(message={Message})", message);
        }

        public void Error(object message)
        {
		Logger.LogDebug("<<< Exiting Error(message={Message})", message);
        }

        public void ErrorFormat(IFormatProvider provider, string format, params object[] args)
        {
		Logger.LogDebug(">>> Entering ErrorFormat(provider={Provider},format={Format},args={Args})", provider, format, args);
		Logger.LogDebug("<<< Exiting ErrorFormat(provider={Provider},format={Format},args={Args})", provider, format, args);
		Logger.LogDebug(">>> Entering ErrorFormat(format={Format},args={Args})", format, args);
        }

        public void ErrorFormat(string format, params object[] args)
        {
		Logger.LogDebug("<<< Exiting ErrorFormat(format={Format},args={Args})", format, args);
        }

        public void Fatal(object message, Exception exception)
        {
		Logger.LogDebug(">>> Entering Fatal(message={Message},exception={Exception})", message, exception);
		Logger.LogDebug("<<< Exiting Fatal(message={Message},exception={Exception})", message, exception);
		Logger.LogDebug(">>> Entering Fatal(message={Message})", message);
        }

        public void Fatal(object message)
        {
		Logger.LogDebug("<<< Exiting Fatal(message={Message})", message);
        }

        public void FatalFormat(IFormatProvider provider, string format, params object[] args)
        {
		Logger.LogDebug(">>> Entering FatalFormat(provider={Provider},format={Format},args={Args})", provider, format, args);
		Logger.LogDebug("<<< Exiting FatalFormat(provider={Provider},format={Format},args={Args})", provider, format, args);
		Logger.LogDebug(">>> Entering FatalFormat(format={Format},args={Args})", format, args);
        }

        public void FatalFormat(string format, params object[] args)
        {
		Logger.LogDebug("<<< Exiting FatalFormat(format={Format},args={Args})", format, args);
        }

        public void Info(object message, Exception exception)
        {
		Logger.LogDebug(">>> Entering Info(message={Message},exception={Exception})", message, exception);
		Logger.LogDebug("<<< Exiting Info(message={Message},exception={Exception})", message, exception);
		Logger.LogDebug(">>> Entering Info(message={Message})", message);
        }

        public void Info(object message)
        {
		Logger.LogDebug("<<< Exiting Info(message={Message})", message);
        }

        public void InfoFormat(IFormatProvider provider, string format, params object[] args)
        {
		Logger.LogDebug(">>> Entering InfoFormat(provider={Provider},format={Format},args={Args})", provider, format, args);
		Logger.LogDebug("<<< Exiting InfoFormat(provider={Provider},format={Format},args={Args})", provider, format, args);
		Logger.LogDebug(">>> Entering InfoFormat(format={Format},args={Args})", format, args);
        }

        public void InfoFormat(string format, params object[] args)
        {
		Logger.LogDebug("<<< Exiting InfoFormat(format={Format},args={Args})", format, args);
        }

        public void Trace(object message, Exception exception)
        {
		Logger.LogDebug(">>> Entering Trace(message={Message},exception={Exception})", message, exception);
		Logger.LogDebug("<<< Exiting Trace(message={Message},exception={Exception})", message, exception);
		Logger.LogDebug(">>> Entering Trace(message={Message})", message);
        }

        public void Trace(object message)
        {
		Logger.LogDebug("<<< Exiting Trace(message={Message})", message);
        }

        public void TraceFormat(IFormatProvider provider, string format, params object[] args)
        {
		Logger.LogDebug(">>> Entering TraceFormat(provider={Provider},format={Format},args={Args})", provider, format, args);
		Logger.LogDebug("<<< Exiting TraceFormat(provider={Provider},format={Format},args={Args})", provider, format, args);
		Logger.LogDebug(">>> Entering TraceFormat(format={Format},args={Args})", format, args);
        }

        public void TraceFormat(string format, params object[] args)
        {
		Logger.LogDebug("<<< Exiting TraceFormat(format={Format},args={Args})", format, args);
        }

        public void Warn(object message, Exception exception)
        {
		Logger.LogDebug(">>> Entering Warn(message={Message},exception={Exception})", message, exception);
		Logger.LogDebug("<<< Exiting Warn(message={Message},exception={Exception})", message, exception);
		Logger.LogDebug(">>> Entering Warn(message={Message})", message);
        }

        public void Warn(object message)
        {
		Logger.LogDebug("<<< Exiting Warn(message={Message})", message);
        }

        public void WarnFormat(IFormatProvider provider, string format, params object[] args)
        {
		Logger.LogDebug(">>> Entering WarnFormat(provider={Provider},format={Format},args={Args})", provider, format, args);
		Logger.LogDebug("<<< Exiting WarnFormat(provider={Provider},format={Format},args={Args})", provider, format, args);
		Logger.LogDebug(">>> Entering WarnFormat(format={Format},args={Args})", format, args);
        }

        public void WarnFormat(string format, params object[] args)
        {
		Logger.LogDebug("<<< Exiting WarnFormat(format={Format},args={Args})", format, args);
        }
    }
}
