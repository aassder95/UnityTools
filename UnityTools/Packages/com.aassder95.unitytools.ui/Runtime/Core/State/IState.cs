namespace UnityTools.Util.Core.State
{
    public interface IState
    {
        //============================================================
        // Logic
        //============================================================
        void Enter();
        void Execute();
        void Exit();
    }
}
