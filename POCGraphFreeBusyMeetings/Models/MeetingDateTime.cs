using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace POCGraphFreeBusyMeetings.Models
{
    public class MeetingDateTime
    {
        /// <summary>
        /// Gets or sets the meeting date and time.
        /// </summary>
        [DataMember(Name = "dateTime")]
        public string DateTime { get; set; }

        /// <summary>
        /// Gets or sets the meeting time zone.
        /// </summary>
        [DataMember(Name = "timeZone")]
        public string TimeZone { get; set; }
    }
}
