using NUnit.Framework;

namespace Warhammer.Tests.Smoke.SeleniumTests
{
    [System.ComponentModel.Category("Integration")]
    [TestFixture]
    public class IntegrationTests : SeleniumSetup
    {
        public void TestIntegrationReality()
        {
            Assert.IsTrue(true);
        }
    }
}
