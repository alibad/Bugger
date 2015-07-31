using BigEgg.Framework.Applications.ViewModels;
using Bugger.Applications.Properties;
using Bugger.Applications.Services;
using Bugger.Proxy;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.Linq;
using System.Threading.Tasks;

namespace Bugger.Applications.Controllers
{
    [Export]
    internal class ProxyController : Controller
    {
        #region Members
        private readonly CompositionContainer container;
        private readonly ProxyService proxyService;
        #endregion

        [ImportingConstructor]
        public ProxyController(CompositionContainer container)
        {
            this.container = container;

            IEnumerable<ITracingSystemProxy> proxys = this.container.GetExportedValues<ITracingSystemProxy>();
            this.proxyService = new ProxyService(proxys);

            ActiveProxyInitializeTask = new Task<bool>(() =>
            {
                if (this.proxyService.ActiveProxy != null)
                {
                    this.proxyService.ActiveProxy.Initialize();
                    return true;
                }
                else
                {
                    return false;
                }
            });
        }

        #region Implement Controller base class
        protected override void OnInitialize()
        {
            if (this.ProxyService.Proxies.Any(x => x.ProxyName == Settings.Default.ActiveProxy))
            {
                this.proxyService.ActiveProxy = this.proxyService.Proxies.First(x => x.ProxyName == Settings.Default.ActiveProxy);
            }
            else if (this.ProxyService.Proxies.Any())
            {

                var jiraProxy = this.proxyService.Proxies.FirstOrDefault(x => x.ProxyName.IndexOf("jira", 0, StringComparison.CurrentCultureIgnoreCase) != -1);
                this.proxyService.ActiveProxy = jiraProxy ?? this.proxyService.Proxies.First();
            }
            else
            {
                this.proxyService.ActiveProxy = null;
            }

            ActiveProxyInitializeTask.Start();
        }

        public void Shutdown()
        { }
        #endregion

        #region Properties
        public ProxyService ProxyService { get { return this.proxyService; } }

        public Task<bool> ActiveProxyInitializeTask { get; set; }
        #endregion
    }
}
