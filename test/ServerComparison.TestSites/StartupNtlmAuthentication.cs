// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.AspNet.Builder;
using Microsoft.AspNet.Http;
using Microsoft.AspNet.Http.Features;
using Microsoft.AspNet.Server.WebListener;
using Microsoft.Dnx.Runtime;
using Microsoft.Framework.Configuration;
using Microsoft.Framework.DependencyInjection;
using Microsoft.Framework.Logging;
using Microsoft.Net.Http.Server;

namespace ServerComparison.TestSites
{
    /// <summary>
    /// To make runtime to load an environment based startup class, specify the environment by the following ways: 
    /// 1. Drop a Microsoft.AspNet.Hosting.ini file in the wwwroot folder
    /// 2. Add a setting in the ini file named 'ASPNET_ENV' with value of the format 'Startup[EnvironmentName]'. For example: To load a Startup class named
    /// 'StartupNtlmAuthentication' the value of the env should be 'NtlmAuthentication' (eg. ASPNET_ENV=NtlmAuthentication). Runtime adds a 'Startup' prefix to this and loads 'StartupNtlmAuthentication'. 
    /// If no environment name is specified the default startup class loaded is 'Startup'. 
    /// Alternative ways to specify environment are:
    /// 1. Set the environment variable named SET ASPNET_ENV=NtlmAuthentication
    /// 2. For selfhost based servers pass in a command line variable named --env with this value. Eg:
    /// "commands": {
    ///    "web": "Microsoft.AspNet.Hosting --server Microsoft.AspNet.Server.WebListener --server.urls http://localhost:5002 --ASPNET_ENV NtlmAuthentication",
    ///  },
    /// </summary>
    public class StartupNtlmAuthentication
    {
        public StartupNtlmAuthentication(IApplicationEnvironment env)
        {
            //Below code demonstrates usage of multiple configuration sources. For instance a setting say 'setting1' is found in both the registered sources, 
            //then the later source will win. By this way a Local config can be overridden by a different setting while deployed remotely.
            var builder = new ConfigurationBuilder(env.ApplicationBasePath)
                        .AddJsonFile("config.json")
                        .AddEnvironmentVariables(); //All environment variables in the process's context flow in as configuration values.
            Configuration = builder.Build();
        }

        public IConfiguration Configuration { get; private set; }

        public void ConfigureServices(IServiceCollection services)
        {
        }

        public void Configure(IApplicationBuilder app, ILoggerFactory loggerFactory)
        {
            loggerFactory.AddConsole(minLevel: LogLevel.Warning);

            // Set up NTLM authentication for WebListener like below.
            // For IIS and IISExpress: Use inetmgr to setup NTLM authentication on the application vDir or modify the applicationHost.config to enable NTLM.
            var listener = app.ServerFeatures.Get<WebListener>();
            if (listener != null)
            {
                listener.AuthenticationManager.AuthenticationSchemes =
                    AuthenticationSchemes.Negotiate | AuthenticationSchemes.NTLM | AuthenticationSchemes.AllowAnonymous;
            }

            app.Use((context, next) => 
            {
                if (context.Request.Path.Equals("/Anonymous"))
                {
                    return context.Response.WriteAsync("Anonymous?" + !context.User.Identity.IsAuthenticated);
                }

                if (context.Request.Path.Equals("/Restricted"))
                {
                    if (context.User.Identity.IsAuthenticated)
                    {
                        return context.Response.WriteAsync(context.User.Identity.AuthenticationType);
                    }
                    else
                    {
                        return context.Authentication.ChallengeAsync();
                    }
                }

                if (context.Request.Path.Equals("/Forbidden"))
                {
                    return context.Authentication.ForbidAsync(string.Empty);
                }

                if (context.Request.Path.Equals("/AutoForbid"))
                {
                    return context.Authentication.ChallengeAsync();
                }

                if (context.Request.Path.Equals("/RestrictedNegotiate"))
                {
                    if (string.Equals("Negotiate", context.User.Identity.AuthenticationType, System.StringComparison.Ordinal))
                    {
                        return context.Response.WriteAsync("Negotiate");
                    }
                    else
                    {
                        return context.Authentication.ChallengeAsync("Negotiate");
                    }
                }

                if (context.Request.Path.Equals("/RestrictedNTLM"))
                {
                    if (string.Equals("NTLM", context.User.Identity.AuthenticationType, System.StringComparison.Ordinal))
                    {
                        return context.Response.WriteAsync("NTLM");
                    }
                    else
                    {
                        return context.Authentication.ChallengeAsync("NTLM");
                    }
                }

                return context.Response.WriteAsync("Hello World");
            });
        }
    }
}