using System;
using DV;
using DV.Customization.Paint;
using HarmonyLib;

namespace better_comms_radio.Patches;

[HarmonyPatch(typeof(CommsRadioPaintjob))]
[HarmonyPatch(nameof(CommsRadioPaintjob.UpdateDisplay))]
public class CommsRadioPaintjob_SetCarTypeToSpawn_Patch
{
	private static bool Prefix(CommsRadioPaintjob __instance)
	{
		if (__instance.opMode == CommsRadioPaintjob.OperationMode.Activation)
		{
			__instance.display.SetDisplay(CommsRadioLocalization.MODE_PAINTJOB, CommsRadioLocalization.MODE_PAINTJOB_ENABLE);
		}
		else if (__instance.opMode == CommsRadioPaintjob.OperationMode.ThemeSelect)
		{
			ThemeSelect(__instance);
		}
		else
		{
			var theme = __instance.themes[__instance.selectedTheme];
			var pointingAtCar = __instance.pointedCar != null;
			var interiorCompatible = pointingAtCar && __instance.pointedCar.PaintInterior != null && __instance.pointedCar.PaintInterior.IsSupported(theme);
			var exteriorCompatible = pointingAtCar && __instance.pointedCar.PaintExterior != null && __instance.pointedCar.PaintExterior.IsSupported(theme);
			var isCompatible = false;
			var content = string.Empty;
			
			switch (__instance.selectedAreas)
			{
				case 0:
					content = CommsRadioLocalization.MODE_PAINTJOB_ALL;
					isCompatible = interiorCompatible | exteriorCompatible;
					break;
				case 1:
					content = CommsRadioLocalization.MODE_PAINTJOB_INTERIOR;
					isCompatible = interiorCompatible;
					break;
				case 2:
					content = CommsRadioLocalization.MODE_PAINTJOB_EXTERIOR;
					isCompatible = exteriorCompatible;
					break;
			}
			__instance.display.SetDisplay(CommsRadioLocalization.MODE_PAINTJOB, content, 
				pointingAtCar ? 
						isCompatible ? CommsRadioLocalization.CONFIRM : CommsRadioLocalization.MODE_PAINTJOB_NOT_COMPATIBLE
						: CommsRadioLocalization.CANCEL);
		}
		
		return false;
	}

	private static void ThemeSelect(CommsRadioPaintjob __instance)
	{
		__instance.display.SetDisplay(CommsRadioLocalization.MODE_PAINTJOB, "", CommsRadioLocalization.SELECT);

		var selectedThemeIndex = __instance.selectedTheme;
		var selectedTheme = __instance.themes[selectedThemeIndex];
		
		var lines = new string[Math.Min(LinesHelper.MAX_LINE_COUNT, __instance.themes.Length)];
		var middleLineIndex = LinesHelper.MiddleLineIndex(lines.Length);
		lines[middleLineIndex] = LinesHelper.CreateLine(selectedTheme, true);

		//next themes, displayed below the selected
		var themeIndex = selectedThemeIndex;
		for (var index = middleLineIndex+1; index < lines.Length; index++)
		{
			lines[index] = LinesHelper.CreateLine(NextPreviousTheme(true, __instance.themes, ref themeIndex), false);
		}
		
		//previous themes, displayed above the selected
		themeIndex = selectedThemeIndex;
		for (var index = middleLineIndex-1; index >= 0; index--)
		{
			lines[index] = LinesHelper.CreateLine(NextPreviousTheme(false, __instance.themes, ref themeIndex), false);
		}

		LinesHelper.SetContent(__instance.display, lines);
	}

	private static PaintTheme NextPreviousTheme(bool next, PaintTheme[] themes, ref int themeIndex)
	{
		themeIndex = next ? themeIndex + 1 : themeIndex - 1;
		
		if (themeIndex < 0)
		{
			themeIndex = themes.Length - 1;
		}
		else if (themeIndex >= themes.Length)
		{
			themeIndex = 0;
		}
		
		return themes[themeIndex];
	}
}