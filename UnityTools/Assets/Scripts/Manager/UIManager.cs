using System.Collections;
using UnityEngine;
using UnityTools.Model;
using UnityTools.Presenter;
using UnityTools.UI;
using UnityTools.Util;

namespace UnityTools.Manager
{
    public class UIManager : Singleton<UIManager>
    {
        [SerializeField] RankView _rankView;
        [SerializeField] int _rankModelCnt = 10;

        RankPresenter _rankPresenter;

        void Start()
        {
            if (_rankView != null)
            {
                RankModel model = new RankModel(_rankModelCnt);
                _rankPresenter = new RankPresenter(model, _rankView);
            }

            PeriodTimer info = new PeriodTimer("OPEN_KEY", "CLOSED_KEY", 1.0, 1.0);
            info.OnWaitFunc += Func;
            info.Init();
        }

        IEnumerator Func()
        {
            yield return new WaitForSeconds(5.0f);
        }
    }
}
