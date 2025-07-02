using System;
using System.Collections.Generic;
using DV;
using DV.ThingTypes;
using HarmonyLib;

namespace better_comms_radio.Patches;

[HarmonyPatch(typeof(CommsRadioCargoLoader))]
[HarmonyPatch(nameof(CommsRadioCargoLoader.SetCargoToLoad))]
public class CommsRadioCargoLoader_SetCargoToLoad_Patch
{
	private static bool Prefix(CommsRadioCargoLoader __instance, CargoType_v2 cargoType)
	{
		__instance.selectedCargoType = cargoType;
		
		//+1 because unloading is also an option
		var lines = new string[Math.Min(LinesHelper.MAX_LINE_COUNT, __instance.cargoChoices.Count+1)];
		var middleLineIndex = LinesHelper.MiddleLineIndex(lines.Length);
		lines[middleLineIndex] = LinesHelper.CreateLine(cargoType, true);
		
		var selectedcargoTypeIndex = __instance.cargoChoices.IndexOf(cargoType);

		//next types, displayed below the selected
		var cargoTypeIndex = selectedcargoTypeIndex;
		for (var index = middleLineIndex+1; index < lines.Length; index++)
		{
			lines[index] = LinesHelper.CreateLine(NextPreviousCargoType(true, __instance.cargoChoices, ref cargoTypeIndex), false);
		}
		
		//previous types, displayed above the selected
		cargoTypeIndex = selectedcargoTypeIndex;
		for (var index = middleLineIndex-1; index >= 0; index--)
		{
			lines[index] = LinesHelper.CreateLine(NextPreviousCargoType(false, __instance.cargoChoices, ref cargoTypeIndex), false);
		}

		LinesHelper.SetContent(__instance.display, lines);
		return false;
	}
	
	private static CargoType_v2 NextPreviousCargoType(bool next, List<CargoType_v2> cargoTypes, ref int cargoTypeIndex)
	{
		//intentionally reversed
		cargoTypeIndex = next ? cargoTypeIndex - 1 : cargoTypeIndex + 1;
		
		// -1 = unload
		if (cargoTypeIndex < -1)
		{
			cargoTypeIndex = cargoTypes.Count - 1;
		}
		else if (cargoTypeIndex >= cargoTypes.Count)
		{
			cargoTypeIndex = -1;
		}
		
		if (cargoTypeIndex == -1)
		{
			return null; 
		}
		
		return cargoTypes[cargoTypeIndex];
	}
}