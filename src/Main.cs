using System;
using System.Reflection;
using HarmonyLib;
using UnityModManagerNet;

namespace better_comms_radio;

[EnableReloading]
internal static class Main
{
	private static UnityModManager.ModEntry MyModEntry;
	private static Harmony harmony;

	//================================================================

	private static bool Load(UnityModManager.ModEntry modEntry)
	{
		try
		{
			MyModEntry = modEntry;
			
			MyModEntry.OnUnload = OnUnload;

			harmony = new Harmony(MyModEntry.Info.Id); 
			harmony.PatchAll(Assembly.GetExecutingAssembly());
		}
		catch (Exception ex)
		{
			MyModEntry.Logger.LogException($"Failed to load {MyModEntry.Info.DisplayName}:", ex);
			harmony?.UnpatchAll(MyModEntry.Info.Id);
			return false;
		}
		
		Log("loaded");

		return true;
	}

	private static bool OnUnload(UnityModManager.ModEntry modEntry)
	{
		harmony?.UnpatchAll(MyModEntry.Info.Id);
		return true;
	}

	// Logger Commands
	public static void Log(string message)
	{
		MyModEntry.Logger.Log(message);
	}

	public static void Warning(string message)
	{
		MyModEntry.Logger.Warning(message);
	}

	public static void Error(string message)
	{
		MyModEntry.Logger.Error(message);
	}
}