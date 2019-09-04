using System;
using System.Collections.Generic;
using System.Text;

namespace POCGraphFreeBusyMeetings.Enums
{
    public enum TokenCachingOptions
    {
        /// <summary>
        /// Issue a call to the AAD and update the cache accordingly
        /// </summary>
        ForceRefreshCache,

        /// <summary>
        /// Issue a call to cache and up to AAD if not found in cache
        /// </summary>
        PreferCache,
    }
}
