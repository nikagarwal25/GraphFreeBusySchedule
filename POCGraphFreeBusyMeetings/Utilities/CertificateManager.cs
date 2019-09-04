using System;
using System.Collections.Generic;
using System.Text;

namespace POCGraphFreeBusyMeetings.Utilities
{
    //----------------------------------------------------------------------------
    // Copyright (c) Microsoft Corporation. All rights reserved.
    //----------------------------------------------------------------------------

    using System.Collections.Concurrent;
    using System.Security.Cryptography.X509Certificates;
    using System.Threading.Tasks;

    
        /// <summary>
        /// A certificate helper to do certificate operations.
        /// </summary>
        public sealed class CertificateManager
        {
            private readonly bool onlyFindValidCertificates = true;
            private readonly ConcurrentDictionary<string, X509Certificate2> certificateCache = new ConcurrentDictionary<string, X509Certificate2>();

            public CertificateManager()
            {
            }

            internal CertificateManager(bool onlyFindValidCertificates)
            {
                this.onlyFindValidCertificates = onlyFindValidCertificates;
            }

            /// <summary>
            /// Find certificate by thumbprint in the given store name and location.
            /// </summary>
            public async Task<X509Certificate2> FindByThumbprintAsync(string thumbprint, StoreName storeName, StoreLocation storeLocation)
            {
                return await Task.FromResult(this.FindByThumbprint(thumbprint, storeName, storeLocation));
            }

            /// <summary>
            /// Find certificate by thumbprint in the given store name and location.
            /// </summary>
            public X509Certificate2 FindByThumbprint(string thumbprint, StoreName storeName, StoreLocation storeLocation)
            {

                string cacheKey = $"{thumbprint}-{storeName}-{storeLocation}";
               
                        return certificateCache.GetOrAdd(
                            cacheKey,
                            (string keyToAdd) =>
                            {
                               // CertificateManagerTrace.Instance.TraceInformation($"Certificate with thumbprint {thumbprint} was not found in the cache. Attempting to load certificate from {storeName} {storeLocation}.");

                                var store = new X509Store(storeName, storeLocation);
                                try
                                {
                                    store.Open(OpenFlags.ReadOnly);

                                    var certificateCollection = store.Certificates.Find(X509FindType.FindByThumbprint, thumbprint, validOnly: onlyFindValidCertificates);
                                    if (certificateCollection == null || certificateCollection.Count == 0)
                                    {
                                        // throw new CertificateNotFoundException(thumbprint, storeName, storeLocation).EnsureTraced();
                                    }

                                    if (certificateCollection.Count > 1)
                                    {
                                       // throw new MultipleCertificatesMatchedException(thumbprint, storeName, storeLocation).EnsureTraced();
                                    }

                                    var pfxCert = certificateCollection[0];
                                   // CertificateManagerTrace.Instance.TraceVerbose($"Found certifcate {pfxCert.Thumbprint}. Has private key: {pfxCert.HasPrivateKey}.");

                                    return pfxCert;
                                }
                                finally
                                {
                                    store.Close();
                                }
                            });
            }
        }
    
}
