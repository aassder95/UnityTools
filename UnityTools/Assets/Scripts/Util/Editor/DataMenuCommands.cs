using System.IO;
using UnityEditor;
using UnityEngine;

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
