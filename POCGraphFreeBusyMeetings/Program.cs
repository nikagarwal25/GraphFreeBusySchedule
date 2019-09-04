using Newtonsoft.Json;
using POCGraphFreeBusyMeetings.Enums;
using POCGraphFreeBusyMeetings.Models;
using POCGraphFreeBusyMeetings.MsGraph.Microsoft.D365.HCM.Common.MSGraph;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
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
            RunPOC();
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
      'freeBusyTimeId': 'zz7b98d0-b43b-9fcc-15cb-517e148f8322',
      'users': [
        {
          'name': 'Nikita Agarwal',
          'id': 'zzdddac2-0069-4bf5-b638-f0753ca153d1',
          'email': 'zz@zz.com',
          'givenName': 'Nikita',
          'surname': 'Agarwal',
          'image': null
        }
      ]
    },
    {
      'freeBusyTimeId': 'a25854a4-4f7c-d4c3-658c-2146f22a817a',
      'users': [
        {
          'name': 'Gopal Pandey',
          'id': 'zz3d473a-05b5-4a92-98bc-0acee9dd9444',
          'email': 'zz@zz.com',
          'givenName': 'Gopal',
          'surname': 'Pandey',
          'image': null
        }
      ]
    }
  ],
  'utcEnd': '2019-08-29T18:30:00.000Z',
  'utcStart': '2019-08-28T18:30:00.000Z'
}";
            var freeBusyRequest = JsonConvert.DeserializeObject < FreeBusyRequest >(freeBusyRequestString);
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

            var resourceToken = "Bearer zz";
            //await graphProvider.GetResourceAccessTokenFromUserToken(userAccessToken, tokenCachingOptions);

            return new AuthenticationHeaderValue("Bearer", resourceToken);
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

        
    }
}
