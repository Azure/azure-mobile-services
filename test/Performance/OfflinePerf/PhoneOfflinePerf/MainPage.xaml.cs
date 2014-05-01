using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using Microsoft.WindowsAzure.MobileServices;
using Microsoft.WindowsAzure.MobileServices.SQLiteStore;
using Microsoft.WindowsAzure.MobileServices.Sync;
using Newtonsoft.Json.Linq;
using OfflinePerfCore.Common;
using OfflinePerfCore.Tests;
using OfflinePerfCore.Types;
using PhoneOfflinePerf.Resources;
using Windows.Storage;

namespace PhoneOfflinePerf
{
    public partial class MainPage : PhoneApplicationPage
    {
        private const string AppUrl = "https://csff.azure-mobile.net";
        private const string AppKey = "ebzJqyrILukZdGKrrAgJYSNrnAOhNQ92";

        public MainPage()
        {
            InitializeComponent();

            // Sample code to localize the ApplicationBar
            //BuildLocalizedApplicationBar();
        }

        private async void btnStart_Click(object sender, RoutedEventArgs e)
        {
            await this.RunTest(new HeavyReadOffline());
        }

        private async Task RunTest(IPerfTest test, int iterations = 10)
        {
            this.btnHeavyUpdate.IsEnabled = false;
            this.btnPullOnGrowingTable.IsEnabled = false;
            this.btnStart.IsEnabled = false;
            this.btnClear.IsEnabled = false;
            MobileServiceInvalidOperationException msioe = null;
            Exception ex = null;

            try
            {
                while (iterations > 0)
                {
                    this.AddToDebug("Starting test, iterations left: " + iterations);
                    var platformInfo = new PlatformInfo();

                    try
                    {
                        var file = await ApplicationData.Current.LocalFolder.GetFileAsync("test.db");
                        await file.DeleteAsync();
                    }
                    catch (Exception) { }

                    var results = await test.Execute(platformInfo, "test.db");
                    await ResultsRepository.UploadResults(AppUrl, AppKey, results, platformInfo);
                    AddToDebug(results.ScenarioName);
                    foreach (var measurement in results.Measurements)
                    {
                        AddToDebug("  Name: {0}", measurement.Name);
                        AddToDebug("    Latency (local ops): {0}", measurement.LocalOperationsLatency);
                        AddToDebug("    Latency (pull): {0}", measurement.PullOperationLatency);
                        if (measurement.SyncOperationLatency.HasValue)
                        {
                            AddToDebug("    Latency (sync): {0}", measurement.SyncOperationLatency);
                        }
                    }

                    iterations--;
                    await Task.Delay(10000);
                    this.txtDebug.Text = "";
                }
            }
            catch (MobileServiceInvalidOperationException e)
            {
                this.AddToDebug("Error: " + e);
                msioe = e;
            }
            catch (Exception e)
            {
                this.AddToDebug("Error: " + e);
                ex = e;
            }
            finally
            {
                this.btnHeavyUpdate.IsEnabled = true;
                this.btnPullOnGrowingTable.IsEnabled = true;
                this.btnStart.IsEnabled = true;
                this.btnClear.IsEnabled = true;
            }

            JObject error = null;
            if (msioe != null)
            {
                var req = msioe.Request;
                var resp = msioe.Response;
                error = new JObject();
                error.Add("message", msioe.Message);
                error.Add("stackTrace", msioe.StackTrace);
                error.Add("request", req.Method.Method + " " + req.RequestUri.ToString());
                error.Add("responseStatus", (int)resp.StatusCode);
                if (resp.Content != null)
                {
                    var respBody = await resp.Content.ReadAsStringAsync();
                    error.Add("responseBody", respBody);
                }
            }
            else if (ex != null)
            {
                error = new JObject();
                error.Add("message", ex.Message);
                error.Add("stackTrace", ex.StackTrace);
            }

            if (error != null)
            {
                await ResultsRepository.UploadError(AppUrl, AppKey, error);
            }
        }

        private void AddToDebug(string text, params object[] args)
        {
            if (args != null && args.Length > 0) text = string.Format(text, args);
            this.txtDebug.Text = this.txtDebug.Text + text + Environment.NewLine;
        }

        private void btnClear_Click(object sender, RoutedEventArgs e)
        {
            this.txtDebug.Text = "";
        }

        private async void btnHeavyUpdate_Click(object sender, RoutedEventArgs e)
        {
            await this.RunTest(new HeavyInsertUpdateScenarioSingleTable());
        }

        private async void btnPullOnGrowingTable_Click(object sender, RoutedEventArgs e)
        {
            await this.RunTest(new PullBasedOnNumberOfItems());
        }
    }
}