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


		[HarmonyPatch(typeof(CareerManagerMainScreen), "Activate")]
		public static class CareerPatchActivate
		{
			public static ChallengeScreen? challengeScreen;
			public static CareerManagerStatsScreen? originalStats;

			public static void Postfix(DV.ServicePenalty.UI.CareerManagerMainScreen __instance)
			{
				if (Main.mod != null)
				{
//					Main.DebugLog(() => "We are in the career manager - Activate");
					try
					{
/*
						if (challengeScreen == null)
						{
							GameObject gameObject = new GameObject("CareerManagerMainScreen");
							challengeScreen = gameObject.AddComponent<ChallengeScreen>();
						}
*/

						if (__instance.screenSwitcher == null)
						{
							Main.DebugLog(() => "screenSwitcher reference isn't set! Screen can't function!");
							return;
						}
						else
						{
							originalStats = __instance.statsScreen;

							if (inStats)
							{
								__instance.stats.text = "Back";
								__instance.licenses.text = "All Job Stats";
								__instance.fees.text = "Game Stats";
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

		[HarmonyPatch(typeof(CareerManagerMainScreen), "HandleInputAction")]
		public static class CareerManagerHandleInput
		{
			public static void Postfix(InputAction input, DV.ServicePenalty.UI.CareerManagerMainScreen __instance)
			{

				if (Main.mod != null)
				{
//					Main.DebugLog(() => "We are in the career manager handle input - post ");

					try
					{
						if (!inStats)
						{
							switch (input)
							{
								case InputAction.Cancel:
									inStats = false;
									__instance.screenSwitcher.SetActiveDisplay(__instance.statsScreen.mainScreen);
									break;
								case InputAction.Confirm:
									if (__instance.selector.Current == 2)
									{
										inStats = true;
										__instance.screenSwitcher.SetActiveDisplay(__instance.statsScreen.mainScreen);
									}
									break;
								default:
									return;
							}
						}
						else
						{
							switch (input)
							{
								case InputAction.Cancel:
									inStats = false;
									__instance.screenSwitcher.SetActiveDisplay(__instance.statsScreen.mainScreen);
									break;
								case InputAction.Confirm:
									if (__instance.selector.Current == 0)
									{
										inStats = true;
										__instance.screenSwitcher.SetActiveDisplay(__instance.statsScreen);
									}
									else if (__instance.selector.Current == 2)
									{
										// double bonus not done tbd
										inStats = false;
										__instance.screenSwitcher.SetActiveDisplay(__instance.statsScreen.mainScreen);
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
								__instance.title1.text = "All Jobs";
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
					AllJob? targetJob = null;
					switch (stationInt)
                    {
						case 10:
							targetJob = Status.getJobStatus("HB");	break;
						case 512:
							targetJob = Status.getJobStatus("SM");  break;
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
					return false;
				}
				return true;
			}
		}
		/*
[Challenges] looking at license TrainDriver 10
[Challenges] looking at license FreightHaul 512
[Challenges] looking at license Shunting 1024
[Challenges] looking at license MultipleUnit 300
[Challenges] looking at license ManualService 100
[Challenges] looking at license LogisticalHaul 2048
[Challenges] looking at license DE2 20
[Challenges] looking at license DE6 25
[Challenges] looking at license SH282 30
[Challenges] looking at license ConcurrentJobs1 210
[Challenges] looking at license ConcurrentJobs2 211
[Challenges] looking at license TrainLength1 16384
[Challenges] looking at license TrainLength2 32768
[Challenges] looking at license Hazmat1 1
[Challenges] looking at license Hazmat2 2
[Challenges] looking at license Hazmat3 4
[Challenges] looking at license Military1 8
[Challenges] looking at license Military2 16
[Challenges] looking at license Military3 32
[Challenges] looking at license DE2 20		 
*/

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
					return false;
				}
				return true;
			}
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

