using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

namespace UnityTools.Ui.Localization
{
    public class TmpLinkHandler : MonoBehaviour, IPointerClickHandler
    {
        //============================================================
        // Inspector Fields
        //============================================================
        [SerializeField] private TMP_Text _txt;

        //============================================================
        // Events
        //============================================================
        private event Action<string> _onLinkClicked;
        public event Action<string> OnLinkClicked { add => _onLinkClicked += value; remove => _onLinkClicked -= value; }

        //============================================================
        // Unity Methods
        //============================================================
        private void OnDestroy()
        {
            _onLinkClicked = null;
        }

        //============================================================
        // Callbacks
        //============================================================
        public void OnPointerClick(PointerEventData eventData)
        {
            if (eventData == null || eventData.button != PointerEventData.InputButton.Left)
                return;

            int idx = TMP_TextUtilities.FindIntersectingLink(_txt, eventData.position, eventData.pressEventCamera);
            if (idx < 0)
                return;

            string id = _txt.textInfo.linkInfo[idx].GetLinkID();
            _onLinkClicked?.Invoke(id);
        }
    }
}
