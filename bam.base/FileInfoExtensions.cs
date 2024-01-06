using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YamlDotNet.Serialization;
using System.Data;

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

        public static object? FromYamlFile(this FileInfo fileInfo, Type type)
        {
            Deserializer deserializer = new Deserializer();
            return deserializer.Deserialize(fileInfo.FullName.SafeReadFile(), type);
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

        public static object FromJsonFile(this FileInfo file, Type type)
        {
            using (StreamReader sr = new StreamReader(file.FullName))
            {
                return sr.ReadToEnd().FromJson(type);
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

        public static object DecodeFromFile(this FileInfo file, Type type)
        {
            return file.FullName.DecodeFromFile(type);
        }

        public static T DecodeFromFile<T>(this FileInfo file)
        {
            return file.FullName.DecodeFromFile<T>();
        }

        static Dictionary<string, SerializationFormat> _serializationFormats;
        static object _serializationFormatsLock = new object();

        public static Dictionary<string, SerializationFormat> SerializationFormats
        {
            get
            {
                return _serializationFormatsLock.DoubleCheckLock(ref _serializationFormats, () =>
                    new Dictionary<string, SerializationFormat>
                    {
                        {".yaml", SerializationFormat.Yaml},
                        {".yml", SerializationFormat.Yaml},
                        {".json", SerializationFormat.Json},
                        {".xml", SerializationFormat.Xml},
             /*           {".dat", SerializationFormat.Binary},
                        {".bin", SerializationFormat.Binary}*/
                    });
            }
        }

        static Dictionary<SerializationFormat, Func<Stream, Type, object>> _deserializers;
        static object _deserializersLock = new object();

        public static Dictionary<SerializationFormat, Func<Stream, Type, object>> Deserializers
        {
            get
            {
                return _deserializersLock.DoubleCheckLock(ref _deserializers, () =>
                    new Dictionary<SerializationFormat, Func<Stream, Type, object>>
                    {
                        {
                            SerializationFormat.Invalid, (stream, type) =>
                            {
                                Args.Throw<InvalidOperationException>("Invalid SerializationFormat specified");
                                return null;
                            }
                        },
                        {SerializationFormat.Xml, (stream, type) => stream.FromXmlStream(type)},
                        {
                            SerializationFormat.Json, (stream, type) => stream.FromJsonStream(type)
                        }, // this might not work; should be tested
                        {
                            SerializationFormat.Yaml, (stream, type) => stream.FromYamlStream(type)
                        }, // this might not work; should be tested
/*                        {
                            SerializationFormat.Binary, (stream, type) => stream.FromBinaryStream()
                        } // this might not work; should be tested*/
                    });
            }
        }

        static Dictionary<SerializationFormat, Action<Stream, object>> _serializeActions;
        static object _serializeActionsLock = new object();

        public static Dictionary<SerializationFormat, Action<Stream, object>> SerializeActions
        {
            get
            {
                return _serializeActionsLock.DoubleCheckLock(ref _serializeActions, () =>
                    new Dictionary<SerializationFormat, Action<Stream, object>>
                    {
                        {
                            SerializationFormat.Invalid,
                            (stream, obj) =>
                                Args.Throw<InvalidOperationException>("Invalid SerializationFormat specified")
                        },
                        {SerializationFormat.Xml, (stream, obj) => obj.ToXmlStream(stream)},
                        {SerializationFormat.Json, (stream, obj) => obj.ToJsonStream(stream)},
                        {SerializationFormat.Yaml, (stream, obj) => obj.ToYamlStream(stream)},
                        //{SerializationFormat.Binary, (stream, obj) => obj.ToBinaryStream(stream)}
                    });
            }
        }

        /// <summary>
        /// Deserialize the specified file using the file extension to determine the format.
        /// </summary>
        /// <param name="file"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static T FromFile<T>(this FileInfo file)
        {
            return Deserialize<T>(file);
        }

        public static T Deserialize<T>(this FileInfo file)
        {
            return (T)Deserialize(file, typeof(T));
        }

        public static object Deserialize(this FileInfo file, Type type)
        {
            string fileExtension = file.Extension;
            if (!SerializationFormats.ContainsKey(fileExtension))
            {
                throw new ArgumentException(
                    $"File extension ({fileExtension}) not supported for deserialization, use one of ({string.Join(",", SerializationFormats.Keys.ToArray())})");
            }

            using (FileStream fs = file.OpenRead())
            {
                return Deserializers[SerializationFormats[fileExtension]](fs, type);
            }
        }
    }
}
