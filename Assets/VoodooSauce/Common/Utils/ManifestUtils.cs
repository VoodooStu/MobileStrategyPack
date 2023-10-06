using System.Collections.Generic;
using System.IO;
using System.Xml;

namespace Voodoo.Sauce.Internal.Utils
{
    public static class ManifestUtils
    {
        private const string DESTINATION_APPLICATION_MANIFEST_PATH = "Assets/Plugins/Android/AndroidManifest.xml";

        public static bool Add(string manifestSourcePath)
        {
            XmlDocument sourceDocument = Load(manifestSourcePath);
            XmlDocument destDocument = Load(DESTINATION_APPLICATION_MANIFEST_PATH);
            // check if manifest xml is ok
            if (sourceDocument?.DocumentElement == null || destDocument?.DocumentElement == null) {
                return false;
            }

            // add all manifest source declarations on the destination file
            foreach (XmlNode nodeSource in sourceDocument.DocumentElement.ChildNodes) {
                if (nodeSource.HasChildNodes) {
                    XmlNode destNode = FindChildNode(destDocument.DocumentElement, nodeSource);
                    foreach (XmlNode node in nodeSource.ChildNodes) {
                        AddChildNode(destDocument, destNode, node);
                    }
                } else if (nodeSource.Name == "uses-permission") {
                    AddPermission(destDocument, nodeSource);
                } else {
                    AddChildNode(destDocument, destDocument, nodeSource);
                }
            }

            save(DESTINATION_APPLICATION_MANIFEST_PATH, destDocument);
            return true;
        }

        public static bool Replace(Dictionary<string, string> keysValues)
        {
            if (File.Exists(DESTINATION_APPLICATION_MANIFEST_PATH)) {
                string manifestContent = File.ReadAllText(DESTINATION_APPLICATION_MANIFEST_PATH);
                foreach (KeyValuePair<string, string> pair in keysValues) {
                    manifestContent = manifestContent.Replace(pair.Key, pair.Value);
                }

                File.WriteAllText(DESTINATION_APPLICATION_MANIFEST_PATH, manifestContent);
                return true;
            }

            return false;
        }

        private static XmlDocument Load(string manifestPath)
        {
            XmlDocument document = null;
            if (File.Exists(manifestPath)) {
                document = new XmlDocument();
                document.Load(manifestPath);
            }

            return document;
        }

        private static void AddPermission(XmlDocument document, XmlNode sourceNode)
        {
            // Check if permissions are already there.
            XmlElement documentElement = document.DocumentElement;
            if (documentElement == null) return;
            foreach (XmlNode node in documentElement.ChildNodes) {
                if (node.Name == "uses-permission") {
                    string namespaceOfPrefix = node.GetNamespaceOfPrefix("android");
                    if (GetAndroidElementName(node, namespaceOfPrefix) ==
                        GetAndroidElementName(sourceNode, namespaceOfPrefix)) {
                        // already exists
                        return;
                    }
                }
            }

            // the permission doesn't exist and should be added
            documentElement.AppendChild(document.ImportNode(sourceNode, true));
        }

        private static void AddChildNode(XmlDocument document, XmlNode parent, XmlNode node)
        {
            if (parent != null && node != null && !FindElementWithAndroidName(parent, node)) {
                parent.AppendChild(document.ImportNode(node, true));
            }
        }

        private static XmlNode FindChildNode(XmlNode parent, XmlNode child)
        {
            XmlNode node = parent.FirstChild;
            while (node != null) {
                if (node.Name.Equals(child.Name)) {
                    return node;
                }

                node = node.NextSibling;
            }

            return null;
        }

        private static bool FindElementWithAndroidName(XmlNode parent, XmlNode child)
        {
            string namespaceOfPrefix = parent.GetNamespaceOfPrefix("android");
            string childName = GetAndroidElementName(child, namespaceOfPrefix);
            if (childName != null) {
                XmlNode node = parent.FirstChild;
                while (node != null) {
                    if (GetAndroidElementName(node, namespaceOfPrefix) == childName) {
                        return true;
                    }

                    node = node.NextSibling;
                }
            }

            return false;
        }

        private static string GetAndroidElementName(XmlNode node, string namespaceOfPrefix) =>
            node is XmlElement element ? element.GetAttribute("name", namespaceOfPrefix) : null;

        private static void save(string manifestPath, XmlDocument document)
        {
            var set = new XmlWriterSettings {
                Indent = true,
                IndentChars = "  ",
                NewLineChars = "\r\n",
                NewLineHandling = NewLineHandling.Replace
            };

            using (var xmlWriter = XmlWriter.Create(manifestPath, set)) {
                document.Save(xmlWriter);
            }
        }
    }
}