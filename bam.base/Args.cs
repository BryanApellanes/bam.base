/*
	Copyright © Bryan Apellanes 2015  
*/

using System.Text;
using System.Reflection;

namespace Bam
{
    /// <summary>
    /// Provides utility methods for argument validation, exception creation, and exception message formatting.
    /// </summary>
    public static class Args
    {
        /// <summary>
        /// Throws an <see cref="Exception"/> if the specified condition is true.
        /// </summary>
        /// <param name="condition">The condition to evaluate.</param>
        /// <param name="messageFormat">The format string for the exception message.</param>
        /// <param name="args">The arguments to format into the message.</param>
		public static void ThrowIf(bool condition, string messageFormat, params object[] args)
		{
			ThrowIf<Exception>(condition, messageFormat, args);
		}

        /// <summary>
        /// Throw an ArgumentNullException if the specified 
        /// param is null
        /// </summary>
        /// <param name="param"></param>
        /// <param name="paramName"></param>
        public static void ThrowIfNull(object? param, string paramName = "param")
        {
            if (param == null)
            {
                throw new ArgumentNullException(paramName);
            }
        }

        /// <summary>
        /// Throws an <see cref="ArgumentNullException"/> with the specified message if the parameter is null.
        /// </summary>
        /// <param name="param">The parameter to check for null.</param>
        /// <param name="message">The error message for the exception.</param>
        /// <param name="paramName">The name of the parameter.</param>
        public static void ThrowIfNull(object? param, string message, string paramName)
        {
            if(param == null)
            {
                throw new ArgumentNullException(paramName, message);
            }
        }

        /// <summary>
        /// Throw an ArgumentNullException if the specified
        /// string is null or an empty string
        /// </summary>
        /// <param name="param"></param>
        /// <param name="paramName"></param>
        public static void ThrowIfNullOrEmpty(string param, string paramName = "param")
        {
            if (string.IsNullOrEmpty(param))
            {
                throw new ArgumentNullException(paramName);
            }
        }

        /// <summary>
        /// Throw an exception of generic type E if the specified
        /// condition is true using the specified format and
        /// format values to define the message of the exception
        /// </summary>
        /// <typeparam name="E"></typeparam>
        /// <param name="condition"></param>
        /// <param name="msgFormat"></param>
        /// <param name="values"></param>
        public static void ThrowIf<E>(bool condition, string msgFormat, params object[] values) where E: Exception
        {
            if(condition)
            {
				Throw<E>(msgFormat, values);
            }
        }

        /// <summary>
        /// Throw an exception of generic type E using the specified
        /// format and format values to define the message of the
        /// exception
        /// </summary>
        /// <typeparam name="E"></typeparam>
        /// <param name="msgFormat"></param>
        /// <param name="values"></param>
        public static void Throw<E>(string msgFormat, params object[] values) where E: Exception
        {
            throw Exception<E>(msgFormat, values);
        }

        /// <summary>
        /// Create an exception using the specified format and
        /// format values to define the message of the exception
        /// </summary>
        /// <param name="msgFormat"></param>
        /// <param name="values"></param>
        /// <returns></returns>
        public static Exception Exception(string msgFormat, params object[] values)
        {
            return Exception<Exception>(msgFormat, values);
        }

        /// <summary>
        /// Create an exception of generic type E using the 
        /// specified format and format values to define
        /// the message of the exception
        /// </summary>
        /// <typeparam name="E">The type of the exception to return</typeparam>
        /// <param name="msgFormat">The string format</param>
        /// <param name="values">The arguments to pass to string.Format</param>
        /// <returns></returns>
        public static E Exception<E>(string msgFormat, params object[] values) where E : Exception
        {
            return Exception<E>(msgFormat, null!, values);
        }

        /// <summary>
        /// Create an exception of generic type E using the 
        /// specified format, format values and inner exception
        /// to define the message and inner exception of the 
        /// resultant exception
        /// </summary>
        /// <typeparam name="E"></typeparam>
        /// <param name="msgFormat"></param>
        /// <param name="innerException"></param>
        /// <param name="values"></param>
        /// <returns></returns>
        public static E Exception<E>(string msgFormat, Exception innerException, params object[] values) where E : Exception
        {
            ConstructorInfo ctor  = typeof(E).GetConstructor(new Type[] { typeof(string), typeof(Exception) })!;
            return (E)ctor!.Invoke(new object[] { string.Format(msgFormat, values), innerException });
        }

        /// <summary>
        /// Gets the stack trace from the exception and its inner exception as a single string.
        /// </summary>
        /// <param name="ex">The exception to extract the stack trace from.</param>
        /// <returns>A string containing the stack trace.</returns>
		public static string GetStackTrace(this Exception ex)
		{
			StringBuilder message = new StringBuilder();
			StringBuilder stackTrace = new StringBuilder();
			SetMessageAndStackTrace(ex, message, stackTrace);
			return stackTrace.ToString();
		}

        /// <summary>
        /// Gets the message and stack trace from the exception and its inner exception as a single string.
        /// </summary>
        /// <param name="ex">The exception to extract the message and stack trace from.</param>
        /// <returns>A string containing both the message and stack trace.</returns>
		public static string GetMessageAndStackTrace(this Exception ex)
		{
			PopMessageAndStackTrace(ex, out StringBuilder message, out StringBuilder stackTrace);
			return message.Append(stackTrace.ToString()).ToString();
		}

		/// <summary>
		/// "Pop" out a StringBuilder for the message and stack trace for the specified 
		/// Exception.
		/// </summary>
		/// <param name="ex"></param>
		/// <param name="message"></param>
		/// <param name="stackTrace"></param>
		public static void PopMessageAndStackTrace(Exception ex, out StringBuilder message, out StringBuilder stackTrace)
		{
			message = new StringBuilder();
			stackTrace = new StringBuilder();
			SetMessageAndStackTrace(ex, message, stackTrace);
		}

        /// <summary>
        /// Writes C0 control characters and DEL as visible escapes (CR as <c>\r</c>, LF as <c>\n</c>, tab as <c>\t</c>,
        /// any other as <c>\uXXXX</c>), so text taken from an exception message cannot start what looks like a new
        /// record in a line-based log. Text without control characters is returned as is.
        /// </summary>
        /// <param name="text">The text to escape, or null.</param>
        /// <returns>The escaped text, or an empty string for null.</returns>
        public static string EscapeControlCharacters(string? text)
        {
            if (string.IsNullOrEmpty(text))
            {
                return string.Empty;
            }
            int first = -1;
            for (int index = 0; index < text.Length; index++)
            {
                if (char.IsControl(text[index]) && text[index] < 0x80)
                {
                    first = index;
                    break;
                }
            }
            if (first < 0)
            {
                return text;
            }
            StringBuilder escaped = new StringBuilder(text.Length + 8);
            escaped.Append(text, 0, first);
            for (int index = first; index < text.Length; index++)
            {
                char c = text[index];
                switch (c)
                {
                    case '\r':
                        escaped.Append("\\r");
                        break;
                    case '\n':
                        escaped.Append("\\n");
                        break;
                    case '\t':
                        escaped.Append("\\t");
                        break;
                    default:
                        if (char.IsControl(c) && c < 0x80)
                        {
                            escaped.Append("\\u").Append(((int)c).ToString("X4", System.Globalization.CultureInfo.InvariantCulture));
                        }
                        else
                        {
                            escaped.Append(c);
                        }
                        break;
                }
            }
            return escaped.ToString();
        }

        /// <summary>
        /// Appends the message and stack trace from the exception and its inner exception to the provided StringBuilders.
        /// If the exception or its stack trace is null, the current stack trace is captured instead.
        /// </summary>
        /// <param name="ex">The exception to extract information from, or null to capture the current stack trace.</param>
        /// <param name="message">The StringBuilder to append message text to.</param>
        /// <param name="stack">The StringBuilder to append stack trace text to.</param>
		public static void SetMessageAndStackTrace(Exception ex, StringBuilder message, StringBuilder stack)
		{
			if (ex != null)
			{
				message.AppendFormat("\r\n{0}\r\n", EscapeControlCharacters(ex.Message));
				if (ex.StackTrace != null)
				{
					stack.AppendFormat("\r\n{0}\r\n", ex.StackTrace);
				}
				else
				{
					stack.AppendFormat("\r\n{0}\r\n", new System.Diagnostics.StackTrace(true).ToString());
				}

				if (ex.InnerException != null)
				{
					message.AppendFormat("\r\n{0}\r\n", EscapeControlCharacters(ex.InnerException.Message));
					if (ex.InnerException.StackTrace != null)
					{
						stack.AppendFormat("\r\n{0}\r\n", ex.InnerException.StackTrace);
					}
					else
					{
						stack.AppendFormat("\r\n{0}\r\n", new System.Diagnostics.StackTrace(true).ToString());
					}
				}
			}
			else
			{
				stack.AppendFormat("\r\n{0}\r\n", new System.Diagnostics.StackTrace(true).ToString());
			}
		}
    }
}
