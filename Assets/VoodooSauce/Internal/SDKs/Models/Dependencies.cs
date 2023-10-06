using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Serialization;
using JetBrains.Annotations;

namespace Voodoo.Sauce.Internal.SDKs.Models
{
    [XmlRoot(ElementName="androidPackage")]
    public class AndroidPackage
    {
        [XmlAttribute(AttributeName="spec")]
        public string Spec { get; set; }
        [XmlAttribute(AttributeName="mopub-key")]
        public string Mopubkey { get; set; }
        [XmlElement(ElementName="repositories")]
        public Repositories Repositories { get; set; }
        
        [CanBeNull] public string Version => Spec?.Split(':').Last().Replace("@aar", "");
    }

    [XmlRoot(ElementName="repositories")]
    public class Repositories {
        [XmlElement(ElementName="repository")]
        public string Repository { get; set; }
    }

    [XmlRoot(ElementName="androidPackages")]
    public class AndroidPackages {
        [XmlElement(ElementName="androidPackage")]
        public List<AndroidPackage> AndroidPackage { get; set; }
    }

    [XmlRoot(ElementName="iosPod")]
    public class IosPod {
        [XmlAttribute(AttributeName="name")]
        public string Name { get; set; }
        [XmlAttribute(AttributeName="subspecs")]
        public string Subspecs { get; set; }
        [XmlAttribute(AttributeName="minTargetSdk")]
        public string MinTargetSdk { get; set; }
        [XmlAttribute(AttributeName="version")]
        public string Version { get; set; }
        [XmlAttribute(AttributeName="mopub-key")]
        public string Mopubkey { get; set; }
    }

    [XmlRoot(ElementName="iosPods")]
    public class IosPods {
        [XmlElement(ElementName="iosPod")]
        public List<IosPod> IosPod { get; set; }
    }

    [XmlRoot(ElementName="dependencies")]
    public class Dependencies {
        [XmlElement(ElementName="androidPackages")]
        public AndroidPackages AndroidPackages { get; set; }
        [XmlElement(ElementName="iosPods")]
        public IosPods IosPods { get; set; }
        
        public static Dependencies GetDependencies(string xmlFilePath)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(Dependencies));
            StreamReader reader = new StreamReader(xmlFilePath);
            Dependencies dependencies = (Dependencies)serializer.Deserialize(reader);
            reader.Close();
            return dependencies;
        }
    }
}
