using Microsoft.Graph;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using Newtonsoft.Json;
using POCGraphFreeBusyMeetings.Enums;
using POCGraphFreeBusyMeetings.Models;
using POCGraphFreeBusyMeetings.MsGraph.Microsoft.D365.HCM.Common.MSGraph;
using POCGraphFreeBusyMeetings.Utilities;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace POCGraphFreeBusyMeetings
{
    public class Program
    {
        /// <summary>
        /// JSON serializer settings for http calls.
        /// </summary>
        private static readonly JsonSerializerSettings jsonSerializerSettings;

        public static IMsGraphProvider graphProvider { get; set; }

        public static void Main(string[] args)
        {
            RunPOC().Wait();
        }

        public static async Task RunPOC()
        {
            var uberLevelUserGroup = new UserGroup
            {
                FreeBusyTimeId = "UberUserGroup",
                Users = new List<GraphPerson>()
            };

            string userAccessToken = string.Empty;
            string freeBusyRequestString = @"{
  'userGroups': [
    {
      'freeBusyTimeId': '457b98d0-b43b-9fcc-15cb-517e148f8322',
      'users': [
        {
          'name': 'GTA Test',
          'id': '9827d274-bb63-40e9-aba2-fafebd8c3e8a',
          'email': 'gtatest@gtasch.onmicrosoft.com',
          'givenName': null,
          'surname': null,
          'image': null
        }
      ]
    }
  ],
  'utcEnd': '2019-08-29T18:30:00.000Z',
  'utcStart': '2019-08-28T18:30:00.000Z'
}";
            var freeBusyRequest = JsonConvert.DeserializeObject<FreeBusyRequest>(freeBusyRequestString);
            foreach (var userGroup in freeBusyRequest.UserGroups)
            {
                if (userGroup?.Users != null)
                {
                    uberLevelUserGroup.Users = uberLevelUserGroup.Users.Concat(userGroup.Users).ToList();
                }
            }

            uberLevelUserGroup.Users = uberLevelUserGroup.Users.Distinct().ToList();
            var requestFreeBusy = GenerateFreeBusyScheduleRequest(freeBusyRequest, uberLevelUserGroup.Users);
            var responsesFreeBusyRaw = await SendPostFindFreeBusySchedule(userAccessToken, requestFreeBusy) ?? new List<FindFreeBusyScheduleResponse>();

        }

        private static async System.Threading.Tasks.Task<List<FindFreeBusyScheduleResponse>> SendPostFindFreeBusySchedule(string token, FindFreeBusyScheduleRequest findFreeBusyRequest)
        {
            var findFreeBusyResponsePayload = new FindFreeBusyScheduleResponsePayload();
            var findFreeBusyResponses = new List<FindFreeBusyScheduleResponse>();
            var graphResourceId = "https://graph.microsoft.com";

            using (var httpClient = new HttpClient())
            {
                httpClient.DefaultRequestHeaders.Authorization = await GetBearerTokenFromUserToken(token, TokenCachingOptions.ForceRefreshCache);

                // Serialize the request object.
                var tasks = new List<Task>();
                var schedules = findFreeBusyRequest.Schedules;
                foreach (var scheduleBatch in Chunk(schedules, 20))
                {
                    tasks.Add(Task.Run(async () =>
                    {
                        for (int i = 1; i <= 5; i++)
                        {
                            findFreeBusyRequest.Schedules = (List<string>)scheduleBatch;
                            var requestData = JsonConvert.SerializeObject(findFreeBusyRequest, jsonSerializerSettings);
                            using (var response = await httpClient.PostAsync(graphResourceId + "/v1.0/me/calendar/getschedule", new StringContent(requestData, Encoding.UTF8, "application/json")))
                            {
                                var responseHeaders = response.Headers.ToString();
                                //this.Trace.TraceInformation($"Response headers for find meeting times are {responseHeaders}");
                                if (response.IsSuccessStatusCode)
                                {
                                    // Read and deserialize response.
                                    var content = await response.Content.ReadAsStringAsync();

                                    findFreeBusyResponsePayload = JsonConvert.DeserializeObject<FindFreeBusyScheduleResponsePayload>(content, jsonSerializerSettings);

                                    if (findFreeBusyResponsePayload.Value?.Count > 0)
                                    {
                                        findFreeBusyResponses.AddRange(findFreeBusyResponsePayload.Value);
                                    }
                                    i = 5;
                                }
                                else
                                {
                                    string content = string.Empty;
                                    if (response != null)
                                    {
                                        content = await response.Content.ReadAsStringAsync();
                                    }

                                    if (i < 5)// && EmailUtils.ShouldRetryOnGraphException(response.StatusCode))
                                    {
                                        //this.Trace.TraceWarning($"Attempt {i} : Exception during {HttpMethod.Post.Method}:{this.graphResourceId}/me/calendar/getschedule call to graph. Response {response.StatusCode.ToString()} with error message {content}");
                                        //await EmailUtils.ExponentialDelay(response, i);
                                    }
                                    else
                                    {
                                        //this.Trace.TraceError($"Exception during {HttpMethod.Post.Method}:{this.graphBaseUrl}/me/getschedule call to graph. Response {response.StatusCode.ToString()} with error message {content}");
                                        break;
                                    }
                                }
                            }
                        }
                    }));
                }
                Task.WaitAll(tasks.ToArray());
                findFreeBusyRequest.Schedules = schedules;
            }
            return findFreeBusyResponses;
        }
        private static async Task<AuthenticationHeaderValue> GetBearerTokenFromUserToken(string userAccessToken, TokenCachingOptions tokenCachingOptions = TokenCachingOptions.PreferCache)
        {
            if (userAccessToken.ToLower().StartsWith("bearer "))
            {
                userAccessToken = userAccessToken.Remove(0, 7);
            }

            // var resourceToken = await graphProvider.GetResourceAccessTokenFromUserToken(userAccessToken, tokenCachingOptions);

            //  return await GetAuthenticationHeaderValueAsync();

            return new AuthenticationHeaderValue("Bearer", await GetGraphToken());
        }

        private static FindFreeBusyScheduleRequest GenerateFreeBusyScheduleRequest(FreeBusyRequest freeBusyRequest, List<GraphPerson> interviewers)
        {
            var request = new FindFreeBusyScheduleRequest()
            {
                Schedules = interviewers.Select(interviewer => interviewer.Email).ToList(),
                AvailabilityViewInterval = SchedulerConstants.ThirtyMinuteFreeBusy,
                StartTime = new Models.MeetingDateTime { DateTime = freeBusyRequest.UtcStart.ToString(), TimeZone = SchedulerConstants.UTCTimezone },
                EndTime = new Models.MeetingDateTime { DateTime = freeBusyRequest.UtcEnd.ToString(), TimeZone = SchedulerConstants.UTCTimezone },
            };

            return request;
        }

        private static IEnumerable<IList<T>> Chunk<T>(List<T> elements, int chunkSize)
        {
            if (elements != null)
            {
                for (var minIndex = 0; minIndex < elements.Count; minIndex += chunkSize)
                {
                    yield return elements.GetRange(minIndex, Math.Min(chunkSize, elements.Count - minIndex));
                }
            }
        }
        public static async Task<AuthenticationHeaderValue> GetAuthenticationHeaderValueAsync()
        {
            try
            {
                var authContext = new AuthenticationContext("https://login.windows.net/12db0337-13c3-4c81-9ae8-2de8bb21c52c");

                string AadClientSecret = @"ZZ-febKE-+2bHezT.-2wn1Z2L[V3ix";

                var authResult = await authContext
                    .AcquireTokenAsync("https://graph.microsoft.com",
                        new ClientCredential("ZZ16709-2860-43fa-b609-af133c58fe0f", AadClientSecret));
                return new AuthenticationHeaderValue("Bearer", authResult.AccessToken);
            }
            catch (Exception ex)
            {

                return null;
            }
        }

        public async static Task<string> GetGraphToken()
        {
            // var principal = ServiceContext.Principal.TryGetCurrent<HCMApplicationPrincipal>();
            string userAccessToken = "GetUserToken";

            //string userAccessToken = principal.EncryptedUserToken;
            UserAssertion userAssertion = new UserAssertion(userAccessToken, "urn:ietf:params:oauth:grant-type:jwt-bearer");
            // UserAssertion userAssertion = new UserAssertion(userAccessToken);

            string aadInstance = "https://login.microsoftonline.com/{0}";
            string tenant = "ZZ697574-167c-4ff2-bda8-989f9afc867f";
            string authority = string.Format(CultureInfo.InvariantCulture, aadInstance, tenant);
            AuthenticationContext authContext = new AuthenticationContext(authority);
            var clientValue = @"ZZZZZDM8/*a1guLn_6ccZJaiZ4pyLuHr++y";

            ClientCredential clientCredential = new ClientCredential("ZZeb1b17-c7f1-4433-b119-8cf3c67451ef", clientValue);
            
            var result = await authContext.AcquireTokenAsync("https://graph.microsoft.com", clientCredential, userAssertion);
            return result.AccessToken;
        }


    }
}
