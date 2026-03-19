namespace UnityTools.Util
{
    public interface IState
    {
        //============================================================
        //Logic
        //============================================================
        void Enter();
        void Execute();
        void Exit();
    }
}
