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
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Reflection;
using DeadNova.Network;
using DeadNova.Tasks;

namespace DeadNova {
    /// <summary> Checks for and applies software updates. </summary>
    public static class Updater {
        
        public static string SourceURL = "https://github.com/RandomStrangers/DeadNova/";
        public const string BaseURL    = "https://github.com/RandomStrangers/DeadNova/blob/master/";
        public const string UploadsURL = "https://github.com/RandomStrangers/DeadNova/tree/master/Uploads";
        public const string UpdatesURL = "https://github.com/RandomStrangers/DeadNova/raw/master/Uploads/";
        public static string WikiURL = "https://github.com/ClassiCube/MCGalaxy/wiki/";
        public const string FlamesURL = "https://github.com/RandomStrangers/Fire";


        const string CurrentVersionURL = BaseURL + "Uploads/current_version.txt";
#if DEV_BUILD_NOVA
        const string dllURL = UpdatesURL + "DeadNova_Core.dll";
        const string guiURL = UpdatesURL + "DeadNova_CoreGUI.exe";
        const string cliURL = UpdatesURL + "DeadNovaCLI_Core.exe";
#else
        const string dllURL = UpdatesURL + "DeadNova_.dll";
        const string guiURL = UpdatesURL + "DeadNova.exe";
        const string cliURL = UpdatesURL + "DeadNovaCLI.exe";
#endif


        public static event EventHandler NewerVersionDetected;
        
        public static void UpdaterTask(SchedulerTask task) {
            UpdateCheck();
            task.Delay = TimeSpan.FromHours(2);
        }

        static void UpdateCheck() {
            if (!Server.Config.CheckForUpdates) return;
            WebClient client = HttpUtil.CreateWebClient();

            try {
                string latest = client.DownloadString(CurrentVersionURL);
                
                if (new Version(Server.Version) >= new Version(latest)) {
                    Logger.Log(LogType.SystemActivity, "No update found!");
                } else if (NewerVersionDetected != null) {
                    NewerVersionDetected(null, EventArgs.Empty);
                }
            } catch (Exception ex) {
                Logger.LogError("Error checking for updates", ex);
            }
            
            client.Dispose();
        }

        public static void PerformUpdate() {
            try {
                try {
                    DeleteFiles("Changelog.txt", "DeadNova_.update", "DeadNova.update", "DeadNovaCLI.update",
#if DEV_BUILD_NOVA
                                "prev_DeadNova_Core.dll", "prev_DeadNova_CoreGUI.exe", "prev_DeadNovaCLI_Core.exe");
#else
                                "prev_DeadNova_.dll", "prev_DeadNova.exe", "prev_DeadNovaCLI.exe");
#endif
                } catch {
                }
                
                WebClient client = HttpUtil.CreateWebClient();
                client.DownloadFile(dllURL, "DeadNova_.update");
                client.DownloadFile(guiURL, "DeadNova.update");
                client.DownloadFile(cliURL, "DeadNovaCLI.update");

                Level[] levels = LevelInfo.Loaded.Items;
                foreach (Level lvl in levels) {
                    if (!lvl.SaveChanges) continue;
                    lvl.Save();
                    lvl.SaveBlockDBChanges();
                }

                Player[] players = PlayerInfo.Online.Items;
                foreach (Player pl in players) pl.SaveStats();

                // Move current files to previous files (by moving instead of copying, 
                //  can overwrite original the files without breaking the server)
#if DEV_BUILD_NOVA
                AtomicIO.TryMove("DeadNova_.dll", "prev_DeadNova_Core.dll");
                AtomicIO.TryMove("DeadNova.exe", "prev_DeadNova_CoreGUI.exe");
                AtomicIO.TryMove("DeadNovaCLI.exe", "prev_DeadNovaCLI_Core.exe");
#else
                AtomicIO.TryMove("DeadNova_.dll", "prev_DeadNova_.dll");
                AtomicIO.TryMove("DeadNova.exe", "prev_DeadNova.exe");
                AtomicIO.TryMove("DeadNovaCLI.exe", "prev_DeadNovaCLI.exe");
#endif
                // Move update files to current files
                File.Move("DeadNova_.update",   "DeadNova_.dll");
                File.Move("DeadNova.update",    "DeadNova.exe");
                File.Move("DeadNovaCLI.update", "DeadNovaCLI.exe");
                Server.Update(true, "Updating server.");
            } catch (Exception ex) {
                Logger.LogError("Error performing update", ex);
            }
        }

        static void DeleteFiles(params string[] paths) {
            foreach (string path in paths) { AtomicIO.TryDelete(path); }
        }
    }
}
