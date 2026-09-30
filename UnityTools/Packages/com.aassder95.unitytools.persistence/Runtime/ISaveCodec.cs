namespace UnityTools.Persistence
{
    public interface ISaveCodec<T> where T : class
    {
        bool TrySerialize(T data, out string payload);
        bool TryDeserialize(string payload, out T data);
    }
}
