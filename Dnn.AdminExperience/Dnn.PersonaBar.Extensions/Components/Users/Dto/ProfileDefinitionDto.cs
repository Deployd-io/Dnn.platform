// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.PersonaBar.Users.Components.Dto
{
    using System;
    using System.Linq;
    using System.Runtime.Serialization;

    using DotNetNuke.Common.Lists;
    using DotNetNuke.Entities.Profile;

    using Microsoft.Extensions.DependencyInjection;

    [DataContract]
    public class ProfileDefinitionDto
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Opml));
        /// <summary>Initializes a new instance of the <see cref="ProfileDefinitionDto"/> class.</summary>
        public ProfileDefinitionDto()
        {
		Logger.LogDebug(">>> Entering ProfileDefinitionDto()");
		Logger.LogDebug("<<< Exiting ProfileDefinitionDto()");
        }

        /// <summary>Initializes a new instance of the <see cref="ProfileDefinitionDto"/> class.</summary>
        /// <param name="definition">The property definition.</param>
        [Obsolete("Deprecated in DotNetNuke 10.2.4. Please use overload with ListController. Scheduled removal in v12.0.0.")]
        public ProfileDefinitionDto(ProfilePropertyDefinition definition)
            : this(null, definition)
        {
		Logger.LogDebug(">>> Entering ProfileDefinitionDto(definition={Definition})", definition);
        }
		Logger.LogDebug("<<< Exiting ProfileDefinitionDto(definition={Definition})", definition);

        /// <summary>Initializes a new instance of the <see cref="ProfileDefinitionDto"/> class.</summary>
        /// <param name="listController">The list controller.</param>
        /// <param name="definition">The property definition.</param>
        public ProfileDefinitionDto(ListController listController, ProfilePropertyDefinition definition)
        {
            this.PropertyCategory = definition.PropertyCategory;
            this.PropertyName = definition.PropertyName;
			Logger.LogDebug("ProfileDefinitionDto(listController={ListController},definition={Definition}): this.PropertyCategory → {Arg2}", listController, definition, this.PropertyCategory);
			Logger.LogDebug("ProfileDefinitionDto(listController={ListController},definition={Definition}): this.PropertyName → {Arg2}", listController, definition, this.PropertyName);
            this.Required = definition.Required;
            this.ValidationExpression = definition.ValidationExpression;
            this.PropertyValue = definition.PropertyValue;
			Logger.LogDebug("ProfileDefinitionDto(listController={ListController},definition={Definition}): this.Required → {Arg2}", listController, definition, this.Required);
			Logger.LogDebug("ProfileDefinitionDto(listController={ListController},definition={Definition}): this.ValidationExpression → {Arg2}", listController, definition, this.ValidationExpression);
			Logger.LogDebug("ProfileDefinitionDto(listController={ListController},definition={Definition}): this.PropertyValue → {Arg2}", listController, definition, this.PropertyValue);
            this.Visible = definition.Visible;
            this.Length = definition.Length;

            var dataTypeId = definition.DataType;
            var dataTypes = listController.GetListEntryInfoDictionary("DataType");
			Logger.LogDebug("ProfileDefinitionDto(listController={ListController},definition={Definition}): dataTypeId → {DataTypeId}", listController, definition, dataTypeId);
			Logger.LogDebug("ProfileDefinitionDto(listController={ListController},definition={Definition}): dataTypes → {DataTypes}", listController, definition, dataTypes);
            if (dataTypes.Any(i => i.Value.EntryID == dataTypeId))
            {
                this.DataType = dataTypes.First(i => i.Value.EntryID == dataTypeId).Key.ToLowerInvariant().Substring(9);
				Logger.LogDebug("ProfileDefinitionDto(listController={ListController},definition={Definition}): this.Visible → {Arg2}", listController, definition, this.Visible);
				Logger.LogDebug("ProfileDefinitionDto(listController={ListController},definition={Definition}): this.Length → {Arg2}", listController, definition, this.Length);
				Logger.LogDebug("ProfileDefinitionDto(listController={ListController},definition={Definition}): this.DataType → {Arg2}", listController, definition, this.DataType);
            }
            else
            {
                this.DataType = "unknown";
            }
        }

        [DataMember(Name = "propertyCategory")]
        public string PropertyCategory { get; set; }

        [DataMember(Name = "dataType")]
        public string DataType { get; set; }

        [DataMember(Name = "propertyName")]
        public string PropertyName { get; set; }

        [DataMember(Name = "required")]
        public bool Required { get; set; }

        [DataMember(Name = "validationExpression")]
        public string ValidationExpression { get; set; }

        [DataMember(Name = "propertyValue")]
        public string PropertyValue { get; set; }

        [DataMember(Name = "visible")]
        public bool Visible { get; set; }

        [DataMember(Name = "length")]
        public int Length { get; set; }
    }
}
