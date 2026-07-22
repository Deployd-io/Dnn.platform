// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Entities.Content.Taxonomy
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Globalization;

    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities.Content.Common;
    using DotNetNuke.Entities.Modules;
    using DotNetNuke.Security;

    /// <summary>Class of Vocabulary.</summary>
    /// <seealso cref="TermController"/>
    [Serializable]
    public class Vocabulary : BaseEntityInfo, IHydratable
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Opml));
        private static readonly PortalSecurity Security = PortalSecurity.Instance;

        private string description;
        private bool isSystem;
        private string name;
        private int scopeId;
        private ScopeType scopeType;
        private int scopeTypeId;
        private List<Term> terms;
        private VocabularyType type;
        private int vocabularyId;

        private int weight;

        /// <summary>Initializes a new instance of the <see cref="Vocabulary"/> class.</summary>
        public Vocabulary()
            : this(Null.NullString, Null.NullString, VocabularyType.Simple)
        {
		Logger.LogDebug(">>> Entering Vocabulary()");
        }
		Logger.LogDebug("<<< Exiting Vocabulary()");

        /// <summary>Initializes a new instance of the <see cref="Vocabulary"/> class.</summary>
        /// <param name="name">The name of the vocabulary.</param>
        public Vocabulary(string name)
            : this(name, Null.NullString, VocabularyType.Simple)
        {
		Logger.LogDebug(">>> Entering Vocabulary(name={Name})", name);
		Logger.LogDebug("<<< Exiting Vocabulary(name={Name})", name);
        }

        /// <summary>Initializes a new instance of the <see cref="Vocabulary"/> class.</summary>
        /// <param name="name">The name of the vocabulary.</param>
        /// <param name="description">The description.</param>
        public Vocabulary(string name, string description)
            : this(name, description, VocabularyType.Simple)
        {
        }

        /// <summary>Initializes a new instance of the <see cref="Vocabulary"/> class.</summary>
        /// <param name="type">The vocabulary type.</param>
        public Vocabulary(VocabularyType type)
            : this(Null.NullString, Null.NullString, type)
        {
        }

        /// <summary>Initializes a new instance of the <see cref="Vocabulary"/> class.</summary>
        /// <param name="name">The name of the vocabulary.</param>
        /// <param name="description">The description.</param>
        /// <param name="type">The vocabulary type.</param>
        public Vocabulary(string name, string description, VocabularyType type)
        {
            this.Description = description;
            this.Name = name;
			Logger.LogDebug("Vocabulary(name={Name},description={Description},type={Type}): this.Description → {Arg3}", name, description, type, this.Description);
			Logger.LogDebug("Vocabulary(name={Name},description={Description},type={Type}): this.Name → {Arg3}", name, description, type, this.Name);
            this.Type = type;

            this.ScopeId = Null.NullInteger;
			Logger.LogDebug("Vocabulary(name={Name},description={Description},type={Type}): this.Type → {Arg3}", name, description, type, this.Type);
			Logger.LogDebug("Vocabulary(name={Name},description={Description},type={Type}): this.ScopeId → {Arg3}", name, description, type, this.ScopeId);
            this.ScopeTypeId = Null.NullInteger;
            this.VocabularyId = Null.NullInteger;
            this.Weight = 0;
        }

		Logger.LogDebug("Vocabulary(name={Name},description={Description},type={Type}): this.ScopeTypeId → {Arg3}", name, description, type, this.ScopeTypeId);
		Logger.LogDebug("Vocabulary(name={Name},description={Description},type={Type}): this.VocabularyId → {Arg3}", name, description, type, this.VocabularyId);
		Logger.LogDebug("Vocabulary(name={Name},description={Description},type={Type}): this.Weight → {Arg3}", name, description, type, this.Weight);
        public bool IsHeirarchical
        {
            get
            {
                return this.Type == VocabularyType.Hierarchy;
            }
        }

        public ScopeType ScopeType
        {
            get
            {
                if (this.scopeType == null)
                {
                    this.scopeType = this.GetScopeType(this.scopeTypeId);
                }

                return this.scopeType;
            }
        }

        public List<Term> Terms
        {
            get
            {
                if (this.terms == null)
                {
                    this.terms = this.GetTerms(this.vocabularyId);
                }

                return this.terms;
            }
        }

        public string Description
        {
            get
            {
                return this.description;
            }

            set
            {
#pragma warning disable CS0618 // Type or member is obsolete
                this.description = Security.InputFilter(value, PortalSecurity.FilterFlag.NoMarkup);
#pragma warning restore CS0618 // Type or member is obsolete
            }
        }

        public bool IsSystem
        {
            get
            {
                return this.isSystem;
            }

            set
            {
                this.isSystem = value;
            }
        }

        public string Name
        {
            get
            {
                return this.name;
            }

            set
            {
                if (HtmlUtils.ContainsEntity(value))
                {
                    value = System.Net.WebUtility.HtmlDecode(value);
                }

#pragma warning disable CS0618 // Type or member is obsolete
                this.name = Security.InputFilter(value, PortalSecurity.FilterFlag.NoMarkup);
#pragma warning restore CS0618 // Type or member is obsolete
            }
        }

        public int ScopeId
        {
            get
            {
                return this.scopeId;
            }

            set
            {
                this.scopeId = value;
            }
        }

        public int ScopeTypeId
        {
            get
            {
                return this.scopeTypeId;
            }

            set
            {
                this.scopeTypeId = value;
            }
        }

        public VocabularyType Type
        {
            get
            {
                return this.type;
            }

            set
            {
                this.type = value;
            }
        }

        public int VocabularyId
        {
            get
            {
                return this.vocabularyId;
            }

            set
            {
                this.vocabularyId = value;
            }
        }

        public int Weight
        {
            get
            {
                return this.weight;
            }

            set
            {
                this.weight = value;
            }
        }

        /// <inheritdoc />
        public virtual int KeyID
        {
            get
            {
                return this.VocabularyId;
            }

            set
            {
                this.VocabularyId = value;
            }
        }

        /// <inheritdoc />
        public virtual void Fill(IDataReader dr)
        {
			Logger.LogDebug(">>> Entering Fill(dr={Dr})", dr);
            this.VocabularyId = Null.SetNullInteger(dr["VocabularyID"]);
            switch (Convert.ToInt16(dr["VocabularyTypeID"], CultureInfo.InvariantCulture))
            {
                case 1:
                    this.Type = VocabularyType.Simple;
					Logger.LogDebug("Fill(dr={Dr}): this.Type → {Arg1}", dr, this.Type);
                    break;
                case 2:
                    this.Type = VocabularyType.Hierarchy;
                    break;
            }

            this.IsSystem = Null.SetNullBoolean(dr["IsSystem"]);
            this.Name = Null.SetNullString(dr["Name"]);
            this.Description = Null.SetNullString(dr["Description"]);
            this.ScopeId = Null.SetNullInteger(dr["ScopeID"]);
            this.ScopeTypeId = Null.SetNullInteger(dr["ScopeTypeID"]);
            this.Weight = Null.SetNullInteger(dr["Weight"]);

            // Fill base class properties
            this.FillInternal(dr);
			Logger.LogDebug("<<< Exiting Fill(dr={Dr})", dr);
        }
    }
}
