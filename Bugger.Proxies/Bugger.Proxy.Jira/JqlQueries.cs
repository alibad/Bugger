namespace Bugger.Proxy.Jira
{
    public class JqlQueries
    {
        public const string AssignedToMeInOpensprints = "assignee = {username} and Sprint in openSprints()";
    }
}
