using System;
using TMPro;
using UnityEngine;

namespace UnityTools.UI
{
    public class TimerView : MonoBehaviour
    {
        [SerializeField] TextMeshProUGUI _txtState;
        [SerializeField] TextMeshProUGUI _txtSubState;
        [SerializeField] TextMeshProUGUI _txtLoop;
        [SerializeField] TextMeshProUGUI _txtCur;
        [SerializeField] TextMeshProUGUI _txtOpen;
        [SerializeField] TextMeshProUGUI _txtClosed;

        public void SetState(string state)
        {
            _txtState.SetText(state);
        }

        public void SetSubState(string state)
        {
            _txtSubState.SetText(state);
        }

        public void SetLoop(int min)
        {
            _txtLoop.SetText("{0}", min);
        }

        public void SetTimer(DateTime cur, DateTime open, DateTime closed)
        {
            _txtCur.SetText($"cur: {cur}");
            _txtOpen.SetText($"open: {open}");
            _txtClosed.SetText($"closed: {closed}");
        }
    }
}