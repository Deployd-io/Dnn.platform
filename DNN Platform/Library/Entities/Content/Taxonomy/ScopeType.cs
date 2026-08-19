// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Entities.Content.Taxonomy
{
    using System;
    using System.Data;

    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities.Modules;

    /// <summary>Class of ScopeType.</summary>
    /// <seealso cref="TermController"/>
    [Serializable]
    public class ScopeType : ScopeTypeMemberNameFixer, IHydratable
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(ScopeType));
        /// <summary>Initializes a new instance of the <see cref="ScopeType"/> class.</summary>
        public ScopeType()
            : this(Null.NullString)
        {
		Logger.LogDebug(">>> Entering ScopeType()");
		Logger.LogDebug("<<< Exiting ScopeType()");
		Logger.LogDebug(">>> Entering ScopeType(scopeType={ScopeType})", scopeType);
        }

        /// <summary>Initializes a new instance of the <see cref="ScopeType"/> class.</summary>
        /// <param name="scopeType">The name of the scope type.</param>
        public ScopeType(string scopeType)
        {
            this.ScopeTypeId = Null.NullInteger;
            this.ScopeType = scopeType;
			Logger.LogDebug("ScopeType(scopeType={ScopeType}): this.ScopeTypeId → {Arg1}", scopeType, this.ScopeTypeId);
			Logger.LogDebug("<<< Exiting ScopeType(scopeType={ScopeType})", scopeType);
        }

        public int ScopeTypeId { get; set; }

        /// <inheritdoc />
        public int KeyID
        {
            get
            {
                return this.ScopeTypeId;
            }

            set
            {
                this.ScopeTypeId = value;
            }
        }

        /// <inheritdoc />
        public void Fill(IDataReader dr)
        {
			Logger.LogDebug(">>> Entering Fill(dr={Dr})", dr);
            this.ScopeTypeId = Null.SetNullInteger(dr["ScopeTypeID"]);
            this.ScopeType = Null.SetNullString(dr["ScopeType"]);
			Logger.LogDebug("<<< Exiting Fill(dr={Dr})", dr);
        }

        /// <inheritdoc />
        public override string ToString()
        {
			Logger.LogDebug(">>> Entering ToString()");
			Logger.LogDebug("<<< Exiting ToString()");
            return this.ScopeType;
        }
    }
}
