﻿using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Threading.Tasks;

namespace Telimena.WebApp.Infrastructure.Repository.FileStorage
{
    public class AssemblyVersionReader : IAssemblyVersionReader
    {
        public async Task<string> GetFileVersion(Stream stream, string expectedFileName, bool expectSingleFile)
        {
            string tempFilePath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            using (Stream file = File.Create(tempFilePath))
            {
                await stream.CopyToAsync(file);
            }

            var unzippedPath = this.GetUnzippedPath(tempFilePath, expectedFileName, expectSingleFile);


            var version = FileVersionInfo.GetVersionInfo(unzippedPath).FileVersion;
            if (string.IsNullOrEmpty(version))
            {
                throw new InvalidOperationException("Cannot extract version info from uploaded file");
            }

            return version;
        }

        public async Task<string> GetVersionFromPackage(string assemblyName, Stream fileStream, bool required = true)
        {
            var zipPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString(), "package.zip");
            Directory.CreateDirectory(Path.GetDirectoryName(zipPath));
            using (FileStream fs = new FileStream(zipPath, FileMode.Create))
            {
                await fileStream.CopyToAsync(fs);
            }

            fileStream.Seek(0, SeekOrigin.Begin);

            ZipFile.ExtractToDirectory(zipPath, Path.GetDirectoryName(zipPath));

            foreach (string file in Directory.GetFiles(Path.GetDirectoryName(zipPath), "*", SearchOption.AllDirectories))
            {
                if (Path.GetFileName(file).Equals(assemblyName, StringComparison.InvariantCultureIgnoreCase) || Path.GetFileNameWithoutExtension(file).Equals(assemblyName, StringComparison.InvariantCultureIgnoreCase))
                {
                    return FileVersionInfo.GetVersionInfo(file).FileVersion;
                }
            }

            if (required)
            {
                throw new FileNotFoundException( $"Failed to find the required assembly in the uploaded package. [{assemblyName}] should be present.", assemblyName);
            }

            return null;
        }


        private string GetUnzippedPath(string maybeZipPath, string expectedFileName, bool expectSingleFile)
        {
            string extractedFolderPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            Directory.CreateDirectory(extractedFolderPath);
            try
            {
                ZipFile.ExtractToDirectory(maybeZipPath, extractedFolderPath);
               
            }
            catch (Exception)
            {
                return maybeZipPath;
            }
            var path = Path.Combine(extractedFolderPath, expectedFileName);
            if (!File.Exists(path))
            {
                throw new InvalidOperationException($"The uploaded package does not contain expected file: {expectedFileName}.");
            }

            if (expectSingleFile)
            {
                if (Directory.GetFiles(extractedFolderPath).Length != 1)
                {
                    throw new InvalidOperationException($"The uploaded package contains more than one file.");
                }
            }
            return path;
        }
    }
}