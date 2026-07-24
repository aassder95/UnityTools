using System.IO;
using UnityEditor;
using UnityEngine;

namespace UnityTools.Timer.Editor
{
    public static class DataMenuCommands
    {
        //============================================================
        // Logic
        //============================================================
        [MenuItem("Util/Data/Clear AllData")]
        private static void ClearAllData()
        {
            if (!EditorUtility.DisplayDialog("전체 데이터 삭제", "PlayerPrefs와 persistentDataPath의 모든 데이터를 삭제합니다. 계속하시겠습니까?", "삭제", "취소"))
                return;

            PlayerPrefs.DeleteAll();

            if (Directory.Exists(Application.persistentDataPath))
                Directory.Delete(Application.persistentDataPath, true);
        }

        [MenuItem("Util/Data/Clear PlayerPrefs")]
        private static void ClearPlayerPrefs()
        {
            if (!EditorUtility.DisplayDialog("Confirm", "Delete PlayerPrefs?", "Delete", "Cancel"))
                return;

            PlayerPrefs.DeleteAll();
        }

        [MenuItem("Util/Data/Clear PersistentData")]
        private static void ClearPersistentData()
        {
            if (!EditorUtility.DisplayDialog("Confirm", "Delete PersistentData?", "Delete", "Cancel"))
                return;

            if (!Directory.Exists(Application.persistentDataPath))
                return;

            Directory.Delete(Application.persistentDataPath, true);
        }
    }
}
