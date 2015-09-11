using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using OpenQA.Selenium;
using OpenQA.Selenium.Firefox;
using OpenQA.Selenium.Support.UI;

namespace Warhammer.Tests.Integration
{
    [TestFixture]
    public class TestingTests
    {
        private IWebDriver _driver;
        private readonly TestSettings _settings = new TestSettings();
        private WebDriverWait _wait;
        [TestFixtureSetUp]
        public void SetUp()
        {
            Assert.IsTrue(_settings.LoadSettings(),"If we can't load the settings we're not going to get far...");
            _driver = new FirefoxDriver();
            _wait = new WebDriverWait(_driver,new TimeSpan(0,0,10));
            _driver.Navigate().GoToUrl(_settings.UpdateUrl);

            _wait.Until(ExpectedConditions.ElementExists(By.Id("Email")));
            IWebElement email = _driver.FindElement(By.Id("Email"));
            IWebElement password = _driver.FindElement(By.Id("Password"));
            email.SendKeys(_settings.Username);
            password.SendKeys(_settings.Password);
            password.Submit();

            Assert.IsTrue(_driver.PageSource.Contains("Did"), "Database update should report DID");

            _driver.Navigate().GoToUrl(_settings.BaseUrl);
        }

        [TestFixtureTearDown]
        public void TearDown()
        {
            _driver.Quit();
        }

        [Test]
        public void HitHomePage()
        {
            _driver.Navigate().GoToUrl(_settings.BaseUrl);
            IWebElement element = _driver.FindElement(By.Id("fullName"));

            Assert.AreEqual("Warhammer",element.Text);
        }


        [Test]
        public void HitSessions()
        {
            _driver.Navigate().GoToUrl(_settings.BaseUrl + "/Home/Sessions");
            Assert.IsTrue(_driver.PageSource.Contains("Sessions"));
        }

        [Test]
        public void HitGraveyard()
        {
            _driver.Navigate().GoToUrl(_settings.BaseUrl + "/Home/Graveyard");
            Assert.IsTrue(_driver.PageSource.Contains("The Graveyard"));
        }

        [Test]
        public void HitCharacterLeague()
        {
            _driver.Navigate().GoToUrl(_settings.BaseUrl + "/Home/CharacterLeague");
            Assert.IsTrue(_driver.PageSource.Contains("League"));
        }

        [Test]
        public void HitAFewPageTypePages()
        {
            _driver.Navigate().GoToUrl(_settings.BaseUrl);
            _wait.Until(ExpectedConditions.ElementExists(By.Id("recentChangesList")));
            IWebElement list = _driver.FindElement(By.Id("recentChangesList"));

            List<string> linkTexts = list.FindElements(By.TagName("a")).Select(l => l.Text).ToList();

            foreach (string linkText in linkTexts)
            {
                _driver.FindElement(By.LinkText(linkText)).Click();
                _wait.Until(ExpectedConditions.ElementExists(By.Id("fullName")));
                IWebElement titleElement = _driver.FindElement(By.Id("fullName"));
                Assert.IsTrue(linkText.Contains(titleElement.Text), "Title should be the front of link text");
                _driver.Navigate().GoToUrl(_settings.BaseUrl);
                _wait.Until(ExpectedConditions.ElementExists(By.Id("recentChangesList")));
            }
        }

        [Test]
        public void HitAllTheCharacters()
        {
            _driver.Navigate().GoToUrl(_settings.BaseUrl + "/Home/CharacterLeague");
            _wait.Until(ExpectedConditions.ElementExists(By.Id("characterLeagueList")));
            IWebElement list = _driver.FindElement(By.Id("characterLeagueList"));

            List<string> linkIds = list.FindElements(By.TagName("a")).Select(l => l.GetAttribute("Id")).ToList();

            foreach (string linkId in linkIds)
            {
                _driver.FindElement(By.Id(linkId)).Click();
                _wait.Until(ExpectedConditions.ElementExists(By.Id("fullName")));
                Assert.IsNotNull(ExpectedConditions.ElementExists(By.Id("fullName")));
                _driver.Navigate().GoToUrl(_settings.BaseUrl + "/Home/CharacterLeague");
                _wait.Until(ExpectedConditions.ElementExists(By.Id("characterLeagueList")));
            }
        }
    }
}
