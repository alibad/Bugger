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
        internal bool TryConnection(Uri connectUri, string userName, string password, out SDK.Jira jira)
        {
            try
            {
                jira = new SDK.Jira();
                jira.Connect(connectUri.AbsoluteUri, userName, password);
                return true;
            }
            catch
            {
                jira = null;
                return false;
            }
        }

        internal IList<JiraField> GetFields()
        {
            return new List<JiraField>
            {
                new JiraField("ID"),
                new JiraField("Title"),
                new JiraField("Description"),
                new JiraField("Assigned To"),
            };
        }

        internal IList<Bug> GetBugs(SDK.Jira jira, string userName, bool isFilterCreatedBy, PropertyMappingDictionary propertyMappingCollection, string bugFilterField, string bugFilterValue, List<string> redFilter)
        {
            var filter = new SDK.Domain.IssueFilter();
            filter.JQL = $"assignee = {userName} and Sprint in openSprints()";
            filter.SetJira(jira);
            var issues = filter.GetIssues();

            return issues.Select(i =>
                new Bug
                {
                    ID = i.Key.GetHashCode(),
                    AssignedTo = i.Assignee.Username,
                    CreatedBy = i.Reporter.Username,
                    Description = i.Description,
                    Severity = i.Severity,
                    State = i.Status.Name,
                    Title = i.Summary,
                }).ToList();
        }
    }
}
