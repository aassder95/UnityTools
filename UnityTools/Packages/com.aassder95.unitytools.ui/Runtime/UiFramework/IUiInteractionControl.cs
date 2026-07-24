namespace UnityTools.Util.UiFramework
{
    public interface IUiInteractionControl
    {
        //============================================================
        // Properties
        //============================================================
        bool IsInteractionEnabled { get; }

        //============================================================
        // Logic
        //============================================================
        void SetInteractionEnabled(bool isEnabled);
    }
}
