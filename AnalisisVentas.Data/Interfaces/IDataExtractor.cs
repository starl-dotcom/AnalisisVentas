namespace AnalisisVentas.Data.Interfaces
{
    public interface IDataExtractor
    {
        IEnumerable<T> Extract<T>(string filePath) where T : class;
    }
}
