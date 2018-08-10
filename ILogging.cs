using System;
using System.Linq;

namespace SlipperyFisch
{
    public interface ILogging
    {
        void Debug(IMessage message );
        void Info(IMessage message  );
        void Warn(IMessage message  );
        void Error(IMessage message );
        void Fatal(IMessage message );

    }
    public interface IMessage {
        string AsString();
    }
}
