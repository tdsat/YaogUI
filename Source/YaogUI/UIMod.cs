using System;
using System.IO;
using System.Xml;
using System.Reflection;
using FairyGUI;
using HarmonyLib;

namespace YaogUI
{
     public abstract class UIMod
	{
		public static GObject GetOrAddChild(GComponent parent, GObject instance, string name)
		{
			if (parent.GetChild(name) != null)
			{
				return parent.GetChild(name);
			}

			instance.name = name;
			parent.AddChild(instance);
			return instance;
		}
	}
}
