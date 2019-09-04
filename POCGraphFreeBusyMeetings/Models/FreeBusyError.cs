using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace POCGraphFreeBusyMeetings.Models
{
    [DataContract]
    public class FreeBusyError
    {
        /// <summary>
        /// Gets or sets error message
        /// </summary>
        [DataMember(Name = "message")]
        public string Message { get; set; }

        /// <summary>
        /// Gets or sets resposne code
        /// </summary>
        [DataMember(Name = "responseCode")]
        public string ResponseCode { get; set; }
    }
}
