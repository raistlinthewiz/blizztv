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
using System.Diagnostics;
using System.Linq;
using System.Timers;
using System.Windows.Forms;
using BlizzTV.Configuration;
using BlizzTV.EmbeddedModules.BlueTracker.Parsers;
using BlizzTV.EmbeddedModules.BlueTracker.Settings;
using BlizzTV.InfraStructure.Modules;
using BlizzTV.InfraStructure.Modules.Settings;
using BlizzTV.Log;
using BlizzTV.Utility.Extensions;
using BlizzTV.Utility.Imaging;
using BlizzTV.Assets.i18n;

namespace BlizzTV.EmbeddedModules.BlueTracker
{
    [ModuleAttributes("BlizzBlues", "Blizzard GM-Blue's aggregator.", "blizzblues")]
    class BlueTrackerModule : Module
    {
        private bool _disposed = false;
        private readonly ModuleNode _moduleNode = new ModuleNode("BlizzBlues");
        private readonly List<BlueParser> _parsers = new List<BlueParser>(); // list of blue-post parsers
        private System.Timers.Timer _updateTimer;

        public static BlueTrackerModule Instance; // the module instance.

        public BlueTrackerModule()
        {
            Instance = this;

            this.CanRenderTreeNodes = true;

            this._moduleNode.Icon = new NamedImage("blizzblue", Assets.Images.Icons.Png._16.blizzblues);

            this._moduleNode.Menu.Add("refresh", new ToolStripMenuItem(i18n.Refresh, Assets.Images.Icons.Png._16.update, new EventHandler(MenuUpdate)));
            this._moduleNode.Menu.Add("markallasread", new ToolStripMenuItem(i18n.MarkAsRead, Assets.Images.Icons.Png._16.read, new EventHandler(MenuMarkAllAsReadClicked)));
            this._moduleNode.Menu.Add("markallasunread", new ToolStripMenuItem(i18n.MarkAsUnread, Assets.Images.Icons.Png._16.unread, new EventHandler(MenuMarkAllAsUnReadClicked)));
            this._moduleNode.Menu.Add("settings", new ToolStripMenuItem(i18n.Settings, Assets.Images.Icons.Png._16.settings, new EventHandler(MenuSettingsClicked))); 
        }

        public override void Startup()
        {
            this.UpdateBlues();
            if (this._updateTimer == null) this.SetupUpdateTimer();
        }

        private void UpdateBlues()
        {
            if (this.RefreshingData) return; // if module is already updating, ignore this request.
            this.RefreshingData = true;

            if (this._parsers.Count > 0) this._parsers.Clear(); // reset the current data.

            if (ModuleSettings.Instance.TrackWorldofWarcraft)
            {
                var wow = new WorldofWarcraft();
                this._parsers.Add(wow);
                wow.StateChanged += OnChildStateChanged;
            }

            if (ModuleSettings.Instance.TrackStarcraft)
            {
                var sc = new Starcraft();
                this._parsers.Add(sc);
                sc.StateChanged += OnChildStateChanged;
            }

            if (this._parsers.Count > 0)
            {
                Workload.WorkloadManager.Instance.Add(this._parsers.Count);

                var stopwatch = new Stopwatch();
                stopwatch.Start();

                foreach (BlueParser parser in this._parsers)
                {
                    parser.Update();
                }

                Module.UITreeView.AsyncInvokeHandler(() =>
                {
                    Module.UITreeView.BeginUpdate();
                    foreach (BlueParser parser in this._parsers)
                    {                    
                        this._moduleNode.Nodes.Add(parser);
                        foreach (KeyValuePair<string, BlueStory> pair in parser.Stories)
                        {
                            parser.Nodes.Add(pair.Value);
                            if (pair.Value.Successors.Count <= 0) continue;
                            foreach (BlueStory post in pair.Value.Successors) { pair.Value.Nodes.Add(post); } // if story have posts more than one..
                        }
                        Workload.WorkloadManager.Instance.Step();
                    }
                    Module.UITreeView.EndUpdate();
                });

                stopwatch.Stop();
                TimeSpan ts = stopwatch.Elapsed;
                LogManager.Instance.Write(LogMessageTypes.Trace, string.Format("Updated {0} blizzblue-parsers in {1}.", this._parsers.Count, String.Format("{0:00}:{1:00}:{2:00}.{3:00}", ts.Hours, ts.Minutes, ts.Seconds, ts.Milliseconds / 10)));
            }

            this.RefreshingData = false;
        }

        public override TreeNode GetModuleNode()
        {
            return this._moduleNode;
        }

        public override Form GetPreferencesForm()
        {
            return new SettingsForm();
        }

        private void OnChildStateChanged(object sender, EventArgs e)
        {
            if (this._moduleNode.State == ((BlueParser)sender).State) return;

            int unread = this._parsers.Count(parser => parser.State == State.Unread);
            this._moduleNode.State = unread > 0 ? State.Unread : State.Read;
        }

        private void MenuMarkAllAsReadClicked(object sender, EventArgs e)
        {
            foreach (BlueParser parser in this._parsers)
            {
                foreach (KeyValuePair<string, BlueStory> pair in parser.Stories)
                {
                    pair.Value.State = State.Read;
                    foreach (BlueStory post in pair.Value.Successors) { post.State=State.Read; }
                }
            }
        }

        private void MenuMarkAllAsUnReadClicked(object sender, EventArgs e)
        {
            foreach (BlueParser parser in this._parsers)
            {
                foreach (KeyValuePair<string, BlueStory> pair in parser.Stories)
                {
                    pair.Value.State = State.Unread;
                    foreach (BlueStory post in pair.Value.Successors) { post.State=State.Unread; }
                }
            }
        }

        private void MenuUpdate(object sender, EventArgs e)
        {
            var thread = new System.Threading.Thread(this.UpdateBlues) { IsBackground = true };
            thread.Start();
        }

        private void MenuSettingsClicked(object sender, EventArgs e)
        {
            var f = new ModuleSettingsHostForm(this.Attributes, this.GetPreferencesForm());
            f.ShowDialog();
        }

        public void OnSaveSettings()
        {
            this.SetupUpdateTimer();
        }

        private void SetupUpdateTimer()
        {
            if (this._updateTimer != null) // clean the old update timer if exists.
            {
                this._updateTimer.Enabled = false;
                this._updateTimer.Elapsed -= OnTimerHit;
                this._updateTimer = null;
            }

            _updateTimer = new System.Timers.Timer(EmbeddedModules.BlueTracker.Settings.ModuleSettings.Instance.UpdatePeriod * 60000);
            _updateTimer.Elapsed += OnTimerHit;
            _updateTimer.Enabled = true;
        }

        private void OnTimerHit(object source, ElapsedEventArgs e)
        {
            if (!RuntimeConfiguration.Instance.InSleepMode) this.UpdateBlues();
        }
    }
}
