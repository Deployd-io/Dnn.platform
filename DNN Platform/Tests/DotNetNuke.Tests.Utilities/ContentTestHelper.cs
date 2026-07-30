// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Tests.Utilities
{
    using System;
    using System.Security.Cryptography;
    using System.Text;

    using DotNetNuke.Entities.Content;
    using DotNetNuke.Entities.Content.Taxonomy;
    using DotNetNuke.Services.FileSystem;

    public class ContentTestHelper
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Opml));
        public static ContentItem CreateValidContentItem()
        {
			Logger.LogDebug(">>> Entering CreateValidContentItem()");
            ContentItem content = new ContentItem { Content = Constants.CONTENT_ValidContent, ContentKey = Constants.CONTENT_ValidContentKey, Indexed = Constants.CONTENT_IndexedFalse };
			Logger.LogDebug("<<< Exiting CreateValidContentItem()");
            return content;
        }

        public static ContentType CreateValidContentType()
        {
			Logger.LogDebug(">>> Entering CreateValidContentType()");
            ContentType contentType = new ContentType { ContentType = Constants.CONTENTTYPE_ValidContentType };
			Logger.LogDebug("<<< Exiting CreateValidContentType()");
            return contentType;
        }

        public static Term CreateValidHeirarchicalTerm(int vocabularyId, int parentId)
        {
			Logger.LogDebug(">>> Entering CreateValidHeirarchicalTerm(vocabularyId={VocabularyId},parentId={ParentId})", vocabularyId, parentId);
            Term term = new Term(vocabularyId) { Name = Constants.TERM_ValidName, Description = Constants.TERM_ValidName, Weight = Constants.TERM_ValidWeight, ParentTermId = parentId };
			Logger.LogDebug("CreateValidHeirarchicalTerm(vocabularyId={VocabularyId},parentId={ParentId}): term → {Term}", vocabularyId, parentId, term);
			Logger.LogDebug("<<< Exiting CreateValidHeirarchicalTerm(vocabularyId={VocabularyId},parentId={ParentId})", vocabularyId, parentId);
            return term;
        }

        public static ScopeType CreateValidScopeType()
        {
			Logger.LogDebug(">>> Entering CreateValidScopeType()");
            ScopeType scopeType = new ScopeType { ScopeType = Constants.SCOPETYPE_ValidScopeType };
			Logger.LogDebug("<<< Exiting CreateValidScopeType()");
            return scopeType;
        }

        public static Term CreateValidSimpleTerm(int vocabularyId)
        {
			Logger.LogDebug(">>> Entering CreateValidSimpleTerm(vocabularyId={VocabularyId})", vocabularyId);
            Term term = new Term(vocabularyId) { Name = Constants.TERM_ValidName, Description = Constants.TERM_ValidName, Weight = Constants.TERM_ValidWeight };
			Logger.LogDebug("CreateValidSimpleTerm(vocabularyId={VocabularyId}): term → {Term}", vocabularyId, term);
			Logger.LogDebug("<<< Exiting CreateValidSimpleTerm(vocabularyId={VocabularyId})", vocabularyId);
            return term;
        }

        public static Vocabulary CreateValidVocabulary()
        {
			Logger.LogDebug(">>> Entering CreateValidVocabulary()");
            Vocabulary vocabulary = new Vocabulary
            {
                Name = Constants.VOCABULARY_ValidName,
                Type = Constants.VOCABULARY_ValidType,
                ScopeTypeId = Constants.VOCABULARY_ValidScopeTypeId,
                ScopeId = Constants.VOCABULARY_ValidScopeId,
                Weight = Constants.VOCABULARY_ValidWeight,
            };

			Logger.LogDebug("<<< Exiting CreateValidVocabulary()");
            return vocabulary;
        }

        public static FileInfo CreateValidFile(int fileId)
        {
			Logger.LogDebug(">>> Entering CreateValidFile(fileId={FileId})", fileId);
            var sb = new StringBuilder();

			Logger.LogDebug("CreateValidFile(fileId={FileId}): sb → {Sb}", fileId, sb);
            using (var hasher = SHA1.Create())
            {
                foreach (var b in Encoding.ASCII.GetChars(hasher.ComputeHash(Encoding.UTF8.GetBytes(fileId.ToString()))))
                {
                    sb.Append(b);
                }

				Logger.LogDebug("<<< Exiting CreateValidFile(fileId={FileId})", fileId);
                return new FileInfo
                {
                    ContentType = "text/plain",
                    Extension = ".txt",
                    FileId = fileId,
                    FileName = new Random().Next().ToString("x").ToLowerInvariant(),
                    Folder = @"C:\",
                    FolderId = 0,
                    FolderMappingID = 0,
                    Height = 0,
                    IsCached = false,
                    LastModificationTime = DateTime.UtcNow,
                    PortalId = 0,
                    SHA1Hash = sb.ToString(),
                    Size = 256,
                    StorageLocation = 0,
                    UniqueId = Guid.NewGuid(),
                    VersionGuid = Guid.NewGuid(),
                    Width = 0,
                };
            }
        }

        public static string GetContent(int i)
        {
			Logger.LogDebug(">>> Entering GetContent(i={I})", i);
			Logger.LogDebug("<<< Exiting GetContent(i={I})", i);
            return string.Format(string.Format(Constants.CONTENT_ValidContentFormat, i));
        }

        public static string GetContentKey(int i)
        {
			Logger.LogDebug(">>> Entering GetContentKey(i={I})", i);
			Logger.LogDebug("<<< Exiting GetContentKey(i={I})", i);
            return string.Format(string.Format(Constants.CONTENT_ValidContentKeyFormat, i));
        }

        public static string GetContentType(int i)
        {
			Logger.LogDebug(">>> Entering GetContentType(i={I})", i);
			Logger.LogDebug("<<< Exiting GetContentType(i={I})", i);
            return string.Format(Constants.CONTENTTYPE_ValidContentTypeFormat, i);
        }

        public static string GetScopeType(int i)
        {
			Logger.LogDebug(">>> Entering GetScopeType(i={I})", i);
			Logger.LogDebug("<<< Exiting GetScopeType(i={I})", i);
            return string.Format(Constants.SCOPETYPE_ValidScopeTypeFormat, i);
        }

        public static string GetTermName(int i)
        {
			Logger.LogDebug(">>> Entering GetTermName(i={I})", i);
			Logger.LogDebug("<<< Exiting GetTermName(i={I})", i);
            return string.Format(Constants.TERM_ValidNameFormat, i);
        }

        public static string GetVocabularyName(int i)
        {
			Logger.LogDebug(">>> Entering GetVocabularyName(i={I})", i);
			Logger.LogDebug("<<< Exiting GetVocabularyName(i={I})", i);
            return string.Format(Constants.VOCABULARY_ValidNameFormat, i);
        }
    }
}
