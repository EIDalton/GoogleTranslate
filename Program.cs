using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Net;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Server.Kestrel;
using System.Security.Cryptography.X509Certificates;
using Microsoft.AspNetCore.Server.Kestrel.Https;
using System.Security.Authentication;

namespace GoogleTranslateAPI
{
    
    public class Program
    {
        private static String rootCertificateName = "ca.iq-x.co.crt";
        public static void Main(string[] args)
        {
            X509Store store = new X509Store(StoreName.Root, StoreLocation.CurrentUser);
            store.Open(OpenFlags.ReadWrite);
            store.Add(new X509Certificate2(X509Certificate2.CreateFromCertFile(rootCertificateName)));
            store.Close();

            var host = new WebHostBuilder()
            .UseKestrel(options =>
            {
                options.Listen(IPAddress.Any, 5000, listenOptions => 
                {
                    var serverCert = new X509Certificate2("translate.iq-x.co.pfx", "P4$$w0RD");
                    var certValidation = new Security.CertificateValidation(rootCertificateName);                    

                    HttpsConnectionAdapterOptions httpsOptions = new HttpsConnectionAdapterOptions();
                    httpsOptions.ServerCertificate = serverCert;                    
                    httpsOptions.ClientCertificateMode = ClientCertificateMode.RequireCertificate; // Change to require
                    httpsOptions.CheckCertificateRevocation = false;
                    httpsOptions.ClientCertificateValidation = certValidation.ClientCertificateValidation;

                    listenOptions.UseHttps(httpsOptions);
                });
            })
            .ConfigureLogging((hostingContext, logging) =>
                {
                    logging.AddConfiguration(hostingContext.Configuration.GetSection("Logging"));
                    logging.AddConsole();
                    logging.AddDebug();
                })            
            .UseContentRoot(Directory.GetCurrentDirectory())
            .ConfigureLogging((hostingContext, logging) =>
            {
                logging.AddConfiguration(hostingContext.Configuration.GetSection("Logging"));
                logging.AddConsole();
                logging.AddDebug();
            })
            .UseIISIntegration()
            .UseStartup<Startup>()
            .Build();

            host.Run();
        }
    }
}