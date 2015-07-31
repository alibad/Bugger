using System.Windows;
using System.Windows.Controls;

namespace Bugger.Proxy.Jira.Presentation.TemplateSelectors
{
    public class JiraFieldComboTemplateSelector : DataTemplateSelector
    {
        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            ContentPresenter presenter = (ContentPresenter)container;

            if (presenter.TemplatedParent is ComboBox)
            {
                return (DataTemplate)presenter.FindResource("JiraFieldComboCollapsed");
            }
            else // Templated parent is ComboBoxItem
            {
                return (DataTemplate)presenter.FindResource("JiraFieldComboExpanded");
            }
        }
    }
}
