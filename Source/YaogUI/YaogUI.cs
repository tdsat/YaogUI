using System;
using System.IO;
using System.Reflection;
using HarmonyLib;

namespace YaogUI
{
	public static class Main
	{
		public static void Patch()
		{
			try
			{
				Assembly.LoadFrom(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "0Harmony.dll"));
				Harmony harmony = new Harmony("YaogUI!");
				harmony.PatchAll();
				KLog.Dbg("[YaogUI] Loaded!", new object[0]);
			}
			catch (Exception e)
			{
				KLog.Dbg("[YaogUI] error" + e.ToString(), new object[0]);
			}
		}
	}
}
