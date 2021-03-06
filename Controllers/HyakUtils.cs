﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web.Hosting;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace ARMExplorer.Controllers
{
    public static class HyakUtils
    {
        static IEnumerable<MetadataObject> _speclessCsmApis = null;

        public static string CSMUrl
        {
            get;
            set;
        }

        public static IEnumerable<MetadataObject> GetSpeclessCsmOperations()
        {
            if (_speclessCsmApis == null)
            {
                _speclessCsmApis = GetRemoteCsmOperations().Concat(GetMissingApis());
            }
            return _speclessCsmApis;
        }

        private static IEnumerable<MetadataObject> GetRemoteCsmOperations()
        {
            using (var sReader = new StreamReader(HostingEnvironment.MapPath("~/App_Data/ProvidersSpecs/ProvidersList.json")))
            {
                var providersSpecs = sReader.ReadToEnd();

                var providersList = (JArray)(JsonConvert.DeserializeObject<JObject>(providersSpecs))["value"];
                var template = HyakUtils.CSMUrl + "/subscriptions/{subscriptionId}/resourceGroups/{resourceGroupName}/providers/";
                var fakeRequestBody = new { properties = new { }, location = string.Empty };
                return  providersList.Where(p => !new[] {
                            "Microsoft.Web",
                            "Microsoft.Compute",
                            "Microsoft.Storage",
                            "Microsoft.Network"
                        }.Any(str => p["namespace"].ToString().IndexOf(str, StringComparison.OrdinalIgnoreCase) >= 0))
                         .Select(provider =>
                         {
                             return provider["resourceTypes"].Select((resourceType) =>
                             {
                                 return new[] { new MetadataObject
                                     {
                                         MethodName = "GET",
                                         HttpMethod = "GET",
                                         Url = template + provider["namespace"] + "/" + ((string)resourceType["resourceType"]).Split('/').Aggregate((a, b) => a + "/{name}/" + b),
                                         ApiVersion = resourceType["apiVersions"].Select(s => s.ToString()).FirstOrDefault()
                                     },
                                     new MetadataObject
                                     {
                                         MethodName = "GET",
                                         HttpMethod = "GET",
                                         Url = template + provider["namespace"] + "/" + ((string)resourceType["resourceType"]).Split('/').Aggregate((a, b) => a + "/{name}/" + b) + "/{name}",
                                         ApiVersion = resourceType["apiVersions"].Select(s => s.ToString()).FirstOrDefault()
                                     },
                                     new MetadataObject
                                     {
                                         MethodName = "CreateOrUpdate",
                                         HttpMethod = "PUT",
                                         RequestBody = fakeRequestBody,
                                         Url = template + provider["namespace"] + "/" + ((string)resourceType["resourceType"]).Split('/').Aggregate((a, b) => a + "/{name}/" + b) + "/{name}",
                                         ApiVersion = resourceType["apiVersions"].Select(s => s.ToString()).FirstOrDefault()
                                     },
                                     new MetadataObject
                                     {
                                         MethodName = "Delete",
                                         HttpMethod = "DELETE",
                                         Url = template + provider["namespace"] + "/" + ((string)resourceType["resourceType"]).Split('/').Aggregate((a, b) => a + "/{name}/" + b) + "/{name}",
                                         ApiVersion = resourceType["apiVersions"].Select(s => s.ToString()).FirstOrDefault()
                                     }
                                 };
                             });
                         })
                         .SelectMany(i => i)
                         .SelectMany(i => i);
            }
        }

        private static IEnumerable<MetadataObject> GetMissingApis()
        {
            var fakeRequestBody = new { properties = new { }, location = string.Empty };
            var fakeSwapBody = new { targetSlot = string.Empty };
            return new List<MetadataObject> {
                new MetadataObject
                {
                    MethodName = "Get",
                    HttpMethod = "GET",
                    Url = HyakUtils.CSMUrl + "/subscriptions/{subscriptionId}/locations",
                    ApiVersion = Utils.CSMApiVersion
                },
                new MetadataObject
                {
                    MethodName = "Delete",
                    HttpMethod = "DELETE",
                    Url = HyakUtils.CSMUrl + "/subscriptions/{subscriptionId}/resourceGroups/{resourceGroupName}",
                    ApiVersion = Utils.CSMApiVersion
                },
                new MetadataObject
                {
                    MethodName = "Get",
                    HttpMethod = "GET",
                    Url = HyakUtils.CSMUrl + "/subscriptions/{subscriptionId}/resourceGroups/{resourceGroupName}",
                    ApiVersion = Utils.CSMApiVersion
                },
                new MetadataObject
                {
                    MethodName = "CreateOrUpdate",
                    HttpMethod = "PUT",
                    RequestBody = fakeRequestBody,
                    Url = HyakUtils.CSMUrl + "/subscriptions/{subscriptionId}/resourceGroups/{resourceGroupName}",
                    ApiVersion = Utils.CSMApiVersion
                },
                new MetadataObject
                {
                    MethodName = "Post",
                    HttpMethod = "POST",
                    Url = HyakUtils.CSMUrl + "/subscriptions/{subscriptionId}/resourceGroups/{resourceGroupName}/exportTemplate",
                    ApiVersion = "2016-02-01",
                    RequestBody = new {options = "IncludeParameterDefaultValue, IncludeComments", resources = new [] {"*"} }
                },
                new MetadataObject
                {
                    MethodName = "Get",
                    HttpMethod = "GET",
                    Url = HyakUtils.CSMUrl + "/subscriptions/{subscriptionId}/resourceGroups",
                    ApiVersion = Utils.CSMApiVersion
                },
                new MetadataObject
                {
                    MethodName = "Get",
                    HttpMethod = "GET",
                    Url = HyakUtils.CSMUrl + "/subscriptions",
                    ApiVersion = Utils.CSMApiVersion
                },
                new MetadataObject
                {
                    MethodName = "Get",
                    HttpMethod = "GET",
                    Url = HyakUtils.CSMUrl + "/subscriptions/{subscriptionId}",
                    ApiVersion = Utils.CSMApiVersion
                },
                new MetadataObject
                {
                    MethodName = "GetInstanceView",
                    HttpMethod = "GET",
                    Url = HyakUtils.CSMUrl + "/subscriptions/{subscriptionId}/resourceGroups/{resourceGroupName}/providers/Microsoft.Compute/virtualMachines/{vmName}/InstanceView",
                    ApiVersion = "2015-05-01-preview"
                },
                new MetadataObject
                {
                    MethodName = "Get",
                    HttpMethod = "GET",
                    Url = HyakUtils.CSMUrl + "/subscriptions/{subscriptionId}/resourceGroups/{resourceGroupName}/providers/Microsoft.Resources/deployments",
                    ApiVersion = "2015-11-01"
                },
                new MetadataObject
                {
                    MethodName = "Get",
                    HttpMethod = "GET",
                    Url = HyakUtils.CSMUrl + "/subscriptions/{subscriptionId}/resourceGroups/{resourceGroupName}/providers/Microsoft.Resources/deployments/{name}",
                    ApiVersion = "2015-11-01"
                },
                new MetadataObject
                {
                    MethodName = "Delete",
                    HttpMethod = "DELETE",
                    Url = HyakUtils.CSMUrl + "/subscriptions/{subscriptionId}/resourceGroups/{resourceGroupName}/providers/Microsoft.Resources/deployments/{name}",
                    ApiVersion = "2015-11-01"
                },
                new MetadataObject
                {
                    MethodName = "Post",
                    HttpMethod = "POST",
                    Url = HyakUtils.CSMUrl + "/subscriptions/{subscriptionId}/resourceGroups/{resourceGroupName}/providers/Microsoft.Resources/deployments/{name}/cancel",
                    ApiVersion = "2015-11-01"
                },
                new MetadataObject
                {
                    MethodName = "Post",
                    HttpMethod = "POST",
                    Url = HyakUtils.CSMUrl + "/subscriptions/{subscriptionId}/resourceGroups/{resourceGroupName}/providers/Microsoft.Resources/deployments/{name}/exportTemplate",
                    ApiVersion = "2016-02-01"
                },
                new MetadataObject
                {
                    MethodName = "Get",
                    HttpMethod = "GET",
                    Url = HyakUtils.CSMUrl + "/subscriptions/{subscriptionId}/resourceGroups/{resourceGroupName}/providers/Microsoft.Resources/deployments/{name}/operations",
                    ApiVersion = "2015-11-01"
                },
                new MetadataObject
                {
                    MethodName = "Get",
                    HttpMethod = "GET",
                    Url = HyakUtils.CSMUrl + "/subscriptions/{subscriptionId}/resourceGroups/{resourceGroupName}/providers/Microsoft.Resources/deployments/{name}/operations/{operationId}",
                    ApiVersion = "2015-11-01"
                },
            };
        }
    }
}
