using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace POCGraphFreeBusyMeetings.Models
{
    public class FindFreeBusyScheduleResponsePayload
    {
        /// <summary>
        /// gets or sets odata context.
        /// </summary>
        [JsonProperty(PropertyName = "context")]
        public string OdataContext { get; set; }

        /// <summary>
        /// List of free busy responses.
        /// </summary>
        [JsonProperty(PropertyName = "value")]
        public List<FindFreeBusyScheduleResponse> Value { get; set; }
    }
}
