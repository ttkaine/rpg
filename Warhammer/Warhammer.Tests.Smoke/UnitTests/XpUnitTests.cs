using NUnit.Framework;
using Warhammer.Core.Concrete;
using Warhammer.Core.Entities;

namespace Warhammer.Tests.Smoke.UnitTests
{
    [TestFixture]
    public class XpUnitTests
    {

        [Test]
        public void RegularSessionWorthPointFive()
        {
            AuthenticatedDataProvider data = new AuthenticatedDataProvider(null, null, null, null, null, null, null, null);
            Session page = new Session();
            int sessionId;
            decimal xp = data.GetDefaultXpForPage(page, out sessionId);

            Assert.AreEqual(0.5m, xp, "Xp for a session should be 0.5");
        }

        [Test]
        public void TextSessionWorthPointTwoFive()
        {
            AuthenticatedDataProvider data = new AuthenticatedDataProvider(null, null, null, null, null, null, null, null);
            Session page = new Session
            {
                IsTextSession = true,
                IsClosed = true
            };
            int sessionId;
            decimal xp = data.GetDefaultXpForPage(page, out sessionId);

            Assert.AreEqual(0.25m, xp, "Xp for a session should be 0.25");
        }

        [Test]
        public void RegualrLogSessionWorthPointOneIfEmpty()
        {
            AuthenticatedDataProvider data = new AuthenticatedDataProvider(null, null, null, null, null, null, null, null);
            SessionLog page = new SessionLog
            {
                Session = new Session(),
            };
            int sessionId;
            decimal xp = data.GetDefaultXpForPage(page, out sessionId);

            Assert.AreEqual(0.1m, xp, "Xp for a session should be 0.1");
        }

        [Test]
        public void TextLogSessionWorthPointOneIfEmpty()
        {
            AuthenticatedDataProvider data = new AuthenticatedDataProvider(null, null, null, null, null, null, null, null);
            SessionLog page = new SessionLog
            {
                Session = new Session { IsTextSession = true , IsClosed = true},
            };
            int sessionId;
            decimal xp = data.GetDefaultXpForPage(page, out sessionId);

            Assert.AreEqual(0.1m, xp, "Xp for a session should be 0.1");
        }
    }
}