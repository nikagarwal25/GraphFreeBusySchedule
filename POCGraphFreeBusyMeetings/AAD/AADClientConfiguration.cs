using System;
using System.Collections.Generic;
using System.Text;

namespace POCGraphFreeBusyMeetings.AAD
{
    //----------------------------------------------------------------------------
    // <copyright company="Microsoft Corporation" file="AADClientConfiguration.cs">
    // Copyright (c) Microsoft Corporation. All rights reserved.
    // </copyright>
    //----------------------------------------------------------------------------

        using System.Collections.Generic;
        using System.Linq;

        public class AADClientConfiguration
        {
            /// <summary>
            /// Gets or sets AAD App ID
            /// </summary>
            public string ClientId { get; set; }

            /// <summary>
            /// Gets or sets AAD client certificate for token issuance
            /// </summary>
            public string ClientCertificateThumbprints { get; set; }

            /// <summary>
            /// Gets or sets graph AAD instance URL
            /// </summary>
            public string AADInstance { get; set; }

            /// <summary>
            /// Access the certificate thumbprints as list.
            /// </summary>
            public IList<string> ClientCertificateThumbprintList
            {
                get
                {
                    var thumbprints = new List<string>();

                    if (!string.IsNullOrWhiteSpace(ClientCertificateThumbprints))
                    {
                        thumbprints.AddRange(ClientCertificateThumbprints.Split(',').Select(t => t.Trim()));
                    }

                    return thumbprints;
                }
            }
        }
    

}
