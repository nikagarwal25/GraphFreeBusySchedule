
using Microsoft.Graph;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace POCGraphFreeBusyMeetings.Models
{
    /// <summary>
    /// Working hours.
    /// </summary>
    public class WorkingHours
    {
        /// <summary>
        /// The days of the week on which the user works.
        /// </summary>
        [JsonProperty(PropertyName = "daysOfWeek")]
        public List<string> DaysOfWeek { get; set; }

        /// <summary>
        /// The time of the day that the user starts working.
        /// </summary>
        [JsonProperty(PropertyName = "startTime")]
        public TimeOfDay StartTime { get; set; }

        /// <summary>
        /// The time of the day that the user ends working.
        /// </summary>
        [JsonProperty(PropertyName = "endTime")]
        public TimeOfDay EndTime { get; set; }

        /// <summary>
        /// The time zone to which the working hours apply.
        /// </summary>
        [JsonProperty(PropertyName = "timeZone")]
        public TimeZoneBase TimeZone { get; set; }
    }
}
