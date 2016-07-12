using System;

namespace Warhammer.Core.Abstract
{
    public interface IExceptionLogHandler
    {
        void LogException(Exception exception, string identifier, int sequence, DateTime timestamp);
    }
}