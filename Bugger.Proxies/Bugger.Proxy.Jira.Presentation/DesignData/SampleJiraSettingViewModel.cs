﻿using BigEgg.Framework.Presentation;
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
            this.ConnectUri = new Uri("https://jira.practicefusion.com");
            this.BugFilterField = "Work Item Type";
            this.JqlQuery = JqlQueries.AssignedToMeInOpensprints;
            this.BugFilterValue = "Bugs";
            this.UserName = "abadereddin";
            this.Password = "Password";
            this.JqlQuery = "assignee = {userame} and Sprint in openSprints()";

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