using System;
using System.Collections.Generic;
using System.Xml;
using System.Linq;
using FairyGUI;
using HarmonyLib;
using JetBrains.Annotations;
using XiaWorld.UI.InGame;
using XiaWorld;
using UnityEngine;

namespace YaogUI
{
    public class StoragePreset
    {
        public string Name;
        public string Priority = "Normal";
        public List<g_emItemLable> Kinds = new List<g_emItemLable>(); //This is not used anywhere atm, but we'll keep it just in case
        public List<g_emItemLable> Labels = new List<g_emItemLable>();
        public List<string> Qualities = new List<string> { "Poor", "Common", "Excellent", "Exquisite", "None" };
        public List<string> Elements = new List<string> { "None", "Metal", "Wood", "Water", "Fire", "Earth" };
        public float[] Tier = { 0f, 12f };
        public bool onlyFSItem;
        public bool CanSale;
        public bool onlyBigFish;

        public static bool GetTruthyValue([CanBeNull] string value)
        {
            if (value == null) return false;
            if (value.Length == 0) return false;
            return value == "true" || value == "1" || value == "yes";
        }

        public void ApplyToArea(AreaStorage area)
        {
            // Clear everything
            area.ExcludeItemLable.Clear();
            area.IncludeItemQ = new bool[5];
            // Original has 7 elements for some reason, so we keep it that way
            area.IncludeElement = new bool[7];
            area.Priority = 1;
            
            //Items
            foreach (g_emItemLable label in Enum.GetValues(typeof(g_emItemLable)))
            {
                if (Labels.Contains(label)) continue;
                area.ExcludeItemLable.Add((int) label);
            }
            // Priority
            switch (Priority.ToLower())
            {
                case "high":
                case "2":
                    area.Priority = 2;
                    break;
                case "low":
                case "0":
                    area.Priority = 0;
                    break;
                default:
                    area.Priority = 1;
                    break;
            }
            // Quality
            area.IncludeItemQ[0] = Qualities.Contains("Poor");
            area.IncludeItemQ[1] = Qualities.Contains("Common");
            area.IncludeItemQ[2] = Qualities.Contains("Excellent");
            area.IncludeItemQ[3] = Qualities.Contains("Exquisite");
            area.IncludeItemQ[4] = Qualities.Contains("None");
            // Tier
            area.IncludeItemRate = new Vector2(Tier[0], Tier[1]);
            // Elements
            area.IncludeElement[0] = Elements.Contains("None");
            area.IncludeElement[1] = Elements.Contains("Metal");
            area.IncludeElement[2] = Elements.Contains("Wood");
            area.IncludeElement[3] = Elements.Contains("Water");
            area.IncludeElement[4] = Elements.Contains("Fire");
            area.IncludeElement[5] = Elements.Contains("Earth");
            // Misc
            area.onlyBigFish = onlyBigFish;
            area.CanSale = CanSale;
            area.onlyFSItem = onlyFSItem;
        }
    }
    public static class StorageAreaHelper
    {
        public static readonly string defaultPresetXMLName = "default_presets.xml";
        public static readonly string userPresetXMLName = "custom_presets.xml";
        
        public static Wnd_StorageArea window;
        public static List<StoragePreset> presets = new List<StoragePreset>();

        public static List<StoragePreset> LoadPresets(string filename)
        {
            var xmlDoc = XmlLoader.ReadXmlFile(filename);
            if (xmlDoc == null)
            {
                throw new Exception(filename + " not found or failed to load");
            }

            presets.Clear();
            var root = xmlDoc.SelectSingleNode("Presets");
            if (root == null) throw new Exception(filename + " failed to load or malformed");

            var index = 0;
            foreach (XmlNode presetNode in root.SelectNodes("Preset"))
            {
                // Names should be unique but...
                ++index;
                var originalName = presetNode.Attributes["name"]?.Value;
                string name = originalName;
                if (originalName == null) name = $"Preset {index}";
                else
                {
                    if (presets.Select(p => p.Name).Contains(originalName))
                    {
                        name = $"{originalName} {index}";
                    }
                }
                
                var preset = new StoragePreset
                {
                    Name = name,
                    Priority = presetNode.Attributes["Priority"]?.Value,
                    CanSale = StoragePreset.GetTruthyValue(presetNode.Attributes["CanSale"]?.Value),
                    onlyFSItem = StoragePreset.GetTruthyValue(presetNode.Attributes["RelicsOnly"]?.Value),
                    onlyBigFish = StoragePreset.GetTruthyValue(presetNode.Attributes["LargeFishOnly"]?.Value)
                };
                
                // Parse Kinds (Items categories)
                XmlNode kindNode = presetNode.SelectSingleNode("Kind");
                if (kindNode != null)
                {
                    foreach (XmlNode labelNode in kindNode.SelectNodes("Label"))
                    {
                        preset.Labels.Add((g_emItemLable) Enum.Parse(typeof(g_emItemLable), labelNode.InnerText));
                    }
                }
                else
                {
                    preset.Labels = Enum.GetValues(typeof(g_emItemLable)).Cast<g_emItemLable>().ToList();
                }

                XmlNode qualityNode = presetNode.SelectSingleNode("Quality");
                if (qualityNode != null)
                {
                    preset.Qualities.Clear();
                    foreach (XmlNode labelNode in qualityNode.SelectNodes("Label"))
                    {
                        preset.Qualities.Add(labelNode.InnerText);
                    }
                }

                // Parse Tier settings
                XmlNode tierNode = presetNode.SelectSingleNode("Tier");
                if (tierNode != null)
                {
                    var minAttr = tierNode.Attributes["min"];
                    if (minAttr != null)
                    {
                        float.TryParse(minAttr.Value, out preset.Tier[0]);
                    }

                    var maxAttr = tierNode.Attributes["max"];
                    if (maxAttr != null)
                    {
                        float.TryParse(maxAttr.Value, out preset.Tier[1]);
                    }
                }

                // Parse Element settings
                XmlNode elementNode = presetNode.SelectSingleNode("Elements");
                if (elementNode != null)
                {
                    preset.Elements.Clear();
                    foreach (XmlNode labelNode in elementNode.SelectNodes("Label"))
                    {
                        preset.Elements.Add(labelNode.InnerText);
                    }
                }

                presets.Add(preset);
            }
            Main.Debug("Loaded " + presets.Count + " presets");

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

    [HarmonyPatch(typeof(Wnd_StorageArea), "OnShowUpdate")]
    public static class InitWndStorageAreaPatch
    {
        public static void Prefix(Wnd_StorageArea __instance)
        {
            StorageAreaHelper.window = __instance;
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
                    Main.Debug("Loading presets...");
                    LoadPresets();
                }
                
                var UI = StorageAreaHelper.UI;
                // Creating this combobox is a pain in the ass...
                var cBox = UI_ComboBox.CreateInstance();
                cBox.x = UI.m_n25.x + (UI.m_n25.width - cBox.width);
                cBox.y = UI.m_n25.y;
                // Values
                cBox.values = new string[StorageAreaHelper.presets.Count + 1];
                cBox.items = new string[StorageAreaHelper.presets.Count + 1];
                cBox.items[0] = TFMgr.Get("选择预设");
                cBox.values[0] = "-1";
                cBox.value = "-1";
         
                for (var i = 0; i < StorageAreaHelper.presets.Count; i++)
                {
                    cBox.values[i + 1] = i.ToString();
                    cBox.items[i + 1] = StorageAreaHelper.presets[i].Name;
                }
                
                cBox.onChanged.Add(e =>
                {
                    int.TryParse(cBox.value, out var index);
                    if (index == -1) return;
                    // Main.Debug($"Found preset {StorageAreaHelper.presets[index].Name} at index {index}");
                    ApplyPreset(StorageAreaHelper.presets[index]);
                    cBox.value = "-1";
                });
                
                var reloadPresets = UI_Button.CreateInstance();
                reloadPresets.text = "Reload";
                reloadPresets.tooltips = TFMgr.Get("从 XML 重新加载预设");
                reloadPresets.x = cBox.x + cBox.width + 10;
                reloadPresets.y = cBox.y;
                reloadPresets.onClick.Add(LoadPresets);
     
                UI.AddChild(cBox);
                UI.AddChild(reloadPresets);
            }
            catch (Exception e)
            {
                Main.Debug(e.ToString());
            }
        }
        public static void ApplyPreset(StoragePreset preset)
        {
            Main.Debug("Applying preset " + preset.Name);
            preset.ApplyToArea(StorageAreaHelper.area);
            StorageAreaHelper.window.ShowOrUpdate();
        }

        public static void LoadPresets()
        {
            StorageAreaHelper.presets = StorageAreaHelper.LoadPresets(StorageAreaHelper.defaultPresetXMLName);
        }
    }
 
}