using System;
using System.Collections.ObjectModel;
using Crimson.Core;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Imaging;
using Serilog;

namespace Crimson
{
    /// <summary>
    /// Page where we list current and past downloads
    /// </summary>
    public sealed partial class DownloadsPage : Page
    {
        private DownloadManagerItem _currentInstallItem = new DownloadManagerItem();
        private ObservableCollection<DownloadManagerItem> queueItems = new();
        private ObservableCollection<DownloadManagerItem> historyItems = new();

        private readonly ILogger _log;
        private readonly InstallManager _installManager;
        public DownloadsPage()
        {
            this.InitializeComponent();
            _log = DependencyResolver.Resolve<ILogger>();
            _log.Information("DownloadsPage: Loading Page");

            _installManager = DependencyResolver.Resolve<InstallManager>();

            DataContext = _currentInstallItem;
            if (_installManager.CurrentInstall?.AppName == null)
                ActiveDownloadSection.Visibility = Visibility.Collapsed;

            var gameInQueue = _installManager.CurrentInstall;
            HandleInstallationStatusChanged(gameInQueue);
            FetchQueueItemsList();
            FetchHistoryItemsList();
            _installManager.InstallationStatusChanged += HandleInstallationStatusChanged;
            _installManager.InstallProgressUpdate += InstallationProgressUpdate;
        }

        // Handing Installtion State Change
        // This function is never run on UI Thread
        // So always make sure to use Dispatcher Queue to update UI thread
        private void HandleInstallationStatusChanged(InstallItem installItem)
        {
            try
            {
                _log.Information("HandleInstallationStatusChanged: Handling Installation Status Change");
                FetchQueueItemsList();
                FetchHistoryItemsList();
                if (installItem == null)
                {
                    _log.Information("HandleInstallationStatusChanged: No installation in progress");
                    DispatcherQueue.TryEnqueue(() =>
                    {
                        ActiveDownloadSection.Visibility = Visibility.Collapsed;
                    });
                    return;
                }
                DispatcherQueue.TryEnqueue(() =>
                {
                    ActiveDownloadSection.Visibility = Visibility.Visible;
                    DownloadProgressBar.IsIndeterminate = true;

                    //var gameInfo = LibraryManager.GetGameInfo(installItem.AppName);
                    //_log.Information("HandleInstallationStatusChanged: Game Info: {GameInfo}", gameInfo);
                    //_currentInstallItem = new DownloadManagerItem
                    //{
                    //    Name = gameInfo.Name,
                    //    Title = gameInfo.Title,
                    //    InstallState = gameInfo.State,
                    //    Image = Util.GetBitmapImage(gameInfo.Images.FirstOrDefault(image => image.Type == "DieselGameBoxTall")
                    //        ?.Url)
                    //};
                    CurrentDownloadTitle.Text = _currentInstallItem.Title;
                    CurrentDownloadImage.Source = _currentInstallItem.Image;
                });
                _log.Information("HandleInstallationStatusChanged: Installation Status: {Status}", installItem.Status);
                switch (installItem.Status)
                {
                    case ActionStatus.Processing:
                        DispatcherQueue.TryEnqueue(() =>
                        {
                            DownloadProgressBar.IsIndeterminate = false;
                            DownloadProgressBar.Value = Convert.ToDouble(installItem.ProgressPercentage);
                            CurrentDownloadAction.Text = $@"{installItem.Action}ing";
                            CurrentDownloadedSize.Text = $@"{Util.ConvertMiBToGiBOrMiB(installItem.DownloadedSize)} of {Util.ConvertMiBToGiBOrMiB(installItem.TotalDownloadSizeMb)}";
                            CurrentDownloadSpeed.Text = $@"{installItem.DownloadSpeedRaw} MiB/s";
                        });
                        break;
                    case ActionStatus.Success:
                    case ActionStatus.Failed:
                    case ActionStatus.Cancelled:
                        DispatcherQueue.TryEnqueue(() =>
                        {
                            ActiveDownloadSection.Visibility = Visibility.Collapsed;
                        });
                        break;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }

        private void CancelInstallButton_OnClick(object sender, RoutedEventArgs e)
        {
            _log.Information("CancelInstallButton_OnClick: Cancelling Installation");
            _installManager.CancelInstall(_currentInstallItem.Name);
        }

        private void FetchQueueItemsList()
        {
            try
            {
                var queueItemNames = _installManager.GetQueueItemNames();
                if (queueItemNames == null || queueItemNames.Count < 1) return;

                DispatcherQueue.TryEnqueue(() => queueItems.Clear());

                ObservableCollection<DownloadManagerItem> itemList = new();
                foreach (var queueItemName in queueItemNames)
                {

                    //var gameInfo = LibraryManager.GetGameInfo(queueItemName);
                    //if (gameInfo is null) continue;
                    //itemList.Add(new DownloadManagerItem()
                    //{
                    //    Name = queueItemName,
                    //    Title = gameInfo.Title,
                    //    Image = Util.GetBitmapImage(gameInfo.Images.FirstOrDefault(image => image.Type == "DieselGameBoxTall")?.Url)
                    //});
                }
                DispatcherQueue.TryEnqueue(() =>
                {
                    queueItems = itemList;
                    InstallQueueListView.ItemsSource = queueItems;
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }
        private void FetchHistoryItemsList()
        {
            try
            {
                var historyItemsNames = _installManager.GetHistoryItemsNames();
                if (historyItemsNames == null || historyItemsNames.Count < 1) return;

                _log.Information("FetchHistoryItemsList: History Items: {HistoryItems}", historyItemsNames);

                DispatcherQueue.TryEnqueue(() => historyItems.Clear());

                ObservableCollection<DownloadManagerItem> itemList = new();

                DispatcherQueue.TryEnqueue(() =>
                {
                    foreach (var historyItemName in historyItemsNames)
                    {

                        //var gameInfo = LibraryManager.GetGameInfo(historyItemName);
                        //if (gameInfo is null) continue;
                        //itemList.Add(new DownloadManagerItem()
                        //{
                        //    Name = historyItemName,
                        //    Title = gameInfo.Title,
                        //    Image = Util.GetBitmapImage(gameInfo.Images.FirstOrDefault(image => image.Type == "DieselGameBoxTall")?.Url)
                        //});
                    }
                    historyItems = itemList;
                    HistoryItemsList.ItemsSource = historyItems;
                });
            }
            catch (Exception ex)
            {
                _log.Error(ex, "FetchHistoryItemsList: Error while fetching history items");
            }
        }

        private void InstallationProgressUpdate(InstallItem installItem)
        {
            // Its better not to log the progress update as it will be called very frequently
            // It can make the log file very big
            try
            {
                if (installItem == null) return;

                if (installItem.Status != ActionStatus.Processing) return;
                DispatcherQueue.TryEnqueue(() =>
                {
                    DownloadProgressBar.IsIndeterminate = false;
                    DownloadProgressBar.Value = Convert.ToDouble(installItem.ProgressPercentage);
                    CurrentDownloadedSize.Text = $@"{Util.ConvertMiBToGiBOrMiB(installItem.DownloadedSize)} of {Util.ConvertMiBToGiBOrMiB(installItem.TotalDownloadSizeMb)}";
                    CurrentDownloadSpeed.Text = $@"{installItem.DownloadSpeedRaw} MiB/s";
                });
            }
            catch (Exception ex)
            {
                _log.Error(ex, "InstallationProgressUpdate: Error while updating progress");
            }
        }
    }
    public class DownloadManagerItem
    {
        public string Name { get; set; }
        public string Title { get; set; }
        public BitmapImage Image { get; set; }
        //public Game.InstallState InstallState { get; set; }
    }
}
