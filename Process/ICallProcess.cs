using System.Threading.Tasks;

namespace NanoTools2.Process
{
    interface ICallProcess
    {
        bool IsProcessEnable();
        Task<string> CallProcessAsync();
        string CancelProcess();
    }
}
