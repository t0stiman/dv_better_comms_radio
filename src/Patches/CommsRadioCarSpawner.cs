using System;
using DV;
using DV.Localization;
using DV.ThingTypes;
using HarmonyLib;
using TMPro;
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
		lines[0] = LinesHelper.CreateLine(carType, true);

		var carTypeIndex = __instance.selectedCarTypeIndex;
		for (var index = 1; index < LinesHelper.MAX_LINE_COUNT; index++)
		{
			var aCar = CommsRadioCarSpawner.IncOrDec(true, __instance.carTypesToSpawn, ref carTypeIndex,
				CommsRadioCarSpawner.IsCarTypeUnlocked);
			lines[index] = LinesHelper.CreateLine(aCar, false);
		}

		LinesHelper.SetContent(__instance.display, lines);
		return false;
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
			Debug.LogError($"Couldn't load car prefab: {carLivery}! Won't be able to spawn __instance car.", __instance);
		}
		else
		{
			var trainCar = __instance.carPrefabToSpawn.GetComponent<TrainCar>();
			__instance.carBounds = trainCar.Bounds;

			var loco = __instance.state == CommsRadioCarSpawner.State.PickLoco;
			var liveries = loco ? __instance.locoLiveries : __instance.carLiveriesToSpawn[__instance.carTypeToSpawn];
			var lines = new string[Math.Min(LinesHelper.MAX_LINE_COUNT, liveries.Count)];
			lines[0] = LinesHelper.CreateLine(carLivery, true);
			
			var liveryIndex = loco ? __instance.selectedLocoIndex : __instance.selectedCarLiveryIndex;
			for (var index = 1; index < lines.Length; index++)
			{
				var livery = CommsRadioCarSpawner.IncOrDec(true, liveries, ref liveryIndex,
					CommsRadioCarSpawner.IsCarLiveryUnlocked);
				lines[index] = LinesHelper.CreateLine(livery, false);
			}
			
			LinesHelper.SetContent(__instance.display, lines);
		}
		
		return false;
	}
}