using DV;
using DV.Customization.Paint;
using DV.Localization;
using DV.ThingTypes;
using TMPro;
using UnityEngine;

namespace better_comms_radio;

public static class LinesHelper
{
	public const int MAX_LINE_COUNT = 6;
	
	public static string CreateLine(PaintTheme theme, bool selected)
	{
		return CreateLine(theme.LocalizedName, selected);
	}
	
	public static string CreateLine(TrainCarType_v2 carType, bool selected)
	{
		return CreateLine(LocalizationAPI.L(carType.localizationKey), selected);
	}
	
	public static string CreateLine(TrainCarLivery carLivery, bool selected)
	{
		return CreateLine(LocalizationAPI.L(carLivery.localizationKey), selected);
	}
	
	public static string CreateLine(CargoType_v2 cargoType, bool selected)
	{
		return CreateLine(cargoType
			? LocalizationAPI.L(cargoType.localizationKeyShort)
			: CommsRadioLocalization.CARGO_UNLOAD,
		selected);
	}

	public static string CreateLine(string middleText, bool selected)
	{
		var prefix = selected ? "> " : "";
		return $"{prefix}{middleText}\n";
	}

	public static void SetContent(CommsRadioDisplay display, string[] lines, FontStyles contentStyle = FontStyles.UpperCase)
	{
		display.SetContent(string.Join(string.Empty, lines), contentStyle);
	}

	public static int MiddleLineIndex(int lineCount)
	{
		return Mathf.FloorToInt((lineCount - 1) / 2.0f);
	}
}