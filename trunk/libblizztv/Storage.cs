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
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LibBlizzTV
{
    /// <summary>
    /// Key-value storage for plugins
    /// </summary>
    public sealed class Storage
    {
        #region storage API

        /// <summary>
        /// Creates a new storage for the plugin with the given plugin identifier.
        /// </summary>
        public Storage() { }

        /// <summary>
        /// Puts a new key-value pair in plugin storage.
        /// </summary>
        /// <param name="category">The key category.</param>
        /// <param name="key">The key.</param>        
        /// <param name="value">The byte-value</param>
        public void Put(string category, string key, byte value)
        {
            key = string.Format("{0}.{1}", Plugin.Instance.Attributes.Name, key); // append the plugin name to the key.
            Database.Instance.Put(key, value);
        }

        /// <summary>
        /// Get's the byte-value for supplied key.
        /// </summary>
        /// <param name="category">The key category.</param>
        /// <param name="key">The key.</param>
        /// <returns>Returns the byte-value for the supplied key.</returns>
        public byte Get(string category, string key)
        {
            key = string.Format("{0}.{1}", Plugin.Instance.Attributes.Name, key); // append the plugin name to the key.
            return Database.Instance.Get(key);
        }

        /// <summary>
        /// Returns true if the given key-value pair exists in storage.
        /// </summary>
        /// <param name="category">The key category.</param>
        /// <param name="key">The key.</param>
        /// <returns>Returns true if the given key-value pair exists in storage.</returns>
        public bool KeyExists(string category, string key)
        {
            key = string.Format("{0}.{1}", Plugin.Instance.Attributes.Name, key); // append the plugin identifier to the key.
            return Database.Instance.KeyExists(key);
        }

        #endregion
    }
}
