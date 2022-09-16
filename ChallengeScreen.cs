using DV.ServicePenalty.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
namespace DvMod.Challenges
{
    public class ChallengeScreen  : MonoBehaviour , IDisplayScreen
	{
		ChallengeScreen()
        {
		}

		private void Awake()
		{

			Main.DebugLog(() => "challenge awake screen active = " + this.isActiveAndEnabled);



			//			if (this.title == null)
			{
				//				this.title = gameObject.AddComponent<TextMeshPro>();
			}

			
		}
		public void Activate(IDisplayScreen __instance)
		{

			Main.DebugLog(() => "challenge activate screen active = " + this.isActiveAndEnabled);

			try
			{
			}
			catch (Exception ex)
            {
				Main.DebugLog(() => "switching to that fee screen caused " + ex.Message);
			}
		}
		public void Disable()
		{ }
		public void HandleInputAction(InputAction input)
		{ }
		private void HighlightSelected(int newHighlight, int prevHighlighted = -1)
		{ }
		public string GetCurrentSelection()
		{ return ""; }
		public void SubscribeToSelectionChange(IntIterator.IntIteratorCurrentUpdatedDelegate callback)
        {

        }
		public void UnsubscribeToSelectionChange(IntIterator.IntIteratorCurrentUpdatedDelegate callback)
        {

        }
	}
}
