using System;
using System.Collections.Generic;
using System.Text;

namespace POCGraphFreeBusyMeetings.AAD
{
    //----------------------------------------------------------------------------
    // <copyright company="Microsoft Corporation" file="IAzureActiveDirectoryClient.cs">
    // Copyright (c) Microsoft Corporation. All rights reserved.
    // </copyright>
    //----------------------------------------------------------------------------
        using Microsoft.IdentityModel.Clients.ActiveDirectory;
        using POCGraphFreeBusyMeetings.Enums;
        using System.Threading.Tasks;

        /// <summary>
        /// Azure Active Directory client interface
        /// </summary>
        public interface IAzureActiveDirectoryClient
        {
            /// <summary>
            /// Gets an Access token for the given resource on behalf of the user in the provided access token
            /// </summary>      
            /// <param name="userAccessToken">The access token</param>
            /// <param name="tokenCachingOptions">Used to indicate if we prefer to refresh or prefer cache</param>
            /// <returns>AuthenticationResult object</returns>
            Task<AuthenticationResult> GetAccessTokenFromUserTokenAsync(
                string userAccessToken,
                TokenCachingOptions tokenCachingOptions);
            
        }
    }
