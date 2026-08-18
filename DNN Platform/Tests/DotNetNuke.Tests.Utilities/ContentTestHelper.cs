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
	using Microsoft.Extensions.Logging;

    public class ContentTestHelper
    {
		private static readonly Microsoft.Extensions.Logging.ILogger _logger = Microsoft.Extensions.Logging.Abstractions.NullLogger.Instance;
        public static ContentItem CreateValidContentItem()
        {
			_logger.LogDebug(">>> Entering CreateValidContentItem()");
            ContentItem content = new ContentItem { Content = Constants.CONTENT_ValidContent, ContentKey = Constants.CONTENT_ValidContentKey, Indexed = Constants.CONTENT_IndexedFalse };
			_logger.LogDebug("<<< Exiting CreateValidContentItem()");
            return content;
        }

        public static ContentType CreateValidContentType()
        {
			_logger.LogDebug(">>> Entering CreateValidContentType()");
            ContentType contentType = new ContentType { ContentType = Constants.CONTENTTYPE_ValidContentType };
			_logger.LogDebug("<<< Exiting CreateValidContentType()");
            return contentType;
        }

        public static Term CreateValidHeirarchicalTerm(int vocabularyId, int parentId)
        {
			_logger.LogDebug(">>> Entering CreateValidHeirarchicalTerm(vocabularyId={VocabularyId},parentId={ParentId})", vocabularyId, parentId);
            Term term = new Term(vocabularyId) { Name = Constants.TERM_ValidName, Description = Constants.TERM_ValidName, Weight = Constants.TERM_ValidWeight, ParentTermId = parentId };
			_logger.LogDebug("CreateValidHeirarchicalTerm(vocabularyId={VocabularyId},parentId={ParentId}): term → {Term}", vocabularyId, parentId, term);
			_logger.LogDebug("<<< Exiting CreateValidHeirarchicalTerm(vocabularyId={VocabularyId},parentId={ParentId})", vocabularyId, parentId);
            return term;
        }

        public static ScopeType CreateValidScopeType()
        {
			_logger.LogDebug(">>> Entering CreateValidScopeType()");
            ScopeType scopeType = new ScopeType { ScopeType = Constants.SCOPETYPE_ValidScopeType };
			_logger.LogDebug("<<< Exiting CreateValidScopeType()");
            return scopeType;
        }

        public static Term CreateValidSimpleTerm(int vocabularyId)
        {
			_logger.LogDebug(">>> Entering CreateValidSimpleTerm(vocabularyId={VocabularyId})", vocabularyId);
            Term term = new Term(vocabularyId) { Name = Constants.TERM_ValidName, Description = Constants.TERM_ValidName, Weight = Constants.TERM_ValidWeight };
			_logger.LogDebug("CreateValidSimpleTerm(vocabularyId={VocabularyId}): term → {Term}", vocabularyId, term);
			_logger.LogDebug("<<< Exiting CreateValidSimpleTerm(vocabularyId={VocabularyId})", vocabularyId);
            return term;
        }

        public static Vocabulary CreateValidVocabulary()
        {
			_logger.LogDebug(">>> Entering CreateValidVocabulary()");
            Vocabulary vocabulary = new Vocabulary
            {
                Name = Constants.VOCABULARY_ValidName,
                Type = Constants.VOCABULARY_ValidType,
                ScopeTypeId = Constants.VOCABULARY_ValidScopeTypeId,
                ScopeId = Constants.VOCABULARY_ValidScopeId,
                Weight = Constants.VOCABULARY_ValidWeight,
            };

			_logger.LogDebug("<<< Exiting CreateValidVocabulary()");
            return vocabulary;
        }

        public static FileInfo CreateValidFile(int fileId)
        {
			_logger.LogDebug(">>> Entering CreateValidFile(fileId={FileId})", fileId);
            var sb = new StringBuilder();

			_logger.LogDebug("CreateValidFile(fileId={FileId}): sb → {Sb}", fileId, sb);
            using (var hasher = SHA1.Create())
            {
                foreach (var b in Encoding.ASCII.GetChars(hasher.ComputeHash(Encoding.UTF8.GetBytes(fileId.ToString()))))
                {
                    sb.Append(b);
                }

				_logger.LogDebug("<<< Exiting CreateValidFile(fileId={FileId})", fileId);
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
			_logger.LogDebug(">>> Entering GetContent(i={I})", i);
			_logger.LogDebug("<<< Exiting GetContent(i={I})", i);
            return string.Format(string.Format(Constants.CONTENT_ValidContentFormat, i));
        }

        public static string GetContentKey(int i)
        {
			_logger.LogDebug(">>> Entering GetContentKey(i={I})", i);
			_logger.LogDebug("<<< Exiting GetContentKey(i={I})", i);
            return string.Format(string.Format(Constants.CONTENT_ValidContentKeyFormat, i));
        }

        public static string GetContentType(int i)
        {
			_logger.LogDebug(">>> Entering GetContentType(i={I})", i);
			_logger.LogDebug("<<< Exiting GetContentType(i={I})", i);
            return string.Format(Constants.CONTENTTYPE_ValidContentTypeFormat, i);
        }

        public static string GetScopeType(int i)
        {
			_logger.LogDebug(">>> Entering GetScopeType(i={I})", i);
			_logger.LogDebug("<<< Exiting GetScopeType(i={I})", i);
            return string.Format(Constants.SCOPETYPE_ValidScopeTypeFormat, i);
        }

        public static string GetTermName(int i)
        {
			_logger.LogDebug(">>> Entering GetTermName(i={I})", i);
			_logger.LogDebug("<<< Exiting GetTermName(i={I})", i);
            return string.Format(Constants.TERM_ValidNameFormat, i);
        }

        public static string GetVocabularyName(int i)
        {
			_logger.LogDebug(">>> Entering GetVocabularyName(i={I})", i);
			_logger.LogDebug("<<< Exiting GetVocabularyName(i={I})", i);
            return string.Format(Constants.VOCABULARY_ValidNameFormat, i);
        }
    }
}
