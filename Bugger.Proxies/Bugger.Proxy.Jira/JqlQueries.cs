using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bugger.Proxy.Jira
{
    public class JqlQueries
    {
        public const string AssignedToMeInOpensprints = "assignee = {userame} and Sprint in openSprints()";
    }
}
