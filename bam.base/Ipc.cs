using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bam
{
    public static class Ipc
    {
        public static Func<string, Type, string, IIpcMessage?>? CreateMessage { get; set; }

        public static Func<IIpcMessage, bool>? MessageIsPersisted { get; set; }

        public static void EnsureImplementationsAreSetOrDie()
        {
            if (CreateMessage == null)
            {
                throw new InvalidOperationException($"{nameof(CreateMessage)} function not set");
            }

            if (MessageIsPersisted == null)
            {
                throw new InvalidOperationException($"{nameof(MessageIsPersisted)} function not set");
            }
        }

        public static IIpcMessage Get<T>(string name, string? rootDirectory = null)
        {
            return Get(name, typeof(T), rootDirectory);
        }

        /// <summary>
        /// Gets a message with the specified name creating it 
        /// if necessary
        /// </summary>
        /// <param name="name"></param>
        /// <param name="messageType"></param>
        /// <param name="rootDirectory"></param>
        /// <returns></returns>
        public static IIpcMessage Get(string name, Type messageType, string? rootDirectory = null)
        {
            if (!Exists(name, messageType, rootDirectory, out IIpcMessage result))
            {
                result = Create(name, messageType, rootDirectory);
            }

            return result;
        }

        public static IIpcMessage Create<T>(string name)
        {
            return Create(name, typeof(T));
        }

        public static IIpcMessage Create(string name, Type type)
        {
            return Create(name, type, null);
        }

        public static IIpcMessage Create(string name, Type type, string? rootDirectory = null, bool deleteExisting = false)
        {
            EnsureImplementationsAreSetOrDie();
            return CreateMessage(name, type, rootDirectory);
            
            /* old implementation
             
            new IpcMessage(name, type, rootDirectory);

            if (File.Exists(result.MessageFile))
            {
                if (deleteExisting)
                {
                    File.Delete(result.MessageFile);
                }
                else
                {
                    throw new InvalidOperationException($"The specified {nameof(IpcMessage)}.Name={name} is already in use");
                }
            }

            return result;
            */
        }

        public static bool Exists<T>(string name)
        {
            return Exists<T>(name, out IIpcMessage ignore);
        }

        public static bool Exists<T>(string name, out IIpcMessage result)
        {
            EnsureImplementationsAreSetOrDie();
            result = CreateMessage(name, typeof(T), string.Empty);//new IpcMessage(name, typeof(T));
            return MessageIsPersisted(result);//File.Exists(result.MessageFile);
        }

        public static bool Exists(string name, Type messageType, out IIpcMessage result)
        {
            EnsureImplementationsAreSetOrDie();
            result = CreateMessage(name, messageType, string.Empty);//new IpcMessage(name, messageType);
            return MessageIsPersisted(result);//File.Exists(result.MessageFile);
        }

        public static bool Exists(string name, Type messageType, string? rootDirectory)
        {
            return Exists(name, messageType, rootDirectory, out IIpcMessage ignore);
        }

        public static bool Exists(string name, Type messageType, string rootDirectory, out IIpcMessage result)
        {
            EnsureImplementationsAreSetOrDie();
            result = CreateMessage(name, messageType, rootDirectory);//new IpcMessage(name, messageType, rootDirectory);
            return MessageIsPersisted(result);//File.Exists(result.MessageFile);
        }
    }
}
