namespace UnityTools.Sheets
{
    public delegate bool CsvRowReader<T>(CsvRow row, out T item, out string error) where T : class;
}
