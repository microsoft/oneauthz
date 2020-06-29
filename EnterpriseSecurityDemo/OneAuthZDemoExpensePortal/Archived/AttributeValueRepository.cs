//namespace ExpenseDemo.Services
//{
//    using System;
//    using System.Collections.Generic;
//    using System.Linq;
//    using System.Net.Http;
//    using System.Net.Http.Headers;
//    using System.Threading.Tasks;
//    using Models;
//    using Newtonsoft.Json.Linq;
//    using ExpenseDemo.Helpers;

//    internal class AttributeValueRepository
//    {
//        private const string ReportToPropertyName = "extension_18e31482d3fb4a8ea958aa96b662f508_ReportsToEmailName";

//        private const string AttributeStoreAssignmentTemplate = "{0}/api/AttributeStore/AttributeEntityAssignment/?asgns={4}&asgbd={1}&entity={2}" +
//                                                                "&atrns={4}&atrbd={1}&names={3}";

//        private static readonly string AppClientId = ConfigHelper.Configuration["DemoPet/ClientId"];
//        private static readonly string AttributeStoreBaseUrl = ConfigHelper.Configuration["AttributeStore/BaseUrl"];
//        private static readonly string Tenant = ConfigHelper.Configuration["DemoPet/Tenant"];

//        private readonly Func<Task<string>> getAppAuthenticationTokenFunc;

//        private readonly Func<Task<string>> getGraphAccessAuthenticationTokenFunc;

//        public AttributeValueRepository(Func<Task<string>> getGraphAccessAuthenticationTokenFunc, Func<Task<string>> getAppAuthenticationTokenFunc)
//        {
//            this.getGraphAccessAuthenticationTokenFunc = getGraphAccessAuthenticationTokenFunc;
//            this.getAppAuthenticationTokenFunc = getAppAuthenticationTokenFunc;
//        }

//        public async Task<GraphObjectInfo> GetUserGraph(string upn)
//        {
//            var client = new HttpClient();
//            var token = await this.getGraphAccessAuthenticationTokenFunc();
//            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
//            var userGraphUrl = string.Format("https://graph.windows.net/{0}/users/{1}?api-version=1.6", Tenant, upn);
//            var response = await client.GetAsync(userGraphUrl);
//            response.EnsureSuccessStatusCode();
//            var jsonResult = JObject.Parse(await response.Content.ReadAsStringAsync());
//            var userInfo = jsonResult.ToObject<GraphObjectInfo>();
//            return userInfo;
//        }

//        public async Task<List<string>> GetUserReportingChain(string userPrincipalName)
//        {
//            var client = new HttpClient();
//            var token = await this.getGraphAccessAuthenticationTokenFunc();
//            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

//            var managers = new List<string>();
//            var currentManager = userPrincipalName;

//            while (!string.IsNullOrEmpty(currentManager))
//            {
//                managers.Add(currentManager.Substring(0, currentManager.IndexOf('@')));
//                var managerUrl = $"https://graph.windows.net/microsoft.com/users/{currentManager}?api-version=1.6";
//                var response = await client.GetAsync(managerUrl);
//                var result = await response.Content.ReadAsStringAsync();
//                var content = JObject.Parse(result);
//                if (content[ReportToPropertyName] == null)
//                {
//                    currentManager = null;
//                }
//                else
//                {
//                    currentManager = $"{content[ReportToPropertyName]}@microsoft.com";
//                }
//            }

//            managers = managers.Select(m => m.ToLowerInvariant()).ToList();
//            managers.Reverse();
//            return managers;
//        }

//        public async Task<string> GetObjectId(string oboUser)
//        {
//            var client = new HttpClient();
//            var token = await this.getAppAuthenticationTokenFunc();
//            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

//            var userUrl = $"https://graph.windows.net/microsoft.com/users/{oboUser}@microsoft.com?api-version=1.6";
//            var response = await client.GetAsync(userUrl);
//            var content = JObject.Parse(await response.Content.ReadAsStringAsync());
//            return content["objectId"].ToString();
//        }

//        public async Task<IEnumerable<string>> GetAttributeValues(string objectId, string attributeName)
//        {
//            try
//            {
//                var client = new HttpClient();
//                var token = await this.getAppAuthenticationTokenFunc();
//                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
//                var url = string.Format(
//                    AttributeStoreAssignmentTemplate,
//                    AttributeStoreBaseUrl,
//                    AppClientId,
//                    objectId,
//                    attributeName,
//                    Tenant);
//                var response = await client.GetAsync(url);

//                var resultString = await response.Content.ReadAsStringAsync();
//                var jObject = JObject.Parse(resultString);

//                var valuesObject = jObject["assignmentList"].First()["values"] as JArray;

//                return valuesObject.Select(v => v.ToString());
//            }
//            catch (Exception ex)
//            {
//                // TODO: Swallow for now
//                return new List<string>();
//            }
//        }
//    }
//}