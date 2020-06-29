namespace ExpenseDemo.Services
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Text;
    using System.Threading.Tasks;
    using ExpenseDemo.Models;
    using ExpenseDemo.Services.Interfaces;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.Options;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;

    public class AttributeStoreService : IAttributeMetadataService, IUserMetadataService, IAttributeAssignmentService
    {
        private const string AttributeStoreCatalogTemplate = "{0}/api/AttributeStore/AttributeCatalog/?ns={1}&bd={2}";
        private const string AttirbuteStoreUserAssignmentTemplate = "{0}/api/namespaces/{1}/businessDomains/{2}/attributeEntityAssignment/entity/{3}";
        private const string AttirbuteStoreGraphUserTemplate = "{0}/api/namespaces/ExternalDataProvider/businessDomains/AADGraph/attributeEntityAssignment/entity/{1}";

        private const string ReportToPropertyName = "extension_18e31482d3fb4a8ea958aa96b662f508_ReportsToEmailName";
        private const string AccessTokenHeader = "AccessToken";

        private readonly ExpenseDemoOptions options;
        private readonly HttpContext httpContext;

        public AttributeStoreService(IOptions<ExpenseDemoOptions> optionsAccessor, IHttpContextAccessor httpContextAccessor)
        {
            this.options = optionsAccessor.Value;
            this.httpContext = httpContextAccessor.HttpContext;
        }

        public async Task<JObject> GetAttributeCatalog()
        {
            try
            {
                var client = new HttpClient();
                var token = await this.options.ClientOptions.GetAppTokenAsync(this.options.AttributeStoreOptions.ResourceUrl);
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                var url = string.Format(AttributeStoreCatalogTemplate, this.options.AttributeStoreOptions.BaseUrl, this.options.AttributeStoreOptions.Namespace, this.options.ClientOptions.ClientId);
                var response = await client.GetAsync(url);
                response.EnsureSuccessStatusCode();
                var jsonResult = JArray.Parse(await response.Content.ReadAsStringAsync());

                var returnObject = new JObject();
                foreach (var item in jsonResult)
                {
                    returnObject[item["name"].ToString()] = JArray.FromObject(item["values"].Select(value => value["value"].ToString()));
                }

                return returnObject;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public async Task<IEnumerable<AttributeEntityAssignment>> GetAttributeAssignments(UserMetadata userMetadata)
        {
            try
            {
                var client = new HttpClient();
                var token = await this.options.ClientOptions.GetAppTokenAsync(this.options.AttributeStoreOptions.ResourceUrl);
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                var url = string.Format(AttirbuteStoreUserAssignmentTemplate, this.options.AttributeStoreOptions.BaseUrl, this.options.AttributeStoreOptions.Namespace, this.options.ClientOptions.ClientId, userMetadata.ObjectId);

                //TODO: Replace the temporary fix of 415 Unsupported Media type
                var request = new HttpRequestMessage
                {
                    Method = HttpMethod.Get,
                    RequestUri = new Uri(url),
                    Content = new StringContent(JsonConvert.SerializeObject(new
                    {

                    }), Encoding.UTF8, "application/json"),
                };
                var response = await client.SendAsync(request).ConfigureAwait(false);

                response.EnsureSuccessStatusCode();
                var jsonResult = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<IEnumerable<AttributeEntityAssignment>>(JObject.Parse(jsonResult)["assignmentList"].ToString());
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public async Task<UserMetadata> GetUserGraph(string userIdentifier)
        {
            string responseContent = null;
            try
            {
                var httpClient = new HttpClient();
                var accessTokenFromHeader = this.httpContext.Request.Headers[AccessTokenHeader].FirstOrDefault();
                // Use access token from header if it's present and client secret is not
                var token = !string.IsNullOrEmpty(accessTokenFromHeader) && string.IsNullOrEmpty(this.options.ClientOptions.ClientSecret) ? accessTokenFromHeader : await this.options.ClientOptions.GetAppTokenAsync(this.options.AttributeStoreOptions.ResourceUrl);
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                var request = new HttpRequestMessage
                {
                    Method = HttpMethod.Get,
                    RequestUri = new Uri(string.Format(AttirbuteStoreGraphUserTemplate, this.options.AttributeStoreOptions.BaseUrl, userIdentifier)),
                    Content = new StringContent(JsonConvert.SerializeObject(new
                    {
                        queryType = "User",
                        @namespace = "ExternalDataProvider",
                        businessdomain = "AADGraph",
                        attributeNames = UserMetadata.GetJsonAttributeNames(),
                    }), Encoding.UTF8, "application/json"),
                };

                var response = await httpClient.SendAsync(request).ConfigureAwait(false);
                responseContent = await response.Content.ReadAsStringAsync();
                response.EnsureSuccessStatusCode();
                var jsonResult = JObject.Parse(responseContent);
                var attributeENtityAssignments = JsonConvert.DeserializeObject<IEnumerable<AttributeEntityAssignment>>(jsonResult["assignmentList"].ToString());
                return new UserMetadata(attributeENtityAssignments);
            }
            catch (HttpRequestException ex)
            {
                throw new InvalidOperationException($"GetUserGraph failed with error: {responseContent}", ex);
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public async Task<List<string>> GetUserReportingChain(string userPrincipalName)
        {
            var client = new HttpClient();
            var accessTokenFromHeader = this.httpContext.Request.Headers[AccessTokenHeader].FirstOrDefault();
            // Use access token from header if it's present and client secret is not
            var token = !string.IsNullOrEmpty(accessTokenFromHeader) && string.IsNullOrEmpty(this.options.ClientOptions.ClientSecret) ? accessTokenFromHeader : await this.options.ClientOptions.GetAppTokenAsync(this.options.AttributeStoreOptions.ResourceUrl);
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var managers = new List<string>();
            var currentUser = userPrincipalName;

            while (!string.IsNullOrEmpty(currentUser))
            {
                managers.Add(currentUser.Substring(0, currentUser.IndexOf('@')));

                var request = new HttpRequestMessage
                {
                    Method = HttpMethod.Get,
                    RequestUri = new Uri(string.Format(AttirbuteStoreGraphUserTemplate, this.options.AttributeStoreOptions.BaseUrl, currentUser)),
                    Content = new StringContent(JsonConvert.SerializeObject(new
                    {
                        queryType = "User",
                        @namespace = "ExternalDataProvider",
                        businessdomain = "AADGraph",
                        attributeNames = new[]
                        {
                            "extension_18e31482d3fb4a8ea958aa96b662f508_ReportsToEmailName"
                        }
                    }), Encoding.UTF8, "application/json"),
                };

                var response = await client.SendAsync(request).ConfigureAwait(false);
                var jsonResult = JObject.Parse(await response.Content.ReadAsStringAsync());
                response.EnsureSuccessStatusCode();

                var reportTo = jsonResult["assignmentList"].Any() ? jsonResult.SelectToken("assignmentList[0].values[0]").ToString() : null;
                if (string.IsNullOrEmpty(reportTo))
                {
                    currentUser = null;
                }
                else
                {
                    currentUser = $"{reportTo}@{this.options.ClientOptions.TenantName}";
                }
            }

            managers = managers.Select(m => m.ToLowerInvariant()).ToList();
            managers.Reverse();
            return managers;
        }
    }
}
