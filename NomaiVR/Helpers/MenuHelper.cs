using UnityEngine;
using UnityEngine.UI;

namespace NomaiVR.Helpers
{
    public static class MenuHelper
    {
        public static void AddPauseMenuAction(this PauseMenuManager pauseMenu, string name, int order, SubmitAction.SubmitActionEvent onSubmit)
        {
            var pauseItems = pauseMenu._pauseMenu.transform.Find("PauseMenuItemsLayout");
            var buttonTemplate = pauseItems.Find("Button-Options").gameObject;
            var newPauseMenuButton = Object.Instantiate(buttonTemplate);
            newPauseMenuButton.transform.SetParent(pauseItems);
            newPauseMenuButton.transform.SetSiblingIndex(order);

            var text = newPauseMenuButton.GetComponentInChildren<Text>(true);
            Object.Destroy(newPauseMenuButton.GetComponent<Menu>());
            Object.Destroy(newPauseMenuButton.GetComponent<SubmitActionMenu>());
            Object.Destroy(text.gameObject.GetComponent<LocalizedText>());
            var submitAction = newPauseMenuButton.AddComponent<SubmitAction>();
            submitAction.OnSubmitAction += onSubmit;
            text.text = name;
        }
    }
}
