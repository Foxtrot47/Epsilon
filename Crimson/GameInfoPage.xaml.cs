using System;
using System.Linq;
using Windows.Storage.AccessCache;
using Windows.Storage.Pickers;
using Crimson.Core;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Imaging;
using Microsoft.UI.Xaml.Navigation;
using Serilog;
using WinRT.Interop;

namespace Crimson
{
    /// <summary>
    /// Page for Showing Details of individual game and allowing play
    /// download and other options
    /// </summary>
    public sealed partial class GameInfoPage : Page
    {
        //public Game Game { get; set; }

        public GameInfoPage()
        {
            this.InitializeComponent();
        }
        private readonly ILogger _log = ((App)Application.Current).Log;
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            //Game = LibraryManager.GetGameInfo((string)e.Parameter);
            //var gameImage = Game.Images.FirstOrDefault(i => i.Type == "DieselGameBox");
            //TitleImage.SetValue(Image.SourceProperty, gameImage != null ? new BitmapImage(new Uri(gameImage.Url)) : null);

            //CheckGameStatus(Game);

            // Unregister event handlers on start
            // LibraryManager.GameStatusUpdated -= CheckGameStatus;
            // LibraryManager.GameStatusUpdated += CheckGameStatus;
            InstallManager.InstallationStatusChanged -= HandleInstallationStatusChanged;
            InstallManager.InstallationStatusChanged += HandleInstallationStatusChanged;
            InstallManager.InstallProgressUpdate -= HandleInstallationStatusChanged;
            InstallManager.InstallProgressUpdate += HandleInstallationStatusChanged;

        }

        /// <summary>
        /// Check Game Status and update UI accordingly
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void DownloadButtonClick(object sender, RoutedEventArgs e)
        {
            //try
            //{
            //    _log.Information("GameInfoPage: Primary Action Button Clicked for {Game}", Game.Title);
            //    if (Game == null) return;
            //    if (Game.State == Game.InstallState.Installed)
            //    {
            //        _log.Information("GameInfoPage: Starting Game {Game}", Game.Title);
            //        //LibraryManager.StartGame(Game.Name);
            //        return;
            //    }

            //    ConfirmInstallTitleText.Text = Game.Title;
            //    ConfirmInstallImage.Source = Game.Images.FirstOrDefault(i => i.Type == "DieselGameBox") != null ? new BitmapImage(new Uri(Game.Images.FirstOrDefault(i => i.Type == "DieselGameBoxTall").Url)) : null;
            //    InstallLocationText.Text = "C:\\Games\\";
            //    ConfirmInstallDialog.MaxWidth = 4000;
            //    var downloadResult = await ConfirmInstallDialog.ShowAsync();

            //}
            //catch (Exception ex)
            //{
            //    _log.Error(ex.ToString());
            //    DownloadProgressRing.Visibility = Visibility.Collapsed;
            //    PrimaryActionButton.IsEnabled = true;
            //}
        }

        /// <summary>
        /// Handing Installation State Change.
        /// <br/>
        /// This function is never run on UI Thread.
        /// <br/>
        /// So always make sure to use Dispatcher Queue to update UI thread
        /// </summary>
        /// <param name="installItem"></param>
        private void HandleInstallationStatusChanged(InstallItem installItem)
        {
            try
            {
                if (installItem == null) return;
                DispatcherQueue.TryEnqueue(() =>
                {
                    _log.Information("GameInfoPage: Installation Status Changed for {Game}", installItem.AppName);
                    switch (installItem.Status)
                    {
                        case ActionStatus.Processing:
                            DownloadProgressRing.IsIndeterminate = false;
                            DownloadProgressRing.Value = Convert.ToDouble(installItem.ProgressPercentage);
                            DownloadProgressRing.Visibility = Visibility.Visible;
                            PrimaryActionButtonIcon.Visibility = Visibility.Collapsed;
                            PrimaryActionButton.IsEnabled = false;
                            PrimaryActionButtonText.Text = $"{installItem.ProgressPercentage}%";
                            break;

                        case ActionStatus.Pending:

                            PrimaryActionButtonText.Text = "Pending...";
                            DownloadProgressRing.Visibility = Visibility.Visible;
                            DownloadProgressRing.IsIndeterminate = true;
                            PrimaryActionButtonIcon.Visibility = Visibility.Collapsed;

                            break;
                        case ActionStatus.Cancelling:
                            PrimaryActionButtonText.Text = "Cancelling...";
                            DownloadProgressRing.Visibility = Visibility.Visible;
                            DownloadProgressRing.IsIndeterminate = true;
                            PrimaryActionButtonIcon.Visibility = Visibility.Collapsed;
                            break;
                    }
                });
            }
            catch (Exception ex)
            {
                _log.Error(ex.ToString());
            }
        }

        //private void CheckGameStatus(Game updatedGame)
        //{
            //if (updatedGame == null || updatedGame.Name != Game.Name) return;
            //_log.Information("GameInfoPage: Game Status Changed for {Game}", updatedGame.Title);
            //Game = updatedGame;

            //DispatcherQueue.TryEnqueue(() =>
            //{
            //    // Clear ui elements state
            //    PrimaryActionButtonText.Text = "";
            //    PrimaryActionButtonIcon.Glyph = "";

            //    if (Game.State == Game.InstallState.Installing || Game.State == Game.InstallState.Updating || Game.State == Game.InstallState.Repairing)
            //    {
            //        var gameInQueue = InstallManager.GameGameInQueue(Game.Name);
            //        if (gameInQueue == null)
            //        {
            //            // Default button text and glyph if game isn't in instllation queue yet
            //            PrimaryActionButtonText.Text = "Resume";
            //            PrimaryActionButtonIcon.Glyph = "\uE768";
            //        }
            //        HandleInstallationStatusChanged(gameInQueue);
            //        return;
            //    }
            //    PrimaryActionButtonIcon.Visibility = Visibility.Visible;
            //    DownloadProgressRing.Visibility = Visibility.Collapsed;
            //    PrimaryActionButton.IsEnabled = true;
            //    if (Game.State == Game.InstallState.NotInstalled)
            //    {
            //        PrimaryActionButtonText.Text = "Install";
            //        PrimaryActionButtonIcon.Glyph = "\uE896";
            //    }
            //    else if (Game.State == Game.InstallState.Installed)
            //    {
            //        PrimaryActionButtonText.Text = "Play";
            //        PrimaryActionButtonIcon.Glyph = "\uE768";
            //    }

            //    else if (Game.State == Game.InstallState.NeedUpdate)
            //    {
            //        PrimaryActionButtonText.Text = "Update";
            //        PrimaryActionButtonIcon.Glyph = "\uE777";
            //    }
            //    else if (Game.State == Game.InstallState.Broken)
            //    {
            //        PrimaryActionButtonText.Text = "Repair";
            //        PrimaryActionButtonIcon.Glyph = "\uE90F";
            //    }
            //});
        // }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            // Unregister both event handlers before navigating out
            //LibraryManager.GameStatusUpdated -= CheckGameStatus;
            InstallManager.InstallationStatusChanged -= HandleInstallationStatusChanged;
            InstallManager.InstallProgressUpdate -= HandleInstallationStatusChanged;

            // Call the base implementation
            base.OnNavigatedFrom(e);
        }

        private void ToggleModalButton_OnClick(object sender, RoutedEventArgs e)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        ///  Handles the Click event of the InstallLocationChangeButton control.
        /// </summary>
        private async void InstallLocationChangeButton_OnClick(object sender, RoutedEventArgs e)
        {
            //// Create a folder picker
            //var openPicker = new FolderPicker();

            //var window = ((App)Application.Current).GetWindow();
            //var hWnd = WinRT.Interop.WindowNative.GetWindowHandle(window);

            //// Initialize the folder picker with the window handle (HWND).
            //InitializeWithWindow.Initialize(openPicker, hWnd);

            //// Set options for your folder picker
            //openPicker.SuggestedStartLocation = PickerLocationId.Desktop;
            //openPicker.FileTypeFilter.Add("*");

            //// Open the picker for the user to pick a folder
            //var folder = await openPicker.PickSingleFolderAsync();
            //if (folder == null) return;
            //StorageApplicationPermissions.FutureAccessList.AddOrReplace("PickedFolderToken", folder);
            //InstallLocationText.Text = folder.Path;

        }

        private void ConfirmInstallCloseButton_OnClick(object sender, RoutedEventArgs e)
        {
            ConfirmInstallDialog.Hide();
        }
        private async void PrimaryButton_Click(object sender, RoutedEventArgs e)
        {
            //ConfirmInstallDialog.Hide();
            //PrimaryActionButton.IsEnabled = false;
            //PrimaryActionButtonText.Text = "Pending...";
            //DownloadProgressRing.Visibility = Visibility.Visible;
            //DownloadProgressRing.IsIndeterminate = true;
            //PrimaryActionButtonIcon.Visibility = Visibility.Collapsed;
            //LibraryManager.AddToInstallationQueue(Game.Name, ActionType.Install, InstallLocationText.Text);
            //_log.Information("GameInfoPage: Added {Game} to Installation Queue", Game.Title);
        }
    }
}
