using System.Collections;

namespace FW
{
    public interface LoggerUtility
    {
        void Debug(string message);
        void Info(string message);
        void Warning(string message);
        void Error(string message);
    }
}
