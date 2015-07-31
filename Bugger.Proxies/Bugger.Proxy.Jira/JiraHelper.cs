using Bugger.Domain.Models;
using Bugger.Proxy.Jira.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;

namespace Bugger.Proxy.Jira
{
    [Export]
    public class JiraHelper
    {
        internal bool TryConnection(Uri connectUri, string userName, string password)
        {
            return false;
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

        internal IList<Bug> GetBugs(string userName, bool isFilterCreatedBy, PropertyMappingDictionary propertyMappingCollection, string bugFilterField, string bugFilterValue, List<string> redFilter)
        {
            return new List<Bug>
            {
                new Bug
                {
                    ID = 123,
                    AssignedTo = "alibad",
                    ChangedDate = DateTime.Now,
                    CreatedBy = "falihee",
                    Description = "this is a bug",
                    Priority = "Medium",
                    Severity = "Dang",
                    State = "Happy"
                },
                new Bug
                {
                    ID = 111,
                    AssignedTo = "falihee",
                    ChangedDate = DateTime.Now,
                    CreatedBy = "falihee",
                    Description = "this is a task",
                    Priority = "High",
                    Severity = "Dang",
                    State = "Happy"
                }
            };
        }
    }
}
