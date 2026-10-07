using System;
using UnityEngine;

namespace UnityTools.Ui.Assets
{
    [Serializable]
    public class AssetEntry<T> where T : UnityEngine.Object
    {
        //============================================================
        // Fields
        //============================================================
        [SerializeField] private string _key;
        [SerializeField] private T _asset;

        //============================================================
        // Properties
        //============================================================
        public string Key => _key;
        public T Asset => _asset;

        //============================================================
        // Constructors
        //============================================================
        public AssetEntry(string key, T asset)
        {
            _key = key;
            _asset = asset;
        }
    }
}
