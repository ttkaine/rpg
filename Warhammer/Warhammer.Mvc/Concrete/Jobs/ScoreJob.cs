using System;
using System.Web.Hosting;
using System.Web.Mvc;
using FluentScheduler;
using Warhammer.Core.Abstract;

namespace Warhammer.Mvc.Concrete.Jobs
{
    public class ScoreJob : IJob, IRegisteredObject
    {
        private readonly object _lock = new object();

        private bool _shuttingDown;

        public ScoreJob()
        {

            HostingEnvironment.RegisterObject(this);
        }

        public void Execute()
        {
            lock (_lock)
            {
                if (_shuttingDown)
                    return;

                try
                {
                    IScoreCalculator scoreCalculator = DependencyResolver.Current.GetService<IScoreCalculator>();
                    scoreCalculator.UpdateScores();
                }
                catch (Exception ex)
                {
                    IExceptionLogHandler log = DependencyResolver.Current.GetService<IExceptionLogHandler>();
                    log.LogException(ex, "Score Job", 101, DateTime.Now);
                }
            }
        }

        public void Stop(bool immediate)
        {
            lock (_lock)
            {
                _shuttingDown = true;
            }

            HostingEnvironment.UnregisterObject(this);
        }
    }
}