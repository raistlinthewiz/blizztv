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
 */

using System;
using System.Windows.Forms;
using LibBlizzTV.Streams;
using LibBlizzTV.VideoChannels;

namespace BlizzTV
{
    public sealed class ListItem : ListViewItem
    {
        public enum ListItemType
        {
            Stream,
            VideoChannel,
            Story
        }

        private object _storage = null;
        private ListItemType _item_type;

        public ListItemType ItemType { get { return this._item_type; } }

        public ListItem(Stream stream)
        {
            this._item_type = ListItemType.Stream;
            this._storage = stream;
            this.SubItems.Add(string.Format("{0} ({1})", stream.Name, stream.ViewerCount.ToString()));
            this.SubItems.Add(new ListViewSubItem());
        }

        public ListItem(Channel channel)
        {
            this._item_type = ListItemType.VideoChannel;
            this._storage = channel;            
            this.SubItems.Add(string.Format("{0} ({1})",channel.Name,channel.Videos.Count.ToString()));
            this.SubItems.Add(new ListViewSubItem());
        }

        /*public ListItem(Story story)
        {
            this._item_type = ListItemType.Story;
            this._storage = story;
            this.SubItems.Add(string.Format("{0}", story.Title));
            this.SubItems.Add(new ListViewSubItem());
        }*/

        public object GetObject()
        {
            return this._storage;
        }

        public void DoubleClick()
        {
            switch (this._item_type)
            {
                case ListItemType.Stream:
                    {
                        StreamPlayer p = new StreamPlayer((Stream)this._storage);
                        p.Show();
                        break;
                    }
                case ListItemType.VideoChannel:
                    {
                        ChannelStage s = new ChannelStage((Channel)this._storage);
                        s.Show();
                        break;
                    }
                default:
                    break;
            }
        }
    }
}
