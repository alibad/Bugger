using BigEgg.Framework.Applications.ViewModels;
using Bugger.Domain.Models;
using Bugger.Proxy.Jira.Models;
using Bugger.Proxy.Jira.Views;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Input;

namespace Bugger.Proxy.Jira.ViewModels
{
    public class JiraSettingViewModel : ViewModel<IJiraSettingView>
    {
        #region Fields
        private readonly ObservableCollection<JiraField> jiraFields;
        private readonly ObservableCollection<JiraField> bugFilterFields;
        private readonly ObservableCollection<CheckString> priorityValues;
        private readonly PropertyMappingDictionary propertyMappingCollection;
        private Uri connectUri;
        private string userName;
        private string password;
        private string bugFilterField;
        private string bugFilterValue;
        private string priorityRed;
        private string jqlQuery;

        private ICommand testConnectionCommand;

        private ProgressTypes progressType;
        private int progressValue;
        #endregion

        public JiraSettingViewModel(IJiraSettingView view)
            : base(view)
        {
            this.propertyMappingCollection = new PropertyMappingDictionary();
            this.jiraFields = new ObservableCollection<JiraField>();
            this.bugFilterFields = new ObservableCollection<JiraField>();
            this.priorityValues = new ObservableCollection<CheckString>()
            {
                new CheckString("None"),
                new CheckString("Low"),
                new CheckString("Medium"),
                new CheckString("High")
            };

            PropertyDescriptorCollection propertyDescriptorCollection = TypeDescriptor.GetProperties(typeof(Bug));
            foreach (PropertyDescriptor propertyDescriptor in propertyDescriptorCollection)
            {
                if (propertyDescriptor.Name == "Type") continue;

                var mapping = new MappingModel(propertyDescriptor.Name);
                this.propertyMappingCollection.Add(mapping);

                AddWeakEventListener(mapping, PropertyMappingModelPropertyChanged);
            }

            ClearMappingData();
        }

        public ObservableCollection<JiraField> JiraFields { get { return this.jiraFields; } }

        public ObservableCollection<JiraField> BugFilterFields { get { return this.bugFilterFields; } }

        public ObservableCollection<CheckString> PriorityValues { get { return this.priorityValues; } }

        public PropertyMappingDictionary PropertyMappingCollection { get { return this.propertyMappingCollection; } }

        public Uri ConnectUri
        {
            get { return this.connectUri; }
            set
            {
                if (this.connectUri != value)
                {
                    this.connectUri = value;
                    RaisePropertyChanged("ConnectUri");
                }
            }
        }

        public string UserName
        {
            get { return this.userName; }
            set
            {
                if (this.userName != value)
                {
                    this.userName = value;
                    RaisePropertyChanged("UserName");
                }
            }
        }

        public string Password
        {
            get { return this.password; }
            set
            {
                if (this.password != value)
                {
                    this.password = value;
                    RaisePropertyChanged("Password");
                }
            }
        }

        public string BugFilterField
        {
            get { return this.bugFilterField; }
            set
            {
                if (this.bugFilterField != value)
                {
                    this.bugFilterField = value;
                    RaisePropertyChanged("BugFilterField");
                }
            }
        }

        public string BugFilterValue
        {
            get { return this.bugFilterValue; }
            set
            {
                if (this.bugFilterValue != value)
                {
                    this.bugFilterValue = value;
                    RaisePropertyChanged("BugFilterValue");
                }
            }
        }

        public string JqlQuery
        {
            get { return this.jqlQuery; }
            set
            {
                if (this.jqlQuery != value)
                {
                    this.jqlQuery = value;
                    RaisePropertyChanged("JqlQuery");
                }
            }
        }

        public string PriorityRed
        {
            get { return this.priorityRed; }
            set
            {
                if (this.priorityRed != value)
                {
                    this.priorityRed = value;
                    RaisePropertyChanged("PriorityRed");
                }
            }
        }

        public ICommand TestConnectionCommand
        {
            get { return this.testConnectionCommand; }
            internal set
            {
                if (this.testConnectionCommand != value)
                {
                    this.testConnectionCommand = value;
                    RaisePropertyChanged("TestConnectionCommand");
                }
            }
        }

        public ProgressTypes ProgressType
        {
            get { return this.progressType; }
            set
            {
                if (this.progressType != value)
                {
                    this.progressType = value;
                    RaisePropertyChanged("ProgressType");
                }
            }
        }

        public int ProgressValue
        {
            get { return this.progressValue; }
            set
            {
                if (this.progressValue != value)
                {
                    this.progressValue = value;
                    RaisePropertyChanged("ProgressValue");
                }
            }
        }

        public void ClearMappingData()
        {
            JiraFields.Clear();
            BugFilterFields.Clear();
            PriorityValues.Clear();

            foreach (var mapping in PropertyMappingCollection)
            {
                mapping.Value = string.Empty;
            }

            BugFilterField = string.Empty;
            JqlQuery = JqlQueries.AssignedToMeInOpensprints;
            BugFilterValue = string.Empty;
            PriorityRed = string.Empty;

            JqlQuery = "assignee = {userame} and Sprint in openSprints()";

            ProgressType = ProgressTypes.NotWorking;
            ProgressValue = 0;
        }

        private void PropertyMappingModelPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            RaisePropertyChanged("PropertyMappingCollection");
        }
    }
}
