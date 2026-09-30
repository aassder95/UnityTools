using UnityEngine;
using UnityEngine.UI;
using UnityTools.Ui;

namespace UnityTools.Benchmark.Samples
{
    public class UiLabItem : MonoBehaviour, IDynamicScrollItem
    {
        //============================================================
        // Inspector Fields
        //============================================================
        [SerializeField] private Text _txtRow;
        [SerializeField] private Image _imgBg;

        //============================================================
        // Fields
        //============================================================
        private RectTransform _rt;
        private int _idx;

        //============================================================
        // Properties
        //============================================================
        public int Idx => _idx;

        //============================================================
        // Init/Register
        //============================================================
        public void Init()
        {
            _rt = (RectTransform)transform;
        }

        //============================================================
        // Logic
        //============================================================
        public void Bind(string row)
        {
            _txtRow.text = row;
            _imgBg.color = _idx % 2 == 0 ? new Color(0.10f, 0.18f, 0.27f) : new Color(0.13f, 0.23f, 0.33f);
        }

        public void OnGet()
        {
            gameObject.SetActive(true);
        }

        public void OnReturn()
        {
            gameObject.SetActive(false);
        }

        public void SetIdx(int idx)
        {
            _idx = idx;
        }

        public void SetPos(Vector2 pos)
        {
            _rt.anchoredPosition = pos;
        }
    }
}
