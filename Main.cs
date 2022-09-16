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
                if (loaded.version == modEntry.Info.Version)
                    settings = loaded;
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

            Main.DebugLog(()=>"Rods load done 2");


            return true;
        }

        private static void OnGUI(UnityModManager.ModEntry modEntry)
        {
            settings.Draw(modEntry);
        }

        private static void OnSaveGUI(UnityModManager.ModEntry modEntry)
        {
            settings.Save(modEntry);
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

        public static void DebugLog(TrainCar car, Func<string> message)
        {
            if (car == PlayerManager.Car)
                DebugLog(message);
        }

        public static void DebugLog(Func<string> message)
        {
            if (settings.enableLogging || true)
                mod?.Logger.Log(message());
        }
    }
}
