using UnityEditor;

namespace UnityTools.Util
{
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
