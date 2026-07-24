namespace UnityTools.Timer
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
