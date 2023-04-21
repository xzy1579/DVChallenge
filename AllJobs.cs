using DV.Logic.Job;
using HarmonyLib;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace DvMod.Challenges
{
    public static class AllJobs
    {
        public static string curStation = "";
        public static string leftStation = "";
        public static string challengeStation = "";
        public static int numJobs = 0;
        public static int abandonJobs = 0;
        public static int takenJobs = 0;
        public static int completedJobs = 0;

        public static int counter = 0;

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

                    if (Main.mod != null && __instance.stationRange != null && !__instance.logicStation.ID.Equals("MFMB") && !__instance.logicStation.ID.Equals("HMB"))
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
//                            Main.DebugLog("AJ left station " + __instance.logicStation.ID + ": " + __instance.logicStation.name + "t=" + __instance.logicStation.takenJobs.Count + " c="+ __instance.logicStation.completedJobs.Count);
                            leftStation = curStation;
                            curStation = "";
                        }

                        // this gets hit next time around
                        if (leftStation.Equals(__instance.logicStation.ID))
                        {
                            // did we leave our challenge station?
                            if (leftStation.Equals(challengeStation))
                            {
                                if (__instance.logicStation.availableJobs.Count > 0)
                                {
                                    string message = "Left " + __instance.logicStation.availableJobs.Count + " jobs";
                                    Main.DebugLog("AJ "+message);

                                    AllJob newJob = new AllJob();
                                    newJob.stationId = leftStation;
                                    newJob.status = "Fail";
                                    newJob.message = message;
                                    string retVal = Status.save(newJob);
                                    Main.DebugLog(() => "Saving retVal = " + retVal);

                                    challengeStation = "";
                                    numJobs = 0;
                                    abandonJobs = 0;
                                    takenJobs = 0;
                                    completedJobs = 0;
                                }
                            }
                            if(challengeStation.Equals("") && __instance.logicStation.availableJobs.Count==0)
                            {
                                AllJob prevJob = Status.getJobStatus(leftStation);
                                if(!prevJob.status.Equals("Completed"))
                                {
                                    string message = "Took all";
                                    Main.DebugLog("AJ " +message);
                                    challengeStation = leftStation;
                                    takenJobs = __instance.logicStation.takenJobs.Count;

                                    AllJob newJob = new AllJob();
                                    newJob.stationId = leftStation;
                                    newJob.status = "InProgress";
                                    newJob.message = message;
                                    newJob.jobs = "" + takenJobs;
                                    string retVal = Status.save(newJob);
                                    Main.DebugLog(() => "Saving retVal = " + retVal);
                                }
                            }
                            leftStation = "";
                        }

                        // see if we have completed all jobs in the challengestation
                        if (!challengeStation.Equals("") && challengeStation.Equals(__instance.logicStation.ID))
                        {
                            if(__instance.logicStation.takenJobs.Count == 0 && takenJobs!=0 && __instance.logicStation.completedJobs.Count != 0 && __instance.logicStation.abandonedJobs.Count == 0)
                            {
                                string message =  __instance.logicStation.completedJobs.Count + " jobs";
                                Main.DebugLog("AJ " + message);

                                AllJob newJob = new AllJob();
                                newJob.stationId = challengeStation;
                                newJob.status = "Completed ";
                                newJob.message = message;
                                string retVal = Status.save(newJob);
                                Main.DebugLog(() => "Saving retVal = " + retVal);

                                challengeStation = "";
                                numJobs = 0;
                                abandonJobs = 0;
                                takenJobs = 0;
                                completedJobs = 0;
                            }
                            else
                            {
                                counter++;
                                if(counter>1000)
                                {
                                    if (__instance.logicStation.abandonedJobs.Count > 0)
                                    {
                                        string message = "Abandoned a job";
                                        Main.DebugLog("AJ " + message);

                                        AllJob newJob = new AllJob();
                                        newJob.stationId = challengeStation;
                                        newJob.status = "Fail";
                                        newJob.message = message;
                                        string retVal = Status.save(newJob);
                                        Main.DebugLog(() => "Saving retVal = " + retVal);
                                        Main.DebugLog("AJ Abaondon challenge for " + challengeStation+"-" + __instance.logicStation.ID + " t1=" + __instance.logicStation.takenJobs.Count + "t2=" + takenJobs + " c=" + __instance.logicStation.completedJobs.Count + " a=" + __instance.logicStation.abandonedJobs.Count);

                                        challengeStation = "";
                                        numJobs = 0;
                                        abandonJobs = 0;
                                        takenJobs = 0;
                                        completedJobs = 0;
                                    }
                                    Main.DebugLog("AJ Active challenge for " + challengeStation + " t1=" + __instance.logicStation.takenJobs.Count + "t2=" + takenJobs + " c=" + __instance.logicStation.completedJobs.Count + " a=" + __instance.logicStation.abandonedJobs.Count);
                                    counter = 0;
                                }
                            }
                        }


                        // are we at a new station?
                        if (atStation && !curStation.Equals(__instance.logicStation.ID))
                        {
//                            Main.DebugLog("AJ arrive station " + __instance.logicStation.ID + ": " + __instance.logicStation.name + " from " + curStation);
                            curStation = __instance.logicStation.ID;
                        }

                        if (atStation && challengeStation.Equals(__instance.logicStation.ID))
                        {
                        }
                        if (atStation && !challengeStation.Equals("") && !challengeStation.Equals(__instance.logicStation.ID) && __instance.logicStation.takenJobs.Count > 0)
                        {
                            string message = "Took " + __instance.logicStation.takenJobs.Count + " " +__instance.logicStation.ID + "jobs";
                            Main.DebugLog("AJ " + message);

                            AllJob newJob = new AllJob();
                            newJob.stationId = challengeStation;
                            newJob.status = "Fail";
                            newJob.message = message;
                            string retVal = Status.save(newJob);
                            Main.DebugLog(() => "Saving retVal Abandon = " + retVal + " " + newJob.stationId + " " + newJob.status);

                            challengeStation = "";
                            numJobs = 0;
                            abandonJobs = 0;
                            takenJobs = 0;
                            completedJobs = 0;

                        }
                        return true;
                    }
                }
                catch (System.Exception ex)
                {
                    if (Main.mod != null)
                    {
                        Main.DebugLog("AJ in the patchE " + ex.Message);
                    }
                }
                return true;
            }
        }
    }
}