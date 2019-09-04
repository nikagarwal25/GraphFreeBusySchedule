using System;
using System.Collections.Generic;
using System.Text;

namespace POCGraphFreeBusyMeetings.MsGraph
{
    //----------------------------------------------------------------------------
    // <copyright company="Microsoft Corporation" file="MsGraphProvider.cs">
    // Copyright (c) Microsoft Corporation. All rights reserved.
    // </copyright>
    //----------------------------------------------------------------------------

    using System.Web;

    namespace Microsoft.D365.HCM.Common.MSGraph
    {
        using System;
        using System.Threading.Tasks;
        using POCGraphFreeBusyMeetings.AAD;
        using POCGraphFreeBusyMeetings.Enums;

        /// <summary>
        /// Microsoft Graph Provider class
        /// </summary>
        [Obsolete("Please use the user directory service client implementation for any future work on MSGraph, any questions/issues email vanguard@microsoft.com. Going forward this will turn into an error.")]
        public class MsGraphProvider : IMsGraphProvider
        {

            /// <summary>
            /// Azure active directory client instance
            /// </summary>
            private readonly AzureActiveDirectoryClient azureActiveDirectoryClient;

            /// <summary>
            /// AAD client configuration
            /// </summary>
            private readonly AADClientConfiguration aadClientConfig;

            /// <summary>
            /// Graph configuration
            /// </summary>
            private readonly MsGraphSetting graphConfig;

            private const string requestIdConst = "request-id";

            /// <summary>Initializes a new instance of the<see cref="MsGraphProvider"/> class.</summary>
            /// <param name="configurationManager">Configuration manager object</param>
            /// <param name="aadClient">aad client</param>
            /// <param name="trace">Trace source object</param>
            /// <param name="secretManager">Secret manager.</param>
            /// <param name="memCacheName">Optional memory cache name</param>
            /// <param name="tenantId">The tenant Id. If passed the graph client will act in "App" mode, acquiring a token against the graph without a user context.</param>
            public MsGraphProvider(
              )
            {

                this.azureActiveDirectoryClient = new AzureActiveDirectoryClient(this.aadClientConfig, this.graphConfig);              

            }
            
            /// <summary>
            /// Gets an access token for the graph resource Id from the user token.
            /// </summary>
            /// <param name="token">User token.</param>
            /// <param name="tokenCachingOptions">Token caching options.</param>
            /// <returns>Resource access token.</returns>
            [Obsolete("Please use the user directory service client implementation for any future work on MSGraph, any questions/issues email vanguard@microsoft.com. Going forward this will turn into an error.")]
            public async Task<string> GetResourceAccessTokenFromUserToken(string token, TokenCachingOptions tokenCachingOptions = TokenCachingOptions.PreferCache)
            {
                var result = await this.azureActiveDirectoryClient.GetAccessTokenFromUserTokenAsync(
                    token,
                    tokenCachingOptions);

                return result.AccessToken;
            }
          
        }
    }
}
