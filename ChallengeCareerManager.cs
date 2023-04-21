using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using DV.ServicePenalty.UI;
using HarmonyLib;
using UnityEngine;

namespace DvMod.Challenges
{

	public static class ChallengeCareerManager
	{


		public static bool inStats = false;
		public static bool inAllJobs = false;
		public static bool inBonus = false;
		public static bool inSignalSub  = false;
		public static bool inSignal = false;
		public static bool inSigHistory = false;
		public static bool inLocoHistory = false;
		public static bool inSigResults = false;

		public static ChallengeScreen? challengeScreen;
		public static CareerManagerStatsScreen? originalStats;
		public static CareerManagerLicensesScreen? originalLicense;

		[HarmonyPatch(typeof(CareerManagerMainScreen), "Activate")]
		public static class CareerPatchActivate
		{

			public static void Postfix(DV.ServicePenalty.UI.CareerManagerMainScreen __instance)
			{
				if (Main.mod != null)
				{
//					Main.DebugLog(() => "We are in the career manager - Activate");
					try
					{

						if (__instance.screenSwitcher == null)
						{
							Main.DebugLog(() => "screenSwitcher reference isn't set! Screen can't function!");
							return;
						}
						else
						{
							originalStats = __instance.statsScreen;
							originalLicense = __instance.licensesScreen;

						}
					}
					catch (Exception ex)
					{
						Main.DebugLog(() => "exception " + ex.Message);

					}
				}
				return;
			}
		}

		[HarmonyPatch(typeof(CareerManagerMainScreen), "HandleInputAction")]
		public static class CareerManagerHandleInput
		{
			public static void Postfix(InputAction input, DV.ServicePenalty.UI.CareerManagerMainScreen __instance)
			{

				if (Main.mod != null)
				{

					try
					{
						originalStats = __instance.statsScreen;
						originalLicense = __instance.licensesScreen;

						switch (input)
						{
							case InputAction.Cancel:
								inStats = false;
								inAllJobs = false;
								inBonus = false;
								inSignalSub = false;
								inSignal = false;
								inSigHistory = false;
								inLocoHistory = false;
								inSigResults = false;
								break;
							case InputAction.Confirm:
								if (__instance.selector.Current == 2)
								{
									inStats = true;
									__instance.screenSwitcher.SetActiveDisplay(__instance.licensesScreen);
								}
								break;
							default:
								return;
						}
					}
					catch
					{
						Main.DebugLog(() => "exception in handle input post");

					}
				}
				return;
			}
		}
		
		[HarmonyPatch(typeof(CareerManagerStatsScreen), "HandleInputAction")]
		public static class StatsScreenHandleInput
		{
			public static void Postfix(InputAction input, DV.ServicePenalty.UI.CareerManagerStatsScreen __instance)
			{

				if (Main.mod != null)
				{
					//					Main.DebugLog(() => "We are in the career manager handle input - post ");

					try
					{

						switch (input)
						{
							case InputAction.Cancel:
								// we go back to the license screen because that is used as the main stats menu
								inStats = true;
								inAllJobs = false;
								inBonus = false;
								inSignalSub = false;
								inSignal = false;
								inSigHistory = false;
								inLocoHistory = false;
								inSigResults = false;
								__instance.screenSwitcher.SetActiveDisplay(originalLicense);
								break;
							default:
								return;
						}
					}
					catch
					{
						Main.DebugLog(() => "exception in handle input post");

					}
				}
				return;
			}
		}

		[HarmonyPatch(typeof(CareerManagerLicensesScreen), "Activate")]
		public static class LicensePatchActivate
		{
			public static void Postfix(DV.ServicePenalty.UI.CareerManagerLicensesScreen __instance)
			{
				if (Main.mod != null)
				{
//					Main.DebugLog(() => "We are in the license - Activate");
					try
					{
						if (__instance.screenSwitcher == null)
						{
							Main.DebugLog(() => "screenSwitcher reference isn't set! Screen can't function!");
							return;
						}
						else
						{
							if (inStats)
							{
								
								if (inAllJobs) __instance.title1.text = "All Jobs";
								else if (inBonus)
                                {
									__instance.title1.text = "Double Bonus";
								}
								else if (inSignal)
                                {
									__instance.title1.text = "Signal Challenges";
									__instance.selector.Reset();
									__instance.indexOfFirstDisplayedLicense = 0;
									__instance.HighlightSelected(0, 3);
								}
								else if (inSigHistory)
                                {
									__instance.title1.text = "Signal History";
								}
								else if (inLocoHistory)
                                {
									__instance.title1.text = "Direction History";
								}
								else if (inSigResults)
                                {
									__instance.title1.text = "Signal Results";
								}
								else if (inSignalSub)
                                {
									__instance.title1.text = "Signals";
									__instance.selector.Reset();
									__instance.indexOfFirstDisplayedLicense = 0;
									__instance.HighlightSelected(0,3);
								}
								else
                                {
									__instance.title1.text = "Game Stats";
								}
							}

						}
					}
					catch (Exception ex)
					{
						Main.DebugLog(() => "exception " + ex.Message);

					}
				}
				return;
			}
		}

		[HarmonyPatch(typeof(CareerManagerLicensesScreen.LicenseEntry), "UpdateJobLicenseData")]
		public static class JobLicensePatchStatus
		{
			public static void Postfix(DV.ServicePenalty.UI.CareerManagerLicensesScreen.LicenseEntry __instance, JobLicenses jobLicense)
			{
				if (Main.mod != null)
				{
//					Main.DebugLog(() => "We are in the job license entry - UpdateJobLicenseData");
					try
					{
						if (inStats)
						{
							__instance.status.text = "";
						}
					}
					catch (Exception ex)
					{
						Main.DebugLog(() => "exception " + ex.Message);

					}
				}
				return;
			}
		}

		[HarmonyPatch(typeof(CareerManagerLicensesScreen.LicenseEntry), "UpdateGeneralLicenseData")]
		public static class GeneralLicensePatchStatus
		{
			public static void Postfix(DV.ServicePenalty.UI.CareerManagerLicensesScreen.LicenseEntry __instance, GeneralLicenseType generalLicense)
			{
				if (Main.mod != null)
				{
//					Main.DebugLog(() => "We are in the general license entry - UpdateGeneralLicenseData");
					try
					{
						if (inStats)
						{
							__instance.status.text = "";
						}
					}
					catch (Exception ex)
					{
						Main.DebugLog(() => "exception " + ex.Message);

					}
				}
				return;
			}
		}

		[HarmonyPatch(typeof(CareerManagerLicensesScreen), "HandleInputAction")]
		public static class LicenseHandleInput
		{
			public static void Postfix(InputAction input, DV.ServicePenalty.UI.CareerManagerLicensesScreen __instance)
			{

				if (Main.mod != null)
				{
					//					Main.DebugLog(() => "We are in the career manager handle input - post ");

					try
					{
						if (!inStats)
						{
							return;
						}
						else
						{
							switch (input)
							{
								case InputAction.Cancel:
									bool stayOnCurScreen = false;
									if (inAllJobs || inBonus || inSignalSub || inSignal || inSigHistory  || inLocoHistory || inSigResults)
									{
										stayOnCurScreen = true;
										inStats = true;
										if (inSignal || inSigHistory || inLocoHistory || inSigResults)
										{
											inSignalSub = true;
										}
										else inSignalSub = false;
									}
									else inStats = false;
									inAllJobs = false;
									inBonus = false;
									inSignal = false;
									inSigHistory = false;
									inLocoHistory = false;
									inSigResults = false;
									if(stayOnCurScreen) __instance.screenSwitcher.SetActiveDisplay(originalLicense);
									else
                                    {
										__instance.screenSwitcher.SetActiveDisplay(__instance.mainScreen);
									}
									break;
								case InputAction.Confirm:
									if (__instance.selector.Current == 0)
									{
										if (!inSignalSub)
										{
											inAllJobs = false;
											inBonus = false;
											inSignalSub = false;
											inSignal = false;
											inSigHistory = false;
											inLocoHistory = false;
											inSigResults = false;
											__instance.screenSwitcher.SetActiveDisplay(originalStats);
										}
										else
										{
											inAllJobs = false;
											inBonus = false;
											inSignalSub = true;
											inSignal = true;
											inSigHistory = false;
											inLocoHistory = false;
											inSigResults = false;
											__instance.screenSwitcher.SetActiveDisplay(originalLicense);
										}
									}
									else if (__instance.selector.Current == 1)
									{
										if(!inSignalSub)
                                        {
											inAllJobs = true;
											inBonus = false;
											inSignalSub = false;
											inSignal = false;
											inSigHistory = false;
											inLocoHistory = false;
											inSigResults = false;
										}
										else
                                        {
											inAllJobs = false;
											inBonus = false;
											inSignalSub = true;
											inSignal = false;
											inSigHistory = true;
											inLocoHistory = false;
											inSigResults = false;
										}
										__instance.screenSwitcher.SetActiveDisplay(originalLicense);
									}
									else if (__instance.selector.Current == 2)
									{
										if (!inSignalSub)
										{
											// double bonus not done tbd
											inAllJobs = false;
											inBonus = true;
											inSignalSub = false;
											inSignal = false;
											inSigHistory = false;
											inLocoHistory = false;
											inSigResults = false;
										}
										else
                                        {
											inAllJobs = false;
											inBonus = false;
											inSignalSub = true;
											inSignal = false;
											inSigHistory = false;
											inLocoHistory = true;
											inSigResults = false;
										}
										__instance.screenSwitcher.SetActiveDisplay(originalLicense);
									}
									else if (__instance.selector.Current == 3)
									{
										if (!inSignalSub)
										{
											inAllJobs = false;
											inBonus = false;
											inSignalSub = true;
											inSignal = false;
											inSigHistory = false;
											inLocoHistory = false;
											inSigResults = false;
										}
										else
										{
											inAllJobs = false;
											inBonus = false;
											inSignalSub = true;
											inSignal = false;
											inSigHistory = false;
											inLocoHistory = false;
											inSigResults = true;
										}
									__instance.screenSwitcher.SetActiveDisplay(originalLicense);
									}
									break;
								default:
									return;
							}
						}
					}
					catch
					{
						Main.DebugLog(() => "exception in handle input post");

					}
				}
				return;
			}
		}

		[HarmonyPatch(typeof(LicenseManager), "DisplayName")]
		public static class DisplayLicenseGeneral
		{
			[HarmonyPatch(new Type[]
			{
			typeof(GeneralLicenseType)
			})]

			public static bool Prefix(GeneralLicenseType license, ref string __result, DV.ServicePenalty.UI.CareerManagerLicensesScreen __instance)
			{
//				Main.DebugLog(() => "looking at license " + license + " " + ((int)license));
				if (inStats)
				{
					__result = "";
					int stationInt = ((int)license);
					if (inAllJobs)
					{
						__result = getJobsMenuText(stationInt);
					}
					else if (inBonus)
					{
						__result = getBonusMenuText(stationInt);
					}
					else if (inSignal)
					{
						__result = getSignalMenuText(stationInt);	
					}
					else if (inSigHistory)
                    {
						__result = getSignalHistory(stationInt);                    
					}
					else if (inLocoHistory)
					{
						__result = getLocoHistory(stationInt);
					}
					else if (inSigResults)
					{
						__result = getSignalChallengeResults(stationInt);
					}
					else
					{
						__result = getChallengesMenuText(stationInt);
					}

					return false;
				}
				return true;
			}
		}

		[HarmonyPatch(typeof(LicenseManager), "DisplayName")]
		public static class DisplayLicenseJob
		{
			[HarmonyPatch(new Type[]
			{
			typeof(JobLicenses)
			})]

			public static bool Prefix(JobLicenses license, ref string __result, DV.ServicePenalty.UI.CareerManagerLicensesScreen __instance)
			{
				//				Main.DebugLog(() => "looking at license " + license + " " + ((int)license));
				if (inStats)
				{
					__result = "";
					int stationInt = ((int)license);
					if (inAllJobs)
					{
						__result = getJobsMenuText(stationInt);
					}
					else if (inBonus)
					{
						__result = getBonusMenuText(stationInt);
					}
					else if (inSignal)
					{
						__result = getSignalMenuText(stationInt);
					}
					else if (inSigHistory)
					{
						__result = getSignalHistory(stationInt);
					}
					else if (inLocoHistory)
					{
						__result = getLocoHistory(stationInt);
					}
					else if (inSigResults)
					{
						__result = getSignalChallengeResults(stationInt);
					}
					else
					{
						__result = getChallengesMenuText(stationInt);
					}

					return false;
				}
				return true;
			}
		}

		public static string getJobsMenuText(int stationInt)
        {
			string __result = "";
			AllJob? targetJob = null;
			switch (stationInt)
			{
				case 10:
					targetJob = Status.getJobStatus("HB"); break;
				case 512:
					targetJob = Status.getJobStatus("SM"); break;
				case 1024:
					targetJob = Status.getJobStatus("CSW"); break;
				case 300:
					targetJob = Status.getJobStatus("MF"); break;
				case 100:
					targetJob = Status.getJobStatus("FF"); break;
				case 2048:
					targetJob = Status.getJobStatus("GF"); break;
				case 20:
					targetJob = Status.getJobStatus("SW"); break;
				case 25:
					targetJob = Status.getJobStatus("FRC"); break;
				case 30:
					targetJob = Status.getJobStatus("FRS"); break;
				case 210:
					targetJob = Status.getJobStatus("OWC"); break;
				case 211:
					targetJob = Status.getJobStatus("OWN"); break;
				case 16384:
					targetJob = Status.getJobStatus("IME"); break;
				case 32768:
					targetJob = Status.getJobStatus("IMW"); break;
				case 1:
					targetJob = Status.getJobStatus("CM"); break;
				case 2:
					targetJob = Status.getJobStatus("FM"); break;
			}
			if (targetJob != null)
			{
				__result = targetJob.stationId + " " + targetJob.status + " " + targetJob.message;

			}
			return __result;
        }
		public static string getBonusMenuText(int stationInt)
		{
			string __result = "";
			Bonus? targetJob = null;
			switch (stationInt)
			{
				case 10:
					targetJob = Status.getBonusStatus("HB"); break;
				case 512:
					targetJob = Status.getBonusStatus("SM"); break;
				case 1024:
					targetJob = Status.getBonusStatus("CSW"); break;
				case 300:
					targetJob = Status.getBonusStatus("MF"); break;
				case 100:
					targetJob = Status.getBonusStatus("FF"); break;
				case 2048:
					targetJob = Status.getBonusStatus("GF"); break;
				case 20:
					targetJob = Status.getBonusStatus("SW"); break;
				case 25:
					targetJob = Status.getBonusStatus("CM"); break;
				case 30:
					targetJob = Status.getBonusStatus("FM"); break;
				case 210:
					targetJob = Status.getBonusStatus("OWC"); break;
				case 211:
					targetJob = Status.getBonusStatus("OWN"); break;
				case 16384:
					targetJob = Status.getBonusStatus("IME"); break;
				case 32768:
					targetJob = Status.getBonusStatus("IMW"); break;
			}
			if (targetJob != null)
			{
				__result = targetJob.stationId + " " + targetJob.status + " " + targetJob.message;

			}
			return __result;
		}
		public static string getSignalMenuText(int stationInt)
		{
			string __result = "";
			Signal? targetJob = null;
			switch (stationInt)
			{
				case 10:
					targetJob = Status.getSignalStatus("HB"); break;
				case 512:
					targetJob = Status.getSignalStatus("SM"); break;
				case 1024:
					targetJob = Status.getSignalStatus("CSW"); break;
				case 300:
					targetJob = Status.getSignalStatus("MF"); break;
				case 100:
					targetJob = Status.getSignalStatus("FF"); break;
				case 2048:
					targetJob = Status.getSignalStatus("GF"); break;
				case 20:
					targetJob = Status.getSignalStatus("SW"); break;
				case 25:
					targetJob = Status.getSignalStatus("FRC"); break;
				case 30:
					targetJob = Status.getSignalStatus("FRS"); break;
				case 210:
					targetJob = Status.getSignalStatus("OWC"); break;
				case 211:
					targetJob = Status.getSignalStatus("OWN"); break;
				case 16384:
					targetJob = Status.getSignalStatus("IME"); break;
				case 32768:
					targetJob = Status.getSignalStatus("IMW"); break;
				case 1:
					targetJob = Status.getSignalStatus("CM"); break;
				case 2:
					targetJob = Status.getSignalStatus("FM"); break;
			}
			if (targetJob != null)
			{
				__result = targetJob.stationId + " " + targetJob.status + " " + targetJob.message;

			}
			return __result;
		}
		public static string getSignalHistory(int stationInt)
		{
			int numSignals = SignalPattern.signalPatterns.Count();
			if (numSignals == 0) return "";


			int offset = numSignals - 19;
			if (offset < 0) offset = 0; ;
			string __result = "";
			int index = 0;
			switch (stationInt)
			{
				case 10:
					index = 0 + offset;
					if (index >= numSignals) __result = "";
                    else __result = SignalPattern.formatPattern(index);
					break;
				case 512:
					index = 1 + offset;
					if (index >= numSignals) __result = "";
					else __result = SignalPattern.formatPattern(index);
					break;
				case 1024:
					index = 2 + offset;
					if (index >= numSignals) __result = "";
					else __result = SignalPattern.formatPattern(index);
					break;
				case 300:
					index = 3 + offset;
					if (index >= numSignals) __result = "";
					else __result = SignalPattern.formatPattern(index);
					break;
				case 100:
					index = 4 + offset;
					if (index >= numSignals) __result = "";
					else __result = SignalPattern.formatPattern(index);
					break;
				case 2048:
					index = 5 + offset;
					if (index >= numSignals) __result = "";
					else __result = SignalPattern.formatPattern(index);
					break;
				case 20:
					index = 6 + offset;
					if (index >= numSignals) __result = "";
					else __result = SignalPattern.formatPattern(index);
					break;
				case 25:
					index = 7 + offset;
					if (index >= numSignals) __result = "";
					else __result = SignalPattern.formatPattern(index);
					break;
				case 30:
					index = 8 + offset;
					if (index >= numSignals) __result = "";
					else __result = SignalPattern.formatPattern(index);
					break;
				case 210:
					index = 9 + offset;
					if (index >= numSignals) __result = "";
					else __result = SignalPattern.formatPattern(index);
					break;
				case 211:
					index = 10 + offset;
					if (index >= numSignals) __result = "";
					else __result = SignalPattern.formatPattern(index);
					break;
				case 16384:
					index = 11 + offset;
					if (index >= numSignals) __result = "";
					else __result = SignalPattern.formatPattern(index);
					break;
				case 32768:
					index = 12 + offset;
					if (index >= numSignals) __result = "";
					else __result = SignalPattern.formatPattern(index);
					break;
				case 1:
					index = 13 + offset;
					if (index >= numSignals) __result = "";
					else __result = SignalPattern.formatPattern(index);
					break;
				case 2:
					index = 14 + offset;
					if (index >= numSignals) __result = "";
					else __result = SignalPattern.formatPattern(index);
					break;
				case 4:
					index = 15 + offset;
					if (index >= numSignals) __result = "";
					else __result = SignalPattern.formatPattern(index);
					break;
				case 8:
					index = 16 + offset;
					if (index >= numSignals) __result = "";
					else __result = SignalPattern.formatPattern(index);
					break;
				case 16:
					index = 17 + offset;
					if (index >= numSignals) __result = "";
					else __result = SignalPattern.formatPattern(index);
					break;
				case 32:
					index = 18 + offset;
					if (index >= numSignals) __result = "";
					else __result = SignalPattern.formatPattern(index);
					break;
			}
			return __result;
		}

		public static string getLocoHistory(int stationInt)
		{
			if (Signals.locoDirections == null) return "";
			int numSignals = Signals.locoDirections.Count;
			if (numSignals == 0) return "";

			int offset = numSignals - 19;
			if (offset < 0) offset = 0; ;
			string __result = "";
			int index = 0;
			switch (stationInt)
			{
				case 10:
					index = 0 + offset;
					if (index >= numSignals) __result = "";
					else __result = Signals.formatDirection(index);
					break;
				case 512:
					index = 1 + offset;
					if (index >= numSignals) __result = "";
					else __result = Signals.formatDirection(index);
					break;
				case 1024:
					index = 2 + offset;
					if (index >= numSignals) __result = "";
					else __result = Signals.formatDirection(index);
					break;
				case 300:
					index = 3 + offset;
					if (index >= numSignals) __result = "";
					else __result = Signals.formatDirection(index);
					break;
				case 100:
					index = 4 + offset;
					if (index >= numSignals) __result = "";
					else __result = Signals.formatDirection(index);
					break;
				case 2048:
					index = 5 + offset;
					if (index >= numSignals) __result = "";
					else __result = Signals.formatDirection(index);
					break;
				case 20:
					index = 6 + offset;
					if (index >= numSignals) __result = "";
					else __result = Signals.formatDirection(index);
					break;
				case 25:
					index = 7 + offset;
					if (index >= numSignals) __result = "";
					else __result = Signals.formatDirection(index);
					break;
				case 30:
					index = 8 + offset;
					if (index >= numSignals) __result = "";
					else __result = Signals.formatDirection(index);
					break;
				case 210:
					index = 9 + offset;
					if (index >= numSignals) __result = "";
					else __result = Signals.formatDirection(index);
					break;
				case 211:
					index = 10 + offset;
					if (index >= numSignals) __result = "";
					else __result = Signals.formatDirection(index);
					break;
				case 16384:
					index = 11 + offset;
					if (index >= numSignals) __result = "";
					else __result = Signals.formatDirection(index);
					break;
				case 32768:
					index = 12 + offset;
					if (index >= numSignals) __result = "";
					else __result = Signals.formatDirection(index);
					break;
				case 1:
					index = 13 + offset;
					if (index >= numSignals) __result = "";
					else __result = Signals.formatDirection(index);
					break;
				case 2:
					index = 14 + offset;
					if (index >= numSignals) __result = "";
					else __result = Signals.formatDirection(index);
					break;
				case 4:
					index = 15 + offset;
					if (index >= numSignals) __result = "";
					else __result = Signals.formatDirection(index);
					break;
				case 8:
					index = 16 + offset;
					if (index >= numSignals) __result = "";
					else __result = Signals.formatDirection(index);
					break;
				case 16:
					index = 17 + offset;
					if (index >= numSignals) __result = "";
					else __result = Signals.formatDirection(index);
					break;
				case 32:
					index = 18 + offset;
					if (index >= numSignals) __result = "";
					else __result = Signals.formatDirection(index);
					break;
			}
			return __result;
		}
		public static string getSignalChallengeResults(int stationInt)
		{
			if (Signals.challengeResults == null) return "";
			int numSignals = Signals.challengeResults.Count;
			if (numSignals == 0) return "";

			int offset = numSignals - 19;
			if (offset < 0) offset = 0; ;
			string __result = "";
			int index = 0;
			switch (stationInt)
			{
				case 10:
					index = 0 + offset;
					if (index >= numSignals) __result = "";
					else __result = Signals.challengeResults[index];
					break;
				case 512:
					index = 1 + offset;
					if (index >= numSignals) __result = "";
					else __result = Signals.challengeResults[index];
					break;
				case 1024:
					index = 2 + offset;
					if (index >= numSignals) __result = "";
					else __result = Signals.challengeResults[index];
					break;
				case 300:
					index = 3 + offset;
					if (index >= numSignals) __result = "";
					else __result = Signals.challengeResults[index];
					break;
				case 100:
					index = 4 + offset;
					if (index >= numSignals) __result = "";
					else __result = Signals.challengeResults[index];
					break;
				case 2048:
					index = 5 + offset;
					if (index >= numSignals) __result = "";
					else __result = Signals.challengeResults[index];
					break;
				case 20:
					index = 6 + offset;
					if (index >= numSignals) __result = "";
					else __result = Signals.challengeResults[index];
					break;
				case 25:
					index = 7 + offset;
					if (index >= numSignals) __result = "";
					else __result = Signals.challengeResults[index];
					break;
				case 30:
					index = 8 + offset;
					if (index >= numSignals) __result = "";
					else __result = Signals.challengeResults[index];
					break;
				case 210:
					index = 9 + offset;
					if (index >= numSignals) __result = "";
					else __result = Signals.challengeResults[index];
					break;
				case 211:
					index = 10 + offset;
					if (index >= numSignals) __result = "";
					else __result = Signals.challengeResults[index];
					break;
				case 16384:
					index = 11 + offset;
					if (index >= numSignals) __result = "";
					else __result = Signals.challengeResults[index];
					break;
				case 32768:
					index = 12 + offset;
					if (index >= numSignals) __result = "";
					else __result = Signals.challengeResults[index];
					break;
				case 1:
					index = 13 + offset;
					if (index >= numSignals) __result = "";
					else __result = Signals.challengeResults[index];
					break;
				case 2:
					index = 14 + offset;
					if (index >= numSignals) __result = "";
					else __result = Signals.challengeResults[index];
					break;
				case 4:
					index = 15 + offset;
					if (index >= numSignals) __result = "";
					else __result = Signals.challengeResults[index];
					break;
				case 8:
					index = 16 + offset;
					if (index >= numSignals) __result = "";
					else __result = Signals.challengeResults[index];
					break;
				case 16:
					index = 17 + offset;
					if (index >= numSignals) __result = "";
					else __result = Signals.challengeResults[index];
					break;
				case 32:
					index = 18 + offset;
					if (index >= numSignals) __result = "";
					else __result = Signals.challengeResults[index];
					break;
			}
			return __result;
		}
		public static string getChallengesMenuText(int stationInt)
		{
			string __result = "";
			if(!inSignalSub)
            {
				switch (stationInt)
				{
					case 10:
						__result = "Stats"; break;
					case 512:
						__result = "All Jobs Stats"; break;
					case 1024:
						__result = "Double Bonus Stats"; break;
					case 300:
						__result = "Signals"; break;
				}
			}
			else
            {
				switch (stationInt)
				{
					case 10:
						__result = "Signal Stats"; break;
					case 512:
						__result = "Signal History"; break;
					case 1024:
						__result = "Loco History"; break;
					case 300:
						__result = "Signal Challenge Results"; break;
				}
			}
			return __result;
		}

		[HarmonyPatch(typeof(LicenseManager), "IsGeneralLicenseAcquired")]  
		public static class GeneralLicensedAcquired
		{
			[HarmonyPatch(new Type[]
			{
			typeof(GeneralLicenseType)
			})]

			public static bool Prefix(GeneralLicenseType license, ref bool __result, DV.ServicePenalty.UI.CareerManagerLicensesScreen __instance)
			{
				//				Main.DebugLog(() => "setting acquired license " + license);
				if (inStats)
				{
					__result = true;
					return false;
				}
				return true;
			}
		}

		[HarmonyPatch(typeof(LicenseManager), "IsJobLicenseAcquired")]
		public static class JobLicensedAcquired
		{
			[HarmonyPatch(new Type[]
			{
			typeof(JobLicenses)
			})]

			public static bool Prefix(JobLicenses license, ref bool __result, DV.ServicePenalty.UI.CareerManagerLicensesScreen __instance)
			{
//				Main.DebugLog(() => "setting acquired license " + license);
				if (inStats)
				{
					__result = true;
					return false;
				}
				return true;
			}
		}
	}
}

