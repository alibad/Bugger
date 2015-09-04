using System.Collections.Specialized;
using System.ComponentModel.Composition;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using Bugger.Applications.ViewModels;
using Bugger.Applications.Views;

namespace Bugger.Presentation.Views
{
    /// <summary>
    /// Interaction logic for UserBugsView.xaml
    /// </summary>
    [Export(typeof(IUserBugsView))]
    public partial class UserBugsView : UserControl, IUserBugsView
    {
        public UserBugsView()
        {
            InitializeComponent();

            CollectionView myCollectionView = (CollectionView)CollectionViewSource.GetDefaultView(BugDataGrid.Items);
            ((INotifyCollectionChanged)myCollectionView).CollectionChanged += DataGridCollectionChanged;
        }

        /// <summary>
        /// This is hack to update visibility of DataGrid fields
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DataGridCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            var dataContext = this.DataContext as UserBugsViewModel;

            if (dataContext != null)
            {
                foreach (var column in BugDataGrid.Columns)
                {
                    if (column.Header == null)
                    {
                        continue;
                    }

                    if (dataContext.VisibleBugFields.Contains(column.Header.ToString()))
                    {
                        column.Visibility = Visibility.Visible;
                    }
                    else
                    {
                        column.Visibility = Visibility.Collapsed;
                    }
                }
            }
        }
    }
}
