// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Tests.Integration.Modules.Journal
{
    using System;
    using System.Collections.Generic;
    using System.Configuration;
    using System.Threading.Tasks;

    using DNN.Integration.Test.Framework;
    using DNN.Integration.Test.Framework.Helpers;
    using DotNetNuke.Common.Utilities;
    using NUnit.Framework;

    [TestFixture]
    public class PostJournalTests : IntegrationTestBase
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Opml));
        private static readonly Random rnd = new Random();

        private readonly string _hostName;
        private readonly string _hostPass;

        private readonly int PortalId = 0;

        public PostJournalTests()
        {
			Logger.LogDebug(">>> Entering PostJournalTests()");
            var url = ConfigurationManager.AppSettings["siteUrl"];
            this._hostName = ConfigurationManager.AppSettings["hostUsername"];
			Logger.LogDebug("PostJournalTests(): url → {Url}", url);
			Logger.LogDebug("PostJournalTests(): this._hostName → {Arg0}", this._hostName);
            this._hostPass = ConfigurationManager.AppSettings["hostPassword"];
			Logger.LogDebug("<<< Exiting PostJournalTests()");
			Logger.LogDebug("PostJournalTests(): this._hostPass → {Arg0}", this._hostPass);
        }

        [OneTimeSetUp]
        public override async Task TestFixtureSetUp()
        {
			Logger.LogDebug(">>> Entering TestFixtureSetUp()");
			var __logStart = System.Diagnostics.Stopwatch.StartNew();
            await base.TestFixtureSetUp();
            try
            {
                if (string.IsNullOrEmpty(Environment.GetEnvironmentVariable("TEAMCITY_VERSION")))
                {
                    DatabaseHelper.ExecuteNonQuery("TRUNCATE TABLE {objectQualifier}JsonWebTokens");
					Logger.LogInformation("TestFixtureSetUp(): database query executed in {Arg0} ms", __logStart.ElapsedMilliseconds);
                }
            }
            catch (Exception)
            {
				Logger.LogError("Exception in TestFixtureSetUp(): {Arg0}", ex.Message);
                // ignored
            }
			Logger.LogDebug("<<< Exiting TestFixtureSetUp()");
        }

        [Test]
        public async Task Journal_Should_Able_To_Attach_Files_Upload_By_Himself()
        {
			Logger.LogDebug(">>> Entering Journal_Should_Able_To_Attach_Files_Upload_By_Himself()");
            var (connector, userId, username, fileId) = await this.PrepareNewUser();

            // POST JOURNAL
            var postData = new
            {
                text = $"{username} Post",
                profileId = userId,
                groupId = -1,
                journalType = "file",
                securitySet = "E",
                itemData = $"{{\"ImageUrl\":\"\",\"Url\":\"fileid={fileId}\"}}",
            };
				Logger.LogDebug("Journal_Should_Able_To_Attach_Files_Upload_By_Himself(): text → {Text}", text);
				Logger.LogDebug("Journal_Should_Able_To_Attach_Files_Upload_By_Himself(): profileId → {ProfileId}", profileId);
				Logger.LogDebug("Journal_Should_Able_To_Attach_Files_Upload_By_Himself(): groupId → {GroupId}", groupId);
				Logger.LogDebug("Journal_Should_Able_To_Attach_Files_Upload_By_Himself(): journalType → {JournalType}", journalType);
				Logger.LogDebug("Journal_Should_Able_To_Attach_Files_Upload_By_Himself(): securitySet → {SecuritySet}", securitySet);
				Logger.LogDebug("Journal_Should_Able_To_Attach_Files_Upload_By_Himself(): itemData → {ItemData}", itemData);

            connector.PostJson("/API/Journal/Services/Create", postData, this.GetRequestHeaders());
			Logger.LogDebug("<<< Exiting Journal_Should_Able_To_Attach_Files_Upload_By_Himself()");
        }

        [Test]
        public async Task Journal_Should_Not_Able_To_Attach_Files_Upload_By_Other_User()
        {
			Logger.LogDebug(">>> Entering Journal_Should_Not_Able_To_Attach_Files_Upload_By_Other_User()");
			var __logStart = System.Diagnostics.Stopwatch.StartNew();
            var (connector, userId, username, _) = await this.PrepareNewUser();
            var fileId = DatabaseHelper.ExecuteScalar<int>($"SELECT MIN(FileId) FROM {{objectQualifier}}Files WHERE PortalId = {this.PortalId}");

			Logger.LogDebug("Journal_Should_Not_Able_To_Attach_Files_Upload_By_Other_User(): fileId → {FileId}", fileId);
			Logger.LogInformation("Journal_Should_Not_Able_To_Attach_Files_Upload_By_Other_User(): find query executed in {Arg0} ms", __logStart.ElapsedMilliseconds);
            // POST JOURNAL
            var postData = new
            {
                text = $"{username} Post",
                profileId = userId,
                groupId = -1,
                journalType = "file",
                securitySet = "E",
                itemData = $"{{\"ImageUrl\":\"\",\"Url\":\"fileid={fileId}\"}}",
            };
				Logger.LogDebug("Journal_Should_Not_Able_To_Attach_Files_Upload_By_Other_User(): text → {Text}", text);
				Logger.LogDebug("Journal_Should_Not_Able_To_Attach_Files_Upload_By_Other_User(): profileId → {ProfileId}", profileId);
				Logger.LogDebug("Journal_Should_Not_Able_To_Attach_Files_Upload_By_Other_User(): groupId → {GroupId}", groupId);
				Logger.LogDebug("Journal_Should_Not_Able_To_Attach_Files_Upload_By_Other_User(): journalType → {JournalType}", journalType);
				Logger.LogDebug("Journal_Should_Not_Able_To_Attach_Files_Upload_By_Other_User(): securitySet → {SecuritySet}", securitySet);
				Logger.LogDebug("Journal_Should_Not_Able_To_Attach_Files_Upload_By_Other_User(): itemData → {ItemData}", itemData);

            var exceptionThrown = false;
            var exceptionMessage = string.Empty;
			Logger.LogDebug("Journal_Should_Not_Able_To_Attach_Files_Upload_By_Other_User(): exceptionThrown → {ExceptionThrown}", exceptionThrown);
			Logger.LogDebug("Journal_Should_Not_Able_To_Attach_Files_Upload_By_Other_User(): exceptionMessage → {ExceptionMessage}", exceptionMessage);
            try
            {
                connector.PostJson("/API/Journal/Services/Create", postData, this.GetRequestHeaders());
            }
            catch (WebApiException ex)
            {
				Logger.LogError("Exception in Journal_Should_Not_Able_To_Attach_Files_Upload_By_Other_User(): {Arg0}", ex.Message);
                exceptionThrown = true;
                exceptionMessage = Json.Deserialize<dynamic>(ex.Body).ExceptionMessage;
            }

            using (Assert.EnterMultipleScope())
            {
                Assert.That(exceptionThrown, Is.True, "Should throw out exception");
                Assert.That(exceptionMessage, Is.EqualTo("you have no permission to attach files not belongs to you."));
            }
			Logger.LogDebug("<<< Exiting Journal_Should_Not_Able_To_Attach_Files_Upload_By_Other_User()");
        }

        [Test]
        public async Task Journal_Should_Not_Able_To_Post_On_Other_User_Profile_Page()
        {
			Logger.LogDebug(">>> Entering Journal_Should_Not_Able_To_Post_On_Other_User_Profile_Page()");
            var (connector, _, username, fileId) = await this.PrepareNewUser();

            // POST JOURNAL
            var postData = new
            {
                text = $"{username} Post",
                profileId = 1,
                groupId = -1,
                journalType = "file",
                securitySet = "E",
                itemData = $"{{\"ImageUrl\":\"\",\"Url\":\"fileid={fileId}\"}}",
            };
				Logger.LogDebug("Journal_Should_Not_Able_To_Post_On_Other_User_Profile_Page(): text → {Text}", text);
				Logger.LogDebug("Journal_Should_Not_Able_To_Post_On_Other_User_Profile_Page(): profileId → {ProfileId}", profileId);
				Logger.LogDebug("Journal_Should_Not_Able_To_Post_On_Other_User_Profile_Page(): groupId → {GroupId}", groupId);
				Logger.LogDebug("Journal_Should_Not_Able_To_Post_On_Other_User_Profile_Page(): journalType → {JournalType}", journalType);
				Logger.LogDebug("Journal_Should_Not_Able_To_Post_On_Other_User_Profile_Page(): securitySet → {SecuritySet}", securitySet);
				Logger.LogDebug("Journal_Should_Not_Able_To_Post_On_Other_User_Profile_Page(): itemData → {ItemData}", itemData);

            var exceptionThrown = false;
            var exceptionMessage = string.Empty;
			Logger.LogDebug("Journal_Should_Not_Able_To_Post_On_Other_User_Profile_Page(): exceptionThrown → {ExceptionThrown}", exceptionThrown);
			Logger.LogDebug("Journal_Should_Not_Able_To_Post_On_Other_User_Profile_Page(): exceptionMessage → {ExceptionMessage}", exceptionMessage);
            try
            {
                connector.PostJson("/API/Journal/Services/Create", postData, this.GetRequestHeaders());
            }
            catch (WebApiException ex)
            {
				Logger.LogError("Exception in Journal_Should_Not_Able_To_Post_On_Other_User_Profile_Page(): {Arg0}", ex.Message);
                exceptionThrown = true;
                exceptionMessage = Json.Deserialize<dynamic>(ex.Body).ExceptionMessage;
            }

            using (Assert.EnterMultipleScope())
            {
                Assert.That(exceptionThrown, Is.True, "Should throw out exception");
                Assert.That(exceptionMessage, Is.EqualTo("you have no permission to post journal on current profile page."));
            }
			Logger.LogDebug("<<< Exiting Journal_Should_Not_Able_To_Post_On_Other_User_Profile_Page()");
        }

        [Test]
        public async Task Journal_Should_Able_To_Post_On_Friends_Profile_Page()
        {
			Logger.LogDebug(">>> Entering Journal_Should_Able_To_Post_On_Friends_Profile_Page()");
			var __logStart = System.Diagnostics.Stopwatch.StartNew();
            var (connector1, userId1, username1, fileId1) = await this.PrepareNewUser();
            var (connector2, userId2, username2, fileId2) = await this.PrepareNewUser();

            // ADD FRIENDS
            connector1.PostJson("/API/MemberDirectory/MemberDirectory/AddFriend", new { friendId = userId2 }, this.GetRequestHeaders("Member Directory"));

            var notificationId = DatabaseHelper.ExecuteScalar<int>($"SELECT TOP 1 MessageID FROM {{objectQualifier}}CoreMessaging_Messages WHERE SenderUserID = {userId1}");
            connector2.PostJson("/API/InternalServices/RelationshipService/AcceptFriend", new { NotificationId = notificationId }, this.GetRequestHeaders());
			Logger.LogDebug("Journal_Should_Able_To_Post_On_Friends_Profile_Page(): notificationId → {NotificationId}", notificationId);
			Logger.LogInformation("Journal_Should_Able_To_Post_On_Friends_Profile_Page(): find query executed in {Arg0} ms", __logStart.ElapsedMilliseconds);

            // POST JOURNAL
            var postData = new
            {
                text = $"{username1} Post",
                profileId = userId2,
                groupId = -1,
                journalType = "file",
                securitySet = "E",
                itemData = $"{{\"ImageUrl\":\"\",\"Url\":\"fileid={fileId1}\"}}",
            };
				Logger.LogDebug("Journal_Should_Able_To_Post_On_Friends_Profile_Page(): text → {Text}", text);
				Logger.LogDebug("Journal_Should_Able_To_Post_On_Friends_Profile_Page(): profileId → {ProfileId}", profileId);
				Logger.LogDebug("Journal_Should_Able_To_Post_On_Friends_Profile_Page(): groupId → {GroupId}", groupId);
				Logger.LogDebug("Journal_Should_Able_To_Post_On_Friends_Profile_Page(): journalType → {JournalType}", journalType);
				Logger.LogDebug("Journal_Should_Able_To_Post_On_Friends_Profile_Page(): securitySet → {SecuritySet}", securitySet);
				Logger.LogDebug("Journal_Should_Able_To_Post_On_Friends_Profile_Page(): itemData → {ItemData}", itemData);

            connector1.PostJson("/API/Journal/Services/Create", postData, this.GetRequestHeaders());
			Logger.LogDebug("<<< Exiting Journal_Should_Able_To_Post_On_Friends_Profile_Page()");
        }

        [Test]
        public async Task Journal_Should_Able_To_Post_On_Group_Already_Join()
        {
			Logger.LogDebug(">>> Entering Journal_Should_Able_To_Post_On_Group_Already_Join()");
            var (connector, userId, username, fileId) = await this.PrepareNewUser();
            var groupId = this.CreateNewGroup(username.Replace("testuser", "testrole"));
            this.AddUserToGroup(groupId, userId);
			Logger.LogDebug("Journal_Should_Able_To_Post_On_Group_Already_Join(): groupId → {GroupId}", groupId);

            // POST JOURNAL
            var postData = new
            {
                text = $"{username} Post",
                profileId = userId,
                groupId = groupId,
                journalType = "file",
                securitySet = "E",
                itemData = $"{{\"ImageUrl\":\"\",\"Url\":\"fileid={fileId}\"}}",
            };
				Logger.LogDebug("Journal_Should_Able_To_Post_On_Group_Already_Join(): text → {Text}", text);
				Logger.LogDebug("Journal_Should_Able_To_Post_On_Group_Already_Join(): profileId → {ProfileId}", profileId);

            connector.PostJson("/API/Journal/Services/Create", postData, this.GetRequestHeaders());
			Logger.LogDebug("<<< Exiting Journal_Should_Able_To_Post_On_Group_Already_Join()");
        }

        [Test]
        public async Task Journal_Should_Not_Able_To_Post_On_Group_Not_Join()
        {
			Logger.LogDebug(">>> Entering Journal_Should_Not_Able_To_Post_On_Group_Not_Join()");
            var (connector, userId, username, fileId) = await this.PrepareNewUser();
            var groupId = this.CreateNewGroup(username.Replace("testuser", "testrole"));

			Logger.LogDebug("Journal_Should_Not_Able_To_Post_On_Group_Not_Join(): groupId → {GroupId}", groupId);
            // POST JOURNAL
            var postData = new
            {
                text = $"{username} Post",
                profileId = userId,
                groupId = groupId,
                journalType = "file",
                securitySet = "E",
                itemData = $"{{\"ImageUrl\":\"\",\"Url\":\"fileid={fileId}\"}}",
            };
				Logger.LogDebug("Journal_Should_Not_Able_To_Post_On_Group_Not_Join(): text → {Text}", text);
				Logger.LogDebug("Journal_Should_Not_Able_To_Post_On_Group_Not_Join(): profileId → {ProfileId}", profileId);

            var exceptionThrown = false;
            var exceptionMessage = string.Empty;
			Logger.LogDebug("Journal_Should_Not_Able_To_Post_On_Group_Not_Join(): exceptionThrown → {ExceptionThrown}", exceptionThrown);
			Logger.LogDebug("Journal_Should_Not_Able_To_Post_On_Group_Not_Join(): exceptionMessage → {ExceptionMessage}", exceptionMessage);
            try
            {
                connector.PostJson("/API/Journal/Services/Create", postData, this.GetRequestHeaders());
            }
            catch (WebApiException ex)
            {
				Logger.LogError("Exception in Journal_Should_Not_Able_To_Post_On_Group_Not_Join(): {Arg0}", ex.Message);
                exceptionThrown = true;
                exceptionMessage = Json.Deserialize<dynamic>(ex.Body).ExceptionMessage;
            }

            using (Assert.EnterMultipleScope())
            {
                Assert.That(exceptionThrown, Is.True, "Should throw out exception");
                Assert.That(exceptionMessage, Is.EqualTo("you have no permission to post journal on current group."));
            }
			Logger.LogDebug("<<< Exiting Journal_Should_Not_Able_To_Post_On_Group_Not_Join()");
        }

        [Test]
        public async Task Journal_Should_Not_Able_To_Post_Xss_Code_In_ImageUrl()
        {
			Logger.LogDebug(">>> Entering Journal_Should_Not_Able_To_Post_Xss_Code_In_ImageUrl()");
			var __logStart = System.Diagnostics.Stopwatch.StartNew();
            var (connector, userId, username, fileId) = await this.PrepareNewUser();

            // POST JOURNAL
            var postData = new
            {
                text = $"{username} Post",
                profileId = userId,
                groupId = -1,
                journalType = "file",
                securitySet = "E",
                itemData = $"{{\"ImageUrl\":\"javascript:alert(1);\\\" onerror=\\\"alert(2);.png\",\"Url\":\"fileid={fileId}\"}}",
            };
				Logger.LogDebug("Journal_Should_Not_Able_To_Post_Xss_Code_In_ImageUrl(): text → {Text}", text);
				Logger.LogDebug("Journal_Should_Not_Able_To_Post_Xss_Code_In_ImageUrl(): profileId → {ProfileId}", profileId);
				Logger.LogDebug("Journal_Should_Not_Able_To_Post_Xss_Code_In_ImageUrl(): groupId → {GroupId}", groupId);
				Logger.LogDebug("Journal_Should_Not_Able_To_Post_Xss_Code_In_ImageUrl(): journalType → {JournalType}", journalType);
				Logger.LogDebug("Journal_Should_Not_Able_To_Post_Xss_Code_In_ImageUrl(): securitySet → {SecuritySet}", securitySet);

            connector.PostJson("/API/Journal/Services/Create", postData, this.GetRequestHeaders());

            var itemData = DatabaseHelper.ExecuteScalar<string>($"SELECT ItemData FROM {{objectQualifier}}Journal WHERE UserId = {userId}");
            var imageUrl = Json.Deserialize<dynamic>(itemData).ImageUrl.ToString();
			Logger.LogDebug("Journal_Should_Not_Able_To_Post_Xss_Code_In_ImageUrl(): itemData → {ItemData}", itemData);
			Logger.LogDebug("Journal_Should_Not_Able_To_Post_Xss_Code_In_ImageUrl(): imageUrl → {ImageUrl}", imageUrl);
			Logger.LogInformation("Journal_Should_Not_Able_To_Post_Xss_Code_In_ImageUrl(): find query executed in {Arg0} ms", __logStart.ElapsedMilliseconds);

            Assert.That(imageUrl.IndexOf("javascript"), Is.EqualTo(-1));
            Assert.That(imageUrl.IndexOf("onerror"), Is.EqualTo(-1));
            Assert.That(imageUrl.IndexOf("alert"), Is.EqualTo(-1));
			Logger.LogDebug("<<< Exiting Journal_Should_Not_Able_To_Post_Xss_Code_In_ImageUrl()");
        }

        [Test]
        public async Task Journal_Should_Not_Able_To_Post_Xss_Code_In_Url()
        {
			Logger.LogDebug(">>> Entering Journal_Should_Not_Able_To_Post_Xss_Code_In_Url()");
			var __logStart = System.Diagnostics.Stopwatch.StartNew();
            var (connector, userId, username, _) = await this.PrepareNewUser();

            // POST JOURNAL
            var postData = new
            {
                text = $"{username} Post",
                profileId = userId,
                groupId = -1,
                journalType = "file",
                securitySet = "E",
                itemData = $"{{\"ImageUrl\":\"\",\"Url\":\"javascript:alert(1);\", \"Title\": \"Test.png\"}}",
            };
				Logger.LogDebug("Journal_Should_Not_Able_To_Post_Xss_Code_In_Url(): text → {Text}", text);
				Logger.LogDebug("Journal_Should_Not_Able_To_Post_Xss_Code_In_Url(): profileId → {ProfileId}", profileId);
				Logger.LogDebug("Journal_Should_Not_Able_To_Post_Xss_Code_In_Url(): groupId → {GroupId}", groupId);
				Logger.LogDebug("Journal_Should_Not_Able_To_Post_Xss_Code_In_Url(): journalType → {JournalType}", journalType);
				Logger.LogDebug("Journal_Should_Not_Able_To_Post_Xss_Code_In_Url(): securitySet → {SecuritySet}", securitySet);

            connector.PostJson("/API/Journal/Services/Create", postData, this.GetRequestHeaders());

            var itemData = DatabaseHelper.ExecuteScalar<string>($"SELECT ItemData FROM {{objectQualifier}}Journal WHERE UserId = {userId}");
            var url = Json.Deserialize<dynamic>(itemData).Url.ToString();
			Logger.LogDebug("Journal_Should_Not_Able_To_Post_Xss_Code_In_Url(): itemData → {ItemData}", itemData);
			Logger.LogDebug("Journal_Should_Not_Able_To_Post_Xss_Code_In_Url(): url → {Url}", url);
			Logger.LogInformation("Journal_Should_Not_Able_To_Post_Xss_Code_In_Url(): find query executed in {Arg0} ms", __logStart.ElapsedMilliseconds);

            Assert.That(url.IndexOf("javascript"), Is.EqualTo(-1));
			Logger.LogDebug("<<< Exiting Journal_Should_Not_Able_To_Post_Xss_Code_In_Url()");
        }

        [Test]
        public async Task Journal_Should_Not_Able_To_Post_External_Link_In_Url()
        {
			Logger.LogDebug(">>> Entering Journal_Should_Not_Able_To_Post_External_Link_In_Url()");
			var __logStart = System.Diagnostics.Stopwatch.StartNew();
            var (connector, userId, username, _) = await this.PrepareNewUser();

            // POST JOURNAL
            var postData = new
            {
                text = $"{username} Post",
                profileId = userId,
                groupId = -1,
                journalType = "file",
                securitySet = "E",
                itemData = $"{{\"ImageUrl\":\"\",\"Url\":\"http://www.dnnsoftware.com\", \"Title\": \"Test.png\"}}",
            };
				Logger.LogDebug("Journal_Should_Not_Able_To_Post_External_Link_In_Url(): text → {Text}", text);
				Logger.LogDebug("Journal_Should_Not_Able_To_Post_External_Link_In_Url(): profileId → {ProfileId}", profileId);
				Logger.LogDebug("Journal_Should_Not_Able_To_Post_External_Link_In_Url(): groupId → {GroupId}", groupId);
				Logger.LogDebug("Journal_Should_Not_Able_To_Post_External_Link_In_Url(): journalType → {JournalType}", journalType);
				Logger.LogDebug("Journal_Should_Not_Able_To_Post_External_Link_In_Url(): securitySet → {SecuritySet}", securitySet);

            connector.PostJson("/API/Journal/Services/Create", postData, this.GetRequestHeaders());

            var itemData = DatabaseHelper.ExecuteScalar<string>($"SELECT ItemData FROM {{objectQualifier}}Journal WHERE UserId = {userId}");
            var url = Json.Deserialize<dynamic>(itemData).Url.ToString();
			Logger.LogDebug("Journal_Should_Not_Able_To_Post_External_Link_In_Url(): itemData → {ItemData}", itemData);
			Logger.LogDebug("Journal_Should_Not_Able_To_Post_External_Link_In_Url(): url → {Url}", url);
			Logger.LogInformation("Journal_Should_Not_Able_To_Post_External_Link_In_Url(): find query executed in {Arg0} ms", __logStart.ElapsedMilliseconds);

            Assert.That(url.IndexOf("www.dnnsoftware.com"), Is.EqualTo(-1));
			Logger.LogDebug("<<< Exiting Journal_Should_Not_Able_To_Post_External_Link_In_Url()");
        }

        [Test]
        public async Task Journal_Should_Able_See_By_All_When_Set_Security_To_Everyone()
        {
			Logger.LogDebug(">>> Entering Journal_Should_Able_See_By_All_When_Set_Security_To_Everyone()");
            var (connector, userId, username, fileId) = await this.PrepareNewUser();

            // POST JOURNAL
            var journalText = $"{username} Post";
            var postData = new
			Logger.LogDebug("Journal_Should_Able_See_By_All_When_Set_Security_To_Everyone(): journalText → {JournalText}", journalText);
            {
                text = journalText,
                profileId = userId,
                groupId = -1,
                journalType = "file",
                securitySet = "E",
                itemData = $"{{\"ImageUrl\":\"\",\"Url\":\"fileid={fileId}\"}}",
            };
				Logger.LogDebug("Journal_Should_Able_See_By_All_When_Set_Security_To_Everyone(): text → {Text}", text);
				Logger.LogDebug("Journal_Should_Able_See_By_All_When_Set_Security_To_Everyone(): profileId → {ProfileId}", profileId);
				Logger.LogDebug("Journal_Should_Able_See_By_All_When_Set_Security_To_Everyone(): groupId → {GroupId}", groupId);
				Logger.LogDebug("Journal_Should_Able_See_By_All_When_Set_Security_To_Everyone(): journalType → {JournalType}", journalType);
				Logger.LogDebug("Journal_Should_Able_See_By_All_When_Set_Security_To_Everyone(): securitySet → {SecuritySet}", securitySet);
				Logger.LogDebug("Journal_Should_Able_See_By_All_When_Set_Security_To_Everyone(): itemData → {ItemData}", itemData);

            connector.PostJson("/API/Journal/Services/Create", postData, this.GetRequestHeaders());

            var response = connector.GetContent($"/Activity-Feed/userId/{userId}").Content.ReadAsStringAsync().Result;
            Assert.That(response.IndexOf(journalText), Is.GreaterThan(0));
			Logger.LogDebug("Journal_Should_Able_See_By_All_When_Set_Security_To_Everyone(): response → {Response}", response);

            connector.Logout();
            response = connector.GetContent($"/Activity-Feed/userId/{userId}").Content.ReadAsStringAsync().Result;
            Assert.That(response.IndexOf(journalText), Is.GreaterThan(0));
			Logger.LogDebug("<<< Exiting Journal_Should_Able_See_By_All_When_Set_Security_To_Everyone()");
        }

        [Test]
        public async Task Journal_Should_Only_Able_See_By_Members_When_Set_Security_To_CommunityMembers()
        {
			Logger.LogDebug(">>> Entering Journal_Should_Only_Able_See_By_Members_When_Set_Security_To_CommunityMembers()");
            var (connector1, userId1, username1, fileId1) = await this.PrepareNewUser();
            var (connector2, _, _, _) = await this.PrepareNewUser();

            // POST JOURNAL
            var journalText = $"{username1} Post";
            var postData = new
			Logger.LogDebug("Journal_Should_Only_Able_See_By_Members_When_Set_Security_To_CommunityMembers(): journalText → {JournalText}", journalText);
            {
                text = journalText,
                profileId = userId1,
                groupId = -1,
                journalType = "file",
                securitySet = "C",
                itemData = $"{{\"ImageUrl\":\"\",\"Url\":\"fileid={fileId1}\"}}",
            };
				Logger.LogDebug("Journal_Should_Only_Able_See_By_Members_When_Set_Security_To_CommunityMembers(): text → {Text}", text);
				Logger.LogDebug("Journal_Should_Only_Able_See_By_Members_When_Set_Security_To_CommunityMembers(): profileId → {ProfileId}", profileId);
				Logger.LogDebug("Journal_Should_Only_Able_See_By_Members_When_Set_Security_To_CommunityMembers(): groupId → {GroupId}", groupId);
				Logger.LogDebug("Journal_Should_Only_Able_See_By_Members_When_Set_Security_To_CommunityMembers(): journalType → {JournalType}", journalType);
				Logger.LogDebug("Journal_Should_Only_Able_See_By_Members_When_Set_Security_To_CommunityMembers(): securitySet → {SecuritySet}", securitySet);
				Logger.LogDebug("Journal_Should_Only_Able_See_By_Members_When_Set_Security_To_CommunityMembers(): itemData → {ItemData}", itemData);

            connector1.PostJson("/API/Journal/Services/Create", postData, this.GetRequestHeaders());

            var response = connector2.GetContent($"/Activity-Feed/userId/{userId1}").Content.ReadAsStringAsync().Result;
            Assert.That(response.IndexOf(journalText), Is.GreaterThan(0));
			Logger.LogDebug("Journal_Should_Only_Able_See_By_Members_When_Set_Security_To_CommunityMembers(): response → {Response}", response);

            connector2.Logout();
            response = connector2.GetContent($"/Activity-Feed/userId/{userId1}").Content.ReadAsStringAsync().Result;
            Assert.That(response.IndexOf(journalText), Is.EqualTo(-1));
			Logger.LogDebug("<<< Exiting Journal_Should_Only_Able_See_By_Members_When_Set_Security_To_CommunityMembers()");
        }

        [Test]
        public async Task Journal_Should_Only_Able_See_By_Friends_When_Set_Security_To_Friends()
        {
			Logger.LogDebug(">>> Entering Journal_Should_Only_Able_See_By_Friends_When_Set_Security_To_Friends()");
			var __logStart = System.Diagnostics.Stopwatch.StartNew();
            var (connector1, userId1, username1, fileId1) = await this.PrepareNewUser();
            var (connector2, userId2, _, _) = await this.PrepareNewUser();
            var (connector3, _, _, _) = await this.PrepareNewUser();

            // ADD FRIENDS
            connector1.PostJson("/API/MemberDirectory/MemberDirectory/AddFriend", new { friendId = userId2 }, this.GetRequestHeaders("Member Directory"));

            var notificationId = DatabaseHelper.ExecuteScalar<int>($"SELECT TOP 1 MessageID FROM {{objectQualifier}}CoreMessaging_Messages WHERE SenderUserID = {userId1}");
            connector2.PostJson("/API/InternalServices/RelationshipService/AcceptFriend", new { NotificationId = notificationId }, this.GetRequestHeaders());
			Logger.LogDebug("Journal_Should_Only_Able_See_By_Friends_When_Set_Security_To_Friends(): notificationId → {NotificationId}", notificationId);
			Logger.LogInformation("Journal_Should_Only_Able_See_By_Friends_When_Set_Security_To_Friends(): find query executed in {Arg0} ms", __logStart.ElapsedMilliseconds);

            // POST JOURNAL
            var journalText = $"{username1} Post";
            var postData = new
			Logger.LogDebug("Journal_Should_Only_Able_See_By_Friends_When_Set_Security_To_Friends(): journalText → {JournalText}", journalText);
            {
                text = journalText,
                profileId = userId1,
                groupId = -1,
                journalType = "file",
                securitySet = "F",
                itemData = $"{{\"ImageUrl\":\"\",\"Url\":\"fileid={fileId1}\"}}",
            };
				Logger.LogDebug("Journal_Should_Only_Able_See_By_Friends_When_Set_Security_To_Friends(): text → {Text}", text);
				Logger.LogDebug("Journal_Should_Only_Able_See_By_Friends_When_Set_Security_To_Friends(): profileId → {ProfileId}", profileId);
				Logger.LogDebug("Journal_Should_Only_Able_See_By_Friends_When_Set_Security_To_Friends(): groupId → {GroupId}", groupId);
				Logger.LogDebug("Journal_Should_Only_Able_See_By_Friends_When_Set_Security_To_Friends(): journalType → {JournalType}", journalType);
				Logger.LogDebug("Journal_Should_Only_Able_See_By_Friends_When_Set_Security_To_Friends(): securitySet → {SecuritySet}", securitySet);
				Logger.LogDebug("Journal_Should_Only_Able_See_By_Friends_When_Set_Security_To_Friends(): itemData → {ItemData}", itemData);

            connector1.PostJson("/API/Journal/Services/Create", postData, this.GetRequestHeaders());

            var response = connector2.GetContent($"/Activity-Feed/userId/{userId1}").Content.ReadAsStringAsync().Result;
            Assert.That(response.IndexOf(journalText), Is.GreaterThan(0));
			Logger.LogDebug("Journal_Should_Only_Able_See_By_Friends_When_Set_Security_To_Friends(): response → {Response}", response);

            response = connector3.GetContent($"/Activity-Feed/userId/{userId1}").Content.ReadAsStringAsync().Result;
            Assert.That(response.IndexOf(journalText), Is.EqualTo(-1));
			Logger.LogDebug("<<< Exiting Journal_Should_Only_Able_See_By_Friends_When_Set_Security_To_Friends()");
        }

        [Test]
        public async Task Journal_Should_Only_Able_See_By_Himself_When_Set_Security_To_Private()
        {
			Logger.LogDebug(">>> Entering Journal_Should_Only_Able_See_By_Himself_When_Set_Security_To_Private()");
            var (connector, userId, username, fileId) = await this.PrepareNewUser();

            // POST JOURNAL
            var journalText = $"{username} Post";
            var postData = new
			Logger.LogDebug("Journal_Should_Only_Able_See_By_Himself_When_Set_Security_To_Private(): journalText → {JournalText}", journalText);
            {
                text = journalText,
                profileId = userId,
                groupId = -1,
                journalType = "file",
                securitySet = "P",
                itemData = $"{{\"ImageUrl\":\"\",\"Url\":\"fileid={fileId}\"}}",
            };
				Logger.LogDebug("Journal_Should_Only_Able_See_By_Himself_When_Set_Security_To_Private(): text → {Text}", text);
				Logger.LogDebug("Journal_Should_Only_Able_See_By_Himself_When_Set_Security_To_Private(): profileId → {ProfileId}", profileId);
				Logger.LogDebug("Journal_Should_Only_Able_See_By_Himself_When_Set_Security_To_Private(): groupId → {GroupId}", groupId);
				Logger.LogDebug("Journal_Should_Only_Able_See_By_Himself_When_Set_Security_To_Private(): journalType → {JournalType}", journalType);
				Logger.LogDebug("Journal_Should_Only_Able_See_By_Himself_When_Set_Security_To_Private(): securitySet → {SecuritySet}", securitySet);
				Logger.LogDebug("Journal_Should_Only_Able_See_By_Himself_When_Set_Security_To_Private(): itemData → {ItemData}", itemData);

            connector.PostJson("/API/Journal/Services/Create", postData, this.GetRequestHeaders());

            var response = connector.GetContent($"/Activity-Feed/userId/{userId}").Content.ReadAsStringAsync().Result;
            Assert.That(response.IndexOf(journalText), Is.GreaterThan(0));
			Logger.LogDebug("Journal_Should_Only_Able_See_By_Himself_When_Set_Security_To_Private(): response → {Response}", response);

            var hostConnector = WebApiTestHelper.LoginHost();
            response = hostConnector.GetContent($"/Activity-Feed/userId/{userId}").Content.ReadAsStringAsync().Result;
			Logger.LogDebug("Journal_Should_Only_Able_See_By_Himself_When_Set_Security_To_Private(): hostConnector → {HostConnector}", hostConnector);
            Assert.That(response.IndexOf(journalText), Is.EqualTo(-1));
			Logger.LogDebug("<<< Exiting Journal_Should_Only_Able_See_By_Himself_When_Set_Security_To_Private()");
        }

        private async Task<(IWebApiConnector Connector, int UserId, string Username, int FileId)> PrepareNewUser()
        {
            return await WebApiTestHelper.PrepareNewUser(this.PortalId);
        }

        private int CreateNewGroup(string roleName)
        {
			Logger.LogDebug(">>> Entering CreateNewGroup(roleName={RoleName})", roleName);
            var connector = WebApiTestHelper.LoginHost();

			Logger.LogDebug("CreateNewGroup(roleName={RoleName}): connector → {Connector}", roleName, connector);
			var __logStart = System.Diagnostics.Stopwatch.StartNew();
            var url = "/API/PersonaBar/Roles/SaveRole?assignExistUsers=false";
            connector.PostJson(url, new
            {
			Logger.LogDebug("CreateNewGroup(roleName={RoleName}): url → {Url}", roleName, url);
                id = -1,
                name = roleName,
                groupId = -1,
                description = roleName,
                securityMode = 1,
                status = 1,
                isPublic = true,
                autoAssign = false,
                isSystem = false,
            });
				Logger.LogDebug("CreateNewGroup(roleName={RoleName}): name → {Name}", roleName, name);
				Logger.LogDebug("CreateNewGroup(roleName={RoleName}): groupId → {GroupId}", roleName, groupId);
				Logger.LogDebug("CreateNewGroup(roleName={RoleName}): description → {Description}", roleName, description);
				Logger.LogDebug("CreateNewGroup(roleName={RoleName}): securityMode → {SecurityMode}", roleName, securityMode);
				Logger.LogDebug("CreateNewGroup(roleName={RoleName}): status → {Status}", roleName, status);
				Logger.LogDebug("CreateNewGroup(roleName={RoleName}): isPublic → {IsPublic}", roleName, isPublic);
				Logger.LogDebug("CreateNewGroup(roleName={RoleName}): autoAssign → {AutoAssign}", roleName, autoAssign);
				Logger.LogDebug("CreateNewGroup(roleName={RoleName}): isSystem → {IsSystem}", roleName, isSystem);

			Logger.LogDebug("<<< Exiting CreateNewGroup(roleName={RoleName})", roleName);
            return DatabaseHelper.ExecuteScalar<int>($"SELECT RoleId FROM {{objectQualifier}}Roles WHERE RoleName = '{roleName}' AND PortalId = {this.PortalId}");
        }

        private void AddUserToGroup(int groupId, int userId)
        {
            var connector = WebApiTestHelper.LoginHost();

            var url = "/API/PersonaBar/Roles/AddUserToRole?notifyUser=true&isOwner=false";
            connector.PostJson(url, new
            {
                userId = userId,
                roleId = groupId,
                isAdd = true,
            });
        }

        private IDictionary<string, string> GetRequestHeaders(string moduleName = "Journal")
        {
            return WebApiTestHelper.GetRequestHeaders("//ActivityFeed", moduleName, this.PortalId);
        }
    }
}
