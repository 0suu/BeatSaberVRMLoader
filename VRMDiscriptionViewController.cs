using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.Components;
using BeatSaberMarkupLanguage.ViewControllers;
using System;
using HMUI;
using TMPro;
using System.IO;

namespace TestMod20231104
{
    internal class VRMDiscriptionViewController : BSMLResourceViewController
    {
        public override string ResourceName => "TestMod20231104.BSML.vrmSettingView.bsml";

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
            text.text = "Refreshing list...";
            LoadItems();
            text.text = "List refreshed!";
        }

        private void LoadItems()
        {
            string folderPath = "C:/Users/pczuk/Desktop/switchbot"; // 任意のフォルダパスを指定
            customListTableData.data.Clear();

            foreach (var file in Directory.GetFiles(folderPath))
            {
                customListTableData.data.Add(new CustomListTableData.CustomCellInfo(Path.GetFileName(file)));
            }

            customListTableData.tableView.ReloadData();
        }
    }
}