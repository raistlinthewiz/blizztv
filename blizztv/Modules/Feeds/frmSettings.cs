﻿/*    
 * Copyright (C) 2010, BlizzTV Project - http://code.google.com/p/blizztv/
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
using System.Windows.Forms;
using BlizzTV.ModuleLib.Settings;
using BlizzTV.ModuleLib.Subscriptions;

namespace BlizzTV.Modules.Feeds
{
    public partial class frmSettings : Form, IModuleSettingsForm
    {
        public frmSettings()
        {
            InitializeComponent();
        }

        private void frmSettings_Load(object sender, EventArgs e)
        {
            this.LoadSubscriptions();
            this.LoadSettings();
        }

        private void LoadSubscriptions()
        {
            foreach (ISubscription subscription in Subscriptions.Instance.List) this.ListviewSubscriptions.Items.Add(new ListviewFeedSubscription((FeedSubscription)subscription));
        }

        private void LoadSettings()
        {
            numericUpDownUpdateFeedsEveryXMinutes.Value = (decimal)Settings.Instance.UpdateEveryXMinutes;
        }

        public void SaveSettings()
        {
            Settings.Instance.UpdateEveryXMinutes = (int)numericUpDownUpdateFeedsEveryXMinutes.Value;
            Settings.Instance.Save();
            FeedsPlugin.Instance.OnSaveSettings();
        }

        private void buttonAdd_Click(object sender, EventArgs e)
        {
            frmAddFeed f = new frmAddFeed();
            if (f.ShowDialog() == DialogResult.OK)
            {
                Subscriptions.Instance.Add(f.Subscription);
                this.ListviewSubscriptions.Items.Add(new ListviewFeedSubscription(f.Subscription));
            }
        }

        private void buttonRemove_Click(object sender, EventArgs e)
        {
            if (ListviewSubscriptions.SelectedItems.Count > 0)
            {
                ListviewFeedSubscription selection = (ListviewFeedSubscription)ListviewSubscriptions.SelectedItems[0];
                Subscriptions.Instance.Remove(selection.Subscription);        
                selection.Remove();
            }
        }
    }

    public class ListviewFeedSubscription:ListViewItem
    {
        private readonly FeedSubscription _feedSubscription;
        public FeedSubscription Subscription { get { return this._feedSubscription; } }

        public ListviewFeedSubscription(FeedSubscription feedSubscription)
        {
            this._feedSubscription = feedSubscription;
            this.Text = feedSubscription.Name;
            this.SubItems.Add(feedSubscription.Url);
        }
    }
}
