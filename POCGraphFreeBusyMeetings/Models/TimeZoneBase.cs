using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace POCGraphFreeBusyMeetings.Models
{
    [DataContract]
    public class TimeZoneBase
    {
        /// <summary>
        /// The name of a time zone. It can be a standard time zone name such as "Hawaii-Aleutian Standard Time", or "Customized Time Zone" for a custom time zone.
        /// </summary>
        [DataMember(Name = "name")]
        public string Name { get; set; }
    }
}
