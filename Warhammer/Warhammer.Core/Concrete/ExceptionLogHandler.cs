using System;
using Warhammer.Core.Abstract;
using Warhammer.Core.Entities;

namespace Warhammer.Core.Concrete
{
    public class ExceptionLogHandler : IExceptionLogHandler
    {
        private IRepository _repo;

        public ExceptionLogHandler(IRepository repo)
        {
            _repo = repo;
        }

        public void LogException(Exception exception, string identifier, int sequence, DateTime timestamp)
        {
            ExceptionLog log = new ExceptionLog
            {
                DateTime = timestamp,
                ExceptionType = exception.GetType().FullName,
                Identifier = identifier,
                Sequence = sequence,
                StackTrace = exception.StackTrace,
                Message = exception.Message,
                Severity = (int)LogSeverity.Exception,

                
                
            };



            _repo.Save(log);
        }
    }
}