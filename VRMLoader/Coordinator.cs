using System;
using System.Diagnostics;
using BeatSaberMarkupLanguage;
using HMUI;

namespace VRMLoader
{
	internal class Coordinator : FlowCoordinator
	{
        private VRMSelectionViewController avatarSelectionView;
        private VRMAvatarSettingUiViewController avatarSettingUiView;
        private VRMDiscriptionViewController discriptionView;

        public void Awake()
        {
            if (!discriptionView)
            {
                discriptionView = BeatSaberUI.CreateViewController<VRMDiscriptionViewController>();
            }
            
            if (!avatarSettingUiView)
            {
                avatarSettingUiView = BeatSaberUI.CreateViewController<VRMAvatarSettingUiViewController>();
            }
            
            if (!avatarSelectionView)
            {
                avatarSelectionView = BeatSaberUI.CreateViewController<VRMSelectionViewController>();
            }
        }

        protected override void DidActivate(bool firstActivation, bool addedToHierarchy, bool screenSystemEnabling)
        {
            try
            {
                if (firstActivation)
                {
                    SetTitle("VRMAvatar", ViewController.AnimationType.In);
                    showBackButton = true;
                    ProvideInitialViewControllers(avatarSelectionView, discriptionView, avatarSettingUiView);
                }
            }
            catch (Exception ex)
            {
                UnityEngine.Debug.LogWarning(ex);
            }            
        }

        protected override void BackButtonWasPressed(ViewController topViewController)
        {
            BeatSaberUI.MainFlowCoordinator.DismissFlowCoordinator(this);
        }
    }
}
