using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace Voodoo.Sauce.Internal.Editor
{
    public static class AutoUpdatePackageInstaller
    {
        private const string PackageFolder = "Assets/VoodooSaucePackage/";
        private const string PackageFile = PackageFolder + "VoodooSauce.package";

        public delegate void OnPackageInstalled();

        public static void DownloadAndInstallPackage(Package package, OnPackageInstalled callback)
        {
            if (!Directory.Exists(PackageFolder))
                Directory.CreateDirectory(PackageFolder);

            Debug.LogFormat("Downloading {0} to {1}", package.PackageUrl, PackageFile);

            // Start the async download job.
            var downloader = new WebClient {Encoding = Encoding.UTF8};
            string info = $"Downloading {"Package " + package.Version}...";
            downloader.DownloadProgressChanged += (sender, args) => {
                UnityEditor.EditorUtility.DisplayCancelableProgressBar("Package update…", info, args.ProgressPercentage / 100.0f);
            };
            downloader.DownloadFileCompleted += (sender, args) => { DownloadPackageCompleted(args.Error, package, callback); };

            downloader.DownloadFileAsync(new Uri(package.PackageUrl), PackageFile);
        }

        private static void DownloadPackageCompleted(Exception error, Package package, OnPackageInstalled callback)
        {
            // Reset async state so the GUI is operational again.
            UnityEditor.EditorUtility.ClearProgressBar();

            if (error != null) {
                OnError(package, callback);
            } else {
                InstallPackage(package, callback);
            }
        }

        private static void OnError(Package package, OnPackageInstalled callback)
        {
            if (UnityEditor.EditorUtility.DisplayDialog("An error occured", package.FailedMessage,
                package.UpdateInstructionsUrl != null ? "Do it manually" : null, "Retry")) {
                if (package.UpdateInstructionsUrl != null) Application.OpenURL(package.UpdateInstructionsUrl);
            } else {
                DownloadAndInstallPackage(package, callback);
            }
        }

        private static void InstallPackage(Package package, OnPackageInstalled callback)
        {
            RemoveFilesBeforeInstall(package);

            AssetDatabase.importPackageCompleted += delegate { ImportPackageCompleted(package, callback); };
            AssetDatabase.importPackageFailed += delegate { ImportPackageFailed(package, callback); };
            AssetDatabase.importPackageCancelled += delegate { RemovePackageFolder(); };

            if (File.Exists(PackageFile)) {
                AssetDatabase.ImportPackage(PackageFile, false);
            } else {
                ImportPackageFailed(package, callback);
            }
        }

        private static void RemoveFilesBeforeInstall(Package package)
        {
            foreach (string directory in package.DirectoriesToRemove.Where(Directory.Exists)) {
                Directory.Delete(directory, true);
            }
        }

        private static void ImportPackageCompleted(Package package, OnPackageInstalled callback)
        {
            RemovePackageFolder();
            callback?.Invoke();
            if (package.UpdateInstructionsUrl != null) {
                if (UnityEditor.EditorUtility.DisplayDialog("Install success", package.SuccessMessage, "Show update instructions", "Cancel")) {
                    Application.OpenURL(package.UpdateInstructionsUrl);
                }
            } else {
                UnityEditor.EditorUtility.DisplayDialog("Install success", package.SuccessMessage, "Cancel");
            }
        }

        private static void ImportPackageFailed(Package package, OnPackageInstalled callback)
        {
            RemovePackageFolder();
            OnError(package, callback);
        }

        private static void RemovePackageFolder()
        {
            if (Directory.Exists(PackageFolder)) Directory.Delete(PackageFolder, true);
        }
    }
}