using System;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;

namespace GoogleTranslateAPI.Security
{
    public class CertificateValidation
    {
        private string[] BlacklistedClientCertificateThumbprints = new string[] {};

        private string RootCertificatePath { get; set; }

        public CertificateValidation(string rootCertificatePath)
        {
            this.RootCertificatePath = rootCertificatePath;
        }

        public bool ClientCertificateValidation(X509Certificate2 certificate, X509Chain chain, SslPolicyErrors errors)
        {
            try
            {
                var certificateAuthority = new X509Certificate2(RootCertificatePath);
                
                X509Chain certificateAuthorityChain = new X509Chain();
                certificateAuthorityChain.ChainPolicy.RevocationMode = X509RevocationMode.NoCheck;
                certificateAuthorityChain.ChainPolicy.VerificationFlags = X509VerificationFlags.AllFlags;

                certificateAuthorityChain.ChainPolicy.ExtraStore.Add(certificateAuthority);

                if (certificateAuthorityChain.Build(certificate))
                {
                    foreach (var item in certificateAuthorityChain.ChainStatus)
                    {
                        // If the chain status is anything other than an untrusted root, throw an exception
                        // Untrusted root is valid because this is a self-signed cert
                        if (item.Status != X509ChainStatusFlags.UntrustedRoot && item.Status != X509ChainStatusFlags.NoError)
                            throw new Exception("Unable to validate certificate. Chain Status: {0}, Chain Info: {1}" + item.Status.ToString() + item.StatusInformation);
                    }

                    foreach (var blacklistedPrint in BlacklistedClientCertificateThumbprints)
                    {
                        if (blacklistedPrint.ToLower() == certificate.Thumbprint.ToLower())
                            throw new Exception("Certificate has been blacklisted");
                    }

                    return true; // Cert is valid
                }
                else
                    return false; // Cert is not valid
            }
            catch (Exception e)
            {
                Console.WriteLine("Unable to validate certificate. {0}" + e.Message);
                return false; // Cert is not valid
            }
        }
    }
}