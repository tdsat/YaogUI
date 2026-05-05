using System;
using System.Collections.Generic;
using FairyGUI;
using HarmonyLib;
using UnityEngine;
using XiaWorld;
using XiaWorld.UI.InGame;

namespace YaogUI
{
	[HarmonyPatch]
	public static class ManualPavilionEdits
	{
		public static UI_NpcInfoLable yinAttainmentField = GetLabelField("YaogUI.YinAttainmentField");
		public static UI_NpcInfoLable yangAttainmentField = GetLabelField("YaogUI.YangAttainmentField");
		public static UI_NpcInfoLable totalAttainmentField = GetLabelField("YaogUI.TotalAttainmentField");
		public static UI_NpcInfoLable attainmentChangeField = GetLabelField("YaogUI.AttainmentChangeField");

		public static UI_NpcInfoLable NPCYinAttainmentField = GetLabelField("YaogUI.NPCYinAttainmentField");
		public static UI_NpcInfoLable NPCYangAttainmentField = GetLabelField("YaogUI.NPCYangAttainmentField");
		public static UI_NpcInfoLable NPCTotalAttainmentField = GetLabelField("YaogUI.NPCTotalAttainmentField");
		public static UI_NpcInfoLable NPCAttainmentChangeField = GetLabelField("YaogUI.NPCAttainmentChangeField");

		private const int fieldWidth = 40;
		private const int fieldHeight = 20;

		private static UI_NpcInfoLable GetLabelField(string name)
		{
			var field = UI_NpcInfoLable.CreateInstance();
			field.name = name;
			field.height = fieldHeight;
			field.width = fieldWidth;

			return field;
		}

		public static void CreateFields(Wnd_CangJingGeWindow __instance)
		{
			var mainPane = (UI_CangJingGeWindow)__instance.contentPane;
			var bgImage = new GImage();
			bgImage.texture = mainPane.m_n5.texture;
			// Cool magic numbers
			bgImage.width = 246;
			bgImage.height = 100;
			bgImage.x = mainPane.m_n5.x - 20;
			bgImage.y = mainPane.m_n5.y + mainPane.m_n5.height;

			yinAttainmentField.color = Color.black;
			yangAttainmentField.color = Color.white;
			totalAttainmentField.color = new Color32(255, 191, 0, 255);
			totalAttainmentField.fontsize += 3;
			totalAttainmentField.height = 22;

			yinAttainmentField.tooltips = TFMgr.Get("阴性造诣");
			yangAttainmentField.tooltips = TFMgr.Get("阳性造诣");
			totalAttainmentField.tooltips = $"{TFMgr.Get("所选秘籍的总参悟")}\n\n{TFMgr.Get("降低参悟值的功法不计入计算")}";
			attainmentChangeField.tooltips = TFMgr.Get("习得秘籍后的阴阳平衡：黑色代表阴性增强，白色代表阳性增强。");

			PositionFields(new GComponent[]
					{ totalAttainmentField, yinAttainmentField, yangAttainmentField, attainmentChangeField }
				, bgImage.x + 35, bgImage.y + 20, 5);
			

			mainPane.AddChild(bgImage);

			mainPane.AddChild(yinAttainmentField);
			mainPane.AddChild(yangAttainmentField);
			mainPane.AddChild(totalAttainmentField);
			mainPane.AddChild(attainmentChangeField);

			var npc = Traverse.Create(Wnd_CangJingGeWindow.Instance).Field<Npc>("_npc").Value;
			if (npc != null)
			{
				PositionFields(new GComponent[]
						{ NPCTotalAttainmentField, NPCYinAttainmentField, NPCYangAttainmentField, NPCAttainmentChangeField }
					, bgImage.x + 35, bgImage.y + 60, 5);
				
				NPCYinAttainmentField.color = Color.black;
				NPCYangAttainmentField.color = Color.white;
				NPCTotalAttainmentField.color = new Color32(255, 191, 0, 255);
				NPCTotalAttainmentField.fontsize += 3;
				NPCTotalAttainmentField.height = 22;
				
				NPCYinAttainmentField.tooltips = TFMgr.Get("阴性造诣");
				NPCYangAttainmentField.tooltips = TFMgr.Get("阳性造诣");
				NPCYinAttainmentField.tooltips = $"{TFMgr.Get("所选秘籍的总参悟")}\n\n{TFMgr.Get("降低参悟值的功法不计入计算")}";
				NPCAttainmentChangeField.tooltips = TFMgr.Get("习得秘籍后的阴阳平衡：黑色代表阴性增强，白色代表阳性增强。");
				
				mainPane.AddChild(NPCYinAttainmentField);
				mainPane.AddChild(NPCYangAttainmentField);
				mainPane.AddChild(NPCTotalAttainmentField);
				mainPane.AddChild(NPCAttainmentChangeField);
			}
			
			mainPane.m_clearall.onClick.Add(ResetFields);
			
			ResetFields();
		}

		private static void PositionFields(GComponent[] components, float x, float y, float gap = 0f)
		{
			GComponent prev = null;

			for (var i = 0; i < components.Length; i++)
			{
				var offset = prev == null ? x : prev.x + prev.width + gap;
				components[i].y = y;
				components[i].x = offset;
				prev = components[i];
			}
		}

		public static void UpdateAttainmentFields()
		{
			var selectlist = Traverse.Create(Wnd_CangJingGeWindow.Instance).Field<List<string>>("selectlist").Value;

			if (selectlist.Count == 0)
			{
				Main.Debug("List was empty. Resetting");
				ResetFields();
				return;
			}

			var totalYin = 0;
			var totalYang = 0;
			var totalAttainment = 0;
			var difference = 0;

			foreach (var item in selectlist)
			{
				EsotericaData manual = EsotericaMgr.Instance.GetSysEsoterica(item);

				totalAttainment += manual.Difficulty;
				if (manual.GetYinyang() == 0) //Neutral
				{
					totalYin += manual.Difficulty;
					totalYang += manual.Difficulty;
				}
				else if (manual.GetYinyang() == -1)
				{
					totalYin += manual.Difficulty;
				}
				else
				{
					totalYang += manual.Difficulty;
				}
			}

			difference = totalYin - totalYang;
			
			totalAttainmentField.text = totalAttainment.ToString();
			yinAttainmentField.text = totalYin.ToString();
			yangAttainmentField.text = totalYang.ToString();

			if (difference == 0)
			{
				NPCAttainmentChangeField.text = "-";
			}
			else
			{
				attainmentChangeField.text = $"+{Math.Abs(difference)}";
				attainmentChangeField.color = difference > 0 ? Color.black : Color.white;
			}

			UpdateNPCAttainmentFields(totalAttainment, totalYin, totalYang);
		}

		public static void UpdateNPCAttainmentFields(int additionalAttainment = 0, int additionalYin = 0, int additionalYang = 0)
		{
			var npc = Traverse.Create(Wnd_CangJingGeWindow.Instance).Field<Npc>("_npc").Value;
			
			if (npc == null) return; //Not sure if this is even possible
			
			var startingAttainment = npc.PropertyMgr.Practice.GetDaoHang(true);
			var npcStartingYang = npc.PropertyMgr.Practice.GetYangDaoHang(true);
			var npcStartingYin = npc.PropertyMgr.Practice.GetYinDaoHang(true);
			
			
			var npcFinalAttainment = startingAttainment + Math.Abs(additionalYin + additionalYang);
			var npcFinalYin = npcStartingYin + additionalYin;
			var npcFinalYang = npcStartingYang + additionalYang;
			
			var npcBalance = npcFinalYin - npcFinalYang;
			
			NPCTotalAttainmentField.text = npcFinalAttainment.ToString();
			NPCYinAttainmentField.text = npc.PropertyMgr.Practice.Gong.YinYang > 0
				? npcFinalYin.ToString()
				: "-";
			NPCYangAttainmentField.text = npc.PropertyMgr.Practice.Gong.YinYang > 0
				? npcFinalYang.ToString()
				: "-";
			
			if (npcBalance == 0)
			{
				NPCAttainmentChangeField.m_n82.color = Color.white;
				NPCAttainmentChangeField.text = "☯️";
			}
			else
			{
				NPCAttainmentChangeField.text = $"+{Math.Abs(npcBalance)}";
				NPCAttainmentChangeField.color = npcBalance > 0 ? Color.black : Color.white;
			}
		}

		public static void ResetFields()
		{
			//Expert-level c# code right here...
			yinAttainmentField.text =
				yangAttainmentField.text = totalAttainmentField.text = attainmentChangeField.text = "-";

			UpdateNPCAttainmentFields();
		}


		[HarmonyPatch(typeof(Wnd_CangJingGeWindow), "OnInit")]
		[HarmonyPostfix]
		public static void AddAttainmentLabels(Wnd_CangJingGeWindow __instance)
		{
			try
			{
				CreateFields(__instance);
			}
			catch (Exception e)
			{
				Main.Debug(e.ToString());
			}
		}

		[HarmonyPatch(typeof(Wnd_CangJingGeWindow), "OnShowUpdate")]
		[HarmonyPostfix]
		public static void ManualPavilionOnShowUpdate(Wnd_CangJingGeWindow __instance)
		{
			ResetFields();
		}


		[HarmonyPatch(typeof(Wnd_CangJingGeWindow), "SetSelect")]
		[HarmonyPostfix]
		public static void OnCollectEso(Wnd_CangJingGeWindow __instance)
		{
			UpdateAttainmentFields();
		}
	}
}