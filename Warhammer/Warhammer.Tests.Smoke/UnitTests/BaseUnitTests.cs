using NUnit.Framework;

namespace Warhammer.Tests.Smoke.UnitTests
{
    [TestFixture]
    [Category("Unit")]
    public class BaseUnitTests
    {
        [Test]
        public void TestReality()
        {
            Assert.IsTrue(true);
        }
    }
}
