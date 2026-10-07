namespace UnityTools.Sheets
{
    public delegate bool CsvCellReader<T>(string text, out T value);
}
