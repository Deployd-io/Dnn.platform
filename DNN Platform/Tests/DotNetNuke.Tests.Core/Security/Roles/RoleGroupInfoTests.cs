// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Tests.Core.Security.Roles;

using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Xml;

using DotNetNuke.Security.Roles;

using NUnit.Framework;

[TestFixture]
public class RoleGroupInfoTests
{
    [Test]
    public void ReadXml_WithRealisticXml_ReadsRoles()
    {
		mockEventLogger.LogDebug(">>> Entering ReadXml_WithRealisticXml_ReadsRoles()");
        var roleGroup = ReadXml(
            """
            <rolegroups>
              <rolegroup>
                <rolegroupname>GlobalRoles</rolegroupname>
                <description>A dummy role group that represents the Global roles</description>
                <roles>
                  <role>
                    <rolename>Administrators</rolename>
                    <description>Administrators of this Website</description>
                    <billingfrequency>N</billingfrequency>
                    <billingperiod>-1</billingperiod>
                    <servicefee>0</servicefee>
                    <trialfrequency>N</trialfrequency>
                    <trialperiod>-1</trialperiod>
                    <trialfee>0</trialfee>
                    <ispublic>false</ispublic>
                    <autoassignment>false</autoassignment>
                    <rsvpcode />
                    <iconfile />
                    <issystemrole>true</issystemrole>
                    <roletype>adminrole</roletype>
                    <securitymode>securityrole</securitymode>
                    <status>approved</status>
                  </role>
                  <role>
                    <rolename>Registered Users</rolename>
                    <description>Registered Users</description>
                    <billingfrequency>N</billingfrequency>
                    <billingperiod>-1</billingperiod>
                    <servicefee>0</servicefee>
                    <trialfrequency>N</trialfrequency>
                    <trialperiod>-1</trialperiod>
                    <trialfee>0</trialfee>
                    <ispublic>false</ispublic>
                    <autoassignment>true</autoassignment>
                    <rsvpcode />
                    <iconfile />
                    <issystemrole>true</issystemrole>
                    <roletype>registeredrole</roletype>
                    <securitymode>securityrole</securitymode>
                    <status>approved</status>
                  </role>
                  <role>
                    <rolename>Subscribers</rolename>
                    <description>A public role for site subscriptions</description>
                    <billingfrequency>N</billingfrequency>
                    <billingperiod>-1</billingperiod>
                    <servicefee>0</servicefee>
                    <trialfrequency>N</trialfrequency>
                    <trialperiod>-1</trialperiod>
                    <trialfee>0</trialfee>
                    <ispublic>true</ispublic>
                    <autoassignment>true</autoassignment>
                    <rsvpcode />
                    <iconfile />
                    <issystemrole>true</issystemrole>
                    <roletype>subscriberrole</roletype>
                    <securitymode>securityrole</securitymode>
                    <status>approved</status>
                  </role>
                  <role>
                    <rolename>Translator (en-US)</rolename>
                    <description>A role for English (United States) translators</description>
                    <billingfrequency>N</billingfrequency>
                    <billingperiod>-1</billingperiod>
                    <servicefee>0</servicefee>
                    <trialfrequency>N</trialfrequency>
                    <trialperiod>-1</trialperiod>
                    <trialfee>0</trialfee>
                    <ispublic>false</ispublic>
                    <autoassignment>false</autoassignment>
                    <rsvpcode />
                    <iconfile />
                    <issystemrole>false</issystemrole>
                    <roletype>none</roletype>
                    <securitymode>securityrole</securitymode>
                    <status>approved</status>
                  </role>
                  <role>
                    <rolename>Unverified Users</rolename>
                    <description>Unverified Users</description>
                    <billingfrequency>N</billingfrequency>
                    <billingperiod>-1</billingperiod>
                    <servicefee>0</servicefee>
                    <trialfrequency>N</trialfrequency>
                    <trialperiod>-1</trialperiod>
                    <trialfee>0</trialfee>
                    <ispublic>false</ispublic>
                    <autoassignment>false</autoassignment>
                    <rsvpcode />
                    <iconfile />
                    <issystemrole>true</issystemrole>
                    <roletype>unverifiedrole</roletype>
                    <securitymode>securityrole</securitymode>
                    <status>approved</status>
                  </role>
                </roles>
              </rolegroup>
            </rolegroups>
            """);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(roleGroup.RoleGroupName, Is.EqualTo("GlobalRoles"));
            Assert.That(roleGroup.Description, Is.EqualTo("A dummy role group that represents the Global roles"));
            Assert.That(roleGroup.Roles, Has.Count.EqualTo(5));
        }
		mockEventLogger.LogDebug("<<< Exiting ReadXml_WithRealisticXml_ReadsRoles()");
    }

    [Test]
    public void ReadXml_WithEmptyRoles_ReadsZeroRoles()
    {
		mockEventLogger.LogDebug(">>> Entering ReadXml_WithEmptyRoles_ReadsZeroRoles()");
        var roleGroup = ReadXml(
            """
            <rolegroups>
                <rolegroupname>N</rolegroupname>
                <description>D</description>
                <roles></roles>
            </rolegroups>
            """);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(roleGroup.RoleGroupName, Is.EqualTo("N"));
            Assert.That(roleGroup.Description, Is.EqualTo("D"));
            Assert.That(roleGroup.Roles, Is.Null);
        }
		mockEventLogger.LogDebug("<<< Exiting ReadXml_WithEmptyRoles_ReadsZeroRoles()");
    }

    [Test]
    public void ReadXml_WithEmptyRolesWithWhiteSpace_ReadsZeroRoles()
    {
		mockEventLogger.LogDebug(">>> Entering ReadXml_WithEmptyRolesWithWhiteSpace_ReadsZeroRoles()");
        var roleGroup = ReadXml(
            """
            <rolegroups>
                <rolegroupname>N</rolegroupname>
                <description>D</description>
                <roles>
                </roles>
            </rolegroups>
            """);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(roleGroup.RoleGroupName, Is.EqualTo("N"));
            Assert.That(roleGroup.Description, Is.EqualTo("D"));
            Assert.That(roleGroup.Roles, Is.Null);
        }
		mockEventLogger.LogDebug("<<< Exiting ReadXml_WithEmptyRolesWithWhiteSpace_ReadsZeroRoles()");
    }

    [Test]
    public void ReadXml_WithSelfClosingRoles_ReadsZeroRoles()
    {
		mockEventLogger.LogDebug(">>> Entering ReadXml_WithSelfClosingRoles_ReadsZeroRoles()");
        var roleGroup = ReadXml(
            """
            <rolegroups>
                <rolegroupname>N</rolegroupname>
                <description>D</description>
                <roles />
            </rolegroups>
            """);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(roleGroup.RoleGroupName, Is.EqualTo("N"));
            Assert.That(roleGroup.Description, Is.EqualTo("D"));
            Assert.That(roleGroup.Roles, Is.Null);
        }
		mockEventLogger.LogDebug("<<< Exiting ReadXml_WithSelfClosingRoles_ReadsZeroRoles()");
    }

    [Test]
    public void ReadXml_WithNoRoles_ReadsZeroRoles()
    {
		mockEventLogger.LogDebug(">>> Entering ReadXml_WithNoRoles_ReadsZeroRoles()");
        var roleGroup = ReadXml(
            """
            <rolegroups>
                <rolegroupname>N</rolegroupname>
                <description>D</description>
            </rolegroups>
            """);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(roleGroup.RoleGroupName, Is.EqualTo("N"));
            Assert.That(roleGroup.Description, Is.EqualTo("D"));
            Assert.That(roleGroup.Roles, Is.Null);
        }
		mockEventLogger.LogDebug("<<< Exiting ReadXml_WithNoRoles_ReadsZeroRoles()");
    }

    private static RoleGroupInfo ReadXml([StringSyntax(StringSyntaxAttribute.Xml)] string xml)
    {
		mockEventLogger.LogDebug(">>> Entering ReadXml(xml={Xml})", xml);
        var roleGroup = new RoleGroupInfo();

		mockEventLogger.LogDebug("ReadXml(xml={Xml}): roleGroup → {RoleGroup}", xml, roleGroup);
        using var textReader = new StringReader(xml);
		mockEventLogger.LogDebug("ReadXml(xml={Xml}): textReader → {TextReader}", xml, textReader);
        using var xmlReader = XmlReader.Create(textReader);
		mockEventLogger.LogDebug("ReadXml(xml={Xml}): xmlReader → {XmlReader}", xml, xmlReader);
        xmlReader.Read();
        roleGroup.ReadXml(xmlReader);

		mockEventLogger.LogDebug("<<< Exiting ReadXml(xml={Xml})", xml);
        return roleGroup;
    }
}
