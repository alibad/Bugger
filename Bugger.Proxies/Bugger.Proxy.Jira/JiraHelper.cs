using Bugger.Domain.Models;
using Bugger.Proxy.Jira.Models;
using System;
using System.Linq;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using Jira.SDK.Domain;
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
            fields.Add(new JiraField("None"));
            return fields;
        }

        internal IList<Bug> GetBugs(SDK.Jira jira, string userName, bool isFilterCreatedBy, PropertyMappingDictionary propertyMappingList, string jqlQuery, List<string> redFilter)
        {
            if (jira == null) { throw new ArgumentNullException("jira"); }
            if (string.IsNullOrWhiteSpace(userName)) { throw new ArgumentNullException("userName"); }
            if (propertyMappingList == null) { throw new ArgumentNullException("propertyMappingList"); }
            if (string.IsNullOrWhiteSpace(jqlQuery)) { throw new ArgumentNullException("jqlQuery"); }
            if (redFilter == null) { throw new ArgumentNullException("redFilter"); }

            var filter = new SDK.Domain.IssueFilter();
            filter.JQL = jqlQuery.Replace("{username}", userName);
            filter.SetJira(jira);

            var issues = filter.GetIssues();

            return issues.Select(issue => Map(issue, propertyMappingList, redFilter)).ToList();
        }

        /// <summary>
        /// Maps the specified issue to the bug model.
        /// </summary>
        /// <param name="issue">The issue.</param>
        /// <param name="propertyMappingList">The property mapping list.</param>
        /// <param name="redFilter">The red bug filter.</param>
        /// <returns></returns>
        private Bug Map(Issue issue, PropertyMappingDictionary propertyMappingList,
                        IEnumerable<string> redFilter)
        {
            var valuesToMap = new Dictionary<string, object>();

            valuesToMap.Add("Key", issue.Key);
            valuesToMap.Add("Assignee", issue.Assignee.Username);
            valuesToMap.Add("Reporter", issue.Reporter.Username);
            valuesToMap.Add("Description", issue.Description);
            valuesToMap.Add("Severity", issue.Severity);
            valuesToMap.Add("Status", issue.Status.Name);
            valuesToMap.Add("Summary", issue.Summary);
            valuesToMap.Add("Priority", issue.Fields.Priority.Name);
            valuesToMap.Add("Updated", issue.Updated);

            Bug bug = new Bug();

            //  ID
            bug.ID = MapValue<string>("ID", valuesToMap, propertyMappingList);

            //  Title
            bug.Title = MapValue<string>("Title", valuesToMap, propertyMappingList);

            //  Description
            bug.Description = MapValue<string>("Description", valuesToMap, propertyMappingList);

            //  AssignedTo
            bug.AssignedTo = MapValue<string>("AssignedTo", valuesToMap, propertyMappingList);

            //  State
            bug.State = MapValue<string>("State", valuesToMap, propertyMappingList);

            //  ChangedDate
            bug.ChangedDate = MapValue<DateTime?>("ChangedDate", valuesToMap, propertyMappingList); ;

            //  CreatedBy
            bug.CreatedBy = MapValue<string>("CreatedBy", valuesToMap, propertyMappingList);

            //  Priority
            bug.Priority = MapValue<string>("Priority", valuesToMap, propertyMappingList);

            //  Severity
            bug.Severity = MapValue<string>("Severity", valuesToMap, propertyMappingList);

            bug.Type = string.IsNullOrWhiteSpace(bug.Priority)
                           ? BugType.Yellow
                           : (redFilter.Contains(bug.Priority) ? BugType.Red : BugType.Yellow);

            return bug;
        }

        private T MapValue<T>(string key, Dictionary<string, object> valuesToMap, PropertyMappingDictionary propertyMappingList)
        {
            if (!propertyMappingList.ContainsKey(key)
                || string.IsNullOrWhiteSpace(propertyMappingList[key])
                || propertyMappingList[key] == "None"
                || !valuesToMap.ContainsKey(propertyMappingList[key]))
            {
                return default(T);
            }

            object value = valuesToMap[propertyMappingList[key]];
            return value == null ? default(T) : (T)value;
        }
    }
}
