using BigEgg.Framework.Presentation;
using Bugger.Proxy.Jira.Models;
using Bugger.Proxy.Jira.Presentation.Properties;
using Bugger.Proxy.Jira.ViewModels;
using Bugger.Proxy.Jira.Views;
using System;
using System.Linq;

namespace Bugger.Proxy.Jira.Presentation.DesignData
{
    public class SampleJiraSettingViewModel : JiraSettingViewModel
    {
        public SampleJiraSettingViewModel()
            : base(new MockJiraSettingView())
        {
            this.ConnectUri = new Uri("https://Jira.codeplex.com:443/Jira/Jira12");
            this.BugFilterField = "Work Item Type";
            this.BugFilterValue = "Bugs";
            this.UserName = "BigEgg";
            this.Password = "Password";

            this.PriorityValues.Add(new CheckString("High"));
            this.PriorityValues.Add(new CheckString("Medium"));
            this.PriorityValues.Add(new CheckString("Low"));
            var values = this.PriorityValues.Where(x => x.Name == "High" || x.Name == "Medium");
            foreach (var value in values)
            {
                value.IsChecked = true;
            }
            this.PriorityRed = "High;Medium";

            var field = new JiraField("Work Item Type");
            field.AllowedValues.Add("Work Item");
            field.AllowedValues.Add("Bugs");

            var idField = new JiraField("ID");

            this.JiraFields.Add(idField);
            this.JiraFields.Add(field);

            this.PropertyMappingCollection["ID"] = "ID";

            this.BugFilterFields.Add(field);
            this.ProgressType = ProgressTypes.SuccessWithError;
            this.ProgressValue = 100;
        }

        private class MockJiraSettingView : MockView, IJiraSettingView
        {
            public string Title { get { return Resources.SettingViewTitle; } }
        }
    }
}
