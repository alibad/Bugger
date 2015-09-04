namespace Bugger.Proxy.Jira.Models
{
    public enum ProgressTypes
    {
        NotWorking,
        OnConnectProgress,
        OnGetFiledsProgress,
        OnAutoFillMapSettings,
        Success,
        FailedOnConnect,
        FailedOnGetFileds,
        SuccessWithError
    }
}
