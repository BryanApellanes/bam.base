using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YamlDotNet.Serialization;
using Bam.Net;

namespace Bam.Net
{
    public static class FileInfoExtensions
    {
        public static string ReadAllText(this FileInfo file)
        {
            using (StreamReader reader = new StreamReader(file.FullName))
            {
                return reader.ReadToEnd();
            }
        }
        public static bool HasExtension(this FileInfo file, string dotExtension)
        {
            return Path.GetExtension(file.FullName).Equals(dotExtension);
        }

        public static bool HasNoExtension(this FileInfo file)
        {
            return Path.GetExtension(file.FullName).Equals(string.Empty);
        }
        public static T FromYamlFile<T>(this FileInfo fileInfo)
        {
            Deserializer deserializer = new Deserializer();
            return deserializer.Deserialize<T>(fileInfo.FullName.SafeReadFile());
        }

        /// <summary>
        /// Reads the file and deserializes the contents as the specified
        /// generic type T
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="file"></param>
        /// <returns></returns>
        public static T FromJsonFile<T>(this FileInfo file)
        {
            using (StreamReader sr = new StreamReader(file.FullName))
            {
                return sr.ReadToEnd().FromJson<T>();
            }
        }

        /// <summary>
        /// If the specified file exists, a new FileInfo with 
        /// an underscore and a number appended is 
        /// returned where the new FileInfo does not exist.
        /// </summary>
        /// <param name="file">The file.</param>
        /// <returns>A new FileInfo with a number appended or the specified FileInfo.</returns>
        public static FileInfo GetNextFile(this FileInfo file)
        {
            return new FileInfo(file.FullName.GetNextFileName());
        }
    }
}
