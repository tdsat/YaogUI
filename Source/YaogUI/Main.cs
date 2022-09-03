using System;
using System.IO;
using System.Reflection;
using HarmonyLib;

namespace YaogUI
{
    public static class Main
	{
		private static bool Patched = false;
		public static void Patch()
		{
			try
			{
				Assembly.LoadFrom(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "0Harmony.dll"));
				Harmony harmony = new Harmony("YaogUI!");
				if (!Patched)
                {
					harmony.PatchAll();
					Patched = true;
				}
				YaogUIBinder.BindAll();
				Debug("Loaded!");
			}
			catch (Exception e)
			{
				Debug(e.ToString());
			}
		}
		public static void Debug(string message)
		{
			KLog.Dbg(string.Format("[YaogUI]{0}", message), new object[0]);
		}
	}
}
