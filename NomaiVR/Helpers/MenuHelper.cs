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
            var newPauseMenuButton = GameObject.Instantiate(buttonTemplate);
            newPauseMenuButton.transform.SetParent(pauseItems);
            newPauseMenuButton.transform.SetSiblingIndex(order);

            var text = newPauseMenuButton.GetComponentInChildren<Text>(true);
            GameObject.Destroy(newPauseMenuButton.GetComponent<Menu>());
            GameObject.Destroy(newPauseMenuButton.GetComponent<SubmitActionMenu>());
            GameObject.Destroy(text.gameObject.GetComponent<LocalizedText>());
            var submitAction = newPauseMenuButton.AddComponent<SubmitAction>();
            submitAction.OnSubmitAction += onSubmit;
            text.text = name;
        }
    }
}
