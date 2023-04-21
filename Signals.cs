using System;
using HarmonyLib;

namespace DvMod.Challenges
{
    public static class Signals
    {

        public class LocoDirection
        {
            public string? direction;
            public float speed;
            public long startTime;
        }
        public static System.Collections.Generic.List<LocoDirection>? locoDirections = new System.Collections.Generic.List<LocoDirection>();
        public static System.Collections.Generic.List<string>? challengeResults = new System.Collections.Generic.List<string>();

        public static long logtime = 0;
        public static long signalValidationTimer = 0;

        static int SIGNAL_DELAY = 30000;
        static float STOP_SPEED = .3f;
        static float FULL_BRAKE = .88F;

        public static bool challengeStarted = false;

        public static string curStation = "";
        public static string challengeStation = "";
        public static LocoControllerBase? challengeLoco = null;
        public static DV.Logic.Job.Job? takenJob = null;

        public static int counter = 0;
        public static int counter2 = 0;
        public static bool loggedSignal = false;

        public static int prevLocoDirection = 0;
        public static float prevLocoSpeed = 0;
        public static int curLocoDirection = 0;
        public static float curLocoSpeed = 0;
        public static long stopTimerStart = 0;
        public static long stopTimerEnd = 0;
        public static long forwardSignalTime = 0;
        public static long backwardSignalTime = 0;

        public static void processSettings(Settings settings)
        {
            SIGNAL_DELAY = settings.SIGNAL_DELAY;
            STOP_SPEED = settings.STOP_SPEED;
            FULL_BRAKE = settings.FULL_BRAKE;
        }


        [HarmonyPatch(typeof(StationController), nameof(StationController.Update))]
        public static class UpdatePatch
        {
            public static bool Prefix(StationController __instance)
            {

                try
                {
                    DateTimeOffset now = (DateTimeOffset)DateTime.UtcNow;


                    if (__instance.logicStation == null || !SaveLoadController.carsAndJobsLoadingFinished)
                    {
                        if (Main.mod != null)
                        {
                            //                            Main.DebugLog("in the patch1a");
                        }
                        return false;
                    }
                    else
                    {
                        if (Main.mod != null)
                        {
                            //                            Main.DebugLog("in the patch1b");
                        }
                    }

                    if (challengeLoco!=null)
                    {
//                        logLocoSpeed(challengeLoco, challengeLoco.GetComponent<TrainCar>());
                        counter2++;
                        if(counter2==1)
                        {

                            //                            float testSpeed = challengeLoco.GetComponent<TrainCar>().GetVelocity().x;
                            //                            Main.DebugLog(() => "signalChallenge curSpeed = " + testSpeed);
                            //                            Main.DebugLog(() => "signalChallenge reverser = " + challengeLoco.GetReverserSymbol());

                            //                            if(SignalPattern.lastSignalTime > 0 && !loggedSignal)
                            //                            {
                            //                                if(now.ToUnixTimeMilliseconds() - SignalPattern.lastSignalTime > SignalPattern.SIGNAL_LATENCY)
                            //                                {
                            //                                    SignalPattern.logTimings();
                            //                                    loggedSignal = true;
                            //                                    if(SignalPattern.isChallengeStart())
                            //                                    {
                            //                                        SignalPattern.resetPattern();
                            //                                    }
                            //                                }
                            //                            }
                        }
                        if (counter2 > 1000)
                        {
                            counter2 = 0;
                            loggedSignal = false;
                        }

                    }

                    if (!challengeStarted || challengeLoco==null) return true;

                    if (Main.mod != null && __instance.stationRange != null && challengeStation.Equals("")
                        && !__instance.logicStation.ID.Equals("MFMB") && !__instance.logicStation.ID.Equals("HMB"))
                    {
                        // are we at a station?
                        if (__instance.stationRange.IsPlayerInRangeForBookletGeneration(__instance.stationRange.PlayerSqrDistanceFromStationOffice))
                        {
                            // only use the station as the challenge station if no jobs are taken
                            if(__instance.logicStation.takenJobs.Count == 0)
                            {
                                challengeStation = __instance.logicStation.ID;
                                Signal newJob = new Signal();
                                newJob.stationId = challengeStation;
                                newJob.status = "InProgress";
                                newJob.message = "";
                                newJob.jobs = "";
                                string retVal = Status.save(newJob);
                                Main.DebugLog(() => "signalChallenge challenge station = " + challengeStation);
                            }
                            else
                            {
//                                Main.DebugLog(() => "signalChallenge FAIL - jobs already taken at challenge station = " + challengeStation);
                            }
                        }
                    }

                    string workingStation = challengeStation;
                    if (workingStation == "") workingStation = "HB";
                    else
                    {
                        if (takenJob == null)
                        {
                            if (__instance.logicStation.ID.Equals(challengeStation) &&  __instance.logicStation.takenJobs.Count > 0)
                            {
                                takenJob = __instance.logicStation.takenJobs[0];
                                Main.DebugLog(() => "Took job  = " + takenJob.ID + " " + takenJob.State);
                            }

                        }
                        else
                        {
                            if (takenJob.State.Equals(DV.Logic.Job.JobState.Completed) || takenJob.State.Equals(DV.Logic.Job.JobState.Failed) || takenJob.State.Equals(DV.Logic.Job.JobState.Abandoned))
                            {
                                if (takenJob.State.Equals(DV.Logic.Job.JobState.Failed) || takenJob.State.Equals(DV.Logic.Job.JobState.Abandoned))
                                {
                                    Main.DebugLog(() => "Challenge abandoned at " + challengeStation);
                                    Signal newJob = new Signal();
                                    newJob.stationId = challengeStation;
                                    newJob.status = "Abandoned";
                                    string retVal = Status.save(newJob);
                                }
                                else
                                {
                                    string result = verifySignals(takenJob.State);
                                    if (result.Equals(""))
                                    {
                                        Main.DebugLog(() => "Challenge completed successfully at " + challengeStation);
                                        Signal newJob = new Signal();
                                        newJob.stationId = challengeStation;
                                        newJob.status = "Completed";
                                        string retVal = Status.save(newJob);

                                    }
                                    else
                                    {
                                        if (challengeResults != null)
                                        {
                                            if (challengeResults.Count > 0)
                                            {
                                                Signal newJob = new Signal();
                                                newJob.stationId = challengeStation;
                                                newJob.status = "Failed";
                                                newJob.message = challengeResults[0];
                                                string retVal = Status.save(newJob);
                                                Main.DebugLog(() => challengeResults[0]);
                                            }
                                            else
                                            {
                                                Signal newJob = new Signal();
                                                newJob.stationId = challengeStation;
                                                newJob.status = "Failed";
                                                newJob.message = "result not found";
                                                string retVal = Status.save(newJob);
                                                Main.DebugLog(() => "result not found");
                                            }
                                        }
                                        else
                                        {
                                            Signal newJob = new Signal();
                                            newJob.stationId = challengeStation;
                                            newJob.status = "Failed";
                                            newJob.message = "result not found";
                                            string retVal = Status.save(newJob);
                                            Main.DebugLog(() => "result not found");
                                        }
                                    }
                                }
                                resetChallenge();
                            }
                        }
                    }

                    if (workingStation.Equals(__instance.logicStation.ID))
                    {
                        long curTimeStamp = now.ToUnixTimeMilliseconds();

                        if (curTimeStamp > signalValidationTimer)
                        {

                            setLocoState(challengeLoco);


                            if ( (curLocoDirection != prevLocoDirection) )
                            {

                                Main.DebugLog(() => "signalChallenge Direction change timeStamp = " + curTimeStamp + " speed = " + curLocoSpeed + " prevdir = " + prevLocoDirection + " newdir=" + curLocoDirection );
                                LocoDirection locoDirection = new LocoDirection();
                                if (curLocoDirection == 0)
                                {
//                                    Main.DebugLog(() => "brake settings independent = " + challengeLoco.independentBrake + " brake = " + challengeLoco.brake);
                                    if(challengeLoco.independentBrake  >= FULL_BRAKE || challengeLoco.brake >= FULL_BRAKE)
                                    {
                                        locoDirection.direction = "S";
                                        locoDirection.speed = curLocoSpeed;
                                        locoDirection.startTime = curTimeStamp;
                                        if (locoDirections != null) locoDirections.Add(locoDirection);
                                        prevLocoDirection = curLocoDirection;
                                        prevLocoSpeed = curLocoSpeed;
                                    }
                                }
                                else if (curLocoDirection == 1)
                                {
                                    locoDirection.direction = "F";
                                    locoDirection.speed = curLocoSpeed;
                                    locoDirection.startTime = curTimeStamp;
                                    if (locoDirections != null) locoDirections.Add(locoDirection);
                                    prevLocoDirection = curLocoDirection;
                                    prevLocoSpeed = curLocoSpeed;
                                }
                                else
                                {
                                    locoDirection.direction = "B";
                                    locoDirection.speed = curLocoSpeed;
                                    locoDirection.startTime = curTimeStamp;
                                    if (locoDirections != null) locoDirections.Add(locoDirection);
                                    prevLocoDirection = curLocoDirection;
                                    prevLocoSpeed = curLocoSpeed;
                                }
                            }
                            signalValidationTimer = curTimeStamp + 100;
                        }
                    }
                }
                catch (System.Exception ex)
                {
                    if (Main.mod != null)
                    {
                        Main.DebugLog("SIG in the patchE " + ex.Message);
                    }
                }
                return true;
            }
        }

        public static string formatDirection(int i)
        {
            string retVal = "";
            DateTimeOffset now = (DateTimeOffset)DateTime.UtcNow;
            DateTimeOffset today = new DateTimeOffset(now.Year, now.Month, now.Day, 0, 0, 0, new TimeSpan(0));
            if (locoDirections == null) Main.DebugLog(() => "locoDirections is null");
            else
            {
                retVal = locoDirections[i].direction + " " + Math.Round((float)((locoDirections[i].startTime - today.ToUnixTimeMilliseconds()) /(float) 1000), 1);
            }
            return retVal;
        }

        public static void dumpDirections()
        {
            if (locoDirections == null) return;
            for (int i = 0; i < locoDirections.Count; i++)
            {
                Main.DebugLog(() => "direction " + formatDirection(i));
            }
        }

        public static string verifySignals(DV.Logic.Job.JobState jobState)
        {
            string retVal = "";
            int forwardCount = 0;
            int backwardCount = 0;
            int stopCount = 0;
            int timingErrors = 0;

            if (challengeResults == null) challengeResults = new System.Collections.Generic.List<string>();

            if (locoDirections != null)
                for (int i = 0; i < locoDirections.Count; i++)
                {
                    LocoDirection curDir = locoDirections[i];
                    switch (curDir.direction)
                    {
                        case "F": forwardCount++; break;
                        case "B": backwardCount++; break;
                        case "S": stopCount++; break;
                        default: continue;
                    }
                    if (SignalPattern.signalPatterns == null) { 
                        challengeResults.Add("No signal patterns");
                        retVal = "Fail";
                        return retVal;; 
                    }

                    bool foundPattern = false;
                    int j = 0;
                    while (j < SignalPattern.signalPatterns.Count && SignalPattern.signalPatterns[j].startTime < curDir.startTime)
                    {
                        j++;
                    }
                    if (j > 0 && j < SignalPattern.signalPatterns.Count)
                    {
                        int targetIndex = -1;
                        if (curDir.direction == "S")
                        {
                            bool usedPrev = false;
                            if (SignalPattern.signalPatterns[j].patternName == curDir.direction) targetIndex = j;
                            else if (SignalPattern.signalPatterns[j - 1].patternName == curDir.direction) { usedPrev = true; targetIndex = j - 1; }
                            else if (j < SignalPattern.signalPatterns.Count - 1 && SignalPattern.signalPatterns[j + 1].patternName == curDir.direction) targetIndex = j + 1;

                            if (targetIndex != -1)
                            {
                                if (Math.Abs(SignalPattern.signalPatterns[targetIndex].startTime - curDir.startTime) <= SIGNAL_DELAY)
                                {
                                    foundPattern = true;
                                    continue;
                                }
                                else
                                {
                                    // before we fail, see if the following stop pattern was a better match
                                    if(usedPrev && j < SignalPattern.signalPatterns.Count - 1 && SignalPattern.signalPatterns[j + 1].patternName == curDir.direction)
                                    {
                                        if (Math.Abs(SignalPattern.signalPatterns[j+1].startTime - curDir.startTime) <= SIGNAL_DELAY)
                                        {
                                            foundPattern = true;
                                            continue;
                                        }
                                    }
                                    timingErrors++;
                                    challengeResults.Add("TS" + stopCount + "=" + (int)(Math.Abs(SignalPattern.signalPatterns[targetIndex].startTime - curDir.startTime)) / 1000);
                                    retVal = "Fail";
                                    foundPattern = true;
                                    continue;
                                }
                            }
                            else
                            {
                                challengeResults.Add(curDir.direction + " not found for " + curDir.direction + stopCount);
                                retVal = "Fail";
                                continue;
                            }
                        }
                        else
                        {
                            int localCount = forwardCount;
                            if (curDir.direction == "B") localCount = backwardCount;
                            if (SignalPattern.signalPatterns[j - 1].patternName == curDir.direction) targetIndex = j - 1;
                            else if (SignalPattern.signalPatterns[j].patternName == curDir.direction) targetIndex = j;
                            else if (j < SignalPattern.signalPatterns.Count - 1 && SignalPattern.signalPatterns[j + 1].patternName == curDir.direction) targetIndex = j + 1;
                            if (targetIndex != -1)
                            {
                                if (Math.Abs(SignalPattern.signalPatterns[targetIndex].startTime - curDir.startTime) <= SIGNAL_DELAY)
                                {
                                    foundPattern = true;
                                    continue;
                                }
                                else
                                {
                                    timingErrors++;
                                    challengeResults.Add("T" + curDir.direction + localCount + "=" + (int)(Math.Abs(SignalPattern.signalPatterns[targetIndex].startTime - curDir.startTime)) / 1000);
                                    retVal = "Fail";
                                    foundPattern = true;
                                    continue;
                                }
                            }
                            else
                            {
                                challengeResults.Add(curDir.direction + " not found for " + curDir.direction + localCount);
                                retVal = "Fail";
                                continue;
                            }
                        }
                    }
                    else if (j == 0) // first pattern
                    {
                        if (SignalPattern.signalPatterns[j].patternName == curDir.direction)
                        {
                            if (Math.Abs(SignalPattern.signalPatterns[j].startTime - curDir.startTime) <= SIGNAL_DELAY)
                            {
                                foundPattern = true;
                                continue;
                            }
                            else
                            {
                                timingErrors++;
                                challengeResults.Add("TS" + stopCount + "=" + (int)(Math.Abs(SignalPattern.signalPatterns[j].startTime - curDir.startTime)) / 1000);
                                retVal = "Fail";
                                foundPattern = true;
                                continue;
                            }
                        }
                        else
                        {
                            challengeResults.Add("Bad First Signal " + curDir.direction + " " + SignalPattern.signalPatterns[j].patternName);
                            retVal = "Fail";
                            continue;
                        }
                    }
                    else            // last pattern
                    {
                        if (SignalPattern.signalPatterns[j - 1].patternName == curDir.direction)
                        {
                            if (Math.Abs(SignalPattern.signalPatterns[j - 1].startTime - curDir.startTime) <= SIGNAL_DELAY)
                            {
                                foundPattern = true;
                                continue;
                            }
                            else
                            {
                                int localCount = forwardCount;
                                if (curDir.direction == "B") localCount = backwardCount;
                                timingErrors++;
                                challengeResults.Add("T" + curDir.direction + localCount + "=" + (int)(Math.Abs(SignalPattern.signalPatterns[j - 1].startTime - curDir.startTime)) / 1000);
                                retVal = "Fail";
                                foundPattern = true;
                                continue;
                            }
                        }
                        else
                        {
                            challengeResults.Add("Bad Last Signal " + curDir.direction + " " + SignalPattern.signalPatterns[j - 1].patternName);
                            retVal = "Fail";
                            continue;
                        }
                    }
                }

            dumpDirections();
            SignalPattern.dumpPatterns();

            return retVal;
        }


        public static void setLocoState(LocoControllerBase targetLoco)
        {
            
            curLocoSpeed = Math.Abs(targetLoco.GetForwardSpeed());
            if (counter > 10)
            {
//                Main.DebugLog(() => "curSpeed=" + curLocoSpeed);
                counter = 0;
            }
            counter++;
            if (curLocoSpeed - STOP_SPEED > 0)
            {
                if (targetLoco.GetReverserSymbol().Equals("F")) curLocoDirection = 1;
                else if (targetLoco.GetReverserSymbol().Equals("R")) curLocoDirection = -1;
            }
            else
            {
                curLocoDirection = 0;
            }
        }

        public static void startChallenge(LocoControllerBase targetLoco)
        {

            if (targetLoco == null) return;
            DateTimeOffset now = (DateTimeOffset)DateTime.UtcNow;

            curLocoSpeed = prevLocoSpeed;
            curLocoDirection = prevLocoDirection;

            Signals.challengeStarted = true;
            Signals.challengeLoco = targetLoco;

            setLocoState(targetLoco);

            prevLocoSpeed = curLocoSpeed;
            prevLocoDirection = curLocoDirection;

            Signals.stopTimerStart = now.ToUnixTimeMilliseconds() * 2;       // MULTIPLY by 2 to give player plenty of time to move the loco and not fail stop signal

            Main.DebugLog("going to clear out history");
            locoDirections = new System.Collections.Generic.List<LocoDirection>();
            challengeResults = new System.Collections.Generic.List<string>();
            SignalPattern.signalPatterns = new System.Collections.Generic.List<SignalPattern.Pattern>();

            Main.DebugLog("challenge started, speed = " + curLocoSpeed + " direction = " + curLocoDirection);
        }
        public static void resetChallenge()
        {

            Main.DebugLog("reset challenge ");
            challengeStarted = false;

            curStation = "";
            challengeStation = "";
            challengeLoco = null;
            takenJob = null;

            counter = 0;

            prevLocoDirection = 0;
            stopTimerStart = 0;
            stopTimerEnd = 0;
            forwardSignalTime = 0;
            backwardSignalTime = 0;
    }

    public static void logLocoSpeed(LocoControllerBase logLoco, TrainCar logTrainCar)
        {
            DateTimeOffset now = (DateTimeOffset)DateTime.UtcNow;
            if (now.ToUnixTimeMilliseconds() > logtime)
            {

                if (logLoco == null)
                {
                    Main.DebugLog("loco is null");
                    return;
                }
//                Main.DebugLog("Loco x = " + logLoco.GetVelocity().x);
//                Main.DebugLog("Loco y = " + logLoco.GetVelocity().y);
                Main.DebugLog("Loco forwardSpeed = " + logLoco.GetForwardSpeed());
//                Main.DebugLog("TrainCar forwardSpeed = " + logTrainCar.GetForwardSpeed());
                logtime = now.ToUnixTimeMilliseconds() + 1000;
            }
        }

    }
}
