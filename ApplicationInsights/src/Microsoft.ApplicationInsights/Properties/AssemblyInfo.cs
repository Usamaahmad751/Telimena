﻿using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

[assembly: ComVisible(false)]
[assembly: CLSCompliant(true)]

[assembly: InternalsVisibleTo("Microsoft.ApplicationInsights.Net45.Tests" + AssemblyInfo.PublicKey)]
[assembly: InternalsVisibleTo("Microsoft.ApplicationInsights.Net46.Tests" + AssemblyInfo.PublicKey)]
[assembly: InternalsVisibleTo("Microsoft.ApplicationInsights.netcoreapp11.Tests" + AssemblyInfo.PublicKey)]

[assembly: InternalsVisibleTo("Microsoft.ApplicationInsights.TelemetryChannel.Net45.Tests" + AssemblyInfo.PublicKey)]
[assembly: InternalsVisibleTo("Microsoft.ApplicationInsights.TelemetryChannel.NetCore.Tests" + AssemblyInfo.PublicKey)]
[assembly: InternalsVisibleTo("Microsoft.ApplicationInsights.TelemetryChannel.NetCore20.Tests" + AssemblyInfo.PublicKey)]

// Assembly dynamically generated by Moq in unit tests 
[assembly: InternalsVisibleTo("DynamicProxyGenAssembly2" + AssemblyInfo.MoqPublicKey)]
[assembly: InternalsVisibleTo("Telimena.Client, PublicKey=0024000004800000940000000602000000240000525341310004000001000100c971ad3e98a859ebdad5167d153292910c2fd4bbf59e41005addb7bf3f5fcf42552125709c9768719dfdfbc6af2cbac13aa0369b19cfc67f260b5b0f78cc8a3dee96bf2ad1b6795b9a9643d71dc2cc8b90c03a591ef1578f4cd000fce54eb7f7c8c660fd7a12089860ea2e0326b2a1e7635f17ae9c8ef0c62a54176432545ac3")]


internal static class AssemblyInfo
{
    // Public key; assemblies are delay signed or OSS signed
    public const string PublicKey = ", PublicKey=0024000004800000940000000602000000240000525341310004000001000100b5fc90e7027f67871e773a8fde8938c81dd402ba65b9201d60593e96c492651e889cc13f1415ebb53fac1131ae0bd333c5ee6021672d9718ea31a8aebd0da0072f25d87dba6fc90ffd598ed4da35e44c398c454307e8e33b8426143daec9f596836f97c8f74750e5975c64e2189f45def46b2a2b1247adc3652bf5c308055da9";
    public const string MoqPublicKey = ", PublicKey=0024000004800000940000000602000000240000525341310004000001000100c547cac37abd99c8db225ef2f6c8a3602f3b3606cc9891605d02baa56104f4cfc0734aa39b93bf7852f7d9266654753cc297e7d2edfe0bac1cdcf9f717241550e0a7b191195b7667bb4f64bcb8e2121380fd1d9d46ad2d92d2d15605093924cceaf74c4861eff62abf69b9291ed0a340e113be11e6a7d3113e92484cf7045cc7";
}