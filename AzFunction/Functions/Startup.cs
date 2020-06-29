// -------------------------------------------------------------------------------
// <copyright file="Startup.cs" company="Microsoft Corporation">
//      Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
// <IMPORTANT>
//    The following dependencies are pinned to a specific version of the
//    Nuget packages because the function host runtime has specific versions
//    of these same dependencies. If they don't match, type mismatch errors
//    will occur while loading.
//
//    Microsoft.Extensions.Http.dll                             v2.2.0.0
//    Microsoft.Extensions.DependencyInjection.Abstractions.dll v2.2.0.0
//    Microsoft.Extensions.DependencyInjection.dll              v2.2.0.0
//    Microsoft.Extensions.Logging.Console.dll                  v2.2.0.0
// </IMPORTANT>
// -------------------------------------------------------------------------------

using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Enterprise.Authorization.Client;
using Microsoft.Enterprise.Authorization.Client.Middleware;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;

// This provides the host runtime the object to load
[assembly: FunctionsStartup(typeof(Functions.Startup))]

namespace Functions
{
    /// <summary>
    /// Used to initialize the 'host' aka AppDomain for all functions
    /// in this application.
    /// </summary>
    public sealed class Startup : FunctionsStartup, IDisposable
    {
        /// <summary>
        /// Used to signal startup with partner app
        /// </summary>
        private Mutex mutex = null;

        /// <summary>
        /// Constructor
        /// </summary>
        public Startup()
        {
            if (String.Compare(Environment.GetEnvironmentVariable(
                Constants.runtimeEnvironmentVariable),
                Constants.runtimeEnvironmentValue,
                StringComparison.InvariantCultureIgnoreCase) == 0)
            {
                // Delay signalling this app is running to give time for the rest
                // of the Kestrel middleware initialization
                Task.Run(() =>
                {
                    Thread.Sleep(2000);
                    // Initialize the mutex signalled to indicate server host ready
                    mutex = new Mutex(true, Constants.globalMutexName);
                });
            }
        }

        /// <summary>
        /// Clean up resources
        /// </summary>
        public void Dispose()
        {
            if (mutex != null)
            {
                mutex.ReleaseMutex();
                mutex.Dispose();
            }
        }

        /// <summary>
        /// Configures host provided services. Runtime uses dependency
        /// injection to share / create dependent objects as needed.
        /// </summary>
        /// <param name="builder"></param>
        public override void Configure(IFunctionsHostBuilder builder)
        {
            builder.Services.AddHttpClient();
            builder.Services.AddSingleton<ILoggerProvider, ConsoleLoggerProvider>();

            // Load configuration from appSettingsFile
            var config = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile(Constants.appSettingsFile, optional: true, reloadOnChange: true)
                .Build();

            // Save authorization specific settings for downstream services
            builder.Services.AddOptions<AuthorizationClientOptions>().Configure((clientOptions) =>
            {
                config.Bind(Constants.authClientSettingsSection, clientOptions);
                config.Bind(Constants.azureAdSettingsSection, clientOptions.AuthenticationOptions);
            });

            // Configure the authorization client SDK
            builder.Services.AddAadAuthorization(clientOptions =>
            {
                config.Bind(Constants.authClientSettingsSection, clientOptions);
                config.Bind(Constants.azureAdSettingsSection, clientOptions.AuthenticationOptions);
            });

            // Save identity provider settings for downstream services
            builder.Services.AddOptions<IdentityProviderOptions>().Configure((idProviderOptions) =>
            {
                config.Bind(Constants.identityProvider, idProviderOptions);
            });

            // Helper class to use as SDK wrapper
            builder.Services.AddScoped<IAuthorizationHelper, AuthorizationHelper>();
        }
    }
}