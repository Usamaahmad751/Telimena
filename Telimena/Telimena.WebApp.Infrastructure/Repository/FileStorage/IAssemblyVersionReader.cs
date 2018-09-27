﻿using System.IO;
using System.Threading.Tasks;

namespace Telimena.WebApp.Infrastructure.Repository.FileStorage
{
    public interface IAssemblyVersionReader
    {
        Task<string> GetFileVersion(Stream stream, string expectedFileName, bool expectSingleFile);
        Task<string> GetVersionFromPackage(string assemblyName, Stream fileStream, bool required = true);
    }
}