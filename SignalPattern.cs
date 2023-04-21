using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DvMod.Challenges
{
    public static class SignalPattern
    {
        public class Pattern
        {
            public string? patternName;
            public long startTime;
            public long[] signalTiming = new long[4];
        }
        public static System.Collections.Generic.List<Pattern>? signalPatterns = new System.Collections.Generic.List<Pattern>();

        public static int MIN_HORN_SHORT = 300;
        public static int MIN_HORN_LONG = 800;
        public static int MIN_WHISTLE_SHORT = 300;
        public static int MIN_WHISTLE_LONG = 900;
        public static int SIGNAL_LATENCY = 2500;
        public static string CHALLENGE_START = "--0-";
        public static string LOCO_STOP = "-";
        public static string LOCO_FORWARD = "--";
        public static string LOCO_BACKWARD = "000";
        public static bool STRICT_TIMING = true;
        public static float SIGNAL_RATIO = .5f;

        public static long patternStart = 0;
        public static long lastSignalTime=0;
        public static long[] signalTiming = new long[4];
        public static string[] signalIndicator = new string[4];
        public static int curSignal=0;
        public static long maxTiming=0;

        static LocoControllerBase? curLoco = null;


        public static string parseSignalPattern(int pattern)
        {
            string retval = "";

            int workingPattern = pattern;
            if (workingPattern >= 10000) workingPattern = pattern % 10000;       // only want the last 4 digits
            if(workingPattern <= 0) workingPattern = 0;

            int rightMost = workingPattern % 10;
            workingPattern = workingPattern / 10;
            if (rightMost == 1) retval = "1" + retval;
            else retval = "0" + retval;

            if(workingPattern > 0)
            {
                rightMost = workingPattern % 10;
                workingPattern = workingPattern / 10;
                if (rightMost == 1) retval = "1" + retval;
                else retval = "0" + retval;
                if (workingPattern > 0)
                {
                    rightMost = workingPattern % 10;
                    workingPattern = workingPattern / 10;
                    if (rightMost == 1) retval = "1" + retval;
                    else retval = "0" + retval;
                    if (workingPattern > 0)
                    {
                        rightMost = workingPattern % 10;
                        workingPattern = workingPattern / 10;
                        if (rightMost == 1) retval = "1" + retval;
                        else retval = "0" + retval;
                    }
                }
            }

            return retval;
        }

        public static void processSettings(Settings settings)
        {
            MIN_HORN_SHORT = settings.MIN_HORN_SHORT;
            MIN_HORN_LONG = settings.MIN_HORN_LONG;
            MIN_WHISTLE_SHORT = settings.MIN_WHISTLE_SHORT;
            MIN_WHISTLE_LONG= settings.MIN_WHISTLE_LONG;
            SIGNAL_LATENCY = settings.SIGNAL_LATENCY;

            CHALLENGE_START = parseSignalPattern(settings.CHALLENGE_START);
            LOCO_BACKWARD = parseSignalPattern(settings.LOCO_BACKWARD);
            LOCO_FORWARD = parseSignalPattern(settings.LOCO_FORWARD);
            LOCO_STOP = parseSignalPattern(settings.LOCO_STOP);
        }

        public static void logTimings()
        {
            String tempPattern = getPattern();
            if (isChallengeStart()) Main.DebugLog(() => "SignalPattern CHALLENGE START ");
            else if (isLocoStop()) Main.DebugLog(() => "SignalPattern LOCO STOP ");
            else if (isLocoForward()) Main.DebugLog(() => "SignalPattern LOCO FORWARD ");
            else if (isLocoBackward()) Main.DebugLog(() => "SignalPattern LOCO BACKWARD ");
            else if (!tempPattern.Equals(""))  Main.DebugLog(() => "Not identifiable " + tempPattern);

            if (!tempPattern.Equals(""))
            {
                for (int i = 0; i < signalTiming.Length; i++)
                {
                    Main.DebugLog(() => "s " + signalTiming[i]);
                }
            }
        }

        public static string formatPattern(int i)
        {
            DateTimeOffset now = (DateTimeOffset)DateTime.UtcNow;
            DateTimeOffset today = new DateTimeOffset(now.Year, now.Month, now.Day, 0, 0, 0, new TimeSpan(0));
            string retVal = "";

            String patternTimings = signalPatterns[i].patternName + " " +
                                     signalPatterns[i].signalTiming[0] + " " + signalPatterns[i].signalTiming[1] + " " +
                                     signalPatterns[i].signalTiming[2] + " " + signalPatterns[i].signalTiming[3];
            
            retVal = patternTimings + " (" + Math.Round((float)((signalPatterns[i].startTime - today.ToUnixTimeMilliseconds()) / (float)1000), 1) + ")";

            return retVal;
        }

        public static void dumpPatterns()
        {
            DateTimeOffset now = (DateTimeOffset)DateTime.UtcNow;
            DateTimeOffset today = new DateTimeOffset(now.Year, now.Month, now.Day, 0, 0, 0, new TimeSpan(0));

            if (signalPatterns == null) Main.DebugLog(() => "signalePatterns is null");
            else
            {
                for (int i = 0; i < signalPatterns.Count; i++)
                {
                    Main.DebugLog(() => "pattern " + formatPattern(i));
                }
            }
        }

        public static void savePattern()
        {

            string patternName = "";
            String tempPattern = getPattern();
            if (isChallengeStart()) patternName = "C";
            else if (isLocoStop()) patternName = "S";
            else if (isLocoForward()) patternName = "F";
            else if (isLocoBackward()) patternName = "B";
            else if (!tempPattern.Equals("")) patternName = "U";

            if(!patternName.Equals(""))
            {
//                Main.DebugLog(() => "saving a pattern " + patternName) ;
                Pattern newPattern = new Pattern();
                newPattern.patternName = patternName;
                newPattern.startTime = patternStart;
                newPattern.signalTiming[0] = signalTiming[0];
                newPattern.signalTiming[1] = signalTiming[1];
                newPattern.signalTiming[2] = signalTiming[2];
                newPattern.signalTiming[3] = signalTiming[3];
                if (signalPatterns == null) signalPatterns = new System.Collections.Generic.List<Pattern>();
                signalPatterns.Add(newPattern); 
            } 
            if( patternName.Equals("C"))
            {
                if (!Signals.challengeStarted)
                {
                    if(curLoco != null) Signals.startChallenge(curLoco);
                }
            }
        }

        public static void checkForComplete()
        {
            DateTimeOffset now = (DateTimeOffset)DateTime.UtcNow;
           
            if (now.ToUnixTimeMilliseconds() - lastSignalTime > SIGNAL_LATENCY)
            {
                resetPattern();
            }
        }
        public static void resetPattern()
        {
            savePattern();

            for (int i = 0; i < signalTiming.Length; i++) signalTiming[i] = 0;
            for (int i = 0; i < signalIndicator.Length; i++) signalIndicator[i] = "";
            curSignal = 0;
            maxTiming = 0;
            patternStart = 0;
            lastSignalTime = 0;
        }

        public static void addHornSignal(long timing,long signalStartTime, LocoControllerBase targetLoco)
        {
            DateTimeOffset now = (DateTimeOffset)DateTime.UtcNow;
            curLoco = targetLoco;
            if (curSignal >= 4) resetPattern();
            if (curSignal < 4)
            {
                if (timing > maxTiming) maxTiming = timing;

                if(signalStartTime - lastSignalTime > SIGNAL_LATENCY)
                {
                    resetPattern();
                }

                if (curSignal == 0) patternStart = signalStartTime;

                lastSignalTime = now.ToUnixTimeMilliseconds();

                if (timing > MIN_HORN_LONG)
                {
                    signalTiming[curSignal] = timing;
                    signalIndicator[curSignal] = "1";
                    curSignal++;
                }
                else if (timing > MIN_HORN_SHORT)
                {
                    signalTiming[curSignal] = timing;
                    signalIndicator[curSignal] = "0";
                    curSignal++;
                }
            }
        }
        public static void addWhistleSignal(long timing, long signalStartTime, LocoControllerSteam targetLoco)
        {
            DateTimeOffset now = (DateTimeOffset)DateTime.UtcNow;
            curLoco = targetLoco;
            if(curSignal >= 4) resetPattern();
            if (curSignal < 4)
            {
                if (timing > maxTiming) maxTiming = timing;

                if (signalStartTime - lastSignalTime > SIGNAL_LATENCY)
                {
                    resetPattern();
                }

                if (curSignal == 0) patternStart = signalStartTime;

                lastSignalTime = now.ToUnixTimeMilliseconds();

                if (timing > MIN_WHISTLE_LONG)
                {
                    signalTiming[curSignal] = timing;
                    signalIndicator[curSignal] = "1";
                    curSignal++;
                }
                else if (timing > MIN_WHISTLE_SHORT)
                {
                    signalTiming[curSignal] = timing;
                    signalIndicator[curSignal] = "0";
                    curSignal++;
                }
            }
        }

        public static string getPattern()
        {
            string pattern = "";

            if(STRICT_TIMING)
            {
                pattern = signalIndicator[0]  + signalIndicator[1]  + signalIndicator[2] + signalIndicator[3];
                pattern = pattern.Trim();

            }
            else
            {
                string curIndicator = "";
                for (int i = 0; i < signalTiming.Length; i++)
                {
                    if (signalTiming[i] == 0) break;
                    if (signalTiming[i] < maxTiming * SIGNAL_RATIO) curIndicator = "0";
                    else curIndicator = "1";
                    pattern += curIndicator;
                }

            }

            return pattern.Trim();
        }

        public static bool isChallengeStart()
        {
            return getPattern().Equals(CHALLENGE_START.Trim());
        }
        public static bool isLocoStop()
        {
            return getPattern().Equals(LOCO_STOP.Trim());
        }
        public static bool isLocoForward()
        {
            string pattern = getPattern();
//            Main.DebugLog("SignalPattern isForward pattern = " + pattern + " targetPattern = " + LOCO_FORWARD.Trim());
            return pattern.Equals(LOCO_FORWARD.Trim());
        }
        public static bool isLocoBackward()
        {
            return getPattern().Equals(LOCO_BACKWARD.Trim());
        }
    }
}
