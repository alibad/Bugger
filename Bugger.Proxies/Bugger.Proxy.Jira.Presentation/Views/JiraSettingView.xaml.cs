using BigEgg.Framework.Applications.Views;
using Bugger.Proxy.Jira.ViewModels;
using Bugger.Proxy.Jira.Views;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Bugger.Proxy.Jira.Presentation.Views
{
    [Export(typeof(IJiraSettingView))]
    public partial class JiraSettingView : UserControl, IJiraSettingView
    {
        private readonly Lazy<JiraSettingViewModel> viewModel;
        
        
        public JiraSettingView()
        {
            InitializeComponent();

            viewModel = new Lazy<JiraSettingViewModel>(() => ViewHelper.GetViewModel<JiraSettingViewModel>(this));
            Loaded += LoadedHandler;
            Unloaded += UnloadedHandler;
        }


        public string Title { get { return Properties.Resources.SettingViewTitle; } }

        private JiraSettingViewModel ViewModel { get { return viewModel.Value; } }


        private void LoadedHandler(object sender, RoutedEventArgs e)
        {
            password.Password = ViewModel.Password;
            ViewModel.PropertyChanged += SettingsPropertyChanged;
            JiraName.Focus();
        }

        private void UnloadedHandler(object sender, RoutedEventArgs e)
        {
            ViewModel.PropertyChanged -= SettingsPropertyChanged;
        }

        private void SettingsPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "Password")
            {
                password.Password = ViewModel.Password;
            }
        }

        private void PasswordChanged(object sender, RoutedEventArgs e)
        {
            PasswordBox passwordBox = (PasswordBox)sender;
            ViewModel.Password = passwordBox.Password;
        }
    }
}
