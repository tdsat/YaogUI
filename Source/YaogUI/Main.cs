using System;
using System.IO;
using System.Xml;
using System.Reflection;
using HarmonyLib;

namespace YaogUI
{
    public static class Main
	{
		public static bool Patched;
		public static void Patch()
		{
			if (Patched) return;
			try
			{
				Assembly.LoadFrom(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "0Harmony.dll"));
				Harmony harmony = new Harmony("YaogUI!");
				harmony.PatchAll();
				Patched = true;
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
			KLog.Dbg(string.Format("[YaogUI]{0}", message));
		}
	}

    public static class YaogUI
    {
	    public static void OnInit()
	    {
		    Main.Debug("MLL Detected - skip patching");
		    // Since the Patch method will not run, we need to bind the new UI elements here
			YaogUIBinder.BindAll();
		    
		    Main.Patched = true;
	    }
    }

	public static class XmlLoader
	{
		public static XmlDocument ReadXmlFile(string relativePath)
		{
			try
			{
				// Get the directory where the mod DLL is located
				string assemblyLocation = Assembly.GetExecutingAssembly().Location;
				string assemblyDir = Path.GetDirectoryName(assemblyLocation);

				// Combine with the relative path to get the absolute path
				string fullPath = Path.Combine(assemblyDir, relativePath);
				Main.Debug($"Loading XML from {fullPath}");
				if (!File.Exists(fullPath)) return null;
				XmlDocument xmlDoc = new XmlDocument();
				xmlDoc.Load(fullPath);
				return xmlDoc;
			}
			catch (Exception e)
			{
				Main.Debug("Error reading XML: " + e.Message);
				return null;
			}
		}
	}
}
