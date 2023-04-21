using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using DV.ServicePenalty.UI;


namespace DvMod.Challenges
{
	public static class SignalMonitor
	{
		static Boolean signalOn = false;
		static Horn? hornPlaying;
		static WhistleRopeInit? whistlePlaying;
		static int playCount = 0;

		static long completeTimer = 0;
		static long logTimer = 0;

		static long playStartTime = 0;


		static float MIN_WHISTLE_TENSION = .8F;


		[HarmonyPatch(typeof(Horn), "Update")]
		public static class UpdateHorn
		{
			public static void Prefix(Horn __instance)
			{

				if (Main.mod != null)
				{

					try
					{
						checkForComplete();
						if (!__instance.hitPlayed && __instance.input >= __instance.hitThreshold)
						{
							signalOn = true;
							hornPlaying = __instance;
							DateTimeOffset now = (DateTimeOffset)DateTime.UtcNow;
							playStartTime = now.ToUnixTimeMilliseconds();

//							Main.DebugLog(() => "signalmonitor Horn started instance = " + __instance);
						}
//						else if (signalOn && __instance.hitPlayed && hornPlaying == __instance)
//						{
//							playCount++;
							//							Main.DebugLog(() => "horn playing " + __instance.input + " and threshold = " + __instance.hitThreshold + " instance = " + __instance);
//						}
						else if (signalOn && hornPlaying == __instance && !__instance.hitPlayed)
						{
//							Main.DebugLog(() => "signalmonitor Horn ended instance = " + __instance);

							DateTimeOffset now = (DateTimeOffset)DateTime.UtcNow;

							SignalPattern.addHornSignal(now.ToUnixTimeMilliseconds()-playStartTime, playStartTime, __instance.GetComponent<LocoControllerBase>());
							signalOn = false;
							hornPlaying = null;
							playCount = 0;
							playStartTime = 0;
						}
					}
					catch (Exception e)
					{
						Main.DebugLog(() => "exception in horn");
					}
				}

			}
		}


		[HarmonyPatch(typeof(WhistleRopeInit), "Update")]
		public static class UpdateWhistle
		{
			public static void Prefix(WhistleRopeInit __instance)
			{
				if(__instance.ropeTension.value >= MIN_WHISTLE_TENSION)
                {
					if(!signalOn)
                    {
						whistlePlaying = __instance;
						signalOn = true;
						DateTimeOffset now = (DateTimeOffset)DateTime.UtcNow;
						playStartTime = now.ToUnixTimeMilliseconds();
//						Main.DebugLog(() => "Whistle started instance = " + __instance);

						LocoControllerSteam controller = __instance.controller;
//						Main.DebugLog(() => "Loco Id =  " + controller.GetComponent<TrainCar>().GetInstanceID() + " type = " + controller.GetComponent<TrainCar>().GetType() + " speed = " + controller.GetComponent<TrainCar>().GetForwardSpeed());
					}
//					else if(whistlePlaying == __instance)
//                    {
//						playCount++;
//                    }
				}
				else if(signalOn && __instance == whistlePlaying)
                {
					DateTimeOffset now = (DateTimeOffset)DateTime.UtcNow;
					LocoControllerSteam controller = __instance.controller;
					SignalPattern.addWhistleSignal(now.ToUnixTimeMilliseconds()-playStartTime, playStartTime, controller.GetComponent<LocoControllerSteam>());
					signalOn = false;
					whistlePlaying=null;
					playCount=0;
				}
			}
		}

		public static void checkForComplete()
        {
			DateTimeOffset now = (DateTimeOffset)DateTime.UtcNow;

			if (now.ToUnixTimeMilliseconds() > completeTimer)
			{
				SignalPattern.checkForComplete();
				completeTimer = now.ToUnixTimeMilliseconds() + 100;
			}
		}
		public static void logSignalMonitor()
        {
			DateTimeOffset now = (DateTimeOffset)DateTime.UtcNow;

			if(now.ToUnixTimeMilliseconds() > logTimer)
            {
				Main.DebugLog(() => "We are in signal monitor");
				logTimer = now.ToUnixTimeMilliseconds() + 1000;
			}

		}
	}
}


