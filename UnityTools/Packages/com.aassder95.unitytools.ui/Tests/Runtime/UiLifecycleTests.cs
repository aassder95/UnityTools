using NUnit.Framework;
using UnityTools.Util.UiFramework;

namespace UnityTools.Util.Tests.UiFramework
{
    public class UiLifecycleTests
    {
        //============================================================
        // Logic
        //============================================================
        [Test]
        public void ModelBatchUpdateNotifiesOnce()
        {
            LifecycleTestModel model = new();
            int updateCnt = 0;
            model.OnUpdated += () => updateCnt++;

            model.UpdateValues(10, 20);

            Assert.That(updateCnt, Is.EqualTo(1));
            Assert.That(model.FirstValue, Is.EqualTo(10));
            Assert.That(model.SecondValue, Is.EqualTo(20));
        }

        [Test]
        public void ModelDoesNotNotifyForSameValue()
        {
            LifecycleTestModel model = new();
            int updateCnt = 0;
            model.OnUpdated += () => updateCnt++;

            model.UpdateFirstValue(0);

            Assert.That(updateCnt, Is.Zero);
        }

        [Test]
        public void PresenterInitializesAndReleasesViewOnce()
        {
            LifecycleTestModel model = new();
            LifecycleTestView view = new();
            LifecycleTestPresenter presenter = new(model, view);

            presenter.Init();
            presenter.Init();
            presenter.Release();
            presenter.Release();

            Assert.That(view.InitCnt, Is.EqualTo(1));
            Assert.That(view.ReleaseCnt, Is.EqualTo(1));
            Assert.That(presenter.IsInit, Is.False);
        }

        [Test]
        public void PresenterDefersHiddenUpdatesUntilShow()
        {
            LifecycleTestModel model = new();
            LifecycleTestView view = new();
            LifecycleTestPresenter presenter = new(model, view);
            presenter.Init();

            model.UpdateFirstValue(1);
            model.UpdateFirstValue(2);

            Assert.That(view.RefreshCnt, Is.Zero);

            presenter.Show();

            Assert.That(view.RefreshCnt, Is.EqualTo(1));
            Assert.That(view.LastFirstValue, Is.EqualTo(2));
        }

        [Test]
        public void PresenterRefreshesVisibleViewImmediately()
        {
            LifecycleTestModel model = new();
            LifecycleTestView view = new();
            LifecycleTestPresenter presenter = new(model, view);
            presenter.Show();

            model.UpdateFirstValue(1);

            Assert.That(view.RefreshCnt, Is.EqualTo(2));
            Assert.That(view.LastFirstValue, Is.EqualTo(1));
        }

        [Test]
        public void PresenterStopsObservingModelAfterRelease()
        {
            LifecycleTestModel model = new();
            LifecycleTestView view = new();
            LifecycleTestPresenter presenter = new(model, view);
            presenter.Show();
            presenter.Release();

            model.UpdateFirstValue(1);

            Assert.That(view.RefreshCnt, Is.EqualTo(1));
        }

        //============================================================
        // Nested Types
        //============================================================
        private class LifecycleTestModel : BaseModel
        {
            private int _firstValue;
            private int _secondValue;

            public int FirstValue => _firstValue;
            public int SecondValue => _secondValue;

            public void UpdateFirstValue(int value)
            {
                SetField(ref _firstValue, value);
            }

            public void UpdateValues(int firstValue, int secondValue)
            {
                RunBatchUpdate(() =>
                {
                    SetField(ref _firstValue, firstValue);
                    SetField(ref _secondValue, secondValue);
                });
            }
        }

        private class LifecycleTestView : IView<LifecycleTestModel>
        {
            private bool _isInit;
            private bool _isVisible;
            private int _initCnt;
            private int _releaseCnt;
            private int _refreshCnt;
            private int _lastFirstValue;

            public bool IsInit => _isInit;
            public bool IsVisible => _isVisible;
            public int InitCnt => _initCnt;
            public int ReleaseCnt => _releaseCnt;
            public int RefreshCnt => _refreshCnt;
            public int LastFirstValue => _lastFirstValue;

            public void Init()
            {
                _isInit = true;
                _initCnt++;
            }

            public void Release()
            {
                _isInit = false;
                _releaseCnt++;
            }

            public void Show()
            {
                _isVisible = true;
            }

            public void Hide()
            {
                _isVisible = false;
            }

            public void Refresh(LifecycleTestModel model)
            {
                _lastFirstValue = model.FirstValue;
                _refreshCnt++;
            }
        }

        private class LifecycleTestPresenter : BasePresenter<LifecycleTestModel, LifecycleTestView>
        {
            public LifecycleTestPresenter(LifecycleTestModel model, LifecycleTestView view) : base(model, view)
            {
            }
        }
    }
}
