﻿
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace POCGraphFreeBusyMeetings.Models
{
    [DataContract]
    public class FreeBusyRequest
    {
        /// <summary>
        /// Gets or sets the users.
        /// </summary>
        [DataMember(Name = "userGroups")]
        public List<UserGroup> UserGroups { get; set; }

        /// <summary>
        /// Gets or sets the start
        /// </summary>
        [DataMember(Name = "utcStart", IsRequired = false, EmitDefaultValue = false)]
        public DateTime UtcStart { get; set; }

        /// <summary>
        /// Gets or sets the end
        /// </summary>
        [DataMember(Name = "utcEnd", IsRequired = false, EmitDefaultValue = false)]
        public DateTime UtcEnd { get; set; }

        /// <summary>
        /// Gets or sets the isRoom
        /// </summary>
        [DataMember(Name = "isRoom", IsRequired = false, EmitDefaultValue = false)]
        public bool IsRoom { get; set; }
    }
}
