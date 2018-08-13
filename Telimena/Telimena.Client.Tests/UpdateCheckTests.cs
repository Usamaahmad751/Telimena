﻿// -----------------------------------------------------------------------
//  <copyright file="ClientTests.cs" company="SDL plc">
//   Copyright (c) SDL plc. All rights reserved.
//  </copyright>
// -----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Xml.Linq;
using Moq;
using Newtonsoft.Json;
using NUnit.Framework;

namespace Telimena.Client.Tests
{
    #region Using

    #endregion

    [TestFixture]
    public class TestUpdateChecks
    {
        private Mock<ITelimenaHttpClient> GetMockClientForCheckForUpdates(object responseObj)
        {
            Mock<ITelimenaHttpClient> client = new Mock<ITelimenaHttpClient>();
            client.Setup(x => x.PostAsync("api/Statistics/RegisterClient", It.IsAny<HttpContent>())).Returns((string uri, HttpContent requestContent) =>
            {
                HttpResponseMessage response = new HttpResponseMessage();
                RegistrationResponse registrationResponse = new RegistrationResponse
                {
                    Count = 0,
                    ProgramId = 1,
                    UserId = 2
                };
                response.Content = new StringContent(JsonConvert.SerializeObject(registrationResponse));
                return Task.FromResult(response);
            });
            client.Setup(x => x.GetAsync(It.IsAny<string>())).Returns((string uri) =>
            {
                HttpResponseMessage response = new HttpResponseMessage();

                response.Content = new StringContent(JsonConvert.SerializeObject(responseObj));
                return Task.FromResult(response);
            });
            return client;
        }


        [Test]
        public void Test_CheckForUpdates()
        {
            Client.Telimena sut = new Client.Telimena
            {
                SuppressAllErrors = false
            };
            Assert.AreEqual("Telimena.Client", sut.ProgramInfo.PrimaryAssembly.Name);

            sut.LoadHelperAssembliesByName("Telimena.Client.Tests.dll", "Moq.dll");

            UpdateResponse latestVersionResponse = new UpdateResponse
            {
                UpdatePackages = new List<UpdatePackageData>
                {
                    new UpdatePackageData {FileSizeBytes = 666, Id = 10001, IsStandalone = true},
                    new UpdatePackageData {FileSizeBytes = 666, Id = 10002}
                }
            };
            latestVersionResponse.UpdatePackagesIncludingBeta = new List<UpdatePackageData>(latestVersionResponse.UpdatePackages);
            Helpers.SetupMockHttpClient(sut, this.GetMockClientForCheckForUpdates(latestVersionResponse));

            UpdateCheckResult response = sut.CheckForUpdates().GetAwaiter().GetResult();
            Assert.IsTrue(response.IsUpdateAvailable);
            //Assert.AreEqual("3.1.0.0", response.PrimaryAssemblyUpdateInfo.LatestVersionInfo.LatestVersion);
            //Assert.AreEqual("3.1.0.1", response.HelperAssembliesToUpdate.Single().LatestVersionInfo.LatestVersion);
        }


        [Test]
        public void Test_PackageSorting()
        {
            List<UpdatePackageData> packages = new List<UpdatePackageData>
            {
                new UpdatePackageData {Id = 4, Version = "3.5"},
                new UpdatePackageData {Id = 2, Version = "2.0"},
                new UpdatePackageData {Id = 1, Version = "1.0"},
                new UpdatePackageData {Id = 3, Version = "3.0"}
            };

            List<UpdatePackageData> ordered = UpdateInstructionCreator.Sort(packages);
            Assert.AreEqual(1, ordered[0].Id);
            Assert.AreEqual(2, ordered[1].Id);
            Assert.AreEqual(3, ordered[2].Id);
            Assert.AreEqual(4, ordered[3].Id);
        }

        [Test]
        public void Test_UpdateInstructionCreator()
        {
            List<UpdatePackageData> packages = new List<UpdatePackageData>
            {
                new UpdatePackageData {Id = 4, Version = "3.5", StoredFilePath = @"C:\AppFolder\Updates\Update v 3.5\Update v 3.5.zip"},
                new UpdatePackageData {Id = 2, Version = "2.0", StoredFilePath = @"C:\AppFolder\Updates\Update v 3.5\Update v 2.0.zip"},
                new UpdatePackageData {Id = 1, Version = "1.0", StoredFilePath = @"C:\AppFolder\Updates\Update v 3.5\Update v 1.0.zip"},
                new UpdatePackageData {Id = 3, Version = "3.0", StoredFilePath = @"C:\AppFolder\Updates\Update v 3.5\Update v 3.0.zip"}
            };

            Tuple<XDocument, FileInfo> tuple = UpdateInstructionCreator.CreateXDoc(packages);
            XDocument xDoc = tuple.Item1;
            FileInfo file = tuple.Item2;
            Assert.AreEqual(@"C:\AppFolder\Updates\Update v 3.5\UpdateInstructions.xml", file.FullName);
            Assert.AreEqual(@"C:\AppFolder\Updates\Update v 3.5\Update v 1.0.zip", xDoc.Root.Elements().ElementAt(0).Value);
            Assert.AreEqual(@"C:\AppFolder\Updates\Update v 3.5\Update v 2.0.zip", xDoc.Root.Elements().ElementAt(1).Value);
            Assert.AreEqual(@"C:\AppFolder\Updates\Update v 3.5\Update v 3.0.zip", xDoc.Root.Elements().ElementAt(2).Value);
            Assert.AreEqual(@"C:\AppFolder\Updates\Update v 3.5\Update v 3.5.zip", xDoc.Root.Elements().ElementAt(3).Value);
        }
    }
}