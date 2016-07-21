using System.Collections.Generic;
using System.Linq;
using System.Threading;
using NUnit.Framework;
using OpenQA.Selenium;
using OpenQA.Selenium.Interactions;
using OpenQA.Selenium.Support.UI;

namespace Warhammer.Tests.Smoke.SeleniumTests
{
    [Category("Smoke")]
    public class SmokeTests : SeleniumSetup
    {
        [Test]
        public void HitHomePage()
        {
            Driver.Navigate().GoToUrl(Settings.BaseUrl);
            Assert.IsTrue(Driver.PageSource.Contains("Home"));
        }

        [Test]
        public void HitSessions()
        {
            Driver.Navigate().GoToUrl(Settings.BaseUrl + "/Home/Sessions");
            Assert.IsTrue(Driver.PageSource.Contains("Sessions"));
        }

        [Test]
        public void HitGraveyard()
        {
            Driver.Navigate().GoToUrl(Settings.BaseUrl + "/Home/Graveyard");
            Assert.IsTrue(Driver.PageSource.Contains("The Graveyard"));
        }

        [Test]
        public void HitCharacterLeague()
        {
            Driver.Navigate().GoToUrl(Settings.BaseUrl + "/Home/CharacterLeague");
            Assert.IsTrue(Driver.PageSource.Contains("League"));
        }

        [Test]
        public void HitAFewPageTypePages()
        {
            Driver.Navigate().GoToUrl(Settings.BaseUrl);
            Wait.Until(ExpectedConditions.ElementExists(By.Id("recentActivity")));
            IWebElement list = Driver.FindElement(By.Id("recentActivity"));

            List<string> linkTexts = list.FindElements(By.TagName("a")).Select(l => l.Text).ToList();

            foreach (string linkText in linkTexts)
            {
                var element  = Driver.FindElement(By.LinkText(linkText));
                Actions actions = new Actions(Driver);
                actions.MoveToElement(element);
                ((IJavaScriptExecutor)Driver).ExecuteScript("arguments[0].scrollIntoView(true);", element);
                Thread.Sleep(500);
                element.Click();
                Wait.Until(ExpectedConditions.ElementExists(By.Id("fullName")));
                IWebElement titleElement = Driver.FindElement(By.Id("fullName"));
                Assert.IsTrue(linkText.Contains(titleElement.Text), "Title should be the front of link text");
                Driver.Navigate().GoToUrl(Settings.BaseUrl);
                Wait.Until(ExpectedConditions.ElementExists(By.Id("recentActivity")));
            }
        }

        [Test]
        public void HitTopTenTheCharacters()
        {
            Driver.Navigate().GoToUrl(Settings.BaseUrl + "/Home/CharacterLeague");
            Wait.Until(ExpectedConditions.ElementExists(By.Id("characterLeagueList")));
            IWebElement list = Driver.FindElement(By.Id("characterLeagueList"));

            List<string> linkIds = list.FindElements(By.TagName("a")).Select(l => l.GetAttribute("Id")).Where(s => s != null).Take(10).ToList();

            foreach (string linkId in linkIds)
            {
                var element = Driver.FindElement(By.Id(linkId));
                Actions actions = new Actions(Driver);
                actions.MoveToElement(element);
                ((IJavaScriptExecutor)Driver).ExecuteScript("arguments[0].scrollIntoView(true);", element);
                Thread.Sleep(500);
                element.Click();

              //  Driver.FindElement(By.Id(linkId)).Click();
                Wait.Until(ExpectedConditions.ElementExists(By.Id("fullName")));
                Assert.IsNotNull(ExpectedConditions.ElementExists(By.Id("fullName")));
                Driver.Navigate().GoToUrl(Settings.BaseUrl + "/Home/CharacterLeague");
                Wait.Until(ExpectedConditions.ElementExists(By.Id("characterLeagueList")));
            }
        }

    }
}
