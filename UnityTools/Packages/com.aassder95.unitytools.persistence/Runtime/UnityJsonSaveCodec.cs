using System;
using UnityEngine;

namespace UnityTools.Persistence
{
    public class UnityJsonSaveCodec<T> : ISaveCodec<T> where T : class
    {
        //============================================================
        // Logic
        //============================================================
        public bool TrySerialize(T data, out string payload)
        {
            payload = null;
            if (data == null)
                return false;

            try
            {
                payload = JsonUtility.ToJson(data);
                return !string.IsNullOrEmpty(payload);
            }
            catch (ArgumentException)
            {
                return false;
            }
        }

        public bool TryDeserialize(string payload, out T data)
        {
            data = null;
            if (string.IsNullOrEmpty(payload))
                return false;

            try
            {
                data = JsonUtility.FromJson<T>(payload);
                return data != null;
            }
            catch (ArgumentException)
            {
                return false;
            }
        }
    }
}
