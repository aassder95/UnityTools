using UnityTools.Util.UiFramework;

namespace UnityTools.Util.Core
{
    public interface ISampleModule : IPresenter
    {
        //============================================================
        // Properties
        //============================================================
        string ModuleKey { get; }
    }
}
