namespace UnityTools.Ui
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
