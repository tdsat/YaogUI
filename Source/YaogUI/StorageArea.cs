using System;
using System.Collections.Generic;
using System.Xml;
using FairyGUI;
using HarmonyLib;
using JetBrains.Annotations;
using XiaWorld.UI.InGame;
using XiaWorld;

namespace YaogUI
{
    public class StoragePreset
    {
        public string Name;
        public string Priority;
        public List<string> Kinds = new List<string>();
        public List<string> Labels = new List<string>(); //This is not used anywhere atm, but we'll keep it just in case
        public List<string> Qualities = new List<string>();
        public List<string> Elements = new List<string>();
        public bool onlyFSItem = false;
        public bool CanSale = false;
        public bool onlyBigFish = false;

        public static bool GetTruthyValue([CanBeNull] string value)
        {
            if (value == null) return false;
            if (value.Length == 0) return false;
            return value == "true" || value == "1" || value == "yes";
        }
    }

    public static class StorageAreaHelper
    {
        public static readonly string defaultPresetXMLName = "default_presets.xml";
        public static readonly string userPresetXMLName = "custom_presets.xml";
        public static List<StoragePreset> presets = new List<StoragePreset>();

        public static List<StoragePreset> LoadPresets(string filename)
        {
            var xmlDoc = XmlLoader.ReadXmlFile("../" + filename);
            if (xmlDoc == null)
            {
                throw new Exception(filename + " not found or failed to load");
            }

            presets.Clear();
            var root = xmlDoc.SelectSingleNode("Presets");
            if (root == null) throw new Exception(filename + " failed to load or malformed");

            foreach (XmlNode presetNode in root.SelectNodes("Preset"))
            {
                var preset = new StoragePreset
                {
                    Name = presetNode.Attributes["name"]?.Value,
                    Priority = presetNode.Attributes["priority"]?.Value,
                    CanSale = StoragePreset.GetTruthyValue(presetNode.Attributes["CanSale"]?.Value),
                    onlyFSItem = StoragePreset.GetTruthyValue(presetNode.Attributes["RelicsOnly"]?.Value),
                    onlyBigFish = StoragePreset.GetTruthyValue(presetNode.Attributes["LargeFishOnly"]?.Value)
                };

                foreach (XmlNode kindNode in presetNode.SelectNodes("Kind"))
                {
                    preset.Kinds.Add(kindNode.Attributes["name"]?.Value); //Not used anywhere atm
                    foreach (XmlNode labelNode in kindNode.SelectNodes("Label"))
                    {
                        preset.Labels.Add(labelNode.InnerText);
                    }
                }

                foreach (XmlNode qualityNode in presetNode.SelectNodes("Quality"))
                {
                    foreach (XmlNode labelNode in qualityNode.SelectNodes("Label"))
                    {
                        preset.Qualities.Add(labelNode.InnerText);
                    }
                }

                foreach (XmlNode elementNode in presetNode.SelectNodes("Element"))
                {
                    foreach (XmlNode labelNode in elementNode.SelectNodes("Label"))
                    {
                        preset.Elements.Add(labelNode.InnerText);
                    }
                }

                presets.Add(preset);
            }

            return presets;
        }

        public static AreaStorage area =>
            Traverse.Create(Wnd_StorageArea.Instance).Field("area").GetValue<AreaStorage>();

        public static GButton[] buttons =>
            Traverse.Create(Wnd_StorageArea.Instance).Field("bnts").GetValue<GButton[]>();

        public static UI_WindowStorage UI => Wnd_StorageArea.Instance.UIInfo;

        public static UI_Checkbox[] elementCheckboxList =
        {
            UI.m_n58,
            UI.m_n43,
            UI.m_n46,
            UI.m_n49,
            UI.m_n52,
            UI.m_n55
        };

        public static UI_Checkbox[] qualityCheckboxList =
        {
            UI.m_nq0,
            UI.m_nq1,
            UI.m_nq2,
            UI.m_nq3,
            UI.m_nq4,
        };

        public static void ToggleAll()
        {
            var toggle = (UI.m_n25.GetChildAt(0) as UI_Item_Storage).m_title.selected;
            SetAllItems(!toggle);
        }

        public static void SetAllItems(bool selected)
        {
            for (g_emItemKind groupIndex = g_emItemKind.None; groupIndex < g_emItemKind.Count; ++groupIndex)
            {
                var group =
                    UI.m_n25.GetChildAt((int)groupIndex) as UI_Item_Storage;
                group.m_title.selected = selected;
                group.m_title.onClick.Call();
            }
        }

        public static void ClearAllElements()
        {
            foreach (var button in buttons)
            {
                button.selected = false;
                button.onChanged.Call();
            }
        }

        public static void ClearAllQuality()
        {
            StorageAreaHelper.area.IncludeItemQ = new bool[5];
            foreach (var uiCheckbox in StorageAreaHelper.qualityCheckboxList)
            {
                uiCheckbox.selected = false;
            }
        }
    }

    [HarmonyPatch(typeof(Wnd_StorageArea), "OnInit")]
    public static class AddCtrlModifiersToClickHandler // This class name makes no sense...
    {
        public static void Postfix(Wnd_StorageArea __instance)
        {
            try
            {
                var UI = __instance.UIInfo;
                var buttons = Traverse.Create(Wnd_StorageArea.Instance).Field("bnts").GetValue<GButton[]>();

                // Clear all button
                var toggleItems = UIPackage.CreateObjectFromURL("ui://ncbwb41mv9j6ah");
                toggleItems.name = "YaogUI.ToggleItems";
                toggleItems.text = TFMgr.Get("全部切换");
                //Trying to centre this thing is the hardest part...
                toggleItems.x = UI.m_n25.x + (UI.m_n25.width - toggleItems.width) / 2;
                toggleItems.y = UI.m_n25.y;
                toggleItems.visible = true;
                toggleItems.onClick.Add(StorageAreaHelper.ToggleAll);
                UI.AddChild(toggleItems);

                // Ctrl+Click disables all elements except for the selected one
                for (var elementIdx = 0; elementIdx < buttons.Length; elementIdx++)
                {
                    var gButton = buttons[elementIdx];
                    var index = elementIdx;
                    gButton.onClick.Add(e =>
                    {
                        if (!e.inputEvent.ctrl) return;
                        StorageAreaHelper.ClearAllElements();
                        gButton.selected = true;
                        gButton.onChanged.Call();
                        Main.Debug(index.ToString());
                    });
                }

                // Ctrl+Click disables all quality except for the selected one
                for (int i = 0; i < StorageAreaHelper.qualityCheckboxList.Length; i++)
                {
                    var checkbox = StorageAreaHelper.qualityCheckboxList[i];
                    checkbox.onClick.Add(e =>
                    {
                        if (!e.inputEvent.ctrl) return;
                        StorageAreaHelper.ClearAllQuality();
                        checkbox.selected = true;
                    });
                }
            }
            catch (Exception e)
            {
                Main.Debug(e.ToString());
            }
        }
    }

    [HarmonyPatch(typeof(Wnd_StorageArea), "OnInit")]
    public static class FilterPresets
    {
        public static void Postfix(Wnd_StorageArea __instance)
        {
            try
            {
                if (StorageAreaHelper.presets.Count == 0)
                {
                    StorageAreaHelper.presets = StorageAreaHelper.LoadPresets(StorageAreaHelper.defaultPresetXMLName);
                }
            }
            catch (Exception e)
            {
                Main.Debug(e.ToString());
            }
        }
    }
}