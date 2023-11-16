using BeatSaberMarkupLanguage.MenuButtons;
using BeatSaberMarkupLanguage;
using UnityEngine;

namespace VRMLoader
{

	internal class UiManager
	{
        private static readonly MenuButton menuButton = new MenuButton("VRMAvatar", "", MaterialsMenuButtonPressed, true);

        public static Coordinator materialsFlowCoordinator;
        public static bool created = false;

        public static void CreateMenu()
        {
            if (!created)
            {
                MenuButtons.instance.RegisterButton(menuButton);
                menuButton.Interactable = false;
                created = true;
            }
        }

        public static void ActiveMenuButton()
        {
            menuButton.Interactable = true;
        }

        public static void RemoveMenu()
        {
            if (created)
            {
                MenuButtons.instance.UnregisterButton(menuButton);
                created = false;
            }
        }

        public static void ShowMaterialsFlow()
        {
            if (materialsFlowCoordinator == null)
            {
                materialsFlowCoordinator = BeatSaberUI.CreateFlowCoordinator<Coordinator>();
            }

            BeatSaberUI.MainFlowCoordinator.PresentFlowCoordinator(materialsFlowCoordinator);
        }
        
        private static void MaterialsMenuButtonPressed() => ShowMaterialsFlow();
    }
}
