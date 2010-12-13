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

using System.Text.RegularExpressions;

namespace BlizzTV.Modules.Videos.Handlers
{
    public class YoutubeVideo:Video
    {
        private static readonly Regex Regex = new Regex(@"http://www\.youtube\.com/watch\?v\=(.*)\&", RegexOptions.Compiled); // compiled regex for reading video id's.

        public YoutubeVideo(string title, string guid, string link, string provider)
            : base(title, guid, link, provider)
        {
            Match m = Regex.Match(this.Link);
            if (m.Success) this.VideoId = m.Groups[1].Value;
        }
    }
}
