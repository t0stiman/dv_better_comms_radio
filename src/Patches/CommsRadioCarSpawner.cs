using System;
using System.Collections.Generic;
using DV;
using DV.ThingTypes;
using HarmonyLib;
using UnityEngine;

namespace better_comms_radio.Patches;

// switch spawn mode: car <-> locomotive
[HarmonyPatch(typeof(CommsRadioCarSpawner))]
[HarmonyPatch(nameof(CommsRadioCarSpawner.SwitchCategory))]
public class CommsRadioCarSpawner_SwitchCategory_Patch
{
	private static bool Prefix(CommsRadioCarSpawner __instance, CommsRadioCarSpawner.Category? category)
	{
		if (category.HasValue)
		{
			__instance.category = category.Value;
		}
		else {
			__instance.category = __instance.category == CommsRadioCarSpawner.Category.Car
				? CommsRadioCarSpawner.Category.Loco
				: CommsRadioCarSpawner.Category.Car;
		}
		
		var lines = new[]
		{
			CommsRadioLocalization.SPAWNER_CAT_LOCO, //meow
			CommsRadioLocalization.SPAWNER_CAT_CARS
		};

		for (var index = 0; index < lines.Length; index++)
		{
			lines[index] = LinesHelper.CreateLine(lines[index], index == (int)__instance.category);
		}
		
		LinesHelper.SetContent(__instance.display, lines);
		return false;
	}
}

[HarmonyPatch(typeof(CommsRadioCarSpawner))]
[HarmonyPatch(nameof(CommsRadioCarSpawner.SetCarTypeToSpawn))]
public class CommsRadioCarSpawner_SetCarTypeToSpawn_Patch
{
	private static bool Prefix(CommsRadioCarSpawner __instance, TrainCarType_v2 carType)
	{
		__instance.carTypeToSpawn = carType;
		
		var lines = new string[Math.Min(LinesHelper.MAX_LINE_COUNT, __instance.carTypesToSpawn.Count)];
		var middleLineIndex = LinesHelper.MiddleLineIndex(lines.Length);
		lines[middleLineIndex] = LinesHelper.CreateLine(carType, true);

		//next cartypes, displayed below the selected
		var carTypeIndex = __instance.selectedCarTypeIndex;
		for (var index = middleLineIndex+1; index < lines.Length; index++)
		{
			lines[index] = LinesHelper.CreateLine(NextPreviousCartype(true, __instance.carTypesToSpawn, ref carTypeIndex), false);
		}
		
		//previous cartypes, displayed above the selected
		carTypeIndex = __instance.selectedCarTypeIndex;
		for (var index = middleLineIndex-1; index >= 0; index--)
		{
			lines[index] = LinesHelper.CreateLine(NextPreviousCartype(false, __instance.carTypesToSpawn, ref carTypeIndex), false);
		}

		LinesHelper.SetContent(__instance.display, lines);
		return false;
	}
	
	private static TrainCarType_v2 NextPreviousCartype(bool next, List<TrainCarType_v2> carTypes, ref int carTypeIndex)
	{
		return CommsRadioCarSpawner.IncOrDec(false, carTypes, ref carTypeIndex,
			CommsRadioCarSpawner.IsCarTypeUnlocked);
	}
}

[HarmonyPatch(typeof(CommsRadioCarSpawner))]
[HarmonyPatch(nameof(CommsRadioCarSpawner.SetCarLiveryToSpawn))]
public class CommsRadioCarSpawner_SetCarLiveryToSpawn_Patch
{
	private static bool Prefix(CommsRadioCarSpawner __instance, TrainCarLivery carLivery)
	{
		__instance.carPrefabToSpawn = carLivery.prefab;
		
		if (!__instance.carPrefabToSpawn)
		{
			Debug.LogError($"Couldn't load car prefab: {carLivery}! Won't be able to spawn this car.", __instance);
		}
		else
		{
			var trainCar = __instance.carPrefabToSpawn.GetComponent<TrainCar>();
			__instance.carBounds = trainCar.Bounds;

			var pickingLocomotive = __instance.state == CommsRadioCarSpawner.State.PickLoco;
			var liveries = pickingLocomotive ? __instance.locoLiveries : __instance.carLiveriesToSpawn[__instance.carTypeToSpawn];
			var selectedLiveryIndex = pickingLocomotive ? __instance.selectedLocoIndex : __instance.selectedCarLiveryIndex;
			
			var lines = new string[Math.Min(LinesHelper.MAX_LINE_COUNT, liveries.Count)];
			var middleLineIndex = LinesHelper.MiddleLineIndex(lines.Length);
			lines[middleLineIndex] = LinesHelper.CreateLine(carLivery, true);
			
			//next liveries, displayed below the selected
			var liveryIndex = selectedLiveryIndex;
			for (var index = middleLineIndex+1; index < lines.Length; index++)
			{
				lines[index] = LinesHelper.CreateLine(NextPreviousLivery(true, liveries, ref liveryIndex), false);
			}
		
			//previous liveries, displayed above the selected
			liveryIndex = selectedLiveryIndex;
			for (var index = middleLineIndex-1; index >= 0; index--)
			{
				lines[index] = LinesHelper.CreateLine(NextPreviousLivery(false, liveries, ref liveryIndex), false);
			}
			
			LinesHelper.SetContent(__instance.display, lines);
		}
		
		return false;
	}

	private static TrainCarLivery NextPreviousLivery(bool next, List<TrainCarLivery> liveries, ref int liveryIndex)
	{
		return CommsRadioCarSpawner.IncOrDec(next, liveries, ref liveryIndex,
			CommsRadioCarSpawner.IsCarLiveryUnlocked);
	}
}