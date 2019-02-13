﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NUnit.Framework;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Firefox;
using OpenQA.Selenium.IE;
using OpenQA.Selenium.Remote;
using OpenQA.Selenium.Support.PageObjects;
using OpenQA.Selenium.Support.UI;
using Telimena.WebApp.UiStrings;
using Assert = NUnit.Framework.Assert;
using ExpectedConditions = SeleniumExtras.WaitHelpers.ExpectedConditions;
using TestContext = NUnit.Framework.TestContext;

namespace Telimena.WebApp.UITests.Base
{
    [TestFixture]
    public abstract class UiTestBase : IntegrationTestBase
    {

        public void WaitForSuccessConfirmationWithText(WebDriverWait wait, Func<string, bool> validateText)
        {
            this.WaitForSuccessConfirmationWithTextAndClass(wait, "success", validateText);
        }

        public void WaitForErrorConfirmationWithText(WebDriverWait wait, Func<string, bool> validateText)
        {
            this.WaitForSuccessConfirmationWithTextAndClass(wait, "danger", validateText);
        }

        public void WaitForSuccessConfirmationWithTextAndClass(WebDriverWait wait, string cssPart, Func<string, bool> validateText)
        {
            var confirmationBox = wait.Until(ExpectedConditions.ElementIsVisible(By.ClassName(Strings.Css.TopAlertBox)));

            Assert.IsTrue(confirmationBox.GetAttribute("class").Contains(cssPart), "The alert has incorrect class: " + confirmationBox.GetAttribute("class"));
            Assert.IsTrue(validateText(confirmationBox.Text), "Incorrect message: " + confirmationBox.Text);
        }

        internal static Lazy<RemoteWebDriver> RemoteDriver = new Lazy<RemoteWebDriver>(() => GetBrowser("Chrome"));

        private static RemoteWebDriver GetBrowser(string browser)
        {
            switch (browser)
            {
                case "Chrome":
                    ChromeOptions opt = new ChromeOptions();
#if DEBUG

#else
                opt.AddArgument("--headless");
#endif
                    return new ChromeDriver(opt);
                case "Firefox":
                    return new FirefoxDriver();
                case "IE":
                    return new InternetExplorerDriver();
                default:
                    return new ChromeDriver();
            }
        }

        internal IWebDriver Driver => RemoteDriver.Value;
        internal ITakesScreenshot Screenshooter => this.Driver as ITakesScreenshot;

        public void GoToAdminHomePage()
        {
            try
            {
                this.Driver.Navigate().GoToUrl(this.GetAbsoluteUrl(""));
                this.LoginAdminIfNeeded();
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Error while logging in admin", ex);
            }
        }

        public string GetAbsoluteUrl(string relativeUrl)
        {
            return this.TestEngine.GetAbsoluteUrl(relativeUrl);
        }

        public void RecognizeAdminDashboardPage()
        {
            WebDriverWait wait = new WebDriverWait(this.Driver, TimeSpan.FromSeconds(15));
            if (this.Driver.Url.Contains("ChangePassword"))
            {
                this.Log("Going from change password page to Admin dashboard");
                this.Driver.Navigate().GoToUrl(this.GetAbsoluteUrl(""));
            }

            wait.Until(x => x.FindElement(By.Id(Strings.Id.PortalSummary)));
        }

        public IWebElement TryFind(string nameOrId, int timeoutSeconds = 10)
        {
            try
            {
                WebDriverWait wait = new WebDriverWait(this.Driver, TimeSpan.FromSeconds(timeoutSeconds));
                return wait.Until(x => x.FindElement(By.Id(nameOrId)));
            }
            catch
            {
            }

            return null;
        }

        protected IWebElement TryFind(By by, int timeoutSeconds = 10)
        {
            try
            {
                WebDriverWait wait = new WebDriverWait(this.Driver, TimeSpan.FromSeconds(timeoutSeconds));
                return wait.Until(x => x.FindElement(by));
            }
            catch
            {
            }

            return null;
        }

        protected IWebElement TryFind(Func<IWebElement> finderFunc, TimeSpan timeout)
        {
            Stopwatch sw = Stopwatch.StartNew();
            while (sw.ElapsedMilliseconds < timeout.TotalMilliseconds)
            {
                try
                {
                    IWebElement result = finderFunc();
                    if (result != null)
                    {
                        return result;
                    }
                }
                catch
                {
                    //np
                }
            }

            return null;
        }

        protected bool IsLoggedIn()
        {
            if (this.TryFind(Strings.Id.MainHeader) != null)
            {
                return true;
            }

            return false;
        }

        protected void HandleError(Exception ex, List<string> outputs = null, List<string> errors = null, [CallerMemberName] string memberName = "")
        {
            Screenshot screen = this.Screenshooter.GetScreenshot();
            string path = Common.CreatePngPath(memberName);
            screen.SaveAsFile(path, ScreenshotImageFormat.Png);
            string page = this.Driver.PageSource;

            string errorOutputs = "";
            if (errors != null)
            {
                errorOutputs = string.Join("\r\n", errors);
            }

            string normalOutputs = "";
            if (outputs != null)
            {
                normalOutputs = string.Join("\r\n", outputs);
            }

            IAlert alert = this.Driver.WaitForAlert(500);
            alert?.Dismiss();
            throw new AssertFailedException($"{ex}\r\n\r\n{this.PresentParams()}\r\n\r\n{errorOutputs}\r\n\r\n{normalOutputs}\r\n\r\n{page}", ex);

            //this.TestContext.AddResultFile(path);
        }

        private string PresentParams()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("Url: " + this.Driver.Url);
            sb.Append("TestContext Parameters: ");
            foreach (string testParameter in TestContext.Parameters.Names)
            {
                sb.Append(testParameter + ": " + TestContext.Parameters[testParameter] + " ");
            }

            return sb.ToString();
        }

        public void LoginAdminIfNeeded()
        {
            this.LoginIfNeeded(this.AdminName, this.AdminPassword);
        }

        private void LoginIfNeeded(string userName, string password)
        {
            if (!this.IsLoggedIn())
            {
                this.Driver.Navigate().GoToUrl(this.GetAbsoluteUrl("Account/Login"));
            }

            if (this.Driver.Url.IndexOf("Login", StringComparison.InvariantCultureIgnoreCase) != -1 &&
                this.Driver.FindElement(new ByIdOrName(Strings.Id.LoginForm)) != null)
            {
                this.Log("Trying to log in...");
                IWebElement login = this.Driver.FindElement(new ByIdOrName(Strings.Id.Email));

                if (login != null)
                {
                    IWebElement pass = this.Driver.FindElement(new ByIdOrName(Strings.Id.Password));
                    login.SendKeys(userName);
                    pass.SendKeys(password);
                    IWebElement submit = this.Driver.FindElement(new ByIdOrName(Strings.Id.SubmitLogin));
                    submit.Click();
                    this.GoToAdminHomePage();
                    this.RecognizeAdminDashboardPage();
                }
            }
            else
            {
                this.Log("Skipping logging in");
            }
        }
    }
}