using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace POCGraphFreeBusyMeetings.Models
{
    public class FindFreeBusyScheduleResponse
    {
        /// <summary>
        /// An SMTP address of the user, distribution list, or resource, identifying an instance of scheduleInformation.
        /// </summary>
        [JsonProperty(PropertyName = "scheduleId")]
        public string ScheduleId { get; set; }

        /// <summary>
        /// Represents a merged view of availability of all the items in scheduleItems. The view consists of time slots. Availability during each time slot is indicated with: 0= free, 1= tentative, 2= busy, 3= out of office, 4= working elsewhere.
        /// </summary>
        [JsonProperty(PropertyName = "availabilityView")]
        public string AvailabilityView { get; set; }

        /// <summary>
        /// Error information from attempting to get the availability of the user, distribution list, or resource.
        /// </summary>
        [JsonProperty(PropertyName = "error")]
        public FreeBusyError Error { get; set; }

        /// <summary>
        /// Contains the items that describe the availability of the user or resource.
        /// </summary>
        [JsonProperty(PropertyName = "scheduleItems")]
        public List<ScheduleItem> ScheduleItems { get; set; }

        /// <summary>
        /// The days of the week and hours in a specific time zone that the user works. These are set as part of the user's mailboxSettings.
        /// </summary>
        [JsonProperty(PropertyName = "workingHours")]
        public WorkingHours WorkingHours { get; set; }
    }
}
