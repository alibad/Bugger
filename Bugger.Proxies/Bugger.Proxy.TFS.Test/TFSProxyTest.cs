﻿using BigEgg.Framework.Applications.Services;
using BigEgg.Framework.Applications.Views;
using BigEgg.Framework.UnitTesting;
using Bugger.Domain.Models;
using Bugger.Proxy.TFS.Documents;
using Bugger.Proxy.TFS.Presentation.Fake.Views;
using Bugger.Proxy.TFS.Properties;
using Bugger.Proxy.TFS.Test.Services;
using Bugger.Proxy.TFS.ViewModels;
using Bugger.Proxy.TFS.Views;
using BigEgg.Framework.Applications.Views;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using Bugger.Proxy.TFS.Models;

namespace Bugger.Proxy.TFS.Test
{
    [TestClass]
    public class TFSProxyTest : TestClassBase
    {
        private TFSProxy proxy;

        protected override void OnTestInitialize()
        {
            this.proxy = Container.GetExportedValue<ITracingSystemProxy>() as TFSProxy;
        }

        [TestMethod]
        public void GeneralTFSProxyTest()
        {
            Assert.AreEqual("TFS", proxy.ProxyName);
            Assert.IsTrue(this.proxy.IsInitialized);
            Assert.IsFalse(this.proxy.CanQuery);
            Assert.IsNotNull(this.proxy.StateValues);
            Assert.IsFalse(this.proxy.StateValues.Any());
        }

        [TestMethod]
        public void InitializeSettingDialogTest()
        {
            Assert.IsFalse(this.proxy.CanQuery);

            var view = this.proxy.InitializeSettingDialog();
            Assert.IsNotNull(view);
            Assert.IsInstanceOfType(view, typeof(ITFSSettingView));

            var viewModel = (view as ITFSSettingView).GetViewModel<TFSSettingViewModel>();
            Assert.IsNotNull(viewModel);

            Assert.IsNull(viewModel.ConnectUri);
            Assert.IsNull(viewModel.UserName);
            Assert.IsNull(viewModel.Password);

            foreach (var mapping in viewModel.PropertyMappingCollection)
            {
                Assert.AreEqual(string.Empty, viewModel.PropertyMappingCollection[mapping.Key]);
            }

            Assert.IsNotNull(viewModel.TFSFields);
            Assert.IsFalse(viewModel.TFSFields.Any());
            Assert.IsNotNull(viewModel.BugFilterFields);
            Assert.IsFalse(viewModel.BugFilterFields.Any());
            Assert.IsNull(viewModel.BugFilterField);
            Assert.IsNull(viewModel.BugFilterValue);

            Assert.IsNotNull(viewModel.PriorityValues);
            Assert.IsFalse(viewModel.PriorityValues.Any());
            Assert.AreEqual(string.Empty, viewModel.PriorityRed);

            Assert.AreEqual(ProgressTypes.NotWorking, viewModel.ProgressType);
            Assert.AreEqual(0, viewModel.ProgressValue);
        }

        //[TestMethod]
        //public void SaveCommandTest()
        //{
        //    this.viewModel.Settings.ConnectUri = new Uri("https://tfs.codeplex.com:443/tfs/TFS12");
        //    this.viewModel.Settings.BugFilterField = "WorkItemType";
        //    this.viewModel.Settings.BugFilterValue = "Issue";
        //    this.viewModel.Settings.UserName = "snd\\BigEgg_cp";
        //    this.viewModel.Settings.Password = ThePassword;
        //    this.viewModel.Settings.PropertyMappingCollection["ID"] = "ID";
        //    this.viewModel.Settings.PropertyMappingCollection["Title"] = "Title";
        //    this.viewModel.Settings.PropertyMappingCollection["Description"] = "Description";
        //    this.viewModel.Settings.PropertyMappingCollection["AssignedTo"] = "Assigned To";
        //    this.viewModel.Settings.PropertyMappingCollection["State"] = "State";
        //    this.viewModel.Settings.PropertyMappingCollection["ChangedDate"] = "Changed Date";
        //    this.viewModel.Settings.PropertyMappingCollection["CreatedBy"] = "Created By";
        //    this.viewModel.Settings.PropertyMappingCollection["Priority"] = "Code Studio Rank";

        //    Assert.IsTrue(this.viewModel.SaveCommand.CanExecute(null));

        //    this.viewModel.SaveCommand.Execute(null);
        //    Assert.IsTrue(File.Exists(SettingDocumentType.FilePath));
        //}


        //[TestMethod]
        //public void CanTestConnectionCommandTest()
        //{
        //    MockUriHelpView view = Container.GetExportedValue<IUriHelpView>() as MockUriHelpView;
        //    view.ShowDialogAction = (x) =>
        //    {
        //        UriHelpViewModel uriHelpViewModel = x.GetViewModel<UriHelpViewModel>();
        //        uriHelpViewModel.CancelCommand.Execute(null);
        //    };
        //    this.viewModel.UriHelpCommand.Execute(null);
        //    Assert.IsFalse(viewModel.TestConnectionCommand.CanExecute(null));

        //    view.ShowDialogAction = (x) =>
        //    {
        //        UriHelpViewModel uriHelpViewModel = x.GetViewModel<UriHelpViewModel>();
        //        uriHelpViewModel.ServerName = "https://tfs.codeplex.com:443/tfs/TFS12";
        //        uriHelpViewModel.SubmitCommand.Execute(null);
        //    };
        //    this.viewModel.Settings.UserName = "snd\\BigEgg_cp";
        //    this.viewModel.UriHelpCommand.Execute(null);
        //    Assert.IsTrue(viewModel.TestConnectionCommand.CanExecute(null));
        //}

        //[TestMethod]
        //public void TestConnectionCommandTest()
        //{
        //    MockUriHelpView view = Container.GetExportedValue<IUriHelpView>() as MockUriHelpView;
        //    view.ShowDialogAction = (x) =>
        //    {
        //        UriHelpViewModel uriHelpViewModel = x.GetViewModel<UriHelpViewModel>();
        //        uriHelpViewModel.ServerName = "https://tfs.codeplex.com:443/tfs/TFS12";
        //        uriHelpViewModel.SubmitCommand.Execute(null);
        //    };
        //    this.viewModel.UriHelpCommand.Execute(null);
        //    this.viewModel.Settings.UserName = "snd\\BigEgg_cp";

        //    MockMessageService messageService = Container.GetExportedValue<IMessageService>() as MockMessageService;
        //    messageService.Clear();
        //    Assert.IsNull(messageService.Message);
        //    this.viewModel.TestConnectionCommand.Execute(null);
        //    Assert.AreEqual(Resources.CannotConnect, messageService.Message);
        //    Assert.AreEqual(MessageType.Message, messageService.MessageType);

        //    this.viewModel.Settings.Password = ThePassword;
        //    this.viewModel.TestConnectionCommand.Execute(null);
        //    Assert.IsTrue(this.viewModel.CanConnect);
        //    Assert.IsTrue(this.viewModel.TFSFields.Any());
        //}

        //[TestMethod]
        //public void QueryTest()
        //{
        //    AssertHelper.ExpectedException<NotSupportedException>(() => this.proxy.Query("snd\\BigEgg_cp"));

        //    this.viewModel.Settings.ConnectUri = new Uri("https://tfs.codeplex.com:443/tfs/TFS12");
        //    this.viewModel.Settings.BugFilterField = "Work Item Type";
        //    this.viewModel.Settings.BugFilterValue = "Work Item";
        //    this.viewModel.Settings.UserName = "snd\\BigEgg_cp";
        //    this.viewModel.Settings.Password = ThePassword;
        //    this.viewModel.Settings.PropertyMappingCollection["ID"] = "ID";
        //    this.viewModel.Settings.PropertyMappingCollection["Title"] = "Title";
        //    this.viewModel.Settings.PropertyMappingCollection["Description"] = "Description";
        //    this.viewModel.Settings.PropertyMappingCollection["AssignedTo"] = "Assigned To";
        //    this.viewModel.Settings.PropertyMappingCollection["State"] = "State";
        //    this.viewModel.Settings.PropertyMappingCollection["ChangedDate"] = "Changed Date";
        //    this.viewModel.Settings.PropertyMappingCollection["CreatedBy"] = "Created By";
        //    this.viewModel.Settings.PropertyMappingCollection["Priority"] = "Code Studio Rank";

        //    Assert.IsTrue(this.proxy.CanQuery);
        //    ReadOnlyCollection<Bug> bugs = this.proxy.Query("BigEgg_cp");
        //    Assert.IsNotNull(bugs);
        //    Assert.IsTrue(bugs.Any());

        //    bugs = this.proxy.Query("BigEgg_cp", false);
        //    Assert.IsNotNull(bugs);
        //    Assert.IsTrue(bugs.Any());
        //}
    }
}