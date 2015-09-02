using Bugger.Domain.Models;
using Bugger.Proxy.Jira.Models;
using System;
using System.Linq;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using SDK = Jira.SDK;

namespace Bugger.Proxy.Jira
{
    [Export]
    public class JiraHelper
    {
        internal bool TryConnection(Uri connectUri, string userName, string password, out JiraClientWrapper jiraClientWrapper)
        {
            try
            {
                jiraClientWrapper = new JiraClientWrapper(connectUri.AbsoluteUri, userName, password);
                jiraClientWrapper.Jira.Connect(connectUri.AbsoluteUri, userName, password);
                return true;
            }
            catch
            {
                jiraClientWrapper = null;
                return false;
            }
        }

        internal IList<JiraField> GetFields()
        {
            var fields = typeof(SDK.Domain.IssueFields).GetProperties().Select(i => new JiraField(i.Name)).ToList();
            fields.Add(new JiraField("Key"));
            fields.Add(new JiraField("Severity"));
            return fields;
        }

        internal IList<Bug> GetBugs(SDK.Jira jira, string userName, bool isFilterCreatedBy, PropertyMappingDictionary propertyMappingCollection, string jqlQuery, List<string> redFilter)
        {
            var filter = new SDK.Domain.IssueFilter();
            filter.JQL = jqlQuery.Replace("{username}", userName);
            filter.SetJira(jira);
            var issues = filter.GetIssues();

            return issues.Select(i =>
                new Bug
                {
                    ID = i.Key,
                    AssignedTo = i.Assignee.Username,
                    CreatedBy = i.Reporter.Username,
                    Description = i.Description,
                    Severity = i.Severity,
                    State = i.Status.Name,
                    Title = i.Summary,
                    Priority = i.Fields.Priority.Name,
                    Type = redFilter.Contains(i.Fields.Priority.Name) ? BugType.Red : BugType.Yellow,
                    ChangedDate = i.Updated
                }).ToList();
        }
    }
}
