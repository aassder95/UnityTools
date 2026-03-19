using System.IO;
using UnityEditor;
using UnityEngine;
using UnityTools.Util.Core.Collections;
using UnityTools.Util.Core.Events;
using UnityTools.Util.Core.Logging;
using UnityTools.Util.Core.Persistence;
using UnityTools.Util.Core.Pooling;
using UnityTools.Util.Core.Singleton;
using UnityTools.Util.Core.State;
using UnityTools.Util.Core.Timer.Period;
using UnityTools.Util.Core.Timer.Shared;
using UnityTools.Util.Core.Timer.Task;
using UnityTools.Util.Coroutines;
using UnityTools.Util.Extensions;
using UnityTools.Util.UIFramework;
using UnityTools.Util.Utilities;

namespace UnityTools.Util.Editor
{
    public class DataMenuCommands
    {
        //============================================================
        //Logic
        //============================================================
        [MenuItem("Util/Data/Clear AllData")]
        private static void ClearAllData()
        {
            PlayerPrefs.DeleteAll();

            if(Directory.Exists(Application.persistentDataPath))
                Directory.Delete(Application.persistentDataPath, true);
        }

        [MenuItem("Util/Data/Clear PlayerPrefs")]
        private static void ClearPlayerPrefs()
        {
            if(!EditorUtility.DisplayDialog("Confirm", "Delete PlayerPrefs?", "Delete", "Cancel"))
                return;

            PlayerPrefs.DeleteAll();
        }

        [MenuItem("Util/Data/Clear PersistentData")]
        private static void ClearPersistentData()
        {
            if(!EditorUtility.DisplayDialog("Confirm", "Delete PersistentData?", "Delete", "Cancel"))
                return;

            if(!Directory.Exists(Application.persistentDataPath))
                return;

            Directory.Delete(Application.persistentDataPath, true);
        }
    }
}
