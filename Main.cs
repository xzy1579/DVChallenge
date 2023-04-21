using HarmonyLib;
using System;
using UnityModManagerNet;

namespace DvMod.Challenges
{
    [EnableReloading]
    public static class Main
    {
        public static UnityModManager.ModEntry? mod;
        public static Settings settings = new Settings();
        public static bool enabled;

        public static bool inStats = true;


        static public bool Load(UnityModManager.ModEntry modEntry)
        {
            mod = modEntry;

            try
            {
                var loaded = Settings.Load<Settings>(modEntry);
                settings = loaded;

                if (loaded.version == modEntry.Info.Version)
                {
//                    settings = loaded;
                }

                if(settings.resetAllJobs == true)
                {
                    Status.createNewFile();
                    settings.resetAllJobs = false;
                    settings.Save(modEntry);
                    loaded = Settings.Load<Settings>(modEntry);
                    settings = loaded;
                }
                Status.loadChallengeStatus(true);
                Signals.processSettings(settings);
                SignalPattern.processSettings(settings);
            }
            catch
            {
            }

            mod.OnGUI = OnGUI;
            mod.OnSaveGUI = OnSaveGUI;
            mod.OnToggle = OnToggle;

            for(int i = 0; i < AllJob.StationIds.Length; i++)
            {
                AllJob allJob = Status.getJobStatus(AllJob.StationIds[i]);
                if(allJob != null && allJob.status.Equals("InProgress"))
                {
                    AllJobs.challengeStation = AllJob.StationIds[i];
                    int numJobs = 1;
                    try
                    {
                        numJobs = Int32.Parse(allJob.jobs);
                    } 
                    catch(Exception)
                    {

                    }
                    AllJobs.takenJobs = numJobs;
                }
            }


            return true;
        }

        private static void OnGUI(UnityModManager.ModEntry modEntry)
        {
            settings.Draw(modEntry);
        }

        private static void OnSaveGUI(UnityModManager.ModEntry modEntry)
        {
            settings.Save(modEntry);
            settings = Settings.Load<Settings>(modEntry);

            if (settings.resetAllJobs == true)
            {
                Status.createNewFile();
                settings.resetAllJobs = false;
                settings.Save(modEntry);
                settings = Settings.Load<Settings>(modEntry);
            }
            Signals.processSettings(settings);
            SignalPattern.processSettings(settings);
        }

        private static bool OnToggle(UnityModManager.ModEntry modEntry, bool value)
        {
            Harmony harmony = new Harmony(modEntry.Info.Id);

            if (value)
                harmony.PatchAll();
            else
                harmony.UnpatchAll(modEntry.Info.Id);
            return true;
        }

        public static void DebugLog(string message)
        {
            DebugLog(()=>message);
        }

        public static void DebugLog(Func<string> message)
        {
            if (settings.enableLogging)
                mod?.Logger.Log(message());
        }
    }
}
