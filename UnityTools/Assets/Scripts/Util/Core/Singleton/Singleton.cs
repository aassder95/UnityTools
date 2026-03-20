
namespace UnityTools.Util.Core.Singleton
{
    public class Singleton<T> where T : Singleton<T>, new()
    {
        //============================================================
        //Fields
        //============================================================
        private static T _instance;

        //============================================================
        //Properties
        //============================================================
        public static T Instance => _instance ??= new T();

        //============================================================
        //Constructors
        //============================================================
        protected Singleton()
        {
        }
    }
}
