using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace POCGraphFreeBusyMeetings.Models
{
    public class FindFreeBusyScheduleRequest
    {
        /// <summary>
        /// Gets or sets the schedules.
        /// </summary>
        [JsonProperty(PropertyName = "schedules")]
        public List<string> Schedules { get; set; }

        /// <summary>
        /// Gets or sets availability interval (time slot)
        /// </summary>
        [JsonProperty(PropertyName = "availabilityViewInterval")]
        public string AvailabilityViewInterval { get; set; }

        /// <summary>
        /// Gets or sets the meeting start date time and time zone.
        /// </summary>
        [JsonProperty(PropertyName = "startTime")]
        public MeetingDateTime StartTime { get; set; }

        /// <summary>
        /// Gets or sets the meeting end date time and time zone.
        /// </summary>
        [JsonProperty(PropertyName = "endTime")]
        public MeetingDateTime EndTime { get; set; }
    }
}
