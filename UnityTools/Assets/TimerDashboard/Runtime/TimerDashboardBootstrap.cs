using System;
using System.Collections;
using UnityEngine;

namespace UnityTools.TimerDashboard
{
    [RequireComponent(typeof(TimerDashboardController))]
    public class TimerDashboardBootstrap : MonoBehaviour
    {
        //============================================================
        // Fields
        //============================================================
        private TimerDashboardController _controller;
        private TimerDashboardStorage _storage;
        private Coroutine _coInit;

        //============================================================
        // Properties
        //============================================================
        public TimerDashboardStorage Storage => _storage;

        //============================================================
        // Unity Methods
        //============================================================
        private void Awake()
        {
            _controller = GetComponent<TimerDashboardController>();
            _storage = new TimerDashboardStorage();
        }

        private void OnEnable()
        {
            _coInit = StartCoroutine(CoInit());
        }

        private void OnDisable()
        {
            if (_coInit != null)
            {
                StopCoroutine(_coInit);
                _coInit = null;
            }

            _controller.Release();
        }

        //============================================================
        // Coroutines
        //============================================================
        private IEnumerator CoInit()
        {
            yield return null;
            _coInit = null;
            if (!_controller.TryInit(_storage, ReadUtc))
                Debug.LogError("타이머 관리 데모를 초기화하지 못했습니다.");
        }

        //============================================================
        // Utilities
        //============================================================
        private static DateTime ReadUtc() => DateTime.UtcNow;
    }
}
