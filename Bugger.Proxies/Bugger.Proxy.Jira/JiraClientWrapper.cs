using System;
using System.Collections.Generic;
using Jira.SDK.Tools;
using Newtonsoft.Json;
using RestSharp;
using SDK = Jira.SDK;

namespace Bugger.Proxy.Jira
{
    /// <summary>
    /// JiraClientWrapper attempts to fill in the gaps in the Jira Rest Client API we are currently using in this project (Jira.SDK)
    /// The Jira object doesn't expose a way to get the url\user\pass, so it would be cleaner to have this as a wrapper over it rather
    /// than extending the Jira.SDK with extensions that take many repetitive parameters.
    /// </summary>
    public class JiraClientWrapper
    {
        public SDK.Jira Jira { get; private set; }

        private RestClient Client { get; set; }

        private const string JiraApiServiceUri = "/rest/api/latest";

        public JiraClientWrapper()
        {
            Jira = new SDK.Jira();
        }

        public JiraClientWrapper(RestClient client) : this()
        {
            Client = client;
        }

        public JiraClientWrapper(string url) : this(new RestClient(url))
        {
        }

        public JiraClientWrapper(string url, string username, string password) : this(url)
        {
            Client.Authenticator = new HttpBasicAuthenticator(username, password);
        }

        public List<JiraStatus> GetStatuses()
        {
            return Execute<List<JiraStatus>>(string.Format("{0}/status", JiraApiServiceUri));
        }

        private T Execute<T>(string url, Dictionary<string, string> parameters = null, Dictionary<string, string> keys = null) where T : new()
        {
            IRestResponse<T> response = Client.Execute<T>(GetRequest(url, parameters ?? new Dictionary<string, string>(), keys ?? new Dictionary<string, string>()));

            if (response.ErrorException != null)
            {
                throw response.ErrorException;
            }
            if (response.ResponseStatus != ResponseStatus.Completed)
            {
                throw new Exception(response.ErrorMessage);
            }

            return response.Data;
        }

        private RestRequest GetRequest(string url, Dictionary<string, string> parameters, Dictionary<string, string> keys)
        {
            RestRequest request = new RestRequest(url, Method.GET)
            {
                RequestFormat = DataFormat.Json,
                OnBeforeDeserialization = resp => resp.ContentType = "application/json",
                JsonSerializer = new RestSharpJsonNetSerializer()
            };


            foreach (KeyValuePair<string, string> key in keys)
            {
                request.AddParameter(key.Key, key.Value, ParameterType.UrlSegment);
            }

            foreach (KeyValuePair<string, string> parameter in parameters)
            {
                request.AddParameter(parameter.Key, parameter.Value);
            }

            return request;
        }
    }

    public class JiraStatus
    {
        [JsonProperty("description")]
        public string Description { get; set; }

        [JsonProperty("iconUrl")]
        public string IconUrl { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("statusCategory")]
        public StatusCategory StatusCategory { get; set; }
    }

    public class StatusCategory
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("key")]
        public string Key { get; set; }

        [JsonProperty("colorName")]
        public string ColorName { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }
    }
}
