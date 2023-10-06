using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using UnityEngine;

namespace Voodoo.Sauce.Internal.Editor
{
    public static class FileUtility
    {
        private const string TAG = "FileUtility";
        
        // The path separator could be different regarding the OS.
        public static readonly char PathSeparator = Path.DirectorySeparatorChar;

        // Creates a directory if it does not exist.
        public static void CreateDirectory(string path)
        {
            if (string.IsNullOrEmpty(path)) {
                throw new Exception("Directory path can not be empty or null");
            }

            if (Directory.Exists(path)) return;
            
            Directory.CreateDirectory(path);
            Debug.Log($"Directory created at '{path}'");
        }

        // Removes a directory and all its contents.
        public static void DeleteDirectory(string path)
        {
            if (!Directory.Exists(path)) return;
            
            Directory.Delete(path, true);
            Debug.Log($"Directory deleted at '{path}'");
        }
        
        // Moves the directories from a source directory to a destination directory.
        public static void MoveDirectories(string source, string destination)
        {
            var directoryPaths = new List<string>(Directory.EnumerateDirectories(source));
            
            foreach (string directoryPath in directoryPaths) {
                var directoryInfo = new DirectoryInfo(directoryPath);
                string directoryName = directoryInfo.Name;
                string destinationDirectoryPath = Path.Combine(destination, directoryName);
                CreateDirectory(destinationDirectoryPath);
                    
                var filePaths = new List<string>(Directory.EnumerateFiles(directoryPath));
                foreach (string filePath in filePaths) {
                    var fileInfo = new FileInfo(filePath);
                    string fileName = fileInfo.Name;
                    string destinationFilePath = Path.Combine(destinationDirectoryPath, fileName);
                    MoveFile(filePath, destinationFilePath);
                }
            }
        }

        // Writes some contents to a file.
        public static void WriteFile(string data, string path)
        {
            string directory = Path.GetDirectoryName(path);
            CreateDirectory(directory);
            
            File.WriteAllText(path, data);
            Debug.Log($"File created at '{path}'");
        }

        // Moves a file from its source path to its destination path.
        public static void MoveFile(string sourcePath, string destinationPath)
        {
            if (!File.Exists(sourcePath)) {
                throw new Exception("File does not exist");
            }
            
            if (File.Exists(destinationPath)) {
                File.Delete(destinationPath);
                Debug.Log($"File deleted at '{destinationPath}'");
            }
            
            File.Move(sourcePath, destinationPath);
            Debug.Log($"File moved from '{sourcePath}' to '{destinationPath}'");
        }

        // Copies a file.
        public static void CopyFile(string sourcePath, string destinationPath)
        {
            if (!File.Exists(sourcePath)) {
                Debug.Log($"File does not exist at '{sourcePath}'");
                return;
            }
            
            string destinationDirectory = Path.GetDirectoryName(destinationPath);
            CreateDirectory(destinationDirectory);
            
            File.Copy(sourcePath, destinationPath);
            Debug.Log($"File copied from '{sourcePath}' to '{destinationPath}'");
        }

        // Deletes a file.
        public static void DeleteFile(string path)
        {
            if (!File.Exists(path)) return;
            
            File.Delete(path);
            Debug.Log($"File deleted at '{path}'");
        }
        
        // Deletes the files from a list.
        public static void DeleteFiles(IEnumerable<string> files)
        {
            foreach (string file in files.Where(File.Exists)) {
                DeleteFile(file);
            }
        }

        // Calculates and returns the md5 checksum of a file.
        public static string FileMD5(string filePath)
        {
            try {
                var md5 = MD5.Create();
                FileStream stream = File.OpenRead(filePath);
            
                return BitConverter.ToString(md5.ComputeHash(stream)).Replace("-", "").ToLowerInvariant();
            } catch (Exception e) {
                VoodooLog.LogError(Module.COMMON, TAG, e.Message);
            }

            return "";
        }
    }
}