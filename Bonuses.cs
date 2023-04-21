using System;
using HarmonyLib;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DvMod.Challenges
{
    public static class Bonuses
    {
        public static string curStation = "";
        public static string leftStation = "";
        public static string challengeStation = "";
        public static int takenJobs = 0;
        public static System.Collections.Generic.List<DV.Logic.Job.Job>? takenJobList;

        public static int counter = 0;
        public static int counter2 = 0;

        [HarmonyPatch(typeof(StationController), nameof(StationController.Update))]
        public static class UpdatePatch
        {
            public static bool Prefix(StationController __instance)
            {

                try
                {

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

                    if (Main.mod != null && __instance.stationRange != null 
                        && !__instance.logicStation.ID.Equals("MFMB") && !__instance.logicStation.ID.Equals("HMB")
                        && !__instance.logicStation.ID.Equals("FRC") && !__instance.logicStation.ID.Equals("FRS"))
                    {
                        bool atStation = false;
                        // are we at a station?
                        if (__instance.stationRange.IsPlayerInRangeForBookletGeneration(__instance.stationRange.PlayerSqrDistanceFromStationOffice))
                        {
                            atStation = true;
                        }

                        // did we leave a station
                        if (curStation.Equals(__instance.logicStation.ID) && atStation == false && !curStation.Equals(""))
                        {
//                            Main.DebugLog("BN left station " + __instance.logicStation.ID + ": " + __instance.logicStation.name + "t=" + __instance.logicStation.takenJobs.Count + " c=" + __instance.logicStation.completedJobs.Count);
                            leftStation = curStation;
                            curStation = "";
                        }

                        // this gets hit next time around
                        if (leftStation.Equals(__instance.logicStation.ID))
                        {
                            // did we leave our challenge station?
                            if (leftStation.Equals(challengeStation))
                            {
                                if (__instance.logicStation.takenJobs.Count < 2)
                                {
                                    string message = "Did not take 2 or more jobs";
                                    Main.DebugLog("BN " + message);

                                    Bonus newJob = new Bonus();
                                    newJob.stationId = leftStation;
                                    newJob.status = "Fail";
                                    newJob.message = message;
                                    string retVal = Status.save(newJob);
                                    Main.DebugLog(() => "Saving retVal = " + retVal);

                                    challengeStation = "";
                                    takenJobs = 0;
                                }
                            }
                            if (challengeStation == "" && __instance.logicStation.takenJobs.Count == 2)
                            {
                                Bonus prevJob = Status.getBonusStatus(leftStation);
                                if (!prevJob.status.Equals("Completed"))
                                {
                                    DV.Logic.Job.Job job1 = __instance.logicStation.takenJobs[0];
                                    DV.Logic.Job.Job job2 = __instance.logicStation.takenJobs[1];

                                    // both jobs must originate from this station
                                    if (job1.chainData.chainOriginYardId.Equals(leftStation) && job2.chainData.chainOriginYardId.Equals(leftStation))
                                    {
                                        // jobs can't go to same station, or this station
                                        if(!job1.chainData.chainDestinationYardId.Equals(job2.chainData.chainDestinationYardId) &&
                                            !job1.chainData.chainDestinationYardId.Equals(leftStation) &&
                                            !job2.chainData.chainDestinationYardId.Equals(leftStation))
                                        {
                                            string message = "Took 2 jobs ";
                                            Main.DebugLog("BN " + message);
                                            challengeStation = leftStation;
                                            takenJobs = __instance.logicStation.takenJobs.Count;
                                            takenJobList = new System.Collections.Generic.List<DV.Logic.Job.Job>();
                                            for(int i=0; i< __instance.logicStation.takenJobs.Count; i++)
                                            {
                                                takenJobList.Add(__instance.logicStation.takenJobs[i]);
                                                DV.Logic.Job.Job curJob = takenJobList[i];
                                                string debugMessage = curJob.ID + "   START *********************";
                                                debugMessage = debugMessage + "\n dest=" + curJob.chainData.chainDestinationYardId;
                                                debugMessage = debugMessage + "\n orig=" + curJob.chainData.chainOriginYardId;
                                                debugMessage = debugMessage + "\n type=" + curJob.GetType();
                                                debugMessage = debugMessage + "\n state=" + curJob.State;
                                                debugMessage = debugMessage + "\n base=" + curJob.GetBasePaymentForTheJob();
                                                debugMessage = debugMessage + "\n bonus=" + curJob.GetBonusPaymentForTheJob();
                                                debugMessage = debugMessage + "\n potbonus=" + curJob.GetPotentialBonusPaymentForTheJob();
                                                debugMessage = debugMessage + "\n startTime=" + curJob.startTime;
                                                debugMessage = debugMessage + "\n completeTime=" + curJob.GetJobCompletionTime();
                                                debugMessage = debugMessage + "\n timeLimit=" + curJob.TimeLimit;
                                                debugMessage = debugMessage + "\n timeonjob=" + curJob.GetTimeOnJob();
                                                debugMessage = debugMessage + "\n finishTime=" + curJob.finishTime;
                                                debugMessage = debugMessage + "\n wage=" + curJob.GetWageForTheJob();
                                                debugMessage = debugMessage + "\n initwage=" + curJob.initialWage;
                                                debugMessage = debugMessage + "\n";
                                                Main.DebugLog(debugMessage);
                                            }

                                            Bonus newJob = new Bonus();
                                            newJob.stationId = leftStation;
                                            newJob.status = "InProgress";
                                            newJob.message = message;
                                            newJob.jobs = "" + takenJobs;
                                            string retVal = Status.save(newJob);
                                            Main.DebugLog(() => "Saving retVal = " + retVal);
                                        }
                                    }
                                }
                            }
                            leftStation = "";
                        }

                        // see if we have completed a job in the challengestation
                        if (challengeStation.Equals(__instance.logicStation.ID))
                        {
                            int numJobsLeft = 2;
                            if (__instance.logicStation.takenJobs.Count < takenJobs)
                            {
                                string message = "";

                                bool failed = false;


                                if (takenJobList != null)
                                {
                                    for (int i = 0; i < takenJobList.Count; i++)
                                    {
                                        DV.Logic.Job.Job curJob = takenJobList[i];

                                        // is job complete
                                        if (curJob.State.Equals(DV.Logic.Job.JobState.Completed))
                                        {
                                            /*
                                            string debugMessage = curJob.ID + "   END *********************";
                                            debugMessage = debugMessage + "\n dest=" + curJob.chainData.chainDestinationYardId;
                                            debugMessage = debugMessage + "\n orig=" + curJob.chainData.chainOriginYardId;
                                            debugMessage = debugMessage + "\n type=" + curJob.GetType();
                                            debugMessage = debugMessage + "\n state=" + curJob.State;
                                            debugMessage = debugMessage + "\n base=" + curJob.GetBasePaymentForTheJob();
                                            debugMessage = debugMessage + "\n bonus=" + curJob.GetBonusPaymentForTheJob();
                                            debugMessage = debugMessage + "\n potbonus=" + curJob.GetPotentialBonusPaymentForTheJob();
                                            debugMessage = debugMessage + "\n startTime=" + curJob.startTime;
                                            debugMessage = debugMessage + "\n completeTime=" + curJob.GetJobCompletionTime();
                                            debugMessage = debugMessage + "\n timeLimit=" + curJob.TimeLimit;
                                            debugMessage = debugMessage + "\n timeonjob=" + curJob.GetTimeOnJob();
                                            debugMessage = debugMessage + "\n finishTime=" + curJob.finishTime;
                                            debugMessage = debugMessage + "\n wage=" + curJob.GetWageForTheJob();
                                            debugMessage = debugMessage + "\n initwage=" + curJob.initialWage;
                                            debugMessage = debugMessage + "\n";
                                            Main.DebugLog(debugMessage);
                                            */
                                            // if job was completed within time, then bonus was met

                                            // add 60 seconds to TimeLimit because DVR grants bonus until next minute
                                            if ((curJob.finishTime-curJob.startTime) < (curJob.TimeLimit + 60))
                                            {
                                                numJobsLeft--;
                                                message = curJob.ID + " OK";
                                            }
                                            else
                                            {
                                                failed = true;
                                                message = curJob.ID+" EX";
                                            }
                                        }
                                        else if(!curJob.State.Equals(DV.Logic.Job.JobState.InProgress))
                                        {
                                            failed = true;
                                            message = curJob.ID + " ?";
                                            break;
                                        }
                                    }
                                }
                                else
                                {
                                    failed = true;
                                    message = "BN no jobs";
                                }

                                Bonus newJob = new Bonus();
                                newJob.stationId = challengeStation;
                                if (failed)
                                {
                                    newJob.status = "Fail ";
                                }
                                else if (numJobsLeft > 0)
                                {
                                    newJob.status = "InProgress ";
                                }
                                else
                                {
                                    newJob.status = "Completed ";
                                    message = " 2 bonus";
                                }
                                newJob.message = message;
                                string retVal = Status.save(newJob);
//                                Main.DebugLog(() => "Saving retVal = " + retVal);

                                if (failed || numJobsLeft == 0)
                                {
                                    challengeStation = "";
                                    takenJobs = 0;
//                                    Main.DebugLog("Set challenge station to blank, " + failed + " " + numJobsLeft);
                                }

                                if(!message.Equals(""))
                                {
//                                    Main.DebugLog("BN " + message);
                                }

                            }
                            else if (__instance.logicStation.takenJobs.Count == 0)
                            {
                                challengeStation = "";
                                takenJobs = 0;
//                                Main.DebugLog("Set challenge station to blank, taken jobs is 0 ");
                            }
                            else
                            {
                                counter++;
                                if (counter > 1000)
                                {
                                    counter = 0;
                                }
                            }
                            takenJobs = numJobsLeft;
                        }


                        // are we at a new station?
                        if (atStation && !curStation.Equals(__instance.logicStation.ID))
                        {
//                            Main.DebugLog("BN arrive station " + __instance.logicStation.ID + ": " + __instance.logicStation.name + " from " + curStation);
                            curStation = __instance.logicStation.ID;
                        }

                        // check if at challenge station
                        if (atStation && challengeStation.Equals(__instance.logicStation.ID))
                        {
                        }
                        // check if took jobs at different station
                        if (atStation && !challengeStation.Equals("") && !challengeStation.Equals(__instance.logicStation.ID) && __instance.logicStation.takenJobs.Count > 0)
                        {

                        }
                        return true;
                    }
                }
                catch (System.Exception ex)
                {
                    if (Main.mod != null)
                    {
                        Main.DebugLog("BN in the patchE " + ex.Message);
                    }
                }
                return true;
            }
        }
    }
}
/*
 *                                 string debugMessage = curJob.ID + "   END *********************";
                                debugMessage = debugMessage + "\n dest=" + curJob.chainData.chainDestinationYardId;
                                debugMessage = debugMessage + "\n orig=" + curJob.chainData.chainOriginYardId;
                                debugMessage = debugMessage + "\n type=" + curJob.GetType();
                                debugMessage = debugMessage + "\n state=" + curJob.State;
                                debugMessage = debugMessage + "\n base=" + curJob.GetBasePaymentForTheJob();
                                debugMessage = debugMessage + "\n bonus=" + curJob.GetBonusPaymentForTheJob();
                                debugMessage = debugMessage + "\n potbonus=" + curJob.GetPotentialBonusPaymentForTheJob();
                                debugMessage = debugMessage + "\n startTime=" + curJob.startTime;
                                debugMessage = debugMessage + "\n completeTime=" + curJob.GetJobCompletionTime();
                                debugMessage = debugMessage + "\n timeLimit=" + curJob.TimeLimit;
                                debugMessage = debugMessage + "\n timeonjob=" + curJob.GetTimeOnJob();
                                debugMessage = debugMessage + "\n finishTime=" + curJob.finishTime;
                                debugMessage = debugMessage + "\n wage=" + curJob.GetWageForTheJob();
                                debugMessage = debugMessage + "\n initwage=" + curJob.initialWage;
                                debugMessage = debugMessage + "\n";
                                Main.DebugLog(debugMessage);

 [Challenges] OWN-SL-33   START *********************
 dest=HB
 orig=OWN
 type=DV.Logic.Job.Job
 state=InProgress
 base=6951
 bonus=3475.5
 potbonus=3475.5
 startTime=678.8709
 completeTime=-678.8709
 timeLimit=480
 timeonjob=8.336304
 finishTime=0
 wage=10426.5
 initwage=6951

 [Challenges] OWN-SL-33   END *********************
 dest=HB
 orig=OWN
 type=DV.Logic.Job.Job
 state=Completed
 base=6951
 bonus=3475.5
 potbonus=3475.5
 startTime=678.8709
 completeTime=191.5419
 timeLimit=480
 timeonjob=191.5565
 finishTime=870.4128
 wage=10426.5
 initwage=6951

[Challenges] OWN-FH-55   START *********************
 dest=HB
 orig=OWN
 type=DV.Logic.Job.Job
 state=InProgress
 base=28361
 bonus=14180.5
 potbonus=14180.5
 startTime=681.1271
 completeTime=-681.1271
 timeLimit=1560
 timeonjob=6.080139
 finishTime=0
 wage=42541.5
 initwage=28361
 
[Challenges] OWN-FH-55   END *********************
 dest=HB
 orig=OWN
 type=DV.Logic.Job.Job
 state=InProgress
 base=28361
 bonus=14180.5
 potbonus=14180.5
 startTime=681.1271
 completeTime=-681.1271
 timeLimit=1560
 timeonjob=189.3004
 finishTime=0
 wage=42541.5
 initwage=28361
*/
