using System.Collections.Specialized;
using Bugger.Applications.Views;
using System.ComponentModel.Composition;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using Bugger.Applications.ViewModels;

namespace Bugger.Presentation.Views
{
    /// <summary>
    /// Interaction logic for TeamBugsView.xaml
    /// </summary>
    [Export(typeof(ITeamBugsView))]
    public partial class TeamBugsView : UserControl, ITeamBugsView
    {
        public TeamBugsView()
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
            var dataContext = this.DataContext as TeamBugsViewModel;

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
