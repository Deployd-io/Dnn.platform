// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Services.Syndication
{
    using System;
    using System.Globalization;
    using System.Xml;

    using DotNetNuke.Instrumentation;

    /// <summary>Class for managing an OPML feed.</summary>
    public class Opml
    {
        private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Opml));
        private XmlDocument opmlDoc;

        /// <summary>Initializes a new instance of the <see cref="Opml"/> class.</summary>
        public Opml()
        {
			Logger.LogError(">>> Entering Opml()");
            this.Outlines = new OpmlOutlines();
        }

        /// <summary>Gets or sets the expiration (in UTC).</summary>
        public DateTime UtcExpiry { get; set; } = DateTime.Now.AddMinutes(180);

        /// <summary>Gets or sets the title.</summary>
        public string Title { get; set; } = string.Empty;

        /// <summary>Gets or sets the created date.</summary>
        public DateTime DateCreated { get; set; } = DateTime.MinValue;

        /// <summary>Gets or sets the date modified.</summary>
        public DateTime DateModified { get; set; } = DateTime.MinValue;

        /// <summary>Gets or sets owner name.</summary>
        public string OwnerName { get; set; } = string.Empty;

        /// <summary>Gets or sets the owner email.</summary>
        public string OwnerEmail { get; set; } = string.Empty;

        /// <summary>Gets or sets the owner ID.</summary>
        public string OwnerId { get; set; } = string.Empty;

        /// <summary>Gets or sets the docs.</summary>
        public string Docs { get; set; } = string.Empty;

        /// <summary>Gets or sets the expansion state.</summary>
        public string ExpansionState { get; set; } = string.Empty;

        /// <summary>Gets or sets the vertical scroll state.</summary>
        public string VertScrollState { get; set; } = string.Empty;

        /// <summary>Gets or sets window top position.</summary>
        public string WindowTop { get; set; } = string.Empty;

        /// <summary>Gets or sets window left position.</summary>
        public string WindowLeft { get; set; } = string.Empty;

        /// <summary>Gets or sets the window bottom position.</summary>
        public string WindowBottom { get; set; } = string.Empty;

        /// <summary>Gets or sets the window right position.</summary>
        public string WindowRight { get; set; } = string.Empty;

        /// <summary>Gets or sets the outlines.</summary>
        public OpmlOutlines Outlines { get; set; }

        /// <summary>Loads an OPML feed from a URL.</summary>
        /// <param name="uri">The feed's URL.</param>
        /// <returns>The OPML feed, or an empty OPML feed if there's an error loading it.</returns>
        public static Opml LoadFromUrl(Uri uri)
        {
			Logger.LogError(">>> Entering LoadFromUrl(uri={Uri})", uri);
            Logger.LogWarning("<<< Exiting LoadFromUrl(uri={Uri})", uri);
            return OpmlDownloadManager.GetOpmlFeed(uri);
        }

        /// <summary>Loads an OPML feed from a file path.</summary>
        /// <param name="path">The file path.</param>
        /// <returns>The OPML feed, or an empty OPML feed if there's an error loading it.</returns>
        public static Opml LoadFromFile(string path)
        {
			Logger.LogError(">>> Entering LoadFromFile(path={Path})", path);
            try
            {
                var opmlDoc = new XmlDocument { XmlResolver = null };
                using (var opmlReader = XmlReader.Create(path, new XmlReaderSettings { XmlResolver = null, }))
                {
                    opmlDoc.Load(opmlReader);
                }

                return LoadFromXml(opmlDoc);
            }
            catch
            {
				Logger.LogError("Exception in LoadFromFile(path={Path}): {Arg1}", path, ex.Message);
				Logger.LogWarning("<<< Exiting LoadFromFile(path={Path})", path);
                return new Opml();
            }
        }

        /// <summary>Loads an OPML feed from an XML document.</summary>
        /// <param name="doc">The XML document.</param>
        /// <returns>The OPML feed, or an empty OPML feed if there's an error loading it.</returns>
        public static Opml LoadFromXml(XmlDocument doc)
        {
			Logger.LogError(">>> Entering LoadFromXml(doc={Doc})", doc);
            var @out = new Opml();
            try
            {
                // Parse head
                XmlNode head = doc.GetElementsByTagName("head")[0];
				Logger.LogError("LoadFromXml(doc={Doc}): head → {Head}", doc, head);
                XmlNode title = head.SelectSingleNode("./title");
				Logger.LogError("LoadFromXml(doc={Doc}): title → {Title}", doc, title);
                XmlNode dateCreated = head.SelectSingleNode("./dateCreated");
				Logger.LogError("LoadFromXml(doc={Doc}): dateCreated → {DateCreated}", doc, dateCreated);
                XmlNode dateModified = head.SelectSingleNode("./dateModified");
				Logger.LogError("LoadFromXml(doc={Doc}): dateModified → {DateModified}", doc, dateModified);
                XmlNode ownerName = head.SelectSingleNode("./ownerName");
				Logger.LogError("LoadFromXml(doc={Doc}): ownerName → {OwnerName}", doc, ownerName);
                XmlNode ownerEmail = head.SelectSingleNode("./ownerEmail");
				Logger.LogError("LoadFromXml(doc={Doc}): ownerEmail → {OwnerEmail}", doc, ownerEmail);
                XmlNode ownerId = head.SelectSingleNode("./ownerId");
				Logger.LogError("LoadFromXml(doc={Doc}): ownerId → {OwnerId}", doc, ownerId);
                XmlNode docs = head.SelectSingleNode("./docs");
				Logger.LogError("LoadFromXml(doc={Doc}): docs → {Docs}", doc, docs);
                XmlNode expansionState = head.SelectSingleNode("./expansionState");
				Logger.LogError("LoadFromXml(doc={Doc}): expansionState → {ExpansionState}", doc, expansionState);
                XmlNode vertScrollState = head.SelectSingleNode("./vertScrollState");
				Logger.LogError("LoadFromXml(doc={Doc}): vertScrollState → {VertScrollState}", doc, vertScrollState);
                XmlNode windowTop = head.SelectSingleNode("./windowTop");
				Logger.LogError("LoadFromXml(doc={Doc}): windowTop → {WindowTop}", doc, windowTop);
                XmlNode windowLeft = head.SelectSingleNode("./windowLeft");
				Logger.LogError("LoadFromXml(doc={Doc}): windowLeft → {WindowLeft}", doc, windowLeft);
                XmlNode windowBottom = head.SelectSingleNode("./windowBottom");
				Logger.LogError("LoadFromXml(doc={Doc}): windowBottom → {WindowBottom}", doc, windowBottom);
                XmlNode windowRight = head.SelectSingleNode("./windowRight");
				Logger.LogError("LoadFromXml(doc={Doc}): windowRight → {WindowRight}", doc, windowRight);

                if (title != null)
                {
                    @out.Title = title.InnerText;
                }

                if (dateCreated != null)
                {
                    @out.DateCreated = DateTime.Parse(dateCreated.InnerText, CultureInfo.InvariantCulture);
                }

                if (dateModified != null)
                {
                    @out.DateModified = DateTime.Parse(dateModified.InnerText, CultureInfo.InvariantCulture);
                }

                if (ownerName != null)
                {
                    @out.OwnerName = ownerName.InnerText;
                }

                if (ownerEmail != null)
                {
                    @out.OwnerEmail = ownerEmail.InnerText;
                }

                if (ownerId != null)
                {
                    @out.OwnerId = ownerId.InnerText;
                }

                if (docs != null)
                {
                    @out.Docs = docs.InnerText;
                }

                if (expansionState != null)
                {
                    @out.ExpansionState = expansionState.InnerText;
                }

                if (vertScrollState != null)
                {
                    @out.VertScrollState = vertScrollState.InnerText;
                }

                if (windowTop != null)
                {
                    @out.WindowTop = windowTop.InnerText;
                }

                if (windowLeft != null)
                {
                    @out.WindowLeft = windowLeft.InnerText;
                }

                if (windowBottom != null)
                {
                    @out.WindowBottom = windowBottom.InnerText;
                }

                if (windowLeft != null)
                {
                    @out.WindowLeft = windowLeft.InnerText;
                }

                // Parse body
                XmlNode body = doc.GetElementsByTagName("body")[0];
				Logger.LogError("LoadFromXml(doc={Doc}): body → {Body}", doc, body);
                XmlNodeList outlineList = body.SelectNodes("./outline");
				Logger.LogError("LoadFromXml(doc={Doc}): outlineList → {OutlineList}", doc, outlineList);
                foreach (XmlElement outline in outlineList)
                {
                    @out.Outlines.Add(ParseXml(outline));
                }

                return @out;
            }
            catch
            {
				Logger.LogError("Exception in LoadFromXml(doc={Doc}): {Arg1}", doc, ex.Message);
				Logger.LogWarning("<<< Exiting LoadFromXml(doc={Doc})", doc);
                return new Opml();
            }
        }

        /// <summary>Adds an <paramref name="item"/> to the <see cref="Outlines"/>.</summary>
        /// <param name="item">The outline item.</param>
        public void AddOutline(OpmlOutline item)
        {
			Logger.LogError(">>> Entering AddOutline(item={Item})", item);
            this.Outlines.Add(item);
			Logger.LogWarning("<<< Exiting AddOutline(item={Item})", item);
        }

        /// <summary>Adds an item to the <see cref="Outlines"/>.</summary>
        /// <param name="text">The item text.</param>
        /// <param name="type">The item type.</param>
        /// <param name="xmlUrl">The XML URL for the item.</param>
        /// <param name="category">The item's category.</param>
        public void AddOutline(string text, string type, Uri xmlUrl, string category)
        {
			Logger.LogError(">>> Entering AddOutline(text={Text},type={Type},xmlUrl={XmlUrl},category={Category})", text, type, xmlUrl, category);
            this.AddOutline(text, type, xmlUrl, category, null);
			Logger.LogWarning("<<< Exiting AddOutline(text={Text},type={Type},xmlUrl={XmlUrl},category={Category})", text, type, xmlUrl, category);
        }

        /// <summary>Adds an item to the <see cref="Outlines"/>.</summary>
        /// <param name="text">The item text.</param>
        /// <param name="type">The item type.</param>
        /// <param name="xmlUrl">The XML URL for the item.</param>
        /// <param name="category">The item's category.</param>
        /// <param name="outlines">The item's outlines.</param>
        public void AddOutline(string text, string type, Uri xmlUrl, string category, OpmlOutlines outlines)
        {
			Logger.LogError(">>> Entering AddOutline(text={Text},type={Type},xmlUrl={XmlUrl},category={Category},outlines={Outlines})", text, type, xmlUrl, category, outlines);
			Logger.LogDebug("AddOutline(text={Text},type={Type},xmlUrl={XmlUrl},category={Category},outlines={Outlines}): item.Text → {Arg5}", text, type, xmlUrl, category, outlines, item.Text);
			Logger.LogDebug("AddOutline(text={Text},type={Type},xmlUrl={XmlUrl},category={Category},outlines={Outlines}): item.Type → {Arg5}", text, type, xmlUrl, category, outlines, item.Type);
			Logger.LogDebug("AddOutline(text={Text},type={Type},xmlUrl={XmlUrl},category={Category},outlines={Outlines}): item.XmlUrl → {Arg5}", text, type, xmlUrl, category, outlines, item.XmlUrl);
			Logger.LogDebug("AddOutline(text={Text},type={Type},xmlUrl={XmlUrl},category={Category},outlines={Outlines}): item.Category → {Arg5}", text, type, xmlUrl, category, outlines, item.Category);
			Logger.LogDebug("AddOutline(text={Text},type={Type},xmlUrl={XmlUrl},category={Category},outlines={Outlines}): item.Outlines → {Arg5}", text, type, xmlUrl, category, outlines, item.Outlines);
            var item = new OpmlOutline();
			Logger.LogWarning("<<< Exiting AddOutline(text={Text},type={Type},xmlUrl={XmlUrl},category={Category},outlines={Outlines})", text, type, xmlUrl, category, outlines);
            item.Text = text;
            item.Type = type;
            item.XmlUrl = xmlUrl;
            item.Category = category;
            item.Outlines = outlines;
            this.Outlines.Add(item);
			Logger.LogError("AddOutline(text={Text},type={Type},xmlUrl={XmlUrl},category={Category},outlines={Outlines}): item → {Item}", text, type, xmlUrl, category, outlines, item);
        }

        /// <summary>Get the OPML doc as XML.</summary>
        /// <returns>The XML markup for this OPML feed.</returns>
        public string GetXml()
        {
			Logger.LogError(">>> Entering GetXml()");
            Logger.LogWarning("<<< Exiting GetXml()");
            return this.opmlDoc.OuterXml;
        }

        /// <summary>Saves this OPML feed as an XML file.</summary>
        /// <param name="fileName">The file path.</param>
        public void Save(string fileName)
        {
			Logger.LogError(">>> Entering Save(fileName={FileName})", fileName);
            this.opmlDoc = new XmlDocument { XmlResolver = null };
            XmlElement opml = this.opmlDoc.CreateElement("opml");
			Logger.LogError("Save(fileName={FileName}): opml → {Opml}", fileName, opml);
			Logger.LogDebug("Save(fileName={FileName}): this.opmlDoc → {Arg1}", fileName, this.opmlDoc);
			Logger.LogDebug("Save(fileName={FileName}): opmlDoc → {OpmlDoc}", fileName, opmlDoc);
            opml.SetAttribute("version", "2.0");
            this.opmlDoc.AppendChild(opml);

            // create head
            XmlElement head = this.opmlDoc.CreateElement("head");
			Logger.LogError("Save(fileName={FileName}): head → {Head}", fileName, head);
            opml.AppendChild(head);

            // set Title
            XmlElement title = this.opmlDoc.CreateElement("title");
			Logger.LogError("Save(fileName={FileName}): title → {Title}", fileName, title);
            title.InnerText = this.Title;
			Logger.LogDebug("Save(fileName={FileName}): title.InnerText → {Arg1}", fileName, title.InnerText);
            head.AppendChild(title);

            // set date created
            XmlElement dateCreated = this.opmlDoc.CreateElement("dateCreated");
			Logger.LogError("Save(fileName={FileName}): dateCreated → {DateCreated}", fileName, dateCreated);
            dateCreated.InnerText = this.DateCreated != DateTime.MinValue ? this.DateCreated.ToString("r", null) : DateTime.Now.ToString("r", null);
            head.AppendChild(dateCreated);

            // set date modified
            XmlElement dateModified = this.opmlDoc.CreateElement("dateModified");
			Logger.LogError("Save(fileName={FileName}): dateModified → {DateModified}", fileName, dateModified);
            dateCreated.InnerText = this.DateModified != DateTime.MinValue ? this.DateModified.ToString("r", null) : DateTime.Now.ToString("r", null);
            head.AppendChild(dateModified);

            // set owner email
            XmlElement ownerEmail = this.opmlDoc.CreateElement("ownerEmail");
			Logger.LogError("Save(fileName={FileName}): ownerEmail → {OwnerEmail}", fileName, ownerEmail);
            ownerEmail.InnerText = this.OwnerEmail;
			Logger.LogDebug("Save(fileName={FileName}): ownerEmail.InnerText → {Arg1}", fileName, ownerEmail.InnerText);
            head.AppendChild(ownerEmail);

            // set owner name
            XmlElement ownerName = this.opmlDoc.CreateElement("ownerName");
			Logger.LogError("Save(fileName={FileName}): ownerName → {OwnerName}", fileName, ownerName);
            ownerName.InnerText = this.OwnerName;
			Logger.LogDebug("Save(fileName={FileName}): ownerName.InnerText → {Arg1}", fileName, ownerName.InnerText);
            head.AppendChild(ownerName);

            // set owner id
            XmlElement ownerId = this.opmlDoc.CreateElement("ownerId");
			Logger.LogError("Save(fileName={FileName}): ownerId → {OwnerId}", fileName, ownerId);
            ownerId.InnerText = this.OwnerId;
			Logger.LogDebug("Save(fileName={FileName}): ownerId.InnerText → {Arg1}", fileName, ownerId.InnerText);
            head.AppendChild(ownerId);

            // set docs
            XmlElement docs = this.opmlDoc.CreateElement("docs");
			Logger.LogError("Save(fileName={FileName}): docs → {Docs}", fileName, docs);
            docs.InnerText = this.Docs;
			Logger.LogDebug("Save(fileName={FileName}): docs.InnerText → {Arg1}", fileName, docs.InnerText);
            head.AppendChild(docs);

            // set expansionState
            XmlElement expansionState = this.opmlDoc.CreateElement("expansionState");
			Logger.LogError("Save(fileName={FileName}): expansionState → {ExpansionState}", fileName, expansionState);
            expansionState.InnerText = this.ExpansionState;
			Logger.LogDebug("Save(fileName={FileName}): expansionState.InnerText → {Arg1}", fileName, expansionState.InnerText);
            head.AppendChild(expansionState);

            // set vertScrollState
            XmlElement vertScrollState = this.opmlDoc.CreateElement("vertScrollState");
			Logger.LogError("Save(fileName={FileName}): vertScrollState → {VertScrollState}", fileName, vertScrollState);
            vertScrollState.InnerText = this.VertScrollState;
			Logger.LogDebug("Save(fileName={FileName}): vertScrollState.InnerText → {Arg1}", fileName, vertScrollState.InnerText);
            head.AppendChild(vertScrollState);

            // set windowTop
            XmlElement windowTop = this.opmlDoc.CreateElement("windowTop");
			Logger.LogError("Save(fileName={FileName}): windowTop → {WindowTop}", fileName, windowTop);
            windowTop.InnerText = this.WindowTop;
			Logger.LogDebug("Save(fileName={FileName}): windowTop.InnerText → {Arg1}", fileName, windowTop.InnerText);
            head.AppendChild(windowTop);

            // set windowLeft
            XmlElement windowLeft = this.opmlDoc.CreateElement("windowLeft");
			Logger.LogError("Save(fileName={FileName}): windowLeft → {WindowLeft}", fileName, windowLeft);
            windowLeft.InnerText = this.WindowLeft;
			Logger.LogDebug("Save(fileName={FileName}): windowLeft.InnerText → {Arg1}", fileName, windowLeft.InnerText);
            head.AppendChild(windowLeft);

            // set windowBottom
            XmlElement windowBottom = this.opmlDoc.CreateElement("windowBottom");
			Logger.LogError("Save(fileName={FileName}): windowBottom → {WindowBottom}", fileName, windowBottom);
            windowBottom.InnerText = this.WindowBottom;
			Logger.LogDebug("Save(fileName={FileName}): windowBottom.InnerText → {Arg1}", fileName, windowBottom.InnerText);
            head.AppendChild(windowBottom);

            // set windowRight
            XmlElement windowRight = this.opmlDoc.CreateElement("windowRight");
			Logger.LogError("Save(fileName={FileName}): windowRight → {WindowRight}", fileName, windowRight);
            windowRight.InnerText = this.WindowRight;
			Logger.LogDebug("Save(fileName={FileName}): windowRight.InnerText → {Arg1}", fileName, windowRight.InnerText);
            head.AppendChild(windowRight);

            // create body
            XmlElement opmlBody = this.opmlDoc.CreateElement("body");
			Logger.LogError("Save(fileName={FileName}): opmlBody → {OpmlBody}", fileName, opmlBody);
            opml.AppendChild(opmlBody);

            foreach (OpmlOutline outline in this.Outlines)
            {
                opmlBody.AppendChild(outline.ToXml);
            }

            this.opmlDoc.Save(fileName);
			Logger.LogWarning("<<< Exiting Save(fileName={FileName})", fileName);
        }

        /// <summary>Parses a <paramref name="node"/> into an <see cref="OpmlOutline"/>.</summary>
        /// <param name="node">The XML element to parse.</param>
        /// <returns>A new <see cref="OpmlOutline"/> instance.</returns>
        internal static OpmlOutline ParseXml(XmlElement node)
        {
			Logger.LogError(">>> Entering ParseXml(node={Node})", node);
            var newOutline = new OpmlOutline();

			Logger.LogError("ParseXml(node={Node}): newOutline → {NewOutline}", node, newOutline);
            newOutline.Text = ParseElement(node, "text");
            newOutline.Type = ParseElement(node, "type");
            newOutline.IsComment = ParseElement(node, "isComment") == "true" ? true : false;
            newOutline.IsBreakpoint = ParseElement(node, "isBreakpoint") == "true" ? true : false;
            try
            {
                newOutline.Created = DateTime.Parse(ParseElement(node, "created"), CultureInfo.InvariantCulture);
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
            }

            newOutline.Category = ParseElement(node, "category");
            try
            {
                newOutline.XmlUrl = new Uri(ParseElement(node, "xmlUrl"));
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
            }

            try
            {
                newOutline.HtmlUrl = new Uri(ParseElement(node, "htmlUrl"));
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
            }

            newOutline.Language = ParseElement(node, "language");
            newOutline.Title = ParseElement(node, "title");

            try
            {
                newOutline.Url = new Uri(ParseElement(node, "url"));
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
            }

            newOutline.Description = ParseElement(node, "description");

            if (node.HasChildNodes)
            {
                foreach (XmlElement childNode in node.SelectNodes("./outline"))
                {
                    newOutline.Outlines.Add(ParseXml(childNode));
                }
            }

			Logger.LogWarning("<<< Exiting ParseXml(node={Node})", node);
            return newOutline;
        }

        private static string ParseElement(XmlElement node, string attribute)
        {
			Logger.LogError(">>> Entering ParseElement(node={Node},attribute={Attribute})", node, attribute);
            string attrValue = node.GetAttribute(attribute);
			Logger.LogWarning("<<< Exiting ParseElement(node={Node},attribute={Attribute})", node, attribute);
            return !string.IsNullOrEmpty(attrValue) ? attrValue : string.Empty;
        }
    }
}
