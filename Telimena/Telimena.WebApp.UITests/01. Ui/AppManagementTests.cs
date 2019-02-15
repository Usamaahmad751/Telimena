﻿using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using NUnit.Framework;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using Telimena.WebApp.UiStrings;
using Telimena.WebApp.UITests.Base;
using Telimena.WebApp.UITests.Base.TestAppInteraction;
using TelimenaClient;
using TelimenaClient.Model;
using static System.Reflection.MethodBase;

namespace Telimena.WebApp.UITests._01._Ui
{
    using ExpectedConditions = SeleniumExtras.WaitHelpers.ExpectedConditions;

    [TestFixture, Order(1)]
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public partial class _1_UiTests : UiTestBase
    {
        [Test, Order(4)]
        public void _04_RegisterAutomaticTestsClient()
        {
            try
            {
                this.RegisterApp(TestAppProvider.AutomaticTestsClientAppName, AutomaticTestsClientTelemetryKey, "Telimena system tests app", "AutomaticTestsClient.exe", true, false);
            }
            catch (Exception ex)
            {
                this.HandleError(ex);
            }
        }

        [Test, Order(4)]
        public void _04b_RegisterAutomaticTestsClient_PackageUpdaterTest()
        {
            try
            {
                this.RegisterApp(TestAppProvider.PackageUpdaterTestAppName, PackageUpdaterClientTelemetryKey, 
                    "Telimena package updater test app (for apps where update package is an actual installer)", "PackageTriggerUpdaterTestApp.exe", true, false);
            }
            catch (Exception ex)
            {
                this.HandleError(ex);
            }
        }

        [Test]
        public void RegisterTempTestApp()
        {
            try
            {
                this.DeleteApp("Unit test app", true);


                this.RegisterApp("Unit test app", null, "To be deleted", "Auto test TestPlugin.dll", true, false);

                this.RegisterApp("Unit test app", null, "To be deleted", "Auto test TestPlugin.dll", true, true);

                this.DeleteApp("Unit test app", false);
            }
            catch (Exception ex)
            {
                this.HandleError(ex);
            }
            finally
            {
                var alert = this.Driver.WaitForAlert(500);
                alert?.Dismiss();
            }
        }

        private void DeleteApp(string appName, bool maybeNotExists)
        {
            this.GoToAdminHomePage();

            WebDriverWait wait = new WebDriverWait(this.Driver, TimeSpan.FromSeconds(15));
            
            IWebElement element = this.TryFind(By.Id($"{appName}_menu"));
            if (element == null )
            {
                if (maybeNotExists)
                {
                    return;

                }
                else
                {
                    Assert.Fail("Failed to find app button");
                }
            }
            
            element.Click();
            IWebElement link = wait.Until(ExpectedConditions.ElementIsVisible(By.Id($"{appName}_manageLink")));

            link.Click();
            wait.Until(ExpectedConditions.ElementIsVisible(By.Id(Strings.Id.DeleteProgramTab + "Id"))).Click();

            var deleteBtn = wait.Until(ExpectedConditions.ElementToBeClickable(By.Id(Strings.Id.DeleteProgramButton)));
            deleteBtn.Click();
            var alert = this.Driver.WaitForAlert(10000, true);
            alert.Accept();
            Thread.Sleep(2000);
            alert = this.Driver.WaitForAlert(10000, true);
            alert.SendKeys(appName);
            alert.Accept();

            Thread.Sleep(2000);

            this.Driver.Navigate().Refresh();
            wait.Until(ExpectedConditions.ElementIsVisible(By.Id(@Strings.Id.PortalSummary)));
        }

        private void RegisterApp(string name, string key, string description, string assemblyName, bool canAlreadyExist, bool hasToExistAlready)
        {
            this.GoToAdminHomePage();

            WebDriverWait wait = new WebDriverWait(this.Driver, TimeSpan.FromSeconds(15));

            this.Driver.FindElement(By.Id(Strings.Id.RegisterApplicationLink)).Click();
            wait.Until(ExpectedConditions.ElementIsVisible(By.Id(@Strings.Id.RegisterApplicationForm)));
            string autoGeneratedGuid = null;
            if (key != null)
            {
                this.Driver.FindElement(By.Id(Strings.Id.TelemetryKeyInputBox)).Clear();
                this.Driver.FindElement(By.Id(Strings.Id.TelemetryKeyInputBox)).SendKeys(key);
            }
            else
            {
                IWebElement ele = this.Driver.FindElement(By.Id(Strings.Id.TelemetryKeyInputBox));

                autoGeneratedGuid = ele.GetAttribute("value");
                Assert.AreNotEqual(Guid.Empty, Guid.Parse(autoGeneratedGuid));
            }

            this.Driver.FindElement(By.Id(Strings.Id.ProgramNameInputBox)).SendKeys(name);
            this.Driver.FindElement(By.Id(Strings.Id.ProgramDescriptionInputBox)).SendKeys(description);
            this.Driver.FindElement(By.Id(Strings.Id.PrimaryAssemblyNameInputBox)).SendKeys(assemblyName);

            this.Driver.FindElement(By.Id(Strings.Id.SubmitAppRegistration)).Click();

            
                IAlert alert = this.Driver.WaitForAlert(10000);
            if (alert != null)
            {
                if (canAlreadyExist)
                {
                    if (alert.Text != "Use different telemetry key")
                    {
                        Assert.AreEqual($"A program with name [{name}] was already registered by TelimenaSystemDevTeam", alert.Text);
                    }
                    alert.Accept();
                    return;
                }
                else
                {
                    Assert.Fail("Test scenario expects that the app does not exist");
                }
            }
            else
            {
                if (hasToExistAlready)
                {
                    Assert.Fail("The app should already exist and the error was expected");
                }
            }

            IWebElement programTable = wait.Until(ExpectedConditions.ElementIsVisible(By.Id(Strings.Id.ProgramSummaryBox)));

            var infoElements = programTable.FindElements(By.ClassName(Strings.Css.ProgramInfoElement));

            Assert.AreEqual(name, infoElements[0].Text);
            Assert.AreEqual(description, infoElements[1].Text);
            if (key != null)
            {
                Assert.AreEqual(key, infoElements[2].Text);
            }
            else
            {
                Assert.AreEqual(autoGeneratedGuid, infoElements[2].Text);
            }
            Assert.AreEqual(assemblyName, infoElements[3].Text);
        }

        [Test, Order(5)]
        public void _05_UploadTestAppUpdate()
        {

            this.UploadUpdatePackage(TestAppProvider.AutomaticTestsClientAppName, "AutomaticTestsClientv2.zip");

        }



        public void UploadUpdatePackage(string appName, string packageFileName)
        {
            try
            {

                this.LoginAdminIfNeeded();

                WebDriverWait wait = new WebDriverWait(this.Driver, TimeSpan.FromSeconds(15));

                this.Driver.FindElement(By.Id(appName + "_menu")).Click();
                IWebElement link = wait.Until(ExpectedConditions.ElementToBeClickable(By.Id(appName + "_manageLink")));
                link.Click();
                IWebElement form = wait.Until(ExpectedConditions.ElementIsVisible(By.Id(Strings.Id.UploadProgramUpdateForm)));

                FileInfo file = TestAppProvider.GetFile(packageFileName);

                IWebElement input = form.FindElements(By.TagName("input")).FirstOrDefault(x => x.GetAttribute("type") == "file");
                if (input == null)
                {
                    Assert.Fail("Input box not found");
                }

                input.SendKeys(file.FullName);

                wait.Until(x => form.FindElements(By.ClassName("info")).FirstOrDefault(e => e.Text.Contains(file.Name)));
                var notes = GetCurrentMethod().Name + DateTimeOffset.UtcNow.ToString("O");
                this.SetReleaseNotesOnPackageUpload(form,  notes);
                // ReSharper disable once PossibleNullReferenceException

                var btn = wait.Until(ExpectedConditions.ElementToBeClickable(form.FindElements(By.TagName("input"))
                    .FirstOrDefault(x => x.GetAttribute("type") == "submit")));

                btn.Click();
                this.WaitForSuccessConfirmationWithText(wait, x=>x.Contains("Uploaded package with ID"));
                this.Driver.Navigate().Refresh();
                
                //wait for the reload and verify package uploaded and notes set OK
                this.VerifyReleaseNotesOnPkg( notes);

            }
            catch (Exception ex)
            {
                this.HandleError(ex);
            }
        }

        private void VerifyReleaseNotesOnPkg(string expectedNotes)
        {
            var wait = new WebDriverWait(this.Driver, TimeSpan.FromSeconds(15));

            var table = wait.Until(ExpectedConditions.ElementIsVisible(By.Id(Strings.Id.ProgramUpdatePackagesTable)));

            var rows = table.FindElements(By.TagName("tr")).ToList();
            var id = rows[1].FindElements(By.TagName("td"))[0].Text;


            var showBtn = this.TryFind(() => table.FindElements(By.TagName("tr"))[1].FindElement(By.ClassName("expand"))
                , TimeSpan.FromSeconds(15));
            Assert.IsNotNull(showBtn);
            try
            {
                wait.Until(ExpectedConditions.ElementToBeClickable(showBtn));
            }
            catch (Exception ex)
            {

            }
            showBtn.Click();
            IWebElement notesSection =
                this.TryFind(
                    () => table.FindElements(By.TagName("pre")).ToList()
                        .FirstOrDefault(x => x.GetAttribute("data-pkg-id") == id), TimeSpan.FromSeconds(1));


            Assert.AreEqual(expectedNotes, notesSection.Text);
        }


        private void SetReleaseNotesOnExistingPkg(string notes)
        {
            var wait = new WebDriverWait(this.Driver, TimeSpan.FromSeconds(15));
            var table = wait.Until(ExpectedConditions.ElementIsVisible(By.Id(Strings.Id.ProgramUpdatePackagesTable)));

            var rows = table.FindElements(By.TagName("tr")).ToList();
            var setBtn = wait.Until(ExpectedConditions.ElementToBeClickable(table.FindElements(By.TagName("tr"))[1].FindElement(By.ClassName(Strings.Css.PrepareReleaseNotesButton))));

            setBtn.Click();

            this.FillInReleaseNotesModal(wait, notes);
        }

        private void SetReleaseNotesOnPackageUpload(IWebElement form, string notes)
        {
            var wait = new WebDriverWait(this.Driver, TimeSpan.FromSeconds(15));

            var btn = wait.Until(ExpectedConditions.ElementToBeClickable(
                form.FindElement(By.ClassName(@Strings.Css.PrepareReleaseNotesButton))));

            btn.Click();

            this.FillInReleaseNotesModal(wait, notes);
        }

        private void FillInReleaseNotesModal(WebDriverWait wait, string notes)
        {

            var modal = wait.Until(ExpectedConditions.ElementIsVisible(By.Id(Strings.Id.SetReleaseNotesModal)));

            var area = modal.FindElement(By.TagName("textarea"));
            area.Clear();
            area.SendKeys(notes);

            var submit = wait.Until(ExpectedConditions.ElementToBeClickable(By.Id(Strings.Id.SubmitReleaseNotesButton)));
            submit.Click();
        }


        [Test, Order(5)]
        public void _05b_UploadPackageUpdateTestAppUpdate()
        {
            this.UploadUpdatePackage(TestAppProvider.PackageUpdaterTestAppName, "PackageTriggerUpdaterTestApp v.2.0.0.0.zip");
        }

        [Test, Order(6)]
        public void _06_SetUpdaterForPackageTriggerApp()
        { 

            var app = TestAppProvider.PackageUpdaterTestAppName;
            var currentUpdater = this.GetUpdaterForApp(app);

            if (currentUpdater == DefaultToolkitNames.PackageTriggerUpdaterInternalName)
            {
                this.SetUpdaterForApp(app, DefaultToolkitNames.UpdaterInternalName);
                Assert.AreEqual(DefaultToolkitNames.UpdaterInternalName, this.GetUpdaterForApp(app));

                this.SetUpdaterForApp(app, DefaultToolkitNames.PackageTriggerUpdaterInternalName);
                Assert.AreEqual(DefaultToolkitNames.PackageTriggerUpdaterInternalName, this.GetUpdaterForApp(app));
            }
            else
            {
                this.SetUpdaterForApp(app, DefaultToolkitNames.PackageTriggerUpdaterInternalName);
                Assert.AreEqual(DefaultToolkitNames.PackageTriggerUpdaterInternalName, this.GetUpdaterForApp(app));
            }
        }

        [Test, Order(7)]
        public void _07_SetNotesOnExistingPackage()
        {
            try
            {

                this.LoginAdminIfNeeded();

                WebDriverWait wait = new WebDriverWait(this.Driver, TimeSpan.FromSeconds(15));

                this.Driver.FindElement(By.Id(TestAppProvider.AutomaticTestsClientAppName + "_menu")).Click();
                IWebElement link = wait.Until(ExpectedConditions.ElementToBeClickable(By.Id(TestAppProvider.AutomaticTestsClientAppName + "_manageLink")));
                link.Click();
                var notes = GetCurrentMethod().Name + DateTimeOffset.UtcNow.ToString("O");

                this.SetReleaseNotesOnExistingPkg( notes);

                this.WaitForSuccessConfirmationWithText(wait, z=>z.Contains("Updated release notes."));

                wait.Until(ExpectedConditions.InvisibilityOfElementLocated(By.Id(Strings.Id.TopAlertBox)));

                //wait for the reload and verify package uploaded and notes set OK
                this.VerifyReleaseNotesOnPkg( notes);

            }
            catch (Exception ex)
            {
                this.HandleError(ex);
            }

        }


        private string GetUpdaterForApp(string appName)
        {
            try
            {
                this.LoginAdminIfNeeded();

                WebDriverWait wait = new WebDriverWait(this.Driver, TimeSpan.FromSeconds(15));

                IWebElement link = wait.Until( ExpectedConditions.ElementToBeClickable(By.Id(appName + "_menu")));

                link.Click();

                link = wait.Until(ExpectedConditions.ElementToBeClickable(By.Id(appName+ "_manageLink")));
                link.Click();
                IWebElement form = wait.Until(ExpectedConditions.ElementIsVisible(By.Id(Strings.Id.ProgramSummaryBox)));

                IWebElement input = this.Driver.FindElement(By.Id(Strings.Id.UpdaterSelectList));
                if (input == null)
                {
                    Assert.Fail("Select list box not found");
                }

                return new SelectElement(input).SelectedOption.Text;

            }
            catch (Exception ex)
            {
                this.HandleError(ex);
                throw;

            }
        }

        private void SetUpdaterForApp(string appName, string updaterName)
        {
            try
            {
                this.LoginAdminIfNeeded();

                WebDriverWait wait = new WebDriverWait(this.Driver, TimeSpan.FromSeconds(15));

                this.Driver.FindElement(By.Id(appName+ "_menu")).Click();
                IWebElement link =
                    wait.Until(ExpectedConditions.ElementIsVisible(By.Id(appName+ "_manageLink")));
                link.Click();
                IWebElement input = wait.Until(ExpectedConditions.ElementIsVisible(By.Id(Strings.Id.UpdaterSelectList)));

                if (input == null)
                {
                    Assert.Fail("Select list box not found");
                }

                new SelectElement(input).SelectByText(updaterName);
                this.Driver.FindElement(By.Id(Strings.Id.SubmitUpdaterChange)).Click();

                this.WaitForSuccessConfirmationWithText(wait, x=>x.Contains("Updater set to "+ updaterName));
            }
            catch (Exception ex)
            {
                this.HandleError(ex);
            }
        }
    }


}