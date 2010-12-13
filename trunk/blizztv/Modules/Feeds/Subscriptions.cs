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
using System.Collections.Generic;
using System.Xml.Serialization;
using BlizzTV.ModuleLib.Subscriptions;

namespace BlizzTV.Modules.Feeds
{
    public sealed class Subscriptions : SubscriptionsHandler
    {
        private static Subscriptions _instance = new Subscriptions();
        public static Subscriptions Instance { get { return _instance; } }

        private Subscriptions() : base(typeof(FeedSubscription)) { }

        public bool Add(FeedSubscription subscription)
        {
            if (!this.Dictionary.ContainsKey(subscription.URL))
            {
                base.Add(subscription);
                return true;
            }
            return false;
        }

        public Dictionary<string, FeedSubscription> Dictionary
        {
            get
            {
                Dictionary<string, FeedSubscription> dictionary = new Dictionary<string, FeedSubscription>();
                foreach (ISubscription subscription in this.List)
                {
                    dictionary.Add((subscription as FeedSubscription).URL, (subscription as FeedSubscription));
                }
                return dictionary;
            }
        }
    }

    [Serializable]
    [XmlType("Feed")]
    public class FeedSubscription : ISubscription
    {
        [XmlAttribute("Url")]
        public string URL { get; set; }

        public FeedSubscription() { }
    }
}
