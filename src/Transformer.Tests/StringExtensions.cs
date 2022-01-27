using System.IO;
using System.Text;
using System.Xml.Linq;
using System.Xml.Serialization;
using Environment = Transformer.Core.Model.Environment;

namespace Transformer.Tests
{
    public static class StringExtensions
    {
        public static string ToXmlOneLine(this string xml)
        {
            return XElement.Parse(xml).ToString(SaveOptions.DisableFormatting);
        }

        public static void SetReadOnly(this string filepath)
        {
            new FileInfo(filepath).IsReadOnly = true;
        }

        public static string ToXml(this Environment target)
        {
            var sb = new StringBuilder();
            var serializer = new XmlSerializer(typeof(Environment));
            var ns = new XmlSerializerNamespaces();
            ns.Add("", "");
            serializer.Serialize(new StringWriter(sb), target, ns);
            
            return sb.ToString();
        }

        public static string RelativeTo(this string path, string baseDir)
        {
            return Path.Combine(baseDir, path);
        }

        public static string RelativeTo(this string path, DirectoryInfo baseDir)
        {
            return path.RelativeTo(baseDir.FullName);
        }
    }
}