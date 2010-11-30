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

namespace LibBlizzTV.Settings
{
    /// <summary>
    /// Global settings that is used by both the BlizzTV and it's plugins.
    /// </summary>
    public sealed class Global : Settings
    {
        private static Global _instance = new Global();

        /// <summary>
        /// Returns instance of GlobalSettings.
        /// </summary>
        public static Global Instance { get { return _instance; } }

        /// <summary>
        /// The default video player width.
        /// </summary>
        public int VideoPlayerWidth { get { return this.GetInt("VideoPlayerWidth", 640); } set { this.Set("VideoPlayerWidth", value); } }

        /// <summary>
        /// The default video player height.
        /// </summary>
        public int VideoPlayerHeight { get { return this.GetInt("VideoPlayerHeight", 385); } set { this.Set("VideoPlayerHeight", value); } }

        /// <summary>
        /// States if video's should be played automatically.
        /// </summary>
        public bool AutoPlayVideos { get { return this.GetBoolean("AutoPlayVideos", true); } set { this.Set("AutoPlayVideos", value); } }

        /// <summary>
        /// Always on top setting for player windows.
        /// </summary>
        public bool PlayerWindowsAlwaysOnTop { get { return this.GetBoolean("PlayerWindowsAlwaysOnTop", true); } set { this.Set("PlayerWindowsAlwaysOnTop", value); } }

        /// <summary>
        /// The default content viewing-method.
        /// </summary>
        public bool UseInternalViewers { get { return this.GetBoolean("UseInternalViewers", true); } set { this.Set("UseInternalViewers", value); } }

        /// <summary>
        /// States the sleep mode in which plugin's should not automaticly refresh it's data.
        /// </summary>
        public bool InSleepMode = false;

        private Global() : base("Global") { }
    }
}
