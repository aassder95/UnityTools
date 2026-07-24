using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace UnityTools.Util.UiFramework
{
    public class UiFocusScope : MonoBehaviour, IUiFocusControl
    {
        //============================================================
        // Inspector Fields
        //============================================================
        [SerializeField] private EventSystem _eventSystem;
        [SerializeField] private Selectable _defaultSelectable;

        //============================================================
        // Fields
        //============================================================
        private GameObject _goLastSelected;

        //============================================================
        // Logic
        //============================================================
        public void SaveFocus()
        {
            GameObject goSelected = _eventSystem.currentSelectedGameObject;
            if (goSelected == null)
                return;

            Transform trSelected = goSelected.transform;
            if (goSelected == gameObject || trSelected.IsChildOf(transform))
                _goLastSelected = goSelected;
        }

        public void RestoreFocus()
        {
            if (_goLastSelected != null && _goLastSelected.activeInHierarchy)
            {
                _eventSystem.SetSelectedGameObject(_goLastSelected);
                return;
            }

            if (_defaultSelectable != null && _defaultSelectable.isActiveAndEnabled && _defaultSelectable.IsInteractable())
                _eventSystem.SetSelectedGameObject(_defaultSelectable.gameObject);
        }
    }
}
