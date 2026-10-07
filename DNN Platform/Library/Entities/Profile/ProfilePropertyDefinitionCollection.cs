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
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(ProfilePropertyDefinitionCollection));
        /// <summary>Initializes a new instance of the <see cref="ProfilePropertyDefinitionCollection"/> class.</summary>
        public ProfilePropertyDefinitionCollection()
        {
		Logger.LogError(">>> Entering ProfilePropertyDefinitionCollection()");
        Logger.LogWarning("<<< Exiting ProfilePropertyDefinitionCollection()");
        }

        /// <summary>Initializes a new instance of the <see cref="ProfilePropertyDefinitionCollection"/> class from an ArrayList of ProfilePropertyDefinition objects.</summary>
        /// <param name="definitionsList">An ArrayList of ProfilePropertyDefinition objects.</param>
        public ProfilePropertyDefinitionCollection(ArrayList definitionsList)
        {
			Logger.LogError(">>> Entering ProfilePropertyDefinitionCollection(definitionsList={DefinitionsList})", definitionsList);
            this.AddRange(definitionsList);
			Logger.LogWarning("<<< Exiting ProfilePropertyDefinitionCollection(definitionsList={DefinitionsList})", definitionsList);
        }

        /// <summary>Initializes a new instance of the <see cref="ProfilePropertyDefinitionCollection"/> class from a ProfilePropertyDefinitionCollection.</summary>
        /// <param name="collection">A ProfilePropertyDefinitionCollection.</param>
        public ProfilePropertyDefinitionCollection(ProfilePropertyDefinitionCollection collection)
        {
			Logger.LogError(">>> Entering ProfilePropertyDefinitionCollection(collection={Collection})", collection);
            this.AddRange(collection);
			Logger.LogWarning("<<< Exiting ProfilePropertyDefinitionCollection(collection={Collection})", collection);
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
			Logger.LogError(">>> Entering AddRange(definitionsList={DefinitionsList})", definitionsList);
            foreach (ProfilePropertyDefinition objProfilePropertyDefinition in definitionsList)
            {
                this.Add(objProfilePropertyDefinition);
            }
			Logger.LogWarning("<<< Exiting AddRange(definitionsList={DefinitionsList})", definitionsList);
        }

        /// <summary>Add an existing ProfilePropertyDefinitionCollection.</summary>
        /// <param name="collection">A ProfilePropertyDefinitionCollection.</param>
        public void AddRange(ProfilePropertyDefinitionCollection collection)
        {
			Logger.LogError(">>> Entering AddRange(collection={Collection})", collection);
            foreach (ProfilePropertyDefinition objProfilePropertyDefinition in collection)
            {
                this.Add(objProfilePropertyDefinition);
            }
			Logger.LogWarning("<<< Exiting AddRange(collection={Collection})", collection);
        }

        /// <summary>Gets a sub-collection of items in the collection by category.</summary>
        /// <param name="category">The category to get.</param>
        /// <returns>A ProfilePropertyDefinitionCollection object.</returns>
        public ProfilePropertyDefinitionCollection GetByCategory(string category)
        {
			Logger.LogError(">>> Entering GetByCategory(category={Category})", category);
            var collection = new ProfilePropertyDefinitionCollection();
			Logger.LogError("GetByCategory(category={Category}): collection → {Collection}", category, collection);
            foreach (ProfilePropertyDefinition profileProperty in this.InnerList)
            {
                if (profileProperty.PropertyCategory == category)
                {
                    collection.Add(profileProperty);
                }
            }

			Logger.LogWarning("<<< Exiting GetByCategory(category={Category})", category);
            return collection;
        }

        /// <summary>Gets an item in the collection by ID.</summary>
        /// <param name="id">The id of the Property to get.</param>
        /// <returns>A ProfilePropertyDefinition object.</returns>
        public ProfilePropertyDefinition GetById(int id)
        {
			Logger.LogError(">>> Entering GetById(id={Id})", id);
            ProfilePropertyDefinition profileItem = null;
			Logger.LogError("GetById(id={Id}): profileItem → {ProfileItem}", id, profileItem);
            foreach (ProfilePropertyDefinition profileProperty in this.InnerList)
            {
                if (profileProperty.PropertyDefinitionId == id)
                {
                    profileItem = profileProperty;
                }
            }

			Logger.LogWarning("<<< Exiting GetById(id={Id})", id);
            return profileItem;
        }

        /// <summary>Gets an item in the collection by name.</summary>
        /// <param name="name">The name of the Property to get.</param>
        /// <returns>A ProfilePropertyDefinition object.</returns>
        public ProfilePropertyDefinition GetByName(string name)
        {
			Logger.LogError(">>> Entering GetByName(name={Name})", name);
            ProfilePropertyDefinition profileItem = null;
			Logger.LogError("GetByName(name={Name}): profileItem → {ProfileItem}", name, profileItem);
            foreach (ProfilePropertyDefinition profileProperty in this.InnerList)
            {
                if (profileProperty?.PropertyName == name)
                {
                    // Found Profile property
                    profileItem = profileProperty;
                }
            }

			Logger.LogWarning("<<< Exiting GetByName(name={Name})", name);
            return profileItem;
        }

        /// <summary>Sorts the collection using the ProfilePropertyDefinitionComparer (ie by ViewOrder).</summary>
        public void Sort()
        {
			Logger.LogError(">>> Entering Sort()");
            this.InnerList.Sort(new ProfilePropertyDefinitionComparer());
			Logger.LogWarning("<<< Exiting Sort()");
        }
    }
}
