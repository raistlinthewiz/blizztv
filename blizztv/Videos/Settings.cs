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

using BlizzTV.Modules.Settings;

namespace BlizzTV.Videos
{   
    public class Settings:ModuleSettings
    {
        #region instance

        private static Settings _instance = new Settings();
        public static Settings Instance { get { return _instance; } }

        #endregion 

        private Settings() : base("Videos") { }

        /// <summary>
        /// Enables notifications for video channels module.
        /// </summary>
        public bool NotificationsEnabled { get { return this.GetBoolean("NotificationsEnabled", true); } set { this.Set("NotificationsEnabled", value); } }

        /// <summary>
        /// Sets the number of videos to query video channels for.
        /// </summary>
        public int NumberOfVideosToQueryChannelFor { get { return this.GetInt("NumberOfVideosToQueryChannelFor", 10); } set { this.Set("NumberOfVideosToQueryChannelFor", value); } }

        /// <summary>
        /// Sets the update period for video channels.
        /// </summary>
        public int UpdatePeriod { get { return this.GetInt("UpdatePeriod", 60); } set { this.Set("UpdatePeriod", value); } }

        /// <summary>
        /// Sets video player windows width.
        /// </summary>
        public int PlayerWidth { get { return this.GetInt("PlayerWidth", 640); } set { this.Set("PlayerWidth", value); } }

        /// <summary>
        /// Sets video player windows height.
        /// </summary>
        public int PlayerHeight { get { return this.GetInt("PlayerHeight", 385); } set { this.Set("PlayerHeight", value); } }
    }
}
