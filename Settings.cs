using UnityModManagerNet;

namespace DvMod.Challenges
{
    public class Settings : UnityModManager.ModSettings, IDrawable
    {
        /*
                static int MIN_HORN_SHORT = 20;
                static int MIN_HORN_LONG = 80;
                static int MIN_WHISTLE_SHORT = 15;
                static int MIN_WHISTLE_LONG = 90;
                static int SIGNAL_LATENCY = 3;
                static int SIGNAL_DELAY = 3;
                static string CHALLENGE_START = "--0-";
                static string LOCO_STOP = "-";
                static string LOCO_FORWARD = "--";
                static string LOCO_BACKWARD = "000";
                static float MIN_WHISTLE_TENSION = .8F;
        */
        //        static bool STRICT_TIMING = true;
        //        static float SIGNAL_RATIO = .5f;


        [Draw("Clear All Progress")] public bool resetAllJobs = false;
        [Draw("Enable logging")] public bool enableLogging = true;
        [Draw("Signal Timings")] public string holder1 = "";
        [Draw("Minimum Short Horn Duration (helps ignore lever bouncing)")] public int MIN_HORN_SHORT = 300;
        [Draw("Minimum Long Horn Duration")] public int MIN_HORN_LONG = 1000;
        [Draw("Minimum Whistle Rope Tension (helps ignore rope bouncing)")] public float MIN_WHISTLE_TENSION = .8f;
        [Draw("Minimum Short Whistle Duration")] public int MIN_WHISTLE_SHORT = 300;
        [Draw("Minimum Long Whistle Duration")] public int MIN_WHISTLE_LONG = 1000;
        [Draw("Duration allowed between signals in pattern")] public int SIGNAL_LATENCY = 3000;
        [Draw("Duration allowed between pattern and action")] public int SIGNAL_DELAY = 3000;
        [Draw("Maximum speed to be considered a STOP")] public float STOP_SPEED = 1;
        [Draw("Minimum brake to be considered a STOP")] public float FULL_BRAKE = .88f;
        [Draw("Signal Patterns - 1=long, anything but 1 = short")] public string holder2 = "";
        [Draw("Challenge Start Pattern")] public int CHALLENGE_START = 1101;
        [Draw("Locomotive Stop")] public int LOCO_STOP = 1;
        [Draw("Locomotive Forward")] public int LOCO_FORWARD = 11;
        [Draw("Locomotive Backward")] public int LOCO_BACKWARD = 888;
        public readonly string? version = Main.mod?.Info.Version;

        override public void Save(UnityModManager.ModEntry entry)
        {
            Save<Settings>(this, entry);
        }

        public void OnChange()
        {
        }
    }
}
