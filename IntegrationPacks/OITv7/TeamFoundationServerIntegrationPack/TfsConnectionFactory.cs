using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using Microsoft.TeamFoundation.Client;

namespace TeamFoundationServerIntegrationPack
{
    /// <summary>
    ///     Class factory for TfsTeamProjectCollection objects and enable other helper classes to use the same instance of
    ///     object.  It caches object with and without NetworkCredential, and indexed by server URL.
    /// </summary>
    static class TfsConnectionFactory
    {
        private static Dictionary<string, TfsTeamProjectCollection> s_serverCache = new Dictionary<string, TfsTeamProjectCollection>();

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
        public static TfsTeamProjectCollection GetTeamProjectCollection(
            string url,
            string domain,
            string userName,
            string password)
        {
            Debug.Assert(!string.IsNullOrEmpty(url));

            var invariantUrl = url.Trim().ToUpperInvariant();
            TfsTeamProjectCollection tfs = null;

            lock (s_serverCache)
            {
                if (s_serverCache.ContainsKey(invariantUrl))
                {
                    tfs = s_serverCache[invariantUrl];
                }

                if (tfs == null)
                {
                    if (string.IsNullOrEmpty(userName))
                    {
                        tfs = new TfsTeamProjectCollection(new Uri(invariantUrl));
                    }
                    else
                    {
                        var cred = new NetworkCredential(userName, password, domain);
                        tfs = new TfsTeamProjectCollection(new Uri(invariantUrl), cred);
                        tfs.EnsureAuthenticated();
                    }

                    s_serverCache[invariantUrl] = tfs;
                }
            }

            return tfs;
        }
    }
}
