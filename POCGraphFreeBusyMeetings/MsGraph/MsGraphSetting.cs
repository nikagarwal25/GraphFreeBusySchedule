﻿using System;
using System.Collections.Generic;
using System.Text;

namespace POCGraphFreeBusyMeetings.MsGraph
{
        using System;
        using System.Collections.Generic;

        
        public class MsGraphSetting
        {
            /// <summary>
            /// Gets or sets AAD graph resource ID
            /// </summary>
            public string AADGraphResourceId { get; set; }

            /// <summary>
            /// Gets or sets AAD graph base URL
            /// </summary>
            public string AADGraphBaseUrl { get; set; }

            /// <summary>
            /// Gets or sets AAD graph tenant take over URL
            /// </summary>
            public string AADGraphTenantTakeOverUrl { get; set; }

            /// <summary>
            /// Gets or sets Microsoft graph resource ID
            /// </summary>
            public string GraphResourceId { get; set; }

            /// <summary>
            /// Gets or sets Microsoft graph base URL
            /// </summary>
            public string GraphBaseUrl { get; set; }

            /// <summary>
            /// Gets or sets graph tenant
            /// </summary>
            public string GraphTenant { get; set; }

            /// <summary>
            /// Gets or sets the subscription notification url.
            /// </summary>
            public string NotificationUrl { get; set; }

            /// <summary>
            /// Gets or sets certificate thumbprint
            /// </summary>
            [Obsolete("Use CertThumbprints to access thumbprints.")]
            public string CertThumbPrint { get; set; }

            /// <summary>
            /// Gets or sets certificate thumbprints
            /// </summary>
            public string CertThumbPrints { get; set; }

            /// <summary>
            /// Gets or sets Graph Client Id
            /// </summary>
            public string ClientId { get; set; }

            /// <summary>
            /// Access the certificate thumbprints as list.
            /// </summary>
            public IList<string> CertThumbprintList => MultipleCertificateThumprints.GetThumbprints(CertThumbPrints, CertThumbPrint);
        
    }

}
