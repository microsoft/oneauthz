//namespace ExpenseDemo.Helpers
//{
//    using System;
//    using System.Collections.Generic;
//    using System.Linq;
//    using System.Threading.Tasks;
//    using Microsoft.IdentityModel.Authorization;
//    using Microsoft.IdentityModel.Authorization.Internal.Pdp;
//    using Models;
//    using Microsoft.Extensions.Caching.Memory;
//    using ExpenseDemo.Services;

//    internal class CheckAccessHelper
//    {
//        private static readonly MemoryCache ReportingChainCache;

//        private static readonly string AppClientId = ConfigHelper.Configuration["DemoPet/ClientId"];
//        private static readonly string Tenant = ConfigHelper.Configuration["DemoPet/Tenant"];

//        private readonly AttributeValueRepository attributeValueRepository;

//        static CheckAccessHelper()
//        {
//            ReportingChainCache = new MemoryCache(new MemoryCacheOptions() { ExpirationScanFrequency = new TimeSpan(1, 0, 0) });
//            ReportingChainCache.CreateEntry("ReportingChainCache");
//        }

//        public CheckAccessHelper(AttributeValueRepository attributeValueRepository)
//        {
//            this.attributeValueRepository = attributeValueRepository;
//        }

//        public async Task<Dictionary<Guid, bool>> AADCheckAccess(IEnumerable<AccessRequest> accessRequests, GraphObjectInfo currentUserGraphObjectInfo)
//        {
//            var client = new RemoteAadAuthorizationEngine(new Uri("https://authorization.microsoft.com/"), TokenHelper.GetAuthenticationTokenAsync, new Guid(AppClientId));
//            client.AdditionalHeaders = new Dictionary<string, string>
//            {
//                { "x-ms-authorization-no-cache", "True" }
//            };

//            var aadRequests = await Task.WhenAll(accessRequests.Select(item => this.ConvertToAADRequest(item, currentUserGraphObjectInfo)));
//            var results = new Dictionary<Guid, bool>();
//            var aadResults = await Task.WhenAll(aadRequests.Select(r => this.CheckAccessAADRequest(client, r)));

//            foreach (var result in aadResults)
//            {
//                results[result.Key] = result.Value;
//            }

//            return results;
//        }

//        public async Task<GraphObjectInfo> GetSubjectGraphObjectInfo(string currentUser, bool pullAttributeStoreData)
//        {
//            var userInfo = await this.attributeValueRepository.GetUserGraph($"{currentUser}@{Tenant}");
//            userInfo.Alias = currentUser;

//            if (pullAttributeStoreData)
//            {
//                var safeLimit = await this.attributeValueRepository.GetAttributeValues(userInfo.ObjectId, ExpensePortalConstant.Subject.AttributeName.SafeLimit);
//                if (safeLimit.Any())
//                {
//                    int safeLimitInt;
//                    if (int.TryParse(safeLimit.First(), out safeLimitInt))
//                    {
//                        userInfo.SafeLimit = safeLimitInt;
//                    }
//                }
//                else if (!string.IsNullOrEmpty(userInfo.GraphSafeLimit))
//                {
//                    userInfo.SafeLimit = decimal.ToInt32(decimal.Parse(userInfo.GraphSafeLimit));
//                }
//                else
//                {
//                    userInfo.SafeLimit = default(int);
//                }

//                var org = await this.attributeValueRepository.GetAttributeValues(userInfo.ObjectId, ExpensePortalConstant.Subject.AttributeName.Org);

//                // we should not support wild card if user doesn't have the org attribute.
//                if (org.Any() && !string.IsNullOrEmpty(org.First()))
//                {
//                    userInfo.Org = org.First() + "*";
//                }

//                userInfo.Categories = (List<string>)await this.attributeValueRepository.GetAttributeValues(userInfo.ObjectId, ExpensePortalConstant.Subject.AttributeName.Categories);
//            }

//            return userInfo;
//        }

//        private async Task<Tuple<SubjectInfo, ResourceInfo, ActionInfo, Guid>> ConvertToAADRequest(AccessRequest accessRequest, GraphObjectInfo userInfo)
//        {
//            // Subject Info
//            var subjectInfo = new SubjectInfo();

//            // Add all properties from Graph Object Info
//            var properties = userInfo.GetType().GetProperties();
//            foreach (var pi in properties.Where(p => p.Name != "GraphSafeLimit"))
//            {
//                if (pi.Name == ExpensePortalConstant.Subject.AttributeName.SafeLimit)
//                {
//                    subjectInfo.AddAttribute(pi.Name, int.Parse(pi.GetValue(userInfo).ToString()));
//                }
//                else if (pi.Name == ExpensePortalConstant.Subject.AttributeName.Categories)
//                {
//                    var categories = (List<string>)pi.GetValue(userInfo);
//                    if (categories == null)
//                    {
//                        continue;
//                    }

//                    foreach (var category in categories)
//                    {
//                        subjectInfo.AddAttribute(pi.Name, category);
//                    }
//                }
//                else
//                {
//                    var value = (string)pi.GetValue(userInfo);
//                    if (!string.IsNullOrEmpty(value))
//                    {
//                        subjectInfo.AddAttribute(pi.Name, value);
//                    }
//                }
//            }

//            // Extension2: Add subject user reporting chain
//            var managers = ReportingChainCache.Get(userInfo.Alias) as List<string>;
//            if (managers == null)
//            {
//                managers = await this.attributeValueRepository.GetUserReportingChain($"{userInfo.Alias}@microsoft.com");
//                ReportingChainCache.Set(
//                    userInfo.Alias,
//                    managers);
//            }

//            foreach (var manager in managers)
//            {
//                subjectInfo.AddAttribute(ExpensePortalConstant.Subject.AttributeName.ReportingChain, manager);
//            }
//            // Action Info
//            var actoionInfo = new ActionInfo($"{accessRequest.ResourceName}/{accessRequest.ActionName}");

//            // Resource Info
//            if (string.IsNullOrEmpty(accessRequest.Scope))
//            {
//                throw new ArgumentException("Invalid Request Attribute");
//            }

//            var resourceInfo = new ResourceInfo(accessRequest.Scope);

//            ////BUG BUG Find a better way to handle Expense/Submit
//            ////if (accessRequest.ActionName == "Submit")
//            ////{
//            ////    resourceInfo.AddAttribute(ExpensePortalConstant.Resource.AttributeName.OwnerAlias, userInfo.Alias);
//            ////}

//            if ((accessRequest.RequestAttributes != null) && accessRequest.RequestAttributes.Any())
//            {
//                // Extension3: Add Resource Owner Reporting Chain
//                var owner = accessRequest.RequestAttributes.FirstOrDefault(s => s.Name == ExpensePortalConstant.Resource.AttributeName.OwnerAlias);
//                if (!string.IsNullOrEmpty(owner?.Value))
//                {
//                    // Resoure 1: add owner
//                    resourceInfo.AddAttribute(ExpensePortalConstant.Resource.AttributeName.OwnerAlias, owner.Value);

//                    // Resoure 2: add OwnerReportingChain
//                    var ownerManagers = ReportingChainCache.Get(owner.Value) as List<string>;
//                    if (ownerManagers == null)
//                    {
//                        ownerManagers = await this.attributeValueRepository.GetUserReportingChain($"{owner.Value}@microsoft.com");
//                        ReportingChainCache.Set(
//                            owner.Value,
//                            ownerManagers);
//                    }

//                    foreach (var manager in ownerManagers)
//                    {
//                        resourceInfo.AddAttribute(ExpensePortalConstant.Resource.AttributeName.OwnerReportingChain, manager);
//                    }
//                }

//                // Extension3: Add Resource Amount as Integer
//                var amount = accessRequest.RequestAttributes.FirstOrDefault(s => (s.Name == ExpensePortalConstant.Resource.AttributeName.Amount) && (s.Type == "Resource"));
//                if (!string.IsNullOrEmpty(amount?.Value))
//                {
//                    resourceInfo.AddAttribute(amount.Name, int.Parse(amount.Value));
//                }

//                // add resource4 owner org
//                var ownerOrg = accessRequest.RequestAttributes.FirstOrDefault(s => (s.Name == ExpensePortalConstant.Resource.AttributeName.OwnerOrg) && (s.Type == "Resource"));
//                if (!string.IsNullOrEmpty(ownerOrg?.Value))
//                {
//                    resourceInfo.AddAttribute(ExpensePortalConstant.Resource.AttributeName.OwnerOrg, ownerOrg.Value);
//                }

//                // add resource5 category
//                var category = accessRequest.RequestAttributes.FirstOrDefault(s => (s.Name == ExpensePortalConstant.Resource.AttributeName.Category) && (s.Type == "Resource"));
//                if (!string.IsNullOrEmpty(category?.Value))
//                {
//                    resourceInfo.AddAttribute(ExpensePortalConstant.Resource.AttributeName.Category, category.Value);
//                }
//            }

//            return new Tuple<SubjectInfo, ResourceInfo, ActionInfo, Guid>(subjectInfo, resourceInfo, actoionInfo, accessRequest.RequestId);
//        }

//        private async Task<KeyValuePair<Guid, bool>> CheckAccessAADRequest(RemoteAadAuthorizationEngine client, Tuple<SubjectInfo, ResourceInfo, ActionInfo, Guid> aadRequest)
//        {
//            return new KeyValuePair<Guid, bool>(aadRequest.Item4, (await client.CheckAccessAsync(aadRequest.Item1, aadRequest.Item2, aadRequest.Item3, null, true)).IsAccessGranted);
//        }
//    }
//}