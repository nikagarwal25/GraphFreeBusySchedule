using System;
using System.Collections.Generic;
using System.Text;

namespace POCGraphFreeBusyMeetings.MsGraph
{
    //----------------------------------------------------------------------------
    // <copyright company="Microsoft Corporation" file="IMsGraphProvider.cs">
    // Copyright (c) Microsoft Corporation. All rights reserved.
    // </copyright>
    //----------------------------------------------------------------------------

    namespace Microsoft.D365.HCM.Common.MSGraph
    {
        using System;
        using System.Threading.Tasks;
        using POCGraphFreeBusyMeetings.Enums;
               
        /// <summary>
        /// Graph provider interface
        /// </summary>
        [Obsolete("Please use the user directory service client implementation for any future work on MSGraph, any questions/issues email vanguard@microsoft.com. Going forward this will turn into an error.")]
        public interface IMsGraphProvider
        {
            /// <summary>
            /// Gets an access token for the graph resource Id from the user token.
            /// </summary>
            /// <param name="token">User token.</param>
            /// <param name="tokenCachingOptions">Token caching options.</param>
            /// <returns>Resource access token.</returns>
            [Obsolete("Please use the user directory service client implementation for any future work on MSGraph, any questions/issues email vanguard@microsoft.com. Going forward this will turn into an error.")]
            Task<string> GetResourceAccessTokenFromUserToken(string token, TokenCachingOptions tokenCachingOptions = TokenCachingOptions.PreferCache);

             }
    }

}
