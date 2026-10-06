using UnityEngine;
using UnityEngine.UI;
using UnityTools.Ui;

namespace UnityTools.Samples.Features
{
    public class FeatureItem : MonoBehaviour, IDynamicScrollItem
    {
        //============================================================
        // Inspector Fields
        //============================================================
        [SerializeField] private Text _txtRow;

        //============================================================
        // Fields
        //============================================================
        private int _idx;

        //============================================================
        // Properties
        //============================================================
        public int Idx => _idx;

        //============================================================
        // Logic
        //============================================================
        public void Init() { }
        public void OnGet() => gameObject.SetActive(true);
        public void OnReturn() => gameObject.SetActive(false);
        public void SetIdx(int idx) => _idx = idx;
        public void SetPos(Vector2 pos) => ((RectTransform)transform).anchoredPosition = pos;
        public void Bind(float height) => _txtRow.text = $"ROW {_idx + 1} / {height:0} px\nDifferent heights, same virtualized list";
    }
}
