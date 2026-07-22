namespace UnityTools.Util.Core.Events
{
    public delegate void EventDelegate(object sender);
    public delegate void EventDelegate<T>(object sender, T param);
}
