﻿using System;
using System.IO;
using Crimson.Core;
using Crimson.Repository;
using Crimson.Utils;
using Crimson.ViewModels;
using Crimson.Views;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.UI.Xaml;
using Serilog;

namespace Crimson
{
    /// <summary>
    /// Provides application-specific behavior to supplement the default Application class.
    /// </summary>
    public partial class App : Application
    {
        // The .NET Generic Host provides dependency injection, configuration, logging, and other services.
        // https://docs.microsoft.com/dotnet/core/extensions/generic-host
        // https://docs.microsoft.com/dotnet/core/extensions/dependency-injection
        // https://docs.microsoft.com/dotnet/core/extensions/configuration
        // https://docs.microsoft.com/dotnet/core/extensions/logging
        public IHost Host
        {
            get;
        }

        /// <summary>
        /// Initializes the singleton application object.  This is the first line of authored code
        /// executed, and as such is the logical equivalent of main() or WinMain().
        /// </summary>
        public App()
        {
            InitializeComponent();

            Host = Microsoft.Extensions.Hosting.Host.
            CreateDefaultBuilder().
            UseContentRoot(AppContext.BaseDirectory).
            ConfigureServices((context, services) =>
            {
                services.AddSingleton<SettingsManager>();
                services.AddSingleton<ILogger>(provider =>
                {
                    var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
                    _ = Directory.CreateDirectory($@"{appDataPath}\Crimson\logs");
                    var logFilePath = $@"{appDataPath}\Crimson\logs\{DateTime.Now:yyyy-MM-dd}.txt";

                    return new LoggerConfiguration()
                        .MinimumLevel.Information()
                        .WriteTo.File(
                            logFilePath,
                            rollingInterval: RollingInterval.Month,
                            rollOnFileSizeLimit: true,
                            retainedFileCountLimit: 30,
                            outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}"
                        )
                        .CreateLogger();
                });
                services.AddSingleton<Storage>();
                services.AddScoped<IStoreRepository, EpicGamesRepository>();
                services.AddSingleton<AuthManager>();
                services.AddSingleton<LibraryManager>();
                services.AddSingleton<InstallManager>();

                services.AddTransient<SettingsViewModel>();
            }).
            Build();
        }

        /// <summary>
        /// Invoked when the application is launched normally by the end user.  Other entry points
        /// will be used such as when the application is launched to open a specific file.
        /// </summary>
        /// <param name="args">Details about the launch request and process.</param>
        protected override void OnLaunched(Microsoft.UI.Xaml.LaunchActivatedEventArgs args)
        {
            m_window = new MainWindow();
            m_window.Activate();
            m_window.Closed += OnExit;
        }

        // Save gamedata to storage on application exit
        private static async void OnExit(object sender, object e)
        {
            //await LibraryManager.UpdateJsonFileAsync();
        }

        private Window m_window;

        public Window GetWindow()
        {
            return m_window;
        }
        public static T GetService<T>() where T : class
        {
            if ((App.Current as App)!.Host.Services.GetService(typeof(T)) is not T service)
            {
                throw new ArgumentException($"{typeof(T)} needs to be registered in ConfigureServices within App.xaml.cs.");
            }

            return service;
        }
    }
}
