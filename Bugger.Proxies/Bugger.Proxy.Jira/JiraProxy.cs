using BigEgg.Framework.Applications.Commands;
using BigEgg.Framework.Applications.Services;
using Bugger.Domain.Models;
using Bugger.Proxy.Jira.Documents;
using Bugger.Proxy.Jira.Models;
using Bugger.Proxy.Jira.Properties;
using Bugger.Proxy.Jira.ViewModels;
using Bugger.Proxy.Jira.Views;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using SDK = Jira.SDK;

namespace Bugger.Proxy.Jira
{
    [Export(typeof(ITracingSystemProxy)), Export]
    public class JiraProxy : TracingSystemProxyBase
    {
        private readonly CompositionContainer container;
        private readonly IMessageService messageService;
        private readonly JiraHelper jiraHelper;
        private SettingDocument document;
        private JiraSettingViewModel settingViewModel;

        private readonly ObservableCollection<string> stateValues;
        private List<string> ignoreField;
        private const string PriorityRedSeparator = ";";
        private readonly List<JiraField> jiraFieldsCache;
        private readonly List<string> priorityValues;

        private readonly DelegateCommand testConnectionCommand;

        private bool statusTypesLoaded = false;

        /// <summary>
        /// Initializes a new instance of the <see cref="JiraProxy" /> class.
        /// </summary>
        [ImportingConstructor]
        public JiraProxy(CompositionContainer container, IMessageService messageService, JiraHelper jiraHelper)
            : base(Resources.ProxyName)
        {
            if (container == null) { throw new ArgumentNullException("container"); }
            if (messageService == null) { throw new ArgumentNullException("messageService"); }
            if (jiraHelper == null) { throw new ArgumentNullException("jiraHelper"); }

            this.container = container;
            this.messageService = messageService;
            this.jiraHelper = jiraHelper;

            this.testConnectionCommand = new DelegateCommand(TestConnectionCommandExcute, CanTestConnectionCommandExcute);

            this.jiraFieldsCache = new List<JiraField>();
            this.stateValues = new ObservableCollection<string>();
            this.priorityValues = new List<string>() { "None", "Low", "Medium", "High" };

            this.CanQuery = false;
        }
        /// <summary>
        /// Gets the status values.
        /// </summary>
        /// <value>
        /// The status values.
        /// </value>
        public override ObservableCollection<string> StateValues { get { return this.stateValues; } }

        /// <summary>
        /// Initializes the setting dialog.
        /// </summary>
        /// <returns></returns>
        public override ISettingView InitializeSettingDialog()
        {
            if (this.settingViewModel == null)
            {
                IJiraSettingView view = this.container.GetExportedValue<IJiraSettingView>();
                this.settingViewModel = new JiraSettingViewModel(view);
                this.settingViewModel.TestConnectionCommand = this.testConnectionCommand;
            }

            RemoveWeakEventListener(this.settingViewModel, SettingViewModelPropertyChanged);
            this.settingViewModel.ClearMappingData();

            var userName = this.document.UserName;

            if(string.IsNullOrEmpty(userName))
            {
                userName = Environment.UserName;
            }

            this.settingViewModel.ConnectUri = this.document.ConnectUri ?? new Uri("https://jira.practicefusion.com");
            this.settingViewModel.UserName = userName;
            this.settingViewModel.Password = this.document.Password;

            foreach (var mapping in this.document.PropertyMappingCollection)
            {
                this.settingViewModel.PropertyMappingCollection[mapping.Key] = mapping.Value;
            }

            foreach (var field in this.jiraFieldsCache)
            {
                this.settingViewModel.JiraFields.Add(field);
                if (field.AllowedValues.Any())
                {
                    this.settingViewModel.BugFilterFields.Add(field);
                }
            }

            this.settingViewModel.JqlQuery = this.document.JqlQuery;
            this.settingViewModel.PriorityRed = this.document.PriorityRed;

            UpdateSettingDialogPriorityValues();

            if (this.CanQuery)
            {
                if (string.IsNullOrWhiteSpace(this.settingViewModel.JqlQuery)
                    && this.settingViewModel.PropertyMappingCollection
                            .Where(x => !ignoreField.Contains(x.Key))
                            .Any(x =>
                            {
                                return string.IsNullOrWhiteSpace(x.Value);
                            }))
                {
                    this.settingViewModel.ProgressType = ProgressTypes.SuccessWithError;
                }
                else
                {
                    this.settingViewModel.ProgressType = ProgressTypes.Success;
                }
                this.settingViewModel.ProgressValue = 100;
            }

            AddWeakEventListener(this.settingViewModel, SettingViewModelPropertyChanged);

            return this.settingViewModel.View as ISettingView;
        }

        /// <summary>
        /// Do something afters close setting dialog.
        /// </summary>
        /// <param name="submit">If the dialog is return as submit..</param>
        public override void AfterCloseSettingDialog(bool submit)
        {
            RemoveWeakEventListener(this.settingViewModel, SettingViewModelPropertyChanged);

            if (!submit)
            {
                UpdateStateValues();
                return;
            }

            this.document.ConnectUri = this.settingViewModel.ConnectUri;
            this.document.UserName = this.settingViewModel.UserName;
            this.document.Password = this.settingViewModel.Password;

            this.document.PropertyMappingCollection.Clear();
            foreach (var mapping in this.settingViewModel.PropertyMappingCollection)
            {
                this.document.PropertyMappingCollection.Add(mapping.Key, mapping.Value);
            }

            this.jiraFieldsCache.Clear();
            this.jiraFieldsCache.AddRange(this.settingViewModel.JiraFields);

            this.document.JqlQuery = this.settingViewModel.JqlQuery;
            this.document.PriorityRed = this.settingViewModel.PriorityRed;

            this.CanQuery = false;

            if ((this.settingViewModel.ProgressType == ProgressTypes.Success
                || this.settingViewModel.ProgressType == ProgressTypes.SuccessWithError)
                &&
                (!string.IsNullOrWhiteSpace(this.settingViewModel.JqlQuery)
                && this.settingViewModel.PropertyMappingCollection
                         .Where(x => !ignoreField.Contains(x.Key))
                         .Any(x =>
                         {
                             return !string.IsNullOrWhiteSpace(x.Value);
                         })))
            {
                this.CanQuery = true;
            }

            SettingDocumentType.Save(this.document);
        }

        private void UpdateStateValues()
        {
            JiraClientWrapper jiraClientWrapper = null;
            if (!statusTypesLoaded && jiraHelper.TryConnection(this.document.ConnectUri, this.document.UserName, this.document.Password, out jiraClientWrapper))
            {
                statusTypesLoaded = true;
                StateValues.Clear();

                var statuses = jiraClientWrapper.GetStatuses();

                foreach (var status in statuses)
                {
                    StateValues.Add(status.Name);
                }
            }
        }

        /// <summary>
        /// Validate the setting values before close setting dialog.
        /// </summary>
        /// <returns>
        /// The validation result.
        /// </returns>
        public override SettingDialogValidateionResult ValidateBeforeCloseSettingDialog()
        {
            if (this.settingViewModel.ProgressType == ProgressTypes.OnAutoFillMapSettings
                || this.settingViewModel.ProgressType == ProgressTypes.OnConnectProgress
                || this.settingViewModel.ProgressType == ProgressTypes.OnGetFiledsProgress)
            {
                return SettingDialogValidateionResult.Busy;
            }

            if (this.settingViewModel.ConnectUri == null
                || !this.settingViewModel.ConnectUri.IsAbsoluteUri
                || string.IsNullOrWhiteSpace(this.settingViewModel.UserName)
                || this.settingViewModel.ProgressType == ProgressTypes.FailedOnConnect
                || this.settingViewModel.ProgressType == ProgressTypes.FailedOnGetFileds)
            {
                return SettingDialogValidateionResult.ConnectFailed;
            }

            if (this.settingViewModel.ProgressType == ProgressTypes.NotWorking)
            {
                JiraClientWrapper jiraClientWrapper = null;
                if (!jiraHelper.TryConnection(this.settingViewModel.ConnectUri, this.settingViewModel.UserName,
                                             this.settingViewModel.Password, out jiraClientWrapper))
                {
                    return SettingDialogValidateionResult.ConnectFailed;
                }
            }

            if (string.IsNullOrWhiteSpace(this.settingViewModel.JqlQuery)
                && this.settingViewModel.PropertyMappingCollection
                                        .Where(x => !ignoreField.Contains(x.Key))
                                        .Any(x =>
                                        {
                                            return string.IsNullOrWhiteSpace(x.Value);
                                        }))
            {
                return SettingDialogValidateionResult.UnValid;
            }

            return SettingDialogValidateionResult.Valid;
        }

        /// <summary>
        /// The method which will execute when the Controller.Initialize() execute.
        /// </summary>
        protected override void OnInitialize()
        {
            ignoreField = new List<string>();
            ignoreField.Add("Severity");

            //  Open the setting file
            if (File.Exists(SettingDocumentType.FilePath))
            {
                try
                {
                    this.document = SettingDocumentType.Open();
                }
                catch
                {
                    this.messageService.ShowError(Resources.CannotOpenFile);
                    this.document = SettingDocumentType.New();
                }
            }
            else
            {
                this.document = SettingDocumentType.New();
            }

            //  Validate Connect Information
            if (this.document == null || this.document.ConnectUri == null ||
                string.IsNullOrWhiteSpace(this.document.ConnectUri.AbsoluteUri) ||
                string.IsNullOrWhiteSpace(this.document.UserName))
            {
                return;
            }

            //  Connect to Jira
            JiraClientWrapper jiraClientWrapper = null;
            if (!jiraHelper.TryConnection(this.document.ConnectUri, this.document.UserName,
                                        this.document.Password, out jiraClientWrapper))
            {
                return;
            }

            //  Get Fields
            var fields = jiraHelper.GetFields();
            if (fields == null || !fields.Any()) { return; }

            this.jiraFieldsCache.AddRange(jiraHelper.GetFields());
            UpdateStateValues();
            this.CanQuery = true;
        }

        /// <summary>
        /// The core functionality query the bugs with the specified user name which should be query.
        /// </summary>
        /// <param name="userNames">The user names list which the bug assign to.</param>
        /// <param name="isFilterCreatedBy">if set to <c>true</c> indicating whether filter the created by field.</param>
        /// <returns>
        /// The bugs.
        /// </returns>
        protected override ReadOnlyCollection<Bug> QueryCore(List<string> userNames, bool isFilterCreatedBy)
        {
            List<Bug> bugs = new List<Bug>();

            if (!this.CanQuery) { return new ReadOnlyCollection<Bug>(bugs); }

            if (this.document == null || this.document.ConnectUri == null ||
                string.IsNullOrWhiteSpace(this.document.ConnectUri.AbsoluteUri) ||
                string.IsNullOrWhiteSpace(this.document.UserName))
            {
                this.CanQuery = false;
                return new ReadOnlyCollection<Bug>(bugs);
            }

            JiraClientWrapper jiraClientWrapper = null;
            if (jiraHelper.TryConnection(this.document.ConnectUri, this.document.UserName,
                                        this.document.Password, out jiraClientWrapper))
            {
                List<string> redFilter = string.IsNullOrWhiteSpace(this.document.PriorityRed)
                                           ? new List<string>()
                                           : this.document.PriorityRed
                                                          .Split(new string[] { PriorityRedSeparator }, StringSplitOptions.RemoveEmptyEntries)
                                                          .Select(x => x.Trim())
                                                          .ToList();

                foreach (string userName in userNames)
                {
                    var bugCollection = this.jiraHelper.GetBugs(jiraClientWrapper.Jira, userName, isFilterCreatedBy,
                                                               this.document.PropertyMappingCollection,
                                                               this.document.JqlQuery,
                                                               redFilter);
                    if (bugCollection == null) { continue; }

                    bugs.AddRange(bugCollection);
                }

                return new ReadOnlyCollection<Bug>(bugs.Distinct().ToList());
            }
            else
            {
                this.CanQuery = false;
                return new ReadOnlyCollection<Bug>(bugs);
            }
        }

        private void UpdateSettingDialogPriorityValues()
        {
            foreach (var checkString in this.settingViewModel.PriorityValues)
            {
                RemoveWeakEventListener(checkString, PriorityValuePropertyChanged);
            }
            this.settingViewModel.PriorityValues.Clear();

            string fieldName = this.settingViewModel.PropertyMappingCollection["Priority"];
            if (string.IsNullOrWhiteSpace(fieldName))
            {
                this.settingViewModel.PriorityRed = string.Empty;
                return;
            }

            var priorityField = this.settingViewModel.JiraFields.FirstOrDefault(x => x.Name == fieldName);
            if (priorityField == null) { return; }

            foreach (var value in this.priorityValues)
            {
                CheckString checkValue = new CheckString(value);
                checkValue.IsChecked = !string.IsNullOrWhiteSpace(this.document.PriorityRed)
                                       && this.document.PriorityRed.Contains(value);

                AddWeakEventListener(checkValue, PriorityValuePropertyChanged);

                this.settingViewModel.PriorityValues.Add(checkValue);
            }

            this.settingViewModel.PriorityRed = string.Join(PriorityRedSeparator,
                                                            this.settingViewModel.PriorityValues
                                                                                 .Where(x => x.IsChecked)
                                                                                 .Select(x => x.Name));
        }

        private void AutoFillMapSettings(IList<JiraField> jiraFields)
        {
            if (jiraFields == null) { throw new ArgumentException("jiraFields"); }

            if (jiraFields.Any(x => x.Name == "Key"))
            {
                this.settingViewModel.PropertyMappingCollection["ID"] = "Key";
            }
            if (jiraFields.Any(x => x.Name == "Summary"))
            {
                this.settingViewModel.PropertyMappingCollection["Title"] = "Summary";
            }
            if (jiraFields.Any(x => x.Name == "Description"))
            {
                this.settingViewModel.PropertyMappingCollection["Description"] = "Description";
            }
            if (jiraFields.Any(x => x.Name == "Assignee"))
            {
                this.settingViewModel.PropertyMappingCollection["AssignedTo"] = "Assignee";
            }
            if (jiraFields.Any(x => x.Name == "Status"))
            {
                this.settingViewModel.PropertyMappingCollection["State"] = "Status";
            }
            if (jiraFields.Any(x => x.Name == "Updated"))
            {
                this.settingViewModel.PropertyMappingCollection["ChangedDate"] = "Updated";
            }
            if (jiraFields.Any(x => x.Name == "Reporter"))
            {
                this.settingViewModel.PropertyMappingCollection["CreatedBy"] = "Reporter";
            }
            if (jiraFields.Any(x => x.Name == "Priority"))
            {
                this.settingViewModel.PropertyMappingCollection["Priority"] = "Priority";
            }
            if (jiraFields.Any(x => x.Name == "None"))
            {
                this.settingViewModel.PropertyMappingCollection["Severity"] = "None";
            }

            UpdateSettingDialogPriorityValues();
        }

        private void SettingViewModelPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "PropertyMappingCollection")
            {
                UpdateSettingDialogPriorityValues();
                UpdateStateValues();
            }
            else if (e.PropertyName == "ConnectUri" || e.PropertyName == "UserName" || e.PropertyName == "Password")
            {
                this.settingViewModel.ClearMappingData();
            }

            UpdateCommands();
        }

        private void PriorityValuePropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            this.settingViewModel.PriorityRed = string.Join(PriorityRedSeparator,
                                                            this.settingViewModel.PriorityValues
                                                                                 .Where(x => x.IsChecked)
                                                                                 .Select(x => x.Name));
        }

        private bool CanTestConnectionCommandExcute()
        {
            return this.settingViewModel.ProgressType != ProgressTypes.OnAutoFillMapSettings
                && this.settingViewModel.ProgressType != ProgressTypes.OnConnectProgress
                && this.settingViewModel.ProgressType != ProgressTypes.OnGetFiledsProgress
                && this.settingViewModel.ConnectUri != null
                && this.settingViewModel.ConnectUri.IsAbsoluteUri
                && !string.IsNullOrWhiteSpace(this.settingViewModel.UserName);
        }

        private void TestConnectionCommandExcute()
        {
            TestConnectionCommandExcuteCore();
        }

        internal Task TestConnectionCommandExcuteCore()
        {
            this.settingViewModel.ClearMappingData();
            this.settingViewModel.ProgressType = ProgressTypes.OnConnectProgress;
            this.settingViewModel.ProgressValue = 0;

            var testConnectionTask = Task.Factory.StartNew(() =>
            {
                //  Connect
                JiraClientWrapper jiraClientWrapper = null;
                jiraHelper.TryConnection(this.settingViewModel.ConnectUri, this.settingViewModel.UserName,
                                             this.settingViewModel.Password, out jiraClientWrapper);

                return jiraClientWrapper.Jira;
            })
            .ContinueWith(task =>
            {
                if (task.Result == null)
                {
                    this.settingViewModel.ProgressType = ProgressTypes.FailedOnConnect;
                    this.settingViewModel.ProgressValue = 100;
                    throw new OperationCanceledException();
                }
                else
                {
                    this.settingViewModel.ProgressType = ProgressTypes.OnGetFiledsProgress;
                    this.settingViewModel.ProgressValue = 50;
                    return task.Result;
                }
            }, CancellationToken.None, TaskContinuationOptions.OnlyOnRanToCompletion, TaskScheduler.FromCurrentSynchronizationContext())
            .ContinueWith(task =>
            {
                //  Query Jira Fields
                return jiraHelper.GetFields();
            }, TaskContinuationOptions.OnlyOnRanToCompletion)
            .ContinueWith(task =>
            {
                if (task.Result == null)
                {
                    this.settingViewModel.ProgressType = ProgressTypes.FailedOnGetFileds;
                    this.settingViewModel.ProgressValue = 100;
                    throw new OperationCanceledException();
                }
                else
                {
                    this.settingViewModel.ProgressValue = 80;
                    return task.Result;
                }
            }, CancellationToken.None, TaskContinuationOptions.OnlyOnRanToCompletion, TaskScheduler.FromCurrentSynchronizationContext())
            .ContinueWith(task =>
            {
                this.settingViewModel.JiraFields.Clear();
                this.settingViewModel.BugFilterFields.Clear();
                foreach (var field in task.Result)
                {
                    this.settingViewModel.JiraFields.Add(field);
                    if (field.AllowedValues.Any())
                    {
                        this.settingViewModel.BugFilterFields.Add(field);
                    }
                }
                return task.Result;
            }, CancellationToken.None, TaskContinuationOptions.OnlyOnRanToCompletion, TaskScheduler.FromCurrentSynchronizationContext())
            .ContinueWith(task =>
            {
                this.settingViewModel.ProgressValue = 90;
                this.settingViewModel.ProgressType = ProgressTypes.OnAutoFillMapSettings;
                return task.Result;
            }, CancellationToken.None, TaskContinuationOptions.OnlyOnRanToCompletion, TaskScheduler.FromCurrentSynchronizationContext())
            .ContinueWith(task =>
            {
                try
                {
                    AutoFillMapSettings(task.Result);
                    return true;
                }
                catch
                {
                    return false;
                }
            }, CancellationToken.None, TaskContinuationOptions.OnlyOnRanToCompletion, TaskScheduler.FromCurrentSynchronizationContext())
            .ContinueWith(task =>
            {
                if (task.Result)
                {
                    this.settingViewModel.ProgressValue = 100;
                    this.settingViewModel.ProgressType = ProgressTypes.Success;
                }
                else
                {
                    this.settingViewModel.ProgressType = ProgressTypes.SuccessWithError;
                    this.settingViewModel.ProgressValue = 100;
                }
            }, CancellationToken.None, TaskContinuationOptions.OnlyOnRanToCompletion, TaskScheduler.FromCurrentSynchronizationContext());

            return testConnectionTask;
        }

        private void UpdateCommands()
        {
            this.testConnectionCommand.RaiseCanExecuteChanged();
        }
    }
}
