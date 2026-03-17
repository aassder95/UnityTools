using System.IO;
using UnityEditor;
using UnityEngine;

namespace UnityTools.Util
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

    public class ConvertMenuCommands
    {
        //============================================================
        //Logic
        //============================================================
        [MenuItem("Util/Convert/CSV to JSON")]
        private static void ConvertCsvToJson()
        {
            string csvPath = EditorUtility.OpenFilePanel("Select CSV file", "", "csv");
            if(string.IsNullOrEmpty(csvPath))
                return;

            string jsonPath = EditorUtility.SaveFilePanel("Save JSON file", "", "converted.txt", "txt");
            if(string.IsNullOrEmpty(jsonPath))
                return;

            CsvJsonConverter.Convert(csvPath, jsonPath);
        }
    }
}
