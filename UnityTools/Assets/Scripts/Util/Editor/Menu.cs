using System.IO;
using UnityEditor;
using UnityEngine;

namespace UnityTools.Util
{
    public class Menu
    {
        [MenuItem("Util/Data/Clear AllData")]
        static void ClearAllData()
        {
            PlayerPrefs.DeleteAll();
            Directory.Delete(Application.persistentDataPath, true);
        }

        [MenuItem("Util/Data/Clear PlayerPrefs")]
        static void ClearPlayerPrefs()
        {
            PlayerPrefs.DeleteAll();
        }

        [MenuItem("Util/Data/Clear PersistentData")]
        static void ClearPersistentData()
        {
            Directory.Delete(Application.persistentDataPath, true);
        }
    }
}