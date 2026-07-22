// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Entities.Profile
{
    using System;
    using System.Collections;

    using DotNetNuke.Collections;

    /// <summary>The ProfilePropertyDefinitionCollection class provides Business Layer methods for a collection of property Definitions.</summary>
    [Serializable]
    public class ProfilePropertyDefinitionCollection : GenericCollectionBase<ProfilePropertyDefinition>
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Opml));
        /// <summary>Initializes a new instance of the <see cref="ProfilePropertyDefinitionCollection"/> class.</summary>
        public ProfilePropertyDefinitionCollection()
        {
		Logger.LogDebug(">>> Entering ProfilePropertyDefinitionCollection()");
		Logger.LogDebug("<<< Exiting ProfilePropertyDefinitionCollection()");
        }

        /// <summary>Initializes a new instance of the <see cref="ProfilePropertyDefinitionCollection"/> class from an ArrayList of ProfilePropertyDefinition objects.</summary>
        /// <param name="definitionsList">An ArrayList of ProfilePropertyDefinition objects.</param>
        public ProfilePropertyDefinitionCollection(ArrayList definitionsList)
        {
			Logger.LogDebug(">>> Entering ProfilePropertyDefinitionCollection(definitionsList={DefinitionsList})", definitionsList);
            this.AddRange(definitionsList);
			Logger.LogDebug("<<< Exiting ProfilePropertyDefinitionCollection(definitionsList={DefinitionsList})", definitionsList);
        }

        /// <summary>Initializes a new instance of the <see cref="ProfilePropertyDefinitionCollection"/> class from a ProfilePropertyDefinitionCollection.</summary>
        /// <param name="collection">A ProfilePropertyDefinitionCollection.</param>
        public ProfilePropertyDefinitionCollection(ProfilePropertyDefinitionCollection collection)
        {
            this.AddRange(collection);
        }

        /// <summary>Gets an item in the collection.</summary>
        /// <remarks>This overload returns the item by its name.</remarks>
        /// <param name="name">The name of the Property to get.</param>
        /// <returns>A ProfilePropertyDefinition object.</returns>
        public ProfilePropertyDefinition this[string name] => this.GetByName(name);

        /// <summary>Add an ArrayList of ProfilePropertyDefinition objects.</summary>
        /// <param name="definitionsList">An ArrayList of ProfilePropertyDefinition objects.</param>
        public void AddRange(ArrayList definitionsList)
        {
			Logger.LogDebug(">>> Entering AddRange(definitionsList={DefinitionsList})", definitionsList);
            foreach (ProfilePropertyDefinition objProfilePropertyDefinition in definitionsList)
            {
                this.Add(objProfilePropertyDefinition);
            }
			Logger.LogDebug("<<< Exiting AddRange(definitionsList={DefinitionsList})", definitionsList);
        }

        /// <summary>Add an existing ProfilePropertyDefinitionCollection.</summary>
        /// <param name="collection">A ProfilePropertyDefinitionCollection.</param>
        public void AddRange(ProfilePropertyDefinitionCollection collection)
        {
            foreach (ProfilePropertyDefinition objProfilePropertyDefinition in collection)
            {
                this.Add(objProfilePropertyDefinition);
            }
        }

        /// <summary>Gets a sub-collection of items in the collection by category.</summary>
        /// <param name="category">The category to get.</param>
        /// <returns>A ProfilePropertyDefinitionCollection object.</returns>
        public ProfilePropertyDefinitionCollection GetByCategory(string category)
        {
			Logger.LogDebug(">>> Entering GetByCategory(category={Category})", category);
            var collection = new ProfilePropertyDefinitionCollection();
            foreach (ProfilePropertyDefinition profileProperty in this.InnerList)
            {
			Logger.LogDebug("GetByCategory(category={Category}): collection → {Collection}", category, collection);
                if (profileProperty.PropertyCategory == category)
                {
                    collection.Add(profileProperty);
                }
            }

			Logger.LogDebug("<<< Exiting GetByCategory(category={Category})", category);
            return collection;
        }

        /// <summary>Gets an item in the collection by ID.</summary>
        /// <param name="id">The id of the Property to get.</param>
        /// <returns>A ProfilePropertyDefinition object.</returns>
        public ProfilePropertyDefinition GetById(int id)
        {
			Logger.LogDebug(">>> Entering GetById(id={Id})", id);
            ProfilePropertyDefinition profileItem = null;
            foreach (ProfilePropertyDefinition profileProperty in this.InnerList)
            {
			Logger.LogDebug("GetById(id={Id}): profileItem → {ProfileItem}", id, profileItem);
                if (profileProperty.PropertyDefinitionId == id)
                {
                    profileItem = profileProperty;
                }
            }

			Logger.LogDebug("<<< Exiting GetById(id={Id})", id);
            return profileItem;
        }

        /// <summary>Gets an item in the collection by name.</summary>
        /// <param name="name">The name of the Property to get.</param>
        /// <returns>A ProfilePropertyDefinition object.</returns>
        public ProfilePropertyDefinition GetByName(string name)
        {
			Logger.LogDebug(">>> Entering GetByName(name={Name})", name);
            ProfilePropertyDefinition profileItem = null;
            foreach (ProfilePropertyDefinition profileProperty in this.InnerList)
            {
			Logger.LogDebug("GetByName(name={Name}): profileItem → {ProfileItem}", name, profileItem);
                if (profileProperty?.PropertyName == name)
                {
                    // Found Profile property
                    profileItem = profileProperty;
                }
            }

			Logger.LogDebug("<<< Exiting GetByName(name={Name})", name);
            return profileItem;
        }

        /// <summary>Sorts the collection using the ProfilePropertyDefinitionComparer (ie by ViewOrder).</summary>
        public void Sort()
        {
			Logger.LogDebug(">>> Entering Sort()");
            this.InnerList.Sort(new ProfilePropertyDefinitionComparer());
			Logger.LogDebug("<<< Exiting Sort()");
        }
    }
}
