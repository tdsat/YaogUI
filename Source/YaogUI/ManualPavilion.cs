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
		public static UI_NpcInfoLable yinAttainmentField;
		public static UI_NpcInfoLable yangAttainmentField;
		public static UI_NpcInfoLable totalAttainmentField;
		public static UI_NpcInfoLable attainmentChangeField;
		public static UI_NpcInfoLable npcYinAttainmentField;
		public static UI_NpcInfoLable npcYangAttainmentField;
		public static UI_NpcInfoLable npcTotalAttainmentField;
		public static UI_NpcInfoLable npcAttainmentChangeField;

		private const int fieldWidth = 40;
		private const int fieldHeight = 20;

		private static UI_NpcInfoLable GetLabelField(string name)
		{
			var field = UI_NpcInfoLable.CreateInstance();
			field.text = "-";
			field.name = name;
			field.height = fieldHeight;
			field.width = fieldWidth;

			return field;
		}

		public static void CreateFields(Wnd_CangJingGeWindow __instance)
		{
			yinAttainmentField = GetLabelField("YaogUI.YinAttainmentField");
			yangAttainmentField = GetLabelField("YaogUI.YangAttainmentField");
			totalAttainmentField = GetLabelField("YaogUI.TotalAttainmentField");
			attainmentChangeField = GetLabelField("YaogUI.AttainmentChangeField");

			npcYinAttainmentField = GetLabelField("YaogUI.NPCYinAttainmentField");
			npcYangAttainmentField = GetLabelField("YaogUI.NPCYangAttainmentField");
			npcTotalAttainmentField = GetLabelField("YaogUI.NPCTotalAttainmentField");
			npcAttainmentChangeField = GetLabelField("YaogUI.NPCAttainmentChangeField");

			var mainPane = (UI_CangJingGeWindow)__instance.contentPane;
			var bgImage = new GImage();
			bgImage.texture = mainPane.m_n5.texture;
			// Cool magic numbers
			bgImage.width = 246;
			bgImage.height = 100;
			bgImage.x = 780;
			bgImage.y = mainPane.m_n5.y + mainPane.m_n5.height;

			yinAttainmentField.color = Color.black;
			yangAttainmentField.color = Color.white;
			totalAttainmentField.color = new Color32(255, 191, 0, 255);
			totalAttainmentField.fontsize += 2;
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
						{ npcTotalAttainmentField, npcYinAttainmentField, npcYangAttainmentField, npcAttainmentChangeField }
					, bgImage.x + 35, bgImage.y + 60, 5);

				npcYinAttainmentField.color = Color.black;
				npcYangAttainmentField.color = Color.white;
				npcTotalAttainmentField.color = new Color32(255, 191, 0, 255);
				npcTotalAttainmentField.fontsize += 2;
				npcTotalAttainmentField.height = 22;

				npcYinAttainmentField.tooltips = TFMgr.Get("阴性造诣");
				npcYangAttainmentField.tooltips = TFMgr.Get("阳性造诣");
				npcTotalAttainmentField.tooltips = $"{TFMgr.Get("学习完所有选定手册后的最终成果")}\n\n{TFMgr.Get("降低参悟值的功法不计入计算")}";
				npcAttainmentChangeField.tooltips = TFMgr.Get("习得秘籍后的阴阳平衡：黑色代表阴性增强，白色代表阳性增强。");

				mainPane.AddChild(npcYinAttainmentField);
				mainPane.AddChild(npcYangAttainmentField);
				mainPane.AddChild(npcTotalAttainmentField);
				mainPane.AddChild(npcAttainmentChangeField);
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
				npcAttainmentChangeField.text = "-";
			}
			else
			{
				attainmentChangeField.text = $"+{Math.Abs(difference)}";
				attainmentChangeField.color = difference > 0 ? Color.black : Color.white;
			}

			UpdateNPCAttainmentFields(totalAttainment, totalYin, totalYang);
		}

		public static void UpdateNPCAttainmentFields(int additionalAttainment = 0, int additionalYin = 0,
			int additionalYang = 0)
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

			npcTotalAttainmentField.text = npcFinalAttainment.ToString();
			npcYinAttainmentField.text = npc.PropertyMgr.Practice.Gong.YinYang > 0
				? npcFinalYin.ToString()
				: "-";
			npcYangAttainmentField.text = npc.PropertyMgr.Practice.Gong.YinYang > 0
				? npcFinalYang.ToString()
				: "-";

			if (npcBalance == 0)
			{
				npcAttainmentChangeField.color = Color.gray;
				npcAttainmentChangeField.text = "☯️";
			}
			else
			{
				npcAttainmentChangeField.text = $"+{Math.Abs(npcBalance)}";
				npcAttainmentChangeField.color = npcBalance > 0 ? Color.black : Color.white;
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