﻿using System;
using System.IO;
using System.IO.Compression;
using System.Threading;
using System.Windows;

namespace Telimena.Updater
{
    public class UpdateWorker
    {
        public void PerformUpdate(UpdateInstructions instructions)
        {
            foreach (string packagePath in instructions.PackagePaths)
            {
                this.ProcessPackage(new FileInfo(packagePath));
            }
        }

        private static bool PrepareAndValidatePackage(FileInfo package, string updDirPath)
        {
            if (Directory.Exists(updDirPath))
            {
                Directory.Delete(updDirPath, true);
            }

            try
            {
                ZipFile.ExtractToDirectory(package.FullName, updDirPath);
            }
            catch (Exception ex)
            {
                //MoveTheWindowUp();
                MessageBox.Show(
                    "Error occurred while unpacking update package. Exception:\n" + ex.Message + "\nUpdate package will be removed and re-downloaded."
                    , "Update problem");
                // TopInfoLabel = "Updater - error - update canceled";
                try
                {
                    File.Delete(package.FullName);
                    // TopInfoLabel = "Updater - error - update canceled. Package deleted.";
                }
                catch (Exception ex2)
                {
                    MessageBox.Show("Error occurred while deleting update package. Exception:\n" + ex2.Message, "Update problem");
                }

                Thread.Sleep(1500);
                return false;
            }

            return true;
        }

        private void ProcessPackage(FileInfo package)
        {
            // TopInfoLabel = "Updater - unpacking...";
            // Thread.Sleep(700);
            string updatePackageFolderPath = package.FullName.Replace(".zip", "");
            DirectoryInfo programFolder = Directory.GetParent(updatePackageFolderPath).Parent.Parent;
            if (!PrepareAndValidatePackage(package, updatePackageFolderPath))
            {
                return;
            }

            FileReplacer.ReplaceFiles(programFolder, package, new DirectoryInfo(updatePackageFolderPath));

            // TopInfoLabel = "Updater - extracting...";
            //Thread.Sleep(700);
            if (Directory.Exists(updatePackageFolderPath))
            {
                Directory.Delete(updatePackageFolderPath, true);
            }

            //TopInfoLabel = "Updater - cleaning...";
            //Thread.Sleep(700);
        }
    }
}