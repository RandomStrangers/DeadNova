/*
    Copyright 2010 MCSharp team (Modified for use with MCZall/MCLawl/DeadNova)
    
    Dual-licensed under the Educational Community License, Version 2.0 and
    the GNU General Public License, Version 3 (the "Licenses"); you may
    not use this file except in compliance with the Licenses. You may
    obtain a copy of the Licenses at
    
    http://www.opensource.org/licenses/ecl2.php
    http://www.gnu.org/licenses/gpl-3.0.html
    
    Unless required by applicable law or agreed to in writing,
    software distributed under the Licenses are distributed on an "AS IS"
    BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express
    or implied. See the Licenses for the specific language governing
    permissions and limitations under the Licenses.
*/
using System;
using System.Collections.Generic;
using DeadNova.Scripting;

namespace DeadNova
{
    /// <summary> This class provides for more advanced modification to DeadNova </summary>
    public abstract class Plugin_Simple
    {
        /// <summary> Hooks into events and initalises states/resources etc </summary>
        /// <param name="auto"> True if simple plugin is being automatically loaded (e.g. on server startup), false if manually. </param>
        public abstract void Load(bool auto);

        /// <summary> Unhooks from events and disposes of state/resources etc </summary>
        /// <param name="auto"> True if simple plugin is being auto unloaded (e.g. on server shutdown), false if manually. </param>
        public abstract void Unload(bool auto);

        /// <summary> Called when a player does /Help on the simple plugin. Typically tells the player what this simple plugin is about. </summary>
        /// <param name="p"> Player who is doing /Help. </param>
        public virtual void Help(Player p)
        {
            p.Message("No help is available for this simple plugin.");
        }

        /// <summary> Name of the plugin. </summary>
        public abstract string name { get; }
        /// <summary> Oldest version of DeadNova this simple plugin is compatible with. </summary>
        public abstract string DeadNova_Version { get; }
        /// <summary> Version of this simple plugin. </summary>
        public virtual int build { get { return 0; } }
        /// <summary> Message to display once this simple plugin is loaded. </summary>
        public virtual string welcome { get { return ""; } }
        /// <summary> The creator/author of this simple plugin. (Your name) </summary>
        public virtual string creator { get { return ""; } }
        /// <summary> Whether or not to auto load this simple plugin on server startup. </summary>
        public virtual bool LoadAtStartup { get { return true; } }


        public static List<Plugin_Simple> core = new List<Plugin_Simple>();
        public static List<Plugin_Simple> all = new List<Plugin_Simple>();

        public static bool Load(Plugin_Simple ps, bool auto)
        {
            try
            {
                string ver = ps.DeadNova_Version;
                if (!string.IsNullOrEmpty(ver) && new Version(ver) > new Version(Server.Version))
                {
                    Logger.Log(LogType.Warning, "Simple plugin ({0}) requires a more recent version of {1}!", ps.name, Server.SoftwareName);
                    return false;
                }
                all.Add(ps);

                if (ps.LoadAtStartup || !auto)
                {
                    ps.Load(auto);
                    Logger.Log(LogType.SystemActivity, "Simple plugin {0} loaded...build: {1}", ps.name, ps.build);
                }
                else
                {
                    Logger.Log(LogType.SystemActivity, "Simple plugin {0} was not loaded, you can load it with /pload", ps.name);
                }

                if (!string.IsNullOrEmpty(ps.welcome)) Logger.Log(LogType.SystemActivity, ps.welcome);
                return true;
            }
            catch (Exception ex)
            {
                Logger.LogError("Error loading simple plugin " + ps.name, ex);
                if (!string.IsNullOrEmpty(ps.creator)) Logger.Log(LogType.Warning, "You can go bug {0} about it.", ps.creator);
                return false;
            }
        }

        public static bool Unload(Plugin_Simple ps, bool auto)
        {
            bool success = true;
            try
            {
                ps.Unload(auto);
                Logger.Log(LogType.SystemActivity, "Simple plugin {0} was unloaded.", ps.name);
            }
            catch (Exception ex)
            {
                Logger.LogError("Error unloading simple plugin " + ps.name, ex);
                success = false;
            }

            all.Remove(ps);
            return success;
        }

        public static void UnloadAll()
        {
            for (int i = 0; i < all.Count; i++)
            {
                Unload(all[i], true); i--;
            }
        }
        public static void LoadAll()
        {
           // LoadCorePlugin(new CorePlugin());
            IScripting_Simple.AutoloadSimplePlugins();
        }

        static void LoadCorePlugin(Plugin_Simple simpleplugin)
        {
            simpleplugin.Load(true);
            all.Add(simpleplugin);
            core.Add(simpleplugin);
        }
    }
}

