using Bam.Net.CommandLine;
using Bam.Net.Incubation;
using Microsoft.AspNetCore.WebUtilities;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using YamlDotNet.Serialization;

namespace Bam.Net
{
    public static class StringExtensions
    {
        /// <summary>
        /// Convert text to a byte array using the specified encoding or UTF8.
        /// </summary>
        /// <param name="text"></param>
        /// <param name="encoding"></param>
        /// <returns></returns>
        public static byte[] ToBytes(this string text, Encoding encoding = null)
        {
            encoding = encoding ?? Encoding.UTF8;
            return encoding.GetBytes(text);
        }


        public static byte[] FromHexString(this string hexString)
        {
            return HexToBytes(hexString);
        }

        public static byte[] HexToBytes(this string hexString)
        {
            //check for null
            if (hexString == null) return null;
            //get length
            int len = hexString.Length;
            if (len % 2 == 1) return null;
            int len_half = len / 2;
            //create a byte array
            byte[] bs = new byte[len_half];

            //convert the hexstring to bytes
            for (int i = 0; i != len_half; i++)
            {
                bs[i] = (byte)Int32.Parse(hexString.Substring(i * 2, 2), System.Globalization.NumberStyles.HexNumber);
            }

            //return the byte array
            return bs;
        }
        public static byte[] FromBase64(this string data)
        {
            return Convert.FromBase64String(data);
        }

        public static string ToBase64UrlEncoded(this string data)
        {
            return ByteExtensions.ToBase64UrlEncoded(data.ToBytes());
        }

        public static byte[] FromBase64UrlEncoded(this string data)
        {
            return WebEncoders.Base64UrlDecode(data);
        }
        public static string[] SemiColonSplit(this string semicolonSeparatedValues)
        {
            return DelimitSplit(semicolonSeparatedValues, ";");
        }

        public static string[] DelimitSplit(this string valueToSplit, string delimiter)
        {
            return DelimitSplit(valueToSplit, new string[] { delimiter });
        }

        public static string[] DelimitSplit(this string valueToSplit, params string[] delimiters)
        {
            return DelimitSplit(valueToSplit, delimiters, false);
        }

        public static string[] DelimitSplit(this string valueToSplit, string delimiter, bool trimValues)
        {
            return DelimitSplit(valueToSplit, new string[] { delimiter }, trimValues);
        }

        /// <summary>
        /// Replace a specified string with another string where the specified string occurs
        /// between the startDelimiter and endDelimiter.
        /// </summary>
        /// <param name="input"></param>
        /// <param name="toReplace"></param>
        /// <param name="replaceWith"></param>
        /// <param name="startDelimiter"></param>
        /// <param name="endDelimiter"></param>
        /// <returns></returns>
        public static string DelimitedReplace(this string input, string toReplace, string replaceWith,
            string startDelimiter = "$$~", string endDelimiter = "~$$")
        {
            return DelimitedReplace(input, new Dictionary<string, string> { { toReplace, replaceWith } }, startDelimiter,
                endDelimiter);
        }

        /// <summary>
        /// Replace a specified string with another string where the string to replace occurs
        /// between the startDelimiter and endDelimiter.
        /// </summary>
        /// <param name="input"></param>
        /// <param name="replacements"></param>
        /// <param name="startDelimiter"></param>
        /// <param name="endDelimiter"></param>
        /// <returns></returns>
        public static string DelimitedReplace(this string input, Dictionary<string, string> replacements,
            string startDelimiter = "$$~", string endDelimiter = "~$$")
        {
            StringBuilder result = new StringBuilder();
            StringBuilder innerValue = new StringBuilder();
            bool replacing = false;
            foreach (char c in input)
            {
                if (replacing)
                {
                    innerValue.Append(c);
                    string innerSoFar = innerValue.ToString();
                    foreach (string toReplace in replacements.Keys)
                    {
                        if (innerSoFar.EndsWith(toReplace))
                        {
                            StringBuilder tmp = new StringBuilder();
                            tmp.Append(innerSoFar.Truncate(toReplace.Length));
                            tmp.Append(replacements[toReplace]);
                            innerValue = tmp;
                        }
                    }

                    if (innerValue.ToString().EndsWith(endDelimiter))
                    {
                        replacing = false;
                        result.Append(innerValue.ToString().Truncate(endDelimiter.Length));
                        innerValue = new StringBuilder();
                    }
                }
                else
                {
                    result.Append(c);
                    string soFar = result.ToString();
                    if (soFar.EndsWith(startDelimiter))
                    {
                        replacing = true;
                        StringBuilder tmp = new StringBuilder();
                        tmp.Append(result.ToString().Truncate(startDelimiter.Length));
                        result = tmp;
                    }
                }
            }

            return result.ToString();
        }

        /// <summary>
        /// Split the string on the specified delimiters removing empty entries
        /// and optionally trimming each value
        /// </summary>
        /// <param name="valueToSplit"></param>
        /// <param name="delimiters"></param>
        /// <param name="trimValues"></param>
        /// <returns></returns>
        public static string[] DelimitSplit(this string valueToSplit, string[] delimiters, bool trimValues)
        {
            if (string.IsNullOrEmpty(valueToSplit))
            {
                return new string[] { };
            }
            string[] split = valueToSplit.Split(delimiters, StringSplitOptions.RemoveEmptyEntries);
            if (trimValues)
            {
                for (int i = 0; i < split.Length; i++)
                {
                    split[i] = split[i].Trim();
                }
            }

            return split;
        }

        /// <summary>
        /// Removes the specified number of characters from the end of the 
        /// string and returns the result.
        /// </summary>
        /// <param name="toTruncate"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        public static string Truncate(this string toTruncate, int count)
        {
            if (count > toTruncate.Length)
            {
                return string.Empty;
            }

            return toTruncate.Substring(0, toTruncate.Length - count);
        }

        /// <summary>
        /// Removes the specified number of characters from the beginning of the
        /// string and returns the result.
        /// </summary>
        /// <param name="toTruncate"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        public static string TruncateFront(this string toTruncate, int count)
        {
            if (count > toTruncate.Length)
            {
                return string.Empty;
            }

            return toTruncate.Substring(count, toTruncate.Length - count);
        }

        /// <summary>
        /// Return the first specified number of characters
        /// </summary>
        /// <param name="stringToTrim"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        public static string First(this string stringToTrim, int count)
        {
            if (stringToTrim.Length <= count)
            {
                return stringToTrim;
            }

            return stringToTrim.Substring(0, count);
        }

        /// <summary>
        /// Returns a camel cased string from the specified string using the specified 
        /// separators.  For example, the input "The quick brown fox jumps over the lazy
        /// dog" with the separators of "new string[]{" "}" should return the string 
        /// "theQuickBrownFoxJumpsOverTheLazyDog".
        /// </summary>
        /// <param name="stringToCamelize">The string to camelize.</param>
        /// <param name="preserveInnerUppers">if set to <c>true</c> [preserve inner uppers].</param>
        /// <param name="separators">The separators.</param>
        /// <returns></returns>
        public static string CamelCase(this string stringToCamelize, bool preserveInnerUppers = true,
            params string[] separators)
        {
            if (stringToCamelize.Length > 0)
            {
                string pascalCase = stringToCamelize.PascalCase(preserveInnerUppers, separators);
                string camelCase = string.Format("{0}{1}", pascalCase[0].ToString().ToLowerInvariant(),
                    pascalCase.Remove(0, 1));
                return camelCase;
            }
            else
            {
                return string.Empty;
            }
        }
        /// <summary>
        /// Splits the specified text at capital letters inserting a hyphen as a separator.
        /// </summary>
        public static string KabobCase(this string stringToKabobify)
        {
            return PascalSplit(stringToKabobify, "-");
        }

        /// <summary>
        /// Splits the specified text at capital letters inserting the specified separator.
        /// </summary>
        /// <param name="stringToPascalSplit"></param>
        /// <param name="separator"></param>
        /// <returns></returns>
        public static string PascalSplit(this string stringToPascalSplit, string separator)
        {
            StringBuilder returnValue = new StringBuilder();
            for (int i = 0; i < stringToPascalSplit.Length; i++)
            {
                char next = stringToPascalSplit[i];
                if (i == 0 && char.IsLower(next))
                {
                    next = char.ToUpper(next);
                }

                if (char.IsUpper(next) && i > 0)
                {
                    returnValue.Append(separator);
                }

                returnValue.Append(next);
            }

            return returnValue.ToString();
        }


        /// <summary>
        /// Return an acronym for the specified string using the 
        /// capital letters in the string
        /// </summary>
        /// <param name="stringToAcronymize"></param>
        /// <param name="alwaysUseFirst"></param>
        /// <returns></returns>
        public static string CaseAcronym(this string stringToAcronymize, bool alwaysUseFirst = true)
        {
            if (stringToAcronymize?.Length > 0)
            {
                StringBuilder result = new StringBuilder();
                for (int i = 0; i < stringToAcronymize.Length; i++)
                {
                    char current = stringToAcronymize[i];
                    if (i == 0 && alwaysUseFirst || char.IsUpper(current))
                    {
                        result.Append(current.ToString().ToUpperInvariant());
                    }
                }

                return result.ToString();
            }
            else
            {
                return string.Empty;
            }
        }

        /// <summary>
        /// Returns a pascal cased string from the specified string using the specified 
        /// separators.  For example, the input "The quick brown fox jumps over the lazy
        /// dog" with the separators of "new string[]{" "}" should return the string 
        /// "TheQuickBrownFoxJumpsOverTheLazyDog".
        /// </summary>
        /// <param name="stringToPascalize"></param>
        /// <param name="preserveInnerUppers">If true uppercase letters that appear in 
        /// the middle of a word remain uppercase if false they are converted to lowercase.</param>
        /// <param name="separators"></param>
        /// <returns></returns>
        public static string PascalCase(this string stringToPascalize, bool preserveInnerUppers = true,
            params string[] separators)
        {
            string[] splitString = stringToPascalize.Split(separators, StringSplitOptions.RemoveEmptyEntries);
            string retVal = string.Empty;
            foreach (string part in splitString)
            {
                string firstChar = part[0].ToString().ToUpper();
                retVal += firstChar;
                for (int i = 1; i < part.Length; i++)
                {
                    if (!preserveInnerUppers)
                    {
                        retVal += part[i].ToString().ToLowerInvariant();
                    }
                    else
                    {
                        retVal += part[i].ToString();
                    }
                }
            }

            return retVal;
        }

        public static string PrefixWithUnderscoreIfStartsWithNumber(this string targetString)
        {
            return targetString.StartsWithNumber() ? $"_{targetString}" : targetString;
        }

        public static bool StartsWithNumber(this string targetString)
        {
            return targetString[0].IsNumber();
        }

        public static string AlphaNumericOnly(this string targetString)
        {
            StringBuilder result = new StringBuilder();
            foreach (char c in targetString)
            {
                if (c.IsLetter() || c.IsNumber())
                {
                    result.Append(c.ToString());
                }
            }

            return result.ToString();
        }

        public static string LettersOnly(this string targetString)
        {
            StringBuilder result = new StringBuilder();
            foreach (char c in targetString)
            {
                if (c.IsLetter())
                {
                    result.Append(c.ToString());
                }
            }

            return result.ToString();
        }

        /// <summary>
        /// Add the specified length of random characters
        /// to the current string.  Only  lowercase
        /// letters.
        /// </summary>
        /// <param name="result"></param>
        /// <param name="length"></param>
        /// <returns></returns>
        public static string RandomString(this string result, int length)
        {
            for (int i = 0; i < length; i++)
            {
                char ch = Convert.ToChar(Random.Shared.Next(97, 122)); // ascii codes for printable alphabet
                result += ch;
            }

            return result;
        }

        public static bool IsAllCaps(this string value)
        {
            bool result = true;
            foreach (var c in value)
            {
                result = char.IsUpper(c);
                if (!result)
                {
                    break;
                }
            }

            return result;
        }
        /// <summary>
        /// Write the specified textToWrite to the current filePath
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="textToWrite"></param>
        /// <param name="postWriteAction"></param>
        public static void SafeWriteFile(this string filePath, string textToWrite,
            Action<object> postWriteAction = null)
        {
            SafeWriteFile(filePath, textToWrite, false, postWriteAction);
        }

        /// <summary>
        /// Write the current textToWrite to the specified filePath
        /// </summary>
        /// <param name="textToWrite"></param>
        /// <param name="filePath"></param>
        /// <param name="postWriteAction"></param>
        public static void SafeWriteToFile(this string textToWrite, string filePath,
            Action<object> postWriteAction = null)
        {
            filePath.SafeWriteFile(textToWrite, postWriteAction);
        }

        public static void SafeWriteToFile(this string textToWrite, string filePath, bool overwrite,
            Action<object> postWriteAction = null)
        {
            filePath.SafeWriteFile(textToWrite, overwrite, postWriteAction);
        }

        /// <summary>
        /// Write the specified text to the specified file in a thread safe way.
        /// </summary>
        /// <param name="filePath">The path to the file to write.</param>
        /// <param name="textToWrite">The text to write.</param>
        /// <param name="overwrite">True to overwrite.  If false and the file exists an InvalidOperationException will be thrown.</param>
        public static void SafeWriteFile(this string filePath, string textToWrite, bool overwrite,
            Action<object> postWriteAction = null)
        {
            FileInfo fileInfo = HandleExisting(filePath, overwrite);

            lock (FileLock.Named(fileInfo.FullName))
            {
                using (StreamWriter sw = new StreamWriter(filePath))
                {
                    sw.Write(textToWrite);
                }
            }

            postWriteAction?.Invoke(fileInfo);
        }

        /// <summary>
        /// Intended to delimit the specified array of T using the
        /// specified ToDelimitedDelegate.  Each item will be represented
        /// by the return value of the specified ToDelimitedDelegate.
        /// </summary>
        /// <typeparam name="T">The type of objects in the specified array</typeparam>
        /// <param name="objectsToStringify">The objects</param>
        /// <param name="toDelimiteder">The ToDelimitedDelegate used to represent each object</param>
        /// <returns>string</returns>
        public static string ToDelimited<T>(this T[] objectsToStringify, ToDelimitedDelegate<T> toDelimiteder)
        {
            return ToDelimited(objectsToStringify, toDelimiteder, ", ");
        }

        /// <summary>
        /// Intended to delimit the specified array of T using the
        /// specified ToDelimitedDelegate.  Each item will be represented
        /// by the return value of the specified ToDelimitedDelegate.
        /// </summary>
        /// <typeparam name="T">The type of objects in the specified array</typeparam>
        /// <param name="objectsToStringify">The objects</param>
        /// <param name="toDelimiteder">The ToDelimitedDelegate used to represent each object</param>
        /// <returns>string</returns>
        public static string ToDelimited<T>(this T[] objectsToStringify, ToDelimitedDelegate<T> toDelimiteder, string delimiter)
        {
            string retVal = string.Empty;
            bool first = true;
            foreach (T obj in objectsToStringify)
            {
                if (!first)
                {
                    retVal += delimiter;
                }
                retVal += toDelimiteder(obj);
                first = false;
            }
            return retVal;
        }

        private static FileInfo HandleExisting(string filePath, bool overwrite)
        {
            FileInfo fileInfo = new FileInfo(filePath);

            if (!(bool)(fileInfo.Directory?.Exists))//!Directory.Exists(fileInfo.Directory.FullName))
            {
                Directory.CreateDirectory(fileInfo.Directory.FullName);
            }

            if (File.Exists(fileInfo.FullName) && !overwrite)
            {
                throw new InvalidOperationException("File already exists and 'overwrite' parameter was false");
            }

            return fileInfo;
        }

        /// <summary>
        /// An extension method to enable functional programming access
        /// to string.Format.
        /// </summary>
        /// <param name="format"></param>
        /// <param name="formatArgs"></param>
        /// <returns></returns>
        public static string Format(this string format, params object[] formatArgs)
        {
            return string.Format(format, formatArgs);
        }

        public static string DropTrailingNonLetters(this string targetString)
        {
            StringBuilder temp = new StringBuilder();
            bool foundLetter = false;
            for (int i = targetString.Length - 1; i >= 0; i--)
            {
                char c = targetString[i];
                if (c.IsLetter())
                {
                    foundLetter = true;
                }

                if (foundLetter)
                {
                    temp.Append(c);
                }
            }

            StringBuilder result = new StringBuilder();
            temp.ToString().ToCharArray().Reverse().ToArray().Each(c => result.Append(c));
            return result.ToString();
        }

        /// <summary>
        /// Returns the content of the file referenced by the current
        /// string instance.
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        public static string SafeReadFile(this string filePath)
        {
            if (!File.Exists(filePath))
            {
                return string.Empty;
            }

            lock (FileLock.Named(filePath))
            {
                return File.ReadAllText(filePath);
            }
        }

        /// <summary>
        /// Parse the string as the specified generic enum.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        public static T ToEnum<T>(this string value)
        {
            return (T)Enum.Parse(typeof(T), value);
        }

        /// <summary>
        /// Deserialize the contents of the current path as an instance of the specified generic type.
        /// </summary>
        /// <param name="path"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static T FromYamlFile<T>(this string path)
        {
            return new FileInfo(path).FromYamlFile<T>();
        }

        public static T FromYaml<T>(this string yaml)
        {
            Deserializer deserializer = new Deserializer();
            return deserializer.Deserialize<T>(yaml);
        }

        /// <summary>
        /// Deserialize the current string as the specified
        /// generic type T.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="json"></param>
        /// <returns></returns>
        public static T FromJson<T>(this string json)
        {
            return JsonConvert.DeserializeObject<T>(json);
        }

        public static bool TryFromJson<T>(this string json)
        {
            return TryFromJson<T>(json, out T _);
        }

        public static bool TryFromJson<T>(this string json, out T instance)
        {
            return TryFromJson<T>(json, out instance, out Exception ignore);
        }

        public static bool TryFromJson<T>(this string json, out T instance, out Exception exception)
        {
            instance = default(T);
            exception = null;
            try
            {
                instance = FromJson<T>(json);
                return true;
            }
            catch (Exception ex)
            {
                exception = ex;
                return false;
            }
        }

        /// <summary>
        /// Deserialize the current json string as the specified
        /// type
        /// </summary>
        /// <param name="json"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public static object FromJson(this string json, Type type)
        {
            return JsonConvert.DeserializeObject(json, type);
        }

        /// <summary>
        /// Deserialize the contents of the file path specified
        /// in the current string to the type T
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="filePath"></param>
        /// <returns></returns>
        public static T FromJsonFile<T>(this string filePath)
        {
            return filePath.SafeReadFile().FromJson<T>();
        }

        public static object FromJsonFile(this string filePath, Type type)
        {
            return filePath.SafeReadFile().FromJson(type);
        }


        /// <summary>
        /// Deserialize the xml file as the specified generic type
        /// </summary>
        /// <typeparam name="T">The type of the return value</typeparam>
        /// <param name="filePath">The path to the xml file</param>
        /// <returns>instance of T</returns>
        public static T FromXmlFile<T>(this string filePath)
        {
            return (T)FromXmlFile(filePath, typeof(T));
        }

        /// <summary>
        /// Deserialize the xml file as the speicified type
        /// </summary>
        /// <param name="filePath">The path to the xml file</param>
        /// <param name="type">The type of the return value</param>
        /// <returns>instance of specified type deserialized from the specified file</returns>
        public static object FromXmlFile(this string filePath, Type type)
        {
            using (StreamReader sr = new StreamReader(filePath))
            {
                return new XmlSerializer(type).Deserialize(sr);
            }
        }


        /// <summary>
        /// Deserialize the specified xmlString as the specified 
        /// generic type
        /// </summary>
        /// <typeparam name="T">The type of return value</typeparam>
        /// <param name="xmlString">The string to deserialize</param>
        /// <returns>instance of T</returns>
        public static T FromXml<T>(this string xmlString)
        {
            return FromXml<T>(xmlString, Encoding.Default);
        }

        /// <summary>
        /// Deserialize the specified xml as the specified generic type using the specified encoding.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="xmlString"></param>
        /// <returns></returns>
        public static T FromXml<T>(this string xmlString, Encoding encoding)
        {
            XmlSerializer ser = new XmlSerializer(typeof(T));
            byte[] xmlBytes = encoding.GetBytes(xmlString);
            MemoryStream ms = new MemoryStream(xmlBytes);
            return (T)ser.Deserialize(ms);
        }

        public static object FromXml(this string xml, Type type, Encoding encoding = null)
        {
            XmlSerializer ser = new XmlSerializer(type);
            byte[] xmlBytes = encoding.GetBytes(xml);
            MemoryStream ms = new MemoryStream(xmlBytes);
            return ser.Deserialize(ms);
        }

        public static Task<ProcessOutput> RunAsync(this string command, int timeout = 600000)
        {
            return Task.Run(() => Run(command, timeout));
        }

        /// <summary>
        /// Runs the specified command blocking until complete.
        /// </summary>
        /// <param name="command">The command.</param>
        /// <param name="timeout">The timeout.</param>
        /// <returns></returns>
        public static ProcessOutput Run(this string command, int timeout = 600000)
        {
            return command.Run(false, null, null, timeout);
        }

        /// <summary>
        /// Executes the current string on the command line
        /// and returns the output.
        /// </summary>
        /// <param name="command">a valid command line</param>
        /// <returns>ProcessOutput</returns>
        public static ProcessOutput Run(this string command, StringBuilder output, StringBuilder error, int timeout = 600000)
        {
            return command.Run(false, output, error, timeout);
        }

        /// <summary>
        /// Executes the current string on the command line and returns the output.
        /// This operation will block if a timeout greater than 0 is specified
        /// </summary>
        /// <param name="command"></param>
        /// <param name="promptForAdmin"></param>
        /// <param name="timeout"></param>
        /// <returns></returns>
        public static ProcessOutput Run(this string command, bool promptForAdmin, int timeout = 600000)
        {
            return command.Run(promptForAdmin, null, null, timeout);
        }

        /// <summary>
        /// Executes the current string on the command line
        /// and returns the output.
        /// </summary>
        /// <param name="command">The command.</param>
        /// <param name="promptForAdmin">if set to <c>true</c> [prompt for admin].</param>
        /// <param name="output">The output.</param>
        /// <param name="error">The error.</param>
        /// <param name="timeout">The timeout.</param>
        /// <returns></returns>
        public static ProcessOutput Run(this string command, bool promptForAdmin, StringBuilder output = null, StringBuilder error = null, int timeout = 600000)
        {
            // fixed this to handle output correctly based on http://stackoverflow.com/questions/139593/processstartinfo-hanging-on-waitforexit-why
            ValidateCommand(command);

            GetExeAndArguments(command, out string exe, out string arguments);

            return Run(string.IsNullOrEmpty(exe) ? command : exe, arguments, promptForAdmin, output, error, timeout);
        }
        private static void ValidateCommand(string command)
        {
            Expect.IsFalse(string.IsNullOrEmpty(command), "command cannot be blank or null");
            Expect.IsFalse(command.Contains("\r"), "Multiple command lines not supported");
            Expect.IsFalse(command.Contains("\n"), "Multiple command lines not supported");
        }
        // TODO: obsolete this method
        private static void GetExeAndArguments(string command, out string exe, out string arguments)
        {
            exe = command;
            arguments = string.Empty;
            string[] split = command.Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries);
            if (split.Length > 1)
            {
                exe = split[0];
                for (int i = 1; i < split.Length; i++)
                {
                    arguments += split[i];
                    if (i != split.Length - 1)
                        arguments += " ";
                }
            }
        }
        public static ProcessOutput Run(this string command, bool promptForAdmin, ProcessOutputCollector outputCollector, int timeout = 600000)
        {
            ValidateCommand(command);
            GetExeAndArguments(command, out string exe, out string arguments);
            ProcessStartInfo startInfo = ProcessExtensions.CreateStartInfo(promptForAdmin);
            startInfo.FileName = command;
            startInfo.Arguments = arguments;
            return startInfo.Run(outputCollector, timeout);
                //return Run(startInfo, outputCollector, timeout);
        }
        /// <summary>
        /// Returns true if the string equals "q", "quit", "exit" or "bye" using a 
        /// case insensitvie comparison
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static bool IsExitRequest(this string value)
        {
            return value.Equals("q", StringComparison.InvariantCultureIgnoreCase) ||
                   value.Equals("quit", StringComparison.InvariantCultureIgnoreCase) ||
                   value.Equals("exit", StringComparison.InvariantCultureIgnoreCase) ||
                   value.Equals("bye", StringComparison.InvariantCultureIgnoreCase);
        }


        /// <summary>
        /// If the specified file exists, a new path with 
        /// an underscore and a number appended is 
        /// returned where the new path does not exist.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <returns>A file path with a number appended or the specified path.</returns>
        public static string GetNextFileName(this string path)
        {
            return GetNextFileName(path, out int num);
        }

        public static string GetNextFileName(this string path, out int num)
        {
            return GetNextFileName(path, null, out num);
        }

        /// <summary>
        /// If the specified file exists, a new path with 
        /// an underscore and a number appended will be 
        /// returned where the new path does not exist
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static string GetNextFileName(this string path, Func<int, string, string, string> namer, out int num)
        {
            namer = namer ?? ((_i, f, e) => $"{f}_{_i}{e}");
            FileInfo file = new FileInfo(path);
            DirectoryInfo dir = file.Directory;
            string extension = Path.GetExtension(path);
            string fileName = Path.GetFileNameWithoutExtension(path);
            int i = 0;
            num = 0;
            string currentPath = path;
            while (File.Exists(currentPath))
            {
                i++;
                string nextFile = namer(i, fileName, extension);
                currentPath = Path.Combine(dir.FullName, nextFile);
                num = i;
            }

            return currentPath;
        }

        /// <summary>
        /// If the specified directory exists a new path with 
        /// a number appended will be returned where the 
        /// new path does not exist
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static string GetNextDirectoryName(this string path)
        {
            int num;
            return GetNextDirectoryName(path, out num);
        }

        /// <summary>
        /// If the specified directory exists a new path with 
        /// a number appended will be returned where the 
        /// new path does not exist
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static string GetNextDirectoryName(this string path, out int num)
        {
            DirectoryInfo dirInfo = new DirectoryInfo(path);
            num = 0;
            string currentPath = path;
            while (Directory.Exists(currentPath))
            {
                num++;
                currentPath = $"{path}_{num}";
            }

            return currentPath;
        }

        /// <summary>
        /// Read the specified string up to the first instance of the specified charToFind
        /// returning the characters read and producing remainder as an out parameter.  Discards
        /// the specified charToFind returning only values on either side
        /// </summary>
        public static string ReadUntil(this string toRead, char charToFind, bool skipBlanks, out string remainder)
        {
            string result = string.Empty;
            remainder = string.Empty;
            if (skipBlanks)
            {
                string read = toRead;
                while (string.IsNullOrEmpty(result) && !string.IsNullOrEmpty(read))
                {
                    result = ReadUntil(read, charToFind, out remainder);
                    if (!string.IsNullOrEmpty(remainder))
                    {
                        read = remainder;
                    }
                }
            }
            else
            {
                result = ReadUntil(toRead, charToFind, out remainder);
            }
            return result;
        }

        /// <summary>
        /// Read the specified string up to the first instance of the specified charToFind
        /// returning the characters read and producing remainder as an out parameter.  Discards
        /// the specified charToFind returning only values on either side
        /// </summary>
        public static string ReadUntil(this string toRead, char charToFind)
        {
            return ReadUntil(toRead, charToFind, out _);
        }

        /// <summary>
        /// Read the specified string up to the first instance of the specified charToFind
        /// returning the characters read and producing remainder as an out parameter.  Discards
        /// the specified charToFind returning only values on either side
        /// </summary>
        /// <param name="toRead"></param>
        /// <param name="charToFind"></param>
        /// <param name="remainder"></param>
        /// <returns></returns>
        public static string ReadUntil(this string toRead, char charToFind, out string remainder)
        {
            StringBuilder result = new StringBuilder();
            int pos = 0;
            remainder = string.Empty;
            foreach (char c in toRead)
            {
                if (c.Equals(charToFind))
                {
                    remainder = toRead.Substring(pos + 1);
                    break;
                }

                ++pos;
                result.Append(c);
            }

            return result.ToString();
        }

        public static string ReadUntil(this string toRead, string stringToFind, out string remainder)
        {
            StringBuilder readBuffer = new StringBuilder();
            int pos = 0;
            remainder = string.Empty;
            foreach (char c in toRead)
            {
                readBuffer.Append(c);
                if (readBuffer.ToString().EndsWith(stringToFind))
                {
                    remainder = toRead.Substring(pos + 1);
                    break;
                }

                ++pos;
            }

            return readBuffer.ToString().Truncate(stringToFind.Length);
        }

        public static string RemainderAfter(this string toRead, char leadingChar)
        {
            int pos = 0;
            foreach (char c in toRead)
            {
                if (!c.Equals(leadingChar))
                {
                    return toRead.Substring(pos);
                }

                ++pos;
            }

            return toRead;
        }

        /// <summary>
        /// Appends the specified text to the specified file in a thread safe way.
        /// If the file doesn't exist it will be created.
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="textToAppend"></param>
        public static void SafeAppendToFile(this string textToAppend, string filePath)
        {
            FileInfo fileInfo = new FileInfo(filePath);

            lock (FileLock.Named(fileInfo.FullName))
            {
                if (!fileInfo.Directory.Exists)
                {
                    fileInfo.Directory.Create();
                }
                using (StreamWriter sw = new StreamWriter(filePath, true))
                {
                    sw.Write(textToAppend);
                    sw.Flush();
                }
            }
        }
        public static ProcessStartInfo ToStartInfo(this string exe, string arguments, string workingDirectory, bool promptForAdmin = false)
        {
            ProcessStartInfo startInfo = ProcessExtensions.CreateStartInfo(promptForAdmin);
            startInfo.FileName = exe;
            startInfo.Arguments = arguments;
            startInfo.WorkingDirectory = workingDirectory;
            return startInfo;
        }
        private static ProcessOutput Run(this string command, string arguments, bool promptForAdmin, StringBuilder output = null, StringBuilder error = null, int timeout = 600000)
        {
            ProcessStartInfo startInfo = ProcessExtensions.CreateStartInfo(promptForAdmin);

            startInfo.FileName = command;
            startInfo.Arguments = arguments;

            return startInfo.Run(output, error, timeout);//return Run(startInfo, output, error, timeout);
        }

        /// <summary>
        /// Attempts to return the plural version of the supplied word (assumed to be a noun)
        /// using basic rules.
        /// </summary>
        /// <param name="stringToPluralize"></param>
        /// <returns></returns>
        public static string Pluralize(this string stringToPluralize)
        {
            string checkValue = stringToPluralize.ToLowerInvariant();
            if (checkValue.EndsWith("ies"))
            {
                return stringToPluralize;
            }
            else if (checkValue.EndsWith("us"))
            {
                return stringToPluralize.Substring(0, stringToPluralize.Length - 2) + "i";
            }
            else if (checkValue.EndsWith("s") ||
                     checkValue.EndsWith("sh"))
            {
                return stringToPluralize + "es";
            }
            else if (checkValue.EndsWith("ay") ||
                     checkValue.EndsWith("ey") ||
                     checkValue.EndsWith("iy") ||
                     checkValue.EndsWith("oy") ||
                     checkValue.EndsWith("uy"))
            {
                return stringToPluralize + "s";
            }
            else if (checkValue.EndsWith("y"))
            {
                return stringToPluralize.Substring(0, stringToPluralize.Length - 1) + "ies";
            }
            else
            {
                return stringToPluralize + "s";
            }
        }

        /// <summary>
        /// Returns true if the string equals "true", "t", "yes", "y" or "1" using a case
        /// insensitive comparison.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static bool IsAffirmative(this string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return false;
            }

            return value.Equals("true", StringComparison.InvariantCultureIgnoreCase) ||
                   value.Equals("yes", StringComparison.InvariantCultureIgnoreCase) ||
                   value.Equals("t", StringComparison.InvariantCultureIgnoreCase) ||
                   value.Equals("y", StringComparison.InvariantCultureIgnoreCase) ||
                   value.Equals("1");
        }

        public static string MixCase(string retTemp)
        {
            return MixCase(retTemp, 5);
        }

        private static string MixCase(string retTemp, int tryCount)
        {
            if (tryCount <= 0)
                return retTemp;

            if (retTemp.Length < 2)
                return retTemp;

            string upperIzed = string.Empty;
            bool didUpper = false;
            bool keptLower = false;
            foreach (char c in retTemp)
            {
                string upper = string.Empty;
                if (Extensions.RandomBool())
                {
                    upper = c.ToString().ToUpper();
                    didUpper = true;
                }
                else
                {
                    upper = c.ToString();
                    keptLower = true;
                }

                upperIzed += upper;
            }

            if (didUpper && keptLower)
                return upperIzed;
            else
                return MixCase(upperIzed, --tryCount);
        }
        public static string TrimNonLetters(this string targetString)
        {
            return targetString.DropLeadingNonLetters().DropTrailingNonLetters();
        }



        public static string DropLeadingNonLetters(this string targetString)
        {
            StringBuilder result = new StringBuilder();
            bool foundLetter = false;
            for (int i = 0; i < targetString.Length; i++)
            {
                char c = targetString[i];
                if (c.IsLetter())
                {
                    foundLetter = true;
                }

                if (foundLetter)
                {
                    result.Append(c);
                }
            }

            return result.ToString();
        }

        static string[] letters = new string[]
        {
                    "a", "b", "c", "d", "e", "f", "g", "h", "i", "j", "k", "l", "m", "n", "o", "p", "q", "r", "s", "t", "u",
                    "v", "w", "x", "y", "z"
        };

        /// <summary>
        /// Returns a random lowercase letter from a-z."
        /// </summary>
        /// <returns>String</returns>
        public static string RandomLetter()
        {
            return letters[RandomHelper.Next(0, 26)];
        }

        /// <summary>
        /// Append the specified number of characters
        /// to the end of the string
        /// </summary>
        /// <param name="val"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        public static string RandomLetters(this string val, int count)
        {
            StringBuilder txt = new StringBuilder();
            txt.Append(val);
            for (int i = 0; i < count; i++)
            {
                txt.Append(RandomLetter());
            }

            return txt.ToString();
        }

        public static object DecodeFromFile(this string filePath, Type type)
        {
            byte[] data = File.ReadAllBytes(filePath);
            return data.Decode(type);
        }
        
        public static T DecodeFromFile<T>(this string filePath)
        {
            byte[] data = File.ReadAllBytes(filePath);
            return data.Decode<T>();
        }

        /// <summary>
        /// Returns true if the string equals "false", "f", "no", "n" or 0 using a case
        /// insensitive comparison
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static bool IsNegative(this string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return true;
            }

            return value.Equals("false", StringComparison.InvariantCultureIgnoreCase) ||
                   value.Equals("no", StringComparison.InvariantCultureIgnoreCase) ||
                   value.Equals("f", StringComparison.InvariantCultureIgnoreCase) ||
                   value.Equals("n", StringComparison.InvariantCultureIgnoreCase) ||
                   value.Equals("0");
        }

        public static void WriteToStream(this string text, Stream writeTo, bool dispose = true)
        {
            StreamWriter sw = new StreamWriter(writeTo);
            sw.Write(text);
            sw.Flush();

            if (dispose)
            {
                sw.Dispose();
            }
        }

        public static string Times(this string text, int repeatCount)
        {
            StringBuilder result = new StringBuilder();
            for(int i = 0; i < repeatCount; i++)
            {
                result.Append(text);
            }
            return result.ToString();
        }
    }
}
