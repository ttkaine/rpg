using System;
using System.Diagnostics;
using System.IO;
using NUnit.Framework;
using OpenQA.Selenium;
using OpenQA.Selenium.Firefox;
using OpenQA.Selenium.Support.PageObjects;
using OpenQA.Selenium.Support.UI;

namespace Warhammer.Tests.Smoke.SeleniumTests
{
    [TestFixture]
    [Category("Selenium")]
    public class SeleniumSetup
    {
        protected IWebDriver Driver;
        protected TestSettings Settings;
        private TestSettingType _settingsType;

        [Conditional("Dev")]
        private void SetDevSettings()
        {
            _settingsType = TestSettingType.Dev;
        }

        [Conditional("CI01")]
        private void SetCiSettings()
        {
            _settingsType = TestSettingType.Ci;
        }

        [Conditional("Test")]
        private void SetTestSettings()
        {
            _settingsType = TestSettingType.Test;
        }

        [Conditional("Live")]
        private void SetLiveSettings()
        {
            _settingsType = TestSettingType.Live;
        }

        [Conditional("Pirate")]
        private void SetPirateSettings()
        {
            _settingsType = TestSettingType.Pirate;
        }

        [Conditional("Space")]
        private void SetSpaceSettings()
        {
            _settingsType = TestSettingType.Space;
        }

        private WebDriverWait _wait;

        public WebDriverWait Wait
        {
            get
            {
                if (_wait == null)
                {
                    _wait = new WebDriverWait(Driver, new TimeSpan(0, 0, 0, 10));
                }
                return _wait;
            }
        }

        [TestFixtureSetUp]
        public void FixtureSetup()
        {
            try
            {

                ClearScreenShotFolder();
                SetDevSettings();
                SetCiSettings();
                SetTestSettings();
                SetLiveSettings();
                SetPirateSettings();
                SetSpaceSettings();
                Settings = new TestSettings();
                Assert.IsTrue(Settings.LoadSettings(_settingsType),
                    "We're not going to get far if we can't even load our settings file...  The expected location of the Settings file is in the App.Config for the test project, you may need to create this file and put some settings into it - it's not in Sauce Control. ZONE:" + _settingsType);
                Debug.WriteLine("Settings Type: {0}", _settingsType);
                Debug.WriteLine("Base URL : {0}", Settings.BaseUrl);

                Driver = new FirefoxDriver();
                Driver.FindElement(By.TagName("html")).SendKeys(Keys.F11);
                Driver.Manage().Timeouts().ImplicitlyWait(new TimeSpan(0, 0, 30));
                Driver.Navigate().GoToUrl(Settings.UpdateUrl);

                Wait.Until(ExpectedConditions.TitleContains("Identify yourself."));

                IWebElement userNameField = Driver.FindElement(new ByIdOrName("Email"));
                IWebElement passwordField = Driver.FindElement(new ByIdOrName("Password"));

                userNameField.SendKeys(Settings.DefaultUsername);
                passwordField.SendKeys(Settings.DefaultPassword);

                passwordField.Submit();
                TakeScreenshot("Db_Update");
                Assert.IsFalse(Driver.PageSource.Contains("<h2>There has been an error of some kind. Abort.</h2>"),
                    "Error getting database updates to run");
                Assert.IsTrue(Driver.PageSource.Contains("Database Update Done"),
                    "Should get a success message from the database update page");
            }
            catch (Exception ex)
            {
                FixtureTearDown();
                throw new Exception("Error setting up Selenium Tests", ex);
            }
        }

        private static void ClearScreenShotFolder()
        {
            DirectoryInfo directory = new DirectoryInfo(ScreenshotLocation);
            if (directory.Exists)
            {
                foreach (FileInfo file in directory.GetFiles())
                {
                    file.Delete();
                }
                foreach (DirectoryInfo dir in directory.GetDirectories())
                {
                    dir.Delete(true);
                }
            }
        }

        [TestFixtureTearDown]
        public void FixtureTearDown()
        {
            try
            {
                if (Driver != null)
                {
                    Driver.Close();
                    Driver.Quit();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error in Fixture Tear Down");
                Console.WriteLine(ex.Message);
                Console.WriteLine(ex.StackTrace);
            }
        }

        [TearDown]
        public void Down()
        {
            if (TestContext.CurrentContext.Result.Status == TestStatus.Failed)
            {
                Console.WriteLine("Test Failed");
                Console.WriteLine("Take Screen Shot...");
                TakeScreenshot("TEST_FAIL");
            }
        }

        public void TakeScreenshot(string name)
        {
            //Grab a screenshot for TeamCity
            try
            {
                Screenshot screenshot = ((ITakesScreenshot)Driver).GetScreenshot();

                if (!Directory.Exists(ScreenshotLocation))
                {
                    Directory.CreateDirectory(ScreenshotLocation);
                }
                string screenshotFilename = string.Format("{0}_{1}.jpg", name, TestContext.CurrentContext.Test.Name);
                screenshot.SaveAsFile(Path.Combine(ScreenshotLocation, screenshotFilename), System.Drawing.Imaging.ImageFormat.Jpeg);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }

        }

        private static string ScreenshotLocation
        {
            get
            {
                string location = AppDomain.CurrentDomain.BaseDirectory;
                location = Path.Combine(location, "screenshots");
                string fixture = TestContext.CurrentContext.Test.FullName.Replace("Lwap.Tests.SeleniumTests.", "");
                if (fixture.Contains("."))
                {
                    fixture = fixture.Substring(0, fixture.IndexOf('.'));
                }
                location = Path.Combine(location, fixture);
                return location;
            }
        }

        protected enum ContextSwitchState
        {
            Me,
            All
        }

        protected void SetContextSwitch(ContextSwitchState requestedState)
        {
            IWebElement contextSwitch = Driver.FindElement(By.ClassName("globalScopeSwitchBezel"));
            string mode = contextSwitch.GetAttribute("mode");

            if (requestedState.ToString().ToLower() != mode)
            {
                contextSwitch.Click();
            }

            Driver.Navigate().GoToUrl(Settings.BaseUrl);
        }

        [Test]
        public void TestReality()
        {
            Assert.IsTrue(true);
        }
    }
}
