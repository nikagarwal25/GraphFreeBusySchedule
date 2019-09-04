using System;
using System.Collections.Generic;
using System.Text;

namespace POCGraphFreeBusyMeetings.AAD
{
    //----------------------------------------------------------------------------
    // <copyright company="Microsoft Corporation" file="AzureActiveDirectoryClient.cs">
    // Copyright (c) Microsoft Corporation. All rights reserved.
    // </copyright>
    //----------------------------------------------------------------------------

    using System;
    using System.IdentityModel.Tokens.Jwt;
    using System.Linq;
    using System.Security.Cryptography.X509Certificates;
    using System.Text.RegularExpressions;
    using System.Threading.Tasks;
    using POCGraphFreeBusyMeetings;
    using System.Collections.Generic;
    using POCGraphFreeBusyMeetings.Enums;
    using Microsoft.IdentityModel.Clients.ActiveDirectory;
    using Microsoft.Extensions.Caching.Memory;
    using POCGraphFreeBusyMeetings.Utilities;
    using System.Diagnostics;
    using POCGraphFreeBusyMeetings.MsGraph;

    /// <summary>
    /// AAD client class
    /// </summary>
    public sealed class AzureActiveDirectoryClient : IAzureActiveDirectoryClient
    {
        /// <summary>
        /// token cache expiry in minutes
        /// </summary>
        private const int ExpiryMinutes = -5;

        /// <summary>
        /// The AAD constant for identity provider;
        /// </summary>
        private const string IdentityProvider = "idp";

        /// <summary>
        /// Trace source instance
        /// </summary>
        private static TraceSource trace;

        /// <summary>
        /// Certificate Manager
        /// </summary>
        private CertificateManager certificateManager;

        /// <summary>
        /// Memory cache instance
        /// </summary>
        private readonly MemoryCache accessTokenCache;

        /// <summary>
        /// AAD Client configuration
        /// </summary>
        private readonly AADClientConfiguration aadClientConfig;

        /// <summary>
        /// Graph setting
        /// </summary>
        private readonly MsGraphSetting graphConfig;

        /// <summary>
        /// Initializes a new instance of the <see cref="AzureActiveDirectoryClient" /> class.
        /// </summary>
        /// <param name="aadClientConfig">AAD client config</param>
        /// <param name="graphConfig">Graph setting object</param>
        public AzureActiveDirectoryClient(AADClientConfiguration aadClientConfig, MsGraphSetting graphConfig)
        {
            //AzureActiveDirectoryClient.trace = trace;
            this.aadClientConfig = aadClientConfig;
            this.graphConfig = graphConfig;
            //this.accessTokenCache = new MemoryCache(new MemoryCacheOptions());
            // this.certificateManager = new CertificateManager();
        }

        /// <summary>Gets or sets HCM user principal.</summary>    
        public IHCMApplicationPrincipal Principal { get; set; }


        /// <summary>
        /// Gets an Access token for the given resource on behalf of the user in the provided access token
        /// </summary>
        /// <param name="userAccessToken">The access token</param>
        /// <param name="tenantId">Tenant ID</param>
        /// <param name="resource">Resource ID</param>
        /// <param name="tokenCachingOptions">Token caching options</param>
        /// <returns>Authentication result which contains on behalf of token</returns>
        public async Task<AuthenticationResult> GetAccessTokenForResourceFromUserTokenAsync(
            string userAccessToken,
            string tenantId,
            string resource,
            TokenCachingOptions tokenCachingOptions)
        {
            trace.TraceInformation("Start GetAccessTokenForResourceFromUserTokenAsync");
            var aadInstance = GetAADInstance(this.aadClientConfig.AADInstance, tenantId);
            var exceptions = new List<Exception>();
            var thumbprints = this.aadClientConfig.ClientCertificateThumbprintList;
            var userName = this.GetUserName();
            AuthenticationResult result = null;

            try
            {
                if (tokenCachingOptions == TokenCachingOptions.PreferCache &&
                    this.TryGetAccessToken(resource, tenantId, userName, out result))
                {
                    trace.TraceInformation("Retrieved access token from cache.");
                    return result;
                }

                foreach (var thumbprint in thumbprints)
                {
                    try
                    {
                        // Construct context
                        var authority = this.aadClientConfig.AADInstance.FormatWithInvariantCulture(tenantId);
                        var context = new AuthenticationContext(authority, false);
                        context.CorrelationId = new Guid();

                        // Construct client assertion certificate
                        var certificate = this.certificateManager.FindByThumbprint(thumbprint, StoreName.My, StoreLocation.LocalMachine);
                        var clientAssertionCertificate = new ClientAssertionCertificate(this.aadClientConfig.ClientId, certificate);

                        // User Assertion
                        if (string.IsNullOrEmpty(userAccessToken))
                        {
                            trace.TraceInformation("Calling AcquireTokenAsync without User Assertion.");
                            result = await context.AcquireTokenAsync(resource, clientAssertionCertificate);
                        }
                        else
                        {
                            trace.TraceInformation("Calling AcquireTokenAsync with User Assertion.");
                            var userAssertion = new UserAssertion(TrimBearerToken(userAccessToken));

                            result = await context.AcquireTokenAsync(resource, clientAssertionCertificate, userAssertion);
                        }

                        trace.TraceInformation($"Requesting access token for Resource: '{resource}', AADInstance: '{aadInstance}', ClientID: '{this.aadClientConfig.ClientId}, CorrelationId: '{context.CorrelationId}'");

                        if (!string.IsNullOrEmpty(userName))
                        {
                            // Set Cache
                            this.SetAccessTokenCache(this.graphConfig.GraphResourceId, this.graphConfig.GraphTenant, userName, result);
                        }

                        return result;
                    }
                    catch (AdalServiceException ex)
                    {
                       // trace.TraceWarning($"AdalServiceException: error code- {ex.ErrorCode}, error message- {ex.Message}");
                        exceptions.Add(ex);
                        //}
                        //catch (CertificateNotFoundException ex)
                        //{
                        //    exceptions.Add(ex);
                    }
                    catch (Exception ex)
                    {
                        exceptions.Add(ex);
                        break;
                    }
                }
            }
            catch (AdalException exception)
            {
                HandleAzureActiveDirectoryClientException(exception);
                return null;
            }

            throw new AggregateException($"Could not successfully acquire certificate using thumbprints: {string.Join(", ", aadClientConfig.ClientCertificateThumbprintList)}", exceptions);
        }

        /// <summary>
        /// Get on-behalf-of user access token
        /// </summary>
        /// <param name="userAccessToken">Authenticated client access token taken from request header</param>
        /// <param name="tokenCachingOptions">Token caching options</param>
        /// <returns>Authentication result which contains on behalf of token</returns>
        public async Task<AuthenticationResult> GetAccessTokenFromUserTokenAsync(
            string userAccessToken,
            TokenCachingOptions tokenCachingOptions)
        {
            /* Note: In most cases this won't apply as we don't need this for anything but cases where we recieved a hybrid token.
             * Most of the time we will use "common".
             * We need to read the token and check to see if it has an identity provider claim or IDP.
             * This is needed for instances where apps may use a "hybrid" token where it has app claims and user claims mixed together.
             * This will only happen in instances where a token is generated when the issuer is tenant x but we are requesting a token for an app in tenant y
             * and specifically ask that the authority be tenant y. We do this so that the issuer is listed as tenant y instead of x. When this happens the issuer
             * and tenant in the token can no longer be tenant x so it's moved into IDP. This is needed for validation on the server in instances where
             * we only want another app calling and/or we want to see the user token claims with the app token claims.
             * See here for more details: https://docs.microsoft.com/en-us/azure/active-directory/develop/active-directory-token-and-claims#access-tokens
             */

            var tenant = this.graphConfig.GraphTenant;
            if (!string.IsNullOrEmpty(userAccessToken))
            {
                var readToken = (new JwtSecurityTokenHandler()).ReadJwtToken(userAccessToken);
                var identityProviderClaim = readToken.Claims.FirstOrDefault(c => c.Type == IdentityProvider);

                if (identityProviderClaim != null)
                {
                    var tenantMatch = (new Regex(@"https:\/\/sts\.windows\.net\/([0-9A-Za-z-.]*)\/?")).Match(identityProviderClaim.Value);
                    if (tenantMatch.Success)
                    {
                        tenant = tenantMatch.Groups[1].Value;
                        trace.TraceInformation($"Tenant {tenant} was matched from the identity provider claim and will be used to generate a token instead of common");
                    }
                    else
                    {
                        //trace.TraceWarning($"Identity provider claim {identityProviderClaim} was provided but was not matched");
                    }
                }
            }

            return await this.GetAccessTokenForResourceFromUserTokenAsync(userAccessToken, tenant, this.graphConfig.GraphResourceId, tokenCachingOptions);
        }


        /// <summary>
        /// Creates custom AAD Client exception
        /// </summary>
        /// <param name="adalException">ADAL Exception object</param>
        private static void HandleAzureActiveDirectoryClientException(AdalException adalException)
        {
            if (!string.IsNullOrEmpty(adalException.ErrorCode))
            {
                //trace.TraceWarning($"AzureActiveDirectoryClientException;ErrorCode={adalException.ErrorCode}, AdalException={adalException}");
            }
            else
            {
                //trace.TraceWarning($"AzureActiveDirectoryClientException;ErrorMessage={adalException.Message}, AdalException={adalException}");
            }
        }

        /// <summary>
        /// Construct cache key
        /// </summary>
        /// <param name="resourceId">Graph resource ID</param>
        /// <param name="tenant">Graph tenant name (example: common)</param>
        /// <param name="userName">User principal name</param>
        /// <returns>Cache key for on-behalf-of token</returns>
        private static string GetAccessTokenCacheKey(string resourceId, string tenant, string userName)
        {
            trace.TraceInformation($"Retrieve AccessTokenCacheKey: {resourceId}-{tenant}-username");
            return FormattableString.Invariant($"{resourceId}-{tenant}-{userName}");
        }

        /// <summary>
        /// Add tenant to AAD instance URL
        /// </summary>
        /// <param name="aadInstanceUrl">AAD instance URL</param>
        /// <param name="tenant">Graph tenant name</param>
        /// <returns>Formatted URL</returns>
        private static string GetAADInstance(string aadInstanceUrl, string tenant)
        {
            trace.TraceInformation($"GetAADInstance for {tenant}");
            return aadInstanceUrl.FormatWithInvariantCulture(tenant);
        }

        /// <summary>
        /// Trims bearer token
        /// </summary>
        /// <param name="bearerToken">Bearer token taken from request header</param>
        /// <returns>Trimmed token</returns>
        private static string TrimBearerToken(string bearerToken)
        {
            if (bearerToken.StartsWith(Constants.BearerAuthenticationScheme, StringComparison.OrdinalIgnoreCase))
            {
                bearerToken = bearerToken.Substring(Constants.BearerAuthenticationScheme.Length).Trim();
            }

            return bearerToken;
        }

        /// <summary>
        /// Get user identifier 
        /// </summary>
        /// <returns>email address or user name string</returns>
        private string GetUserName()
        {
            var userName = string.Empty;

            //// Case where service context is not set try to use the current principal property set by the caller
            //// This happens when this is called from service fabric middleware like HCMAuthorizer

            var currentPrincipal = this.Principal;
            trace.TraceInformation("Using HCM user principle");

            if (currentPrincipal != null)
            {
                userName = string.IsNullOrEmpty(currentPrincipal.UserPrincipalName) ? currentPrincipal.EmailAddress : currentPrincipal.UserPrincipalName;
            }

            trace.TraceInformation($"Get userName from principle.");
            return userName;
        }

        /// <summary>
        /// Check MemoryCache to see if token is cached
        /// </summary>
        /// <param name="resourceId">Graph resource ID</param>
        /// <param name="tenant">Graph tenant name</param>
        /// <param name="userName">User identifier</param>
        /// <param name="authenticationResult">Authentication result object</param>
        /// <returns>True if token is cached</returns>
        private bool TryGetAccessToken(string resourceId, string tenant, string userName, out AuthenticationResult authenticationResult)
        {
            trace.TraceInformation($"Getting cached access token for resource '{resourceId}'");
            authenticationResult = (AuthenticationResult)this.accessTokenCache.Get(GetAccessTokenCacheKey(resourceId, tenant, userName));
            trace.TraceInformation($"Token retrieved: {authenticationResult?.GetHashCode()}");
            return (authenticationResult != null) && (authenticationResult.ExpiresOn > DateTime.UtcNow);
        }

        /// <summary>
        /// Cache on-behalf-of user access token
        /// </summary>
        /// <param name="resourceId">Graph resource ID</param>
        /// <param name="tenant">Graph tenant name</param>
        /// <param name="userName">User identifier</param>
        /// <param name="authenticationResult">Authentication result object</param>
        private void SetAccessTokenCache(string resourceId, string tenant, string userName, AuthenticationResult authenticationResult)
        {
            trace.TraceInformation($"Setting cached access token for resource '{resourceId}'");
            var cacheExpirationOptions =
                new MemoryCacheEntryOptions
                {
                    AbsoluteExpiration = DateTime.Now.AddMinutes(ExpiryMinutes),
                    Priority = CacheItemPriority.Normal
                };

            var res = (GetAccessTokenCacheKey(resourceId, tenant, userName), authenticationResult);
            this.accessTokenCache.Set(res, DateTime.Now.ToString(), cacheExpirationOptions);

            trace.TraceInformation($"Token set: {authenticationResult?.GetHashCode()}");
        }

    }
}

