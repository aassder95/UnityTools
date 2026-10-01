using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace UnityTools.Showcase
{
    public class ShowcaseController : MonoBehaviour
    {
        //============================================================
        // Inspector Fields
        //============================================================
        [Header("Destinations")]
        [SerializeField] private string[] _scenePaths;
        [SerializeField] private Button _btnUi;
        [SerializeField] private Button _btnSave;
        [SerializeField] private Button _btnTimer;
        [Header("Navigation")]
        [SerializeField] private GameObject _goHubView;
        [SerializeField] private GameObject _goHubInput;
        [SerializeField] private GameObject _goReturnView;
        [SerializeField] private Button _btnReturn;
        [SerializeField] private Text _txtStatus;

        //============================================================
        // Fields
        //============================================================
        private Coroutine _coNavigate;
        private int _activeLabIdx = -1;

        //============================================================
        // Properties
        //============================================================
        public bool IsBusy => _coNavigate != null;
        public bool IsHubVisible => _goHubView.activeSelf;
        public int ActiveLabIdx => _activeLabIdx;

        //============================================================
        // Unity Methods
        //============================================================
        private void OnEnable()
        {
            _btnUi.onClick.AddListener(OpenUi);
            _btnSave.onClick.AddListener(OpenSave);
            _btnTimer.onClick.AddListener(OpenTimer);
            _btnReturn.onClick.AddListener(ReturnToHub);
        }

        private void OnDisable()
        {
            _btnUi.onClick.RemoveListener(OpenUi);
            _btnSave.onClick.RemoveListener(OpenSave);
            _btnTimer.onClick.RemoveListener(OpenTimer);
            _btnReturn.onClick.RemoveListener(ReturnToHub);
        }

        private void OnDestroy()
        {
            if (_coNavigate != null)
                StopCoroutine(_coNavigate);

            _coNavigate = null;
        }

        //============================================================
        // Logic
        //============================================================
        public void OpenLab(int idx)
        {
            if (IsBusy || _activeLabIdx >= 0)
                return;

            if (idx < 0 || idx >= _scenePaths.Length || !Application.CanStreamedLevelBeLoaded(_scenePaths[idx]))
            {
                _txtStatus.text = "Lab unavailable. Add the Showcase and its three lab scenes to Build Settings.";
                return;
            }

            _coNavigate = StartCoroutine(CoOpenLab(idx));
        }

        public void ReturnToHub()
        {
            if (IsBusy || _activeLabIdx < 0)
                return;

            _coNavigate = StartCoroutine(CoReturnToHub());
        }

        //============================================================
        // Coroutines
        //============================================================
        private IEnumerator CoOpenLab(int idx)
        {
            _goHubView.SetActive(false);
            _goHubInput.SetActive(false);
            _goReturnView.SetActive(true);
            _btnReturn.interactable = false;
            yield return SceneManager.LoadSceneAsync(_scenePaths[idx], LoadSceneMode.Additive);
            SceneManager.SetActiveScene(SceneManager.GetSceneByPath(_scenePaths[idx]));
            _activeLabIdx = idx;
            _btnReturn.interactable = true;
            _coNavigate = null;
        }

        private IEnumerator CoReturnToHub()
        {
            _btnReturn.interactable = false;
            SceneManager.SetActiveScene(gameObject.scene);
            yield return SceneManager.UnloadSceneAsync(_scenePaths[_activeLabIdx]);
            _activeLabIdx = -1;
            _goReturnView.SetActive(false);
            _goHubView.SetActive(true);
            _goHubInput.SetActive(true);
            _txtStatus.text = "Choose a lab. Each visit starts with a fresh scene.";
            _coNavigate = null;
        }

        //============================================================
        // Callbacks
        //============================================================
        private void OpenUi() => OpenLab(0);
        private void OpenSave() => OpenLab(1);
        private void OpenTimer() => OpenLab(2);
    }
}
