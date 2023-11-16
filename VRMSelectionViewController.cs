using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.Components;
using BeatSaberMarkupLanguage.ViewControllers;
using System;
using HMUI;
using TMPro;
using System.IO;
using System.Diagnostics;

namespace VRMLoader
{
    /// <summary>
    /// 真ん中のパネル
    /// </summary>
	internal class VRMSelectionViewController : BSMLResourceViewController
	{
        public override string ResourceName => "VRMLoader.BSML.vrmSelectionView.bsml";

        [UIComponent("folderList")]
        CustomListTableData customListTableData;

        readonly string folderPath = "VRMAvatars";

        protected override void DidActivate(bool firstActivation, bool addedToHierarchy, bool screenSystemEnabling)
        {
            if (firstActivation)
            {
                base.DidActivate(firstActivation, addedToHierarchy, screenSystemEnabling);
                LoadItems();

                customListTableData.tableView.didSelectCellWithIdxEvent += OnListItemSelect;
            }
        }

        [UIAction("press")]
        void ButtonPress()
        {
            LoadItems();
        }

        // リストの項目が選択されたときに呼ばれるメソッド
        void OnListItemSelect(TableView tableView, int index)
        {
            var selectedItem = customListTableData.data[index];
            string path = folderPath + "/" + selectedItem.text;

            VRMLoaderController.Instance.LoadVRM(path);
        }

        void LoadItems()
        {
            customListTableData.data.Clear();

            if(!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }

            foreach (var file in Directory.GetFiles(folderPath, "*.vrm"))
            {
                customListTableData.data.Add(new CustomListTableData.CustomCellInfo(Path.GetFileName(file)));
            }

            customListTableData.tableView.ReloadData();
        }
    }
}