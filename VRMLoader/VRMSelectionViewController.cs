using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.Components;
using BeatSaberMarkupLanguage.ViewControllers;
using System;
using HMUI;
using TMPro;
using System.IO;
using System.Diagnostics;

namespace TestMod20231104
{
    /// <summary>
    /// 真ん中のパネル
    /// </summary>
	internal class VRMSelectionViewController : BSMLResourceViewController
	{
        public override string ResourceName => "TestMod20231104.BSML.vrmSelectionView.bsml";

        [UIComponent("some-text")]
        private TextMeshProUGUI text;

        [UIComponent("folderList")]
        private CustomListTableData customListTableData;

        protected override void DidActivate(bool firstActivation, bool addedToHierarchy, bool screenSystemEnabling)
        {
            if (firstActivation)
            {
                base.DidActivate(firstActivation, addedToHierarchy, screenSystemEnabling);
                LoadItems();
            }
        }

        [UIAction("press")]
        private void ButtonPress()
        {
            LoadItems();
        }

        private void LoadItems()
        {
            string folderPath = "C:/Users/pczuk/Desktop/switchbot"; // 任意のフォルダパスを指定
            customListTableData.data.Clear();
            UnityEngine.Debug.Log(text.gameObject.layer);

            foreach (var file in Directory.GetFiles(folderPath))
            {
                customListTableData.data.Add(new CustomListTableData.CustomCellInfo(Path.GetFileName(file)));
            }

            customListTableData.tableView.ReloadData();
        }
    }
}