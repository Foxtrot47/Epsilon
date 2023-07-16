using System;
using System.Linq;
using System.Runtime.InteropServices;
using Microsoft.UI.Composition.SystemBackdrops;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Animation;
using Microsoft.UI.Xaml.Navigation;
using WinRT;
using System.Threading.Tasks;
using static Epsilon.Core.Legendary;
using static System.Net.WebRequestMethods;
using Windows.Storage;
using Serilog;
using Epsilon.Core;

namespace Epsilon;

/// <summary>
///     Main Window
/// </summary>
public sealed partial class MainWindow : Window
{
    public bool IsLoggedIn;
    private string _legendaryBinaryPath;
    public ILogger Log;

    public MainWindow()
    {
        InitializeComponent();
        Title = "Epsilon";
        IsLoggedIn = false;
        Task.Run(async () =>
        {
            var localFolder = ApplicationData.Current.LocalFolder;
            var logFilePath = $@"{localFolder.Path}\logs\{DateTime.Now:yyyy-MM-dd}.txt";
            Log = new LoggerConfiguration().WriteTo.File(logFilePath).CreateLogger();
            Log.Information("Starting up");

            var res = await Legendary.DownloadBinaryAsync(localFolder, Log);
            _legendaryBinaryPath = res.Path;
            var legendaryInstance = new Legendary(_legendaryBinaryPath, Log);
            legendaryInstance.CheckAuthentication();
            legendaryInstance.AuthenticationStatusChanged += HandleAuthenticationChanges;
        });

    }
    private void navControl_BackRequested(NavigationView sender,
        NavigationViewBackRequestedEventArgs args)
    {
        if (!ContentFrame.CanGoBack)
            return;

        // Don't go back if the nav pane is overlayed.
        if (NavControl.IsPaneOpen &&
            NavControl.DisplayMode is NavigationViewDisplayMode.Compact or NavigationViewDisplayMode.Minimal)
            return;

        ContentFrame.GoBack();
    }

    private void navControl_ItemInvoked(NavigationView sender, NavigationViewItemInvokedEventArgs args)
    {
        //if (args.IsSettingsSelected == true)
        //{
        //    NavView_Navigate(typeof(SettingsPage), args.RecommendedNavigationTransitionInfo);
        //}
        //else
        if (args.InvokedItemContainer != null)
        {
            var navPageType = Type.GetType(args.InvokedItemContainer.Tag.ToString() ?? string.Empty);
            navControl_Navigate(navPageType, args.RecommendedNavigationTransitionInfo);
        }
    }

    private void navControl_Navigate(
        Type navPageType,
        NavigationTransitionInfo transitionInfo)
    {
        // Get the page type before navigation so you can prevent duplicate
        // entries in the backstack.
        var preNavPageType = ContentFrame.CurrentSourcePageType;

        // Only navigate if the selected page isn't currently loaded.
        if (navPageType is not null && !Equals(preNavPageType, navPageType))
            ContentFrame.Navigate(navPageType, null, transitionInfo);
    }

    private void UpdateUIBasedOnAuthenticationStatus(AuthenticationStatus authStatus)
    {
        LoginModal.Visibility = Visibility.Visible;
        Log.Information($"Auth status: {authStatus}");

        switch (authStatus)
        {
            case AuthenticationStatus.Checking:
                LoginModalTitle.Text = "Logging in to Epic Games Store";
                LoginModalDescription.Text = "Please wait...";
                break;

            case AuthenticationStatus.LoginWindowOpen:
                LoginModalTitle.Text = "Logging in to Epic Games Store";
                LoginModalDescription.Text = "Please switch to the opened window";
                break;

            case AuthenticationStatus.LoggedIn:
                Log.Information("Logged in");
                LoginModalTitle.Text = "Login Success";
                LoginModalDescription.Text = "Please wait...";

                StateManager.Initialize(_legendaryBinaryPath, Log);
                _ = StateManager.UpdateLibraryAsync();

                InstallManager.Initialize(_legendaryBinaryPath, Log);

                LoginModal.Visibility = Visibility.Collapsed;
                NavControl.SelectedItem = NavControl.MenuItems[0];
                navControl_Navigate(typeof(LibraryPage), new EntranceNavigationTransitionInfo());
                Log.Information("Opening Library Page");
                break;

            case AuthenticationStatus.LoginFailed:
                LoginModalTitle.Text = "Login failed";
                LoginModalDescription.Text = "Please try again";
                break;
        }
    }

    private void HandleAuthenticationChanges(AuthenticationStatus authStatus)
    {
        DispatcherQueue.TryEnqueue(() => UpdateUIBasedOnAuthenticationStatus(authStatus));
    }
}


