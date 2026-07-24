using UnityTools.Ui;

namespace UnityTools.Samples.Core
{
    public interface ISampleModule : IPresenter
    {
        //============================================================
        // Properties
        //============================================================
        string ModuleKey { get; }
    }
}
