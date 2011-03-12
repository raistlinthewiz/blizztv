﻿/*    
 * Copyright (C) 2010-2011, BlizzTV Project - http://code.google.com/p/blizztv/
 *  
 * This program is free software: you can redistribute it and/or modify it under the terms of the GNU General 
 * Public License as published by the Free Software Foundation, either version 3 of the License, or (at your 
 * option) any later version.
 * 
 * This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the 
 * implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU General Public License 
 * for more details.
 * 
 * You should have received a copy of the GNU General Public License along with this program.  If not, see 
 * <http://www.gnu.org/licenses/>. 
 * 
 * $Id$
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Timers;
using System.Diagnostics;
using BlizzTV.Assets.i18n;
using BlizzTV.Configuration;
using BlizzTV.EmbeddedModules.Podcasts.Settings;
using BlizzTV.InfraStructure.Modules;
using BlizzTV.InfraStructure.Modules.Settings;
using BlizzTV.InfraStructure.Modules.Subscriptions.Catalog;
using BlizzTV.Log;
using BlizzTV.Utility.Extensions;
using BlizzTV.Utility.Imaging;
using BlizzTV.Utility.UI;

namespace BlizzTV.EmbeddedModules.Podcasts
{
    [ModuleAttributes("Podcasts", "Podcast aggregator.", "podcast")]
    public class PodcastsModule : Module , ISubscriptionConsumer
    {
        private bool _disposed = false;
        private readonly ModuleNode _moduleNode = new ModuleNode("Podcasts");
        private System.Timers.Timer _updateTimer;
        private readonly Regex _subscriptionConsumerRegex = new Regex("blizztv\\://podcast/(?<Name>.*?)/(?<Url>.*)", RegexOptions.Compiled);

        public static PodcastsModule Instance;

        public PodcastsModule()
        {
            PodcastsModule.Instance = this;

            this.CanRenderTreeNodes = true;

            this._moduleNode.Icon = new NamedImage("feed", Assets.Images.Icons.Png._16.podcast);

            this._moduleNode.Menu.Add("refresh", new ToolStripMenuItem(i18n.Refresh, Assets.Images.Icons.Png._16.update, new EventHandler(RunManualUpdate)));
            this._moduleNode.Menu.Add("markallasread", new ToolStripMenuItem(i18n.MarkAllAsRead, Assets.Images.Icons.Png._16.read, new EventHandler(MenuMarkAllAsReadClicked)));
            this._moduleNode.Menu.Add("markallasunread", new ToolStripMenuItem(i18n.MarkAllAsUnread, Assets.Images.Icons.Png._16.unread, new EventHandler(MenuMarkAllAsUnReadClicked)));
            this._moduleNode.Menu.Add("settings", new ToolStripMenuItem(i18n.Settings, Assets.Images.Icons.Png._16.settings, new EventHandler(MenuSettingsClicked)));
        }

        public override void Startup()
        {
            this.Refresh();
        }

        public override void Refresh()
        {
            this.UpdatePodcasts();
            if (this._updateTimer == null) this.SetupUpdateTimer();
        }

        public override bool AddSubscriptionFromUrl(string link) // Tries parsing a drag & dropped link to see if it's a podcast and parsable.
        {
            if (Subscriptions.Instance.Dictionary.ContainsKey(link))
            {
                MessageBox.Show(string.Format(i18n.PodcastSubscriptionAlreadyExists, Subscriptions.Instance.Dictionary[link].Name), i18n.SubscriptionExists, MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }

            var podcastSubscription = new PodcastSubscription { Name = "test-podcast", Url = link };

            using (var podcast = new Podcast(podcastSubscription))
            {
                if (podcast.IsValid())
                {
                    var subscription = new PodcastSubscription();
                    string podcastName = "";

                    if (InputBox.Show(i18n.AddNewPodcastTitle, i18n.AddNewPodcastMessage, ref podcastName) == DialogResult.OK)
                    {
                        subscription.Name = podcastName;
                        subscription.Url = link;
                        if (Subscriptions.Instance.Add(subscription))
                        {
                            this.RunManualUpdate(this, new EventArgs());
                            return true;
                        }
                        return false;
                    }
                }
            }

            return false;
        }

        public override TreeNode GetModuleNode()
        {
            return this._moduleNode;
        }

        private void UpdatePodcasts()
        {
            if (this.RefreshingData) return;
            this.RefreshingData = true;

            Workload.WorkloadManager.Instance.Add(Subscriptions.Instance.Dictionary.Count);        

            var stopwatch = new Stopwatch();
            stopwatch.Start();

            int i = 0;
            var tasks = new Task<Podcast>[Subscriptions.Instance.Dictionary.Count];

            foreach (KeyValuePair<string, PodcastSubscription> pair in Subscriptions.Instance.Dictionary)
            {
                var podcast = new Podcast(pair.Value);
                podcast.StateChanged += OnChildStateChanged;                
                tasks[i] = Task.Factory.StartNew(() => TaskProcessPodcast(podcast));
                i++;
            }

            var tasksWaitQueue = tasks;

            while (tasksWaitQueue.Length > 0)
            {
                int taskIndex = Task.WaitAny(tasksWaitQueue);
                tasksWaitQueue = tasksWaitQueue.Where((t) => t != tasksWaitQueue[taskIndex]).ToArray();
                Workload.WorkloadManager.Instance.Step();
            }

            try { Task.WaitAll(tasks); }
            catch (AggregateException aggregateException)
            {
                foreach (var exception in aggregateException.InnerExceptions)
                {
                    LogManager.Instance.Write(LogMessageTypes.Error, string.Format("Podcasts module caught an exception while running podcast processing task:  {0}", exception));
                }
            }

            Module.UITreeView.AsyncInvokeHandler(() =>
            {
                Module.UITreeView.BeginUpdate();

                if (this._moduleNode.Nodes.Count > 0) this._moduleNode.Nodes.Clear();
                foreach (Task<Podcast> task in tasks)
                {
                    this._moduleNode.Nodes.Add(task.Result);
                    foreach(Episode episode in task.Result.Episodes)
                    {
                        task.Result.Nodes.Add(episode);
                    }
                }

                Module.UITreeView.EndUpdate();
            });

            stopwatch.Stop();
            TimeSpan ts = stopwatch.Elapsed;
            LogManager.Instance.Write(LogMessageTypes.Trace, string.Format("Updated {0} podcasts in {1}.", Subscriptions.Instance.Dictionary.Count, String.Format("{0:00}:{1:00}:{2:00}.{3:00}", ts.Hours, ts.Minutes, ts.Seconds, ts.Milliseconds / 10)));

            this.RefreshingData = false;
        }

        private static Podcast TaskProcessPodcast(Podcast podcast)
        {
            podcast.Update();
            return podcast;
        }

        private void OnChildStateChanged(object sender, EventArgs e)
        {
            if (this._moduleNode.GetState() == ((Podcast)sender).GetState()) return;

            int unread = this._moduleNode.Nodes.Cast<Podcast>().Count(feed => feed.GetState() == NodeState.Unread);
            this._moduleNode.SetState(unread > 0 ? NodeState.Unread : NodeState.Read);
        }

        public override Form GetPreferencesForm()
        {
            return new SettingsForm();
        }

        public void OnSaveSettings()
        {
            this.SetupUpdateTimer();
        }

        private void SetupUpdateTimer()
        {
            if (this._updateTimer != null)
            {
                this._updateTimer.Enabled = false;
                this._updateTimer.Elapsed -= OnTimerHit;
                this._updateTimer = null;
            }

            _updateTimer = new System.Timers.Timer(EmbeddedModules.Podcasts.Settings.ModuleSettings.Instance.UpdatePeriod * 60000);
            _updateTimer.Elapsed += OnTimerHit;
            _updateTimer.Enabled = true;
        }

        private void OnTimerHit(object source, ElapsedEventArgs e)
        {
            if (!RuntimeConfiguration.Instance.InSleepMode) this.UpdatePodcasts();
        }

        private void MenuMarkAllAsReadClicked(object sender, EventArgs e)
        {
            foreach (Episode episode in from Podcast podcast in this._moduleNode.Nodes from Episode episode in podcast.Nodes select episode)
            {
                episode.SetState(NodeState.Read);
            }
        }

        private void MenuMarkAllAsUnReadClicked(object sender, EventArgs e)
        {
            foreach (Episode episode in from Podcast podcast in this._moduleNode.Nodes from Episode episode in podcast.Nodes select episode)
            {
                episode.SetState(NodeState.Unread);
            }
        }

        private void MenuSettingsClicked(object sender, EventArgs e)
        {
            var f = new ModuleSettingsHostForm(this.Attributes, this.GetPreferencesForm());
            f.ShowDialog();
        }

        private void RunManualUpdate(object sender, EventArgs e)
        {
            var thread = new System.Threading.Thread(this.UpdatePodcasts) { IsBackground = true };
            thread.Start();
        }

        public string GetCatalogUrl()
        {
            return "http://www.blizztv.com/catalog/podcasts";
        }

        public void ConsumeSubscription(string entryUrl)
        {
            Match match = this._subscriptionConsumerRegex.Match(entryUrl);
            if (!match.Success) return;

            string name = match.Groups["Name"].Value;
            string url = match.Groups["Url"].Value;

            var subscription = new PodcastSubscription { Name = name, Url = url };
            Subscriptions.Instance.Add(subscription);
        }


        #region de-ctor

        protected override void Dispose(bool disposing)
        {
            if (this._disposed) return;
            if (disposing) // managed resources
            {
                this._updateTimer.Enabled = false;
                this._updateTimer.Elapsed -= OnTimerHit;
                this._updateTimer.Dispose();
                this._updateTimer = null;
                foreach (Podcast podcast in this._moduleNode.Nodes) { podcast.Dispose(); }
            }
            base.Dispose(disposing);
        }

        #endregion
    }
}
