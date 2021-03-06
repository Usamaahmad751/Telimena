﻿using System;
using FluentAssertions;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Text;
using DotNetLittleHelpers;
using Newtonsoft.Json;
using NUnit.Framework;
using TelimenaClient.Model;
using TelimenaClient.Serializer;

namespace TelimenaClient.Tests
{
    #region Using

    #endregion

    [TestFixture]
    public class TestSerializer
    {

        [Test]
        public void Test_Serializer()
        {
            UpdateRequest model = new UpdateRequest(Guid.Parse("dc13cced-30ea-4628-a81d-21d86f37df95"), new VersionData("1.2.0", "2.0.0"), Guid.Parse("4e80652e-d0ba-4742-a78c-3a63de4a63f0"), true, "3.2.1.3", "1.0.0.0");

            ITelimenaSerializer sut = new TelimenaSerializer();
            string stringified = sut.Serialize(model);

            UpdateRequest objectified = sut.Deserialize<UpdateRequest>(stringified);
            model.ShouldBeEquivalentTo(objectified);
        }
    }
}