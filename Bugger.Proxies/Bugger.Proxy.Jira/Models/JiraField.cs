using BigEgg.Framework.Foundation;
using System;
using System.Collections.ObjectModel;

namespace Bugger.Proxy.Jira.Models
{
    public class JiraField : Model
    {
        #region Fields
        private readonly string name;
        private readonly ObservableCollection<string> allowedValues;
        #endregion

        public JiraField(string name)
        {
            if (string.IsNullOrWhiteSpace(name)) { throw new ArgumentException("name"); }

            this.name = name;
            this.allowedValues = new ObservableCollection<string>();
        }

        #region Properties
        public string Name
        {
            get { return this.name; }
        }

        public ObservableCollection<string> AllowedValues
        {
            get { return this.allowedValues; }
        }
        #endregion
    }
}
