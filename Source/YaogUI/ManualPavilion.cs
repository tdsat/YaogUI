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
		public static UI_NpcInfoLable yinAttainmentField = UI_NpcInfoLable.CreateInstance();
		public static UI_NpcInfoLable yangAttainmentField = UI_NpcInfoLable.CreateInstance();
		public static UI_NpcInfoLable totalAttainmentField = UI_NpcInfoLable.CreateInstance();
		public static UI_NpcInfoLable attainmentChangeField = UI_NpcInfoLable.CreateInstance();

		public static void CreateFields(Wnd_CangJingGeWindow __instance)
		{
			var mainPane = (UI_CangJingGeWindow)__instance.contentPane;
			var bgImage = new GImage();
			bgImage.texture = mainPane.m_n5.texture;
			// Cool magic numbers
			bgImage.width = 200;
			bgImage.height = 100;
			bgImage.x = 800;
			bgImage.y = 490;

			yinAttainmentField.color = Color.white;
			yangAttainmentField.color = Color.black;
			// bro I can't believe I miss CSS...
			var firstRow = bgImage.y + 10;
			var firstCol = bgImage.x + 10;

			yinAttainmentField.width = yangAttainmentField.width =
				totalAttainmentField.width = attainmentChangeField.width = 40;
			yinAttainmentField.height = yangAttainmentField.height =
				totalAttainmentField.height = attainmentChangeField.height = 8;

			yinAttainmentField.y = yangAttainmentField.y = totalAttainmentField.y = attainmentChangeField.y = firstRow;
			yinAttainmentField.x = firstCol + 10;
			yangAttainmentField.x = firstCol + 70;
			totalAttainmentField.x = firstCol + 130;
			attainmentChangeField.x = firstCol + 190;

			ResetFields();


			var b2 = (GButton)UIPackage.CreateObjectFromURL("ui://ncbwb41mv9j6ah");
			mainPane.AddChild(bgImage);
			mainPane.AddChild(b2);

			b2.onClick.Add(UpdateAttainmentFields);

			mainPane.AddChild(yinAttainmentField);
			mainPane.AddChild(yangAttainmentField);
			mainPane.AddChild(totalAttainmentField);
			mainPane.AddChild(attainmentChangeField);
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
				if (manual.Element == g_emElementKind.None || manual.Element == g_emElementKind.Tu)
				{
					totalYin += manual.Difficulty;
					totalYang += manual.Difficulty;
				}
				else if (manual.Element == g_emElementKind.Jin || manual.Element == g_emElementKind.Shui)
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

			;
		}

		[HarmonyPatch(typeof(Wnd_CangJingGeWindow), "OnShowUpdate")]
		[HarmonyPostfix]
		public static void ManualPavilionOnShowUpdate(Wnd_CangJingGeWindow __instance)
		{
			ResetFields();
		}


		[HarmonyPatch(typeof(Wnd_CangJingGeWindow), "CollectEso")]
		[HarmonyPostfix]
		public static void OnCollectEso(Wnd_CangJingGeWindow __instance)
		{
			UpdateAttainmentFields();
		}
	}
}