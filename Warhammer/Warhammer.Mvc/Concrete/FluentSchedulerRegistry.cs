using FluentScheduler;
using Warhammer.Mvc.Concrete.Jobs;

namespace Warhammer.Mvc.Concrete
{
    public class FluentSchedulerRegistry : Registry
    {
        public FluentSchedulerRegistry()
        {
            Schedule<ScoreJob>().NonReentrant().ToRunNow().AndEvery(1).Minutes();
        }
    }
}