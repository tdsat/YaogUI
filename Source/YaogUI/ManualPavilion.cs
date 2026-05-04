using System;
using System.Collections.Generic;
using FairyGUI;
using HarmonyLib;
using KTV;
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
			bgImage.width = 240;
			bgImage.height = 100;
			bgImage.x = 800;
			bgImage.y = 490;

			yinAttainmentField.color = Color.white;
			yangAttainmentField.color = Color.black;
			totalAttainmentField.color = new Color32(255, 191, 0, 255);
			totalAttainmentField.fontsize += 2;

			yinAttainmentField.tooltips = TFMgr.Get("阴性造诣");
			yangAttainmentField.tooltips = TFMgr.Get("阳性造诣");
			totalAttainmentField.tooltips = $"{TFMgr.Get("所选秘籍的总参悟")}\n\n{TFMgr.Get("降低参悟值的功法不计入计算")}";
			attainmentChangeField.tooltips = TFMgr.Get("阴阳变化：白色代表阴气较重，黑色代表阳气较重");

			totalAttainmentField.height = 22;

			PositionFields(new GComponent[]
					{ totalAttainmentField, yinAttainmentField, yangAttainmentField, attainmentChangeField }
				, bgImage.x + 30, bgImage.y + 15, 5);

			ResetFields();

			mainPane.AddChild(bgImage);

			mainPane.AddChild(yinAttainmentField);
			mainPane.AddChild(yangAttainmentField);
			mainPane.AddChild(totalAttainmentField);
			mainPane.AddChild(attainmentChangeField);
			mainPane.m_clearall.onClick.Add(ResetFields);
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

			Main.Debug(
				$"Total Yin: {totalYin} Total Yang: {totalYang} Total Attainment: {totalAttainment} Diff: {difference}");

			difference = totalYin - totalYang;

			totalAttainmentField.text = totalAttainment.ToString();
			yinAttainmentField.text = totalYin.ToString();
			yangAttainmentField.text = totalYang.ToString();

			attainmentChangeField.text = $"+{Math.Abs(difference)}";
			attainmentChangeField.color = difference > 0 ? Color.black : Color.white;
		}

		public static void ResetFields()
		{
			//Expert-level c# code right here...
			yinAttainmentField.text =
				yangAttainmentField.text = totalAttainmentField.text = attainmentChangeField.text = "-";
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