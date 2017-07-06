﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.TeamFoundation.WorkItemTracking.WebApi;
using Microsoft.TeamFoundation.WorkItemTracking.WebApi.Models;
using Microsoft.VisualStudio.Services.Common;
using Microsoft.VisualStudio.Services.WebApi.Patch.Json;
using Microsoft.VisualStudio.Services.WebApi.Patch;
using Microsoft.VisualStudio.Services.WebApi;
using System.Net.Http.Headers;
using System.Net.Http;
using Newtonsoft.Json;

namespace Microsoft.TeamServices.Samples.Client.QuickStarts
{
    [ClientSample(WitConstants.WorkItemTrackingWebConstants.RestAreaName, "quickstarts")]
    public class CreateBug : ClientSample
    {
        readonly string _uri;
        readonly string _personalAccessToken;
        readonly string _project;

        /// <summary>
        /// Constructor. Manaully set values to match your account.
        /// </summary>
        public CreateBug()
        {
            _uri = "https://basicprocess.visualstudio.com";
            _personalAccessToken = "6orjg6yrismbi2qhpdrluwcmwdofk6uv6ebm2h5sn6yvn7zbhmba";
            _project = "project name";
        }

        /// <summary>
        /// Create a bug using the .NET client library
        /// </summary>
        /// <returns>Microsoft.TeamFoundation.WorkItemTracking.WebApi.Models.WorkItem</returns>
        [ClientSampleMethod]
        public WorkItem CreateBugUsingClientLib()
        {
            Uri uri = new Uri(_uri);
            string personalAccessToken = _personalAccessToken;
            string project = _project;

            VssBasicCredential credentials = new VssBasicCredential("", _personalAccessToken);

            JsonPatchDocument patchDocument = new JsonPatchDocument();

            //add fields and thier values to your patch document
            patchDocument.Add(
                new JsonPatchOperation()
                {
                    Operation = Operation.Add,
                    Path = "/fields/System.Title",
                    Value = "Authorization Errors"
                }
            );

            patchDocument.Add(
                new JsonPatchOperation()
                {
                    Operation = Operation.Add,
                    Path = "/fields/Microsoft.VSTS.TCM.ReproSteps",
                    Value = "Our authorization logic needs to allow for users with Microsoft accounts (formerly Live Ids) - http:// msdn.microsoft.com/en-us/library/live/hh826547.aspx"
                }
            );

            patchDocument.Add(
                new JsonPatchOperation()
                {
                    Operation = Operation.Add,
                    Path = "/fields/Microsoft.VSTS.Common.Priority",
                    Value = "1"
                }
            );

            patchDocument.Add(
                new JsonPatchOperation()
                {
                    Operation = Operation.Add,
                    Path = "/fields/Microsoft.VSTS.Common.Severity",
                    Value = "2 - High"
                }
            );

            VssConnection connection = new VssConnection(uri, credentials);
            WorkItemTrackingHttpClient workItemTrackingHttpClient = connection.GetClient<WorkItemTrackingHttpClient>();

            try
            {
                WorkItem result = workItemTrackingHttpClient.CreateWorkItemAsync(patchDocument, project, "Bug").Result;

                Console.WriteLine("Bug Successfully Created: Bug #{0}", result.Id);              

                return result;
            }
            catch (AggregateException ex)
            {
                Console.WriteLine("Error creating bug: {0}", ex.InnerException.Message);
                return null;
            }            
        }

        /// <summary>
        /// Create a bug using direct HTTP
        /// </summary>
        [ClientSampleMethod]
        public WorkItem CreateBugUsingHTTP()
        {
            string uri = _uri;
            string personalAccessToken = _personalAccessToken;
            string project = _project;
            string credentials = Convert.ToBase64String(System.Text.ASCIIEncoding.ASCII.GetBytes(string.Format("{0}:{1}", "", personalAccessToken)));

            Object[] patchDocument = new Object[4];

            patchDocument[0] = new { op = "add", path = "/fields/System.Title", value = "Authorization Errors" };
            patchDocument[1] = new { op = "add", path = "/fields/Microsoft.VSTS.TCM.ReproSteps", value = "Our authorization logic needs to allow for users with Microsoft accounts (formerly Live Ids) - http://msdn.microsoft.com/en-us/library/live/hh826547.aspx" };
            patchDocument[2] = new { op = "add", path = "/fields/Microsoft.VSTS.Common.Priority", value = "1" };
            patchDocument[3] = new { op = "add", path = "/fields/Microsoft.VSTS.Common.Severity", value = "2 - High" };

            using (var client = new HttpClient())
            {
                //set our headers
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", credentials);

                //serialize the fields array into a json string
                var patchValue = new StringContent(JsonConvert.SerializeObject(patchDocument), Encoding.UTF8, "application/json-patch+json");

                var method = new HttpMethod("PATCH");
                var request = new HttpRequestMessage(method, uri + "/" + project + "/_apis/wit/workitems/$Bug?api-version=2.2") { Content = patchValue };
                var response = client.SendAsync(request).Result;

                //if the response is successfull, set the result to the workitem object
                if (response.IsSuccessStatusCode)
                {
                    var workItem = response.Content.ReadAsAsync<WorkItem>().Result;

                    Console.WriteLine("Bug Successfully Created: Bug #{0}", workItem.Id); 
                    return workItem
                }
                else
                {
                    Console.WriteLine("Error creating bug: {0}", response.Content);
                    return null;
                }
            }
        }
    }
}