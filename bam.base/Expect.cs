/*
	Copyright © Bryan Apellanes 2015  
*/

namespace Bam
{
    /// <summary>
    /// A utility for making assertions
    /// </summary>
    public static class Expect
    {
        /// <summary>
        /// Gets or sets whether exception messages should be HTML-encoded.
        /// </summary>
        public static bool ShouldHtmlEncodeExceptions { get; set; }

        /// <summary>
        /// Asserts that the boolean value is true. Throws an <see cref="ExpectationFailedException"/> if false.
        /// </summary>
        /// <param name="boolToCheck">The boolean value to check.</param>
        /// <param name="failureMessage">Optional message to include in the exception.</param>
        public static void ShouldBeTrue(this bool boolToCheck, string? failureMessage = null)
        {
            IsTrue(boolToCheck, failureMessage ?? "Expected <true>, Actual <false>");
        }

        /// <summary>
        /// Asserts that the boolean value is true. Throws an <see cref="ExpectationFailedException"/> if false.
        /// </summary>
        /// <param name="boolToCheck">The boolean value to check.</param>
        /// <param name="failureMessage">The message to include in the exception if the assertion fails.</param>
        public static void IsTrue(this bool boolToCheck, string failureMessage = "Expected <true>, Actual <false>")
        {
            if (!boolToCheck)
            {
                if (string.IsNullOrEmpty(failureMessage))
                {
                    throw new ExpectationFailedException(true, boolToCheck, ShouldHtmlEncodeExceptions);
                }
                else
                {
                    throw new ExpectationFailedException(failureMessage);
                }
            }
        }

        /// <summary>
        /// Asserts that a file exists at the specified path. Throws an <see cref="ExpectationFailedException"/> if the file does not exist.
        /// </summary>
        /// <param name="filePath">The file path to check.</param>
        /// <param name="failureMessage">The message to include in the exception if the file does not exist.</param>
        public static void FileExists(string filePath, string failureMessage = "File not found.")
        {
            Expect.IsTrue(File.Exists(filePath), failureMessage);
        }

        /// <summary>
        /// Asserts that the boolean value is false. Throws an <see cref="ExpectationFailedException"/> if true.
        /// </summary>
        /// <param name="boolToCheck">The boolean value to check.</param>
        /// <param name="failureMessage">Optional message to include in the exception.</param>
        public static void ShouldBeFalse(this bool boolToCheck, string? failureMessage = null)
        {
            IsFalse(boolToCheck, failureMessage!);
        }

        /// <summary>
        /// Asserts that the boolean value is false. Throws an <see cref="ExpectationFailedException"/> if true.
        /// </summary>
        /// <param name="boolToCheck">The boolean value to check.</param>
        public static void IsFalse(this bool boolToCheck)
        {
            IsFalse(boolToCheck, string.Empty);
        }

        /// <summary>
        /// Asserts that the boolean value is false. Throws an <see cref="ExpectationFailedException"/> if true.
        /// </summary>
        /// <param name="boolToCheck">The boolean value to check.</param>
        /// <param name="failureMessage">The message to include in the exception if the assertion fails.</param>
        public static void IsFalse(this bool boolToCheck, string failureMessage)
        {
            if (boolToCheck)
            {
                if (string.IsNullOrEmpty(failureMessage))
                {
                    throw new ExpectationFailedException(false, boolToCheck, ShouldHtmlEncodeExceptions);
                }
                else
                {
                    throw new ExpectationFailedException(failureMessage);
                }
            }
        }

        /// <summary>
        /// Asserts that the specified instance can be cast to type T and returns the cast result.
        /// Throws an <see cref="ExpectationFailedException"/> if the cast fails.
        /// </summary>
        /// <typeparam name="T">The target type to cast to.</typeparam>
        /// <param name="instance">The object to cast.</param>
        /// <param name="failureMessage">Optional message to include in the exception if the cast fails.</param>
        /// <returns>The instance cast to type T.</returns>
        public static T CanCast<T>(object instance, string? failureMessage = null)
        {
            try
            {
                return (T)instance;
            }
            catch (Exception ex)
            {
                string o = instance == null ? "[null]" : instance.GetType().Name;
                string exceptionMessage = failureMessage == null ? $"Couldn't cast object {o} to type {typeof(T).Name}: {ex.Message}" : $"{failureMessage}: {ex.Message}";
                throw new ExpectationFailedException(exceptionMessage);
            }
        }

        /// <summary>
        /// Executes the specified actionThatThrowsException action passing the exception to the specified 
        /// catchDelegate and throws an ExpectFailedException if the actionThatThrowsException doesn't
        /// throw an Exception
        /// </summary>
        /// <param name="actionThatThrowsException"></param>
        /// <param name="failureMessage"></param>
        public static void Throws(Action actionThatThrowsException, string? failureMessage = null)
        {
            Throws(actionThatThrowsException, null, failureMessage);
        }

        /// <summary>
        /// Executes the specified actionThatThrowsException action passing the exception to the specified 
        /// catchDelegate and throws an ExpectFailedException if the actionThatThrowsException doesn't
        /// throw an Exception
        /// </summary>
        /// <param name="actionThatThrowsException"></param>
        /// <param name="catchDelegate"></param>
        /// <param name="failureMessage"></param>
        public static void Throws(Action actionThatThrowsException, Action<Exception>? catchDelegate = null, string? failureMessage = null)
        {
            catchDelegate = catchDelegate ?? ((e) => { });
            bool thrown = false;
            try
            {
                actionThatThrowsException();
            }
            catch (Exception ex)
            {
                catchDelegate(ex);
                thrown = true;
            }

            if (!thrown)
            {
                if (string.IsNullOrEmpty(failureMessage))
                {
                    failureMessage = "Exception was not thrown";
                }

                throw new ExpectationFailedException(failureMessage);
            }
        }

        /// <summary>
        /// Checks if the specified "left" value is greater than the specified "right" value.
        /// </summary>
        /// <param name="left">int on the left of &gt;</param>
        /// <param name="right">int on the right of &gt;</param>
        public static void IsGreaterThan(int left, int right)
        {
            IsGreaterThan((long)left, (long)right);
        }

        /// <summary>
        /// Checks if the specified "left" value is greater than the specified "right" value.
        /// </summary>
        /// <param name="left">int on the left of &gt;</param>
        /// <param name="right">int on the right of &gt;</param>
        public static void IsGreaterThan(long left, long right)
        {
            IsGreaterThan(left, right, string.Format("{0} is not greater than {1}", left, right));
        }

        /// <summary>
        /// Checks if the specified "left" value is greater than the specified "right" value.
        /// </summary>
        /// <param name="left">int on the left of &gt;</param>
        /// <param name="right">int on the right of &gt;</param>
        public static void IsGreaterThan(ulong left, ulong right)
        {
            IsGreaterThan(left, right, string.Format("{0} is not greater than {1}", left, right));
        }

        /// <summary>
        /// Checks if the specified "left" value is greater than the specified "right" value.
        /// </summary>
        /// <param name="left">int on the left of &gt;</param>
        /// <param name="right">int on the right of &gt;</param>
        public static void IsGreaterThan(long left, long right, string failureMessage)
        {
            if (!(left > right))
                throw new ExpectationFailedException(failureMessage);
        }

        /// <summary>
        /// Checks if the specified "left" value is greater than the specified "right" value.
        /// </summary>
        /// <param name="left">int on the left of &gt;</param>
        /// <param name="right">int on the right of &gt;</param>
        public static void IsGreaterThan(ulong left, ulong right, string failureMessage)
        {
            if (!(left > right))
                throw new ExpectationFailedException(failureMessage);
        }

        /// <summary>
        /// Checks if the specified "left" value is greater than or equal to the specified "right" value.
        /// </summary>
        /// <param name="left">The value on the left side of the comparison.</param>
        /// <param name="right">The value on the right side of the comparison.</param>
        public static void IsGreaterThanOrEqualTo(int left, int right)
        {
            IsGreaterThanOrEqualTo(left, right, string.Format("{0} is not greater than or equal to {1}", left, right));
        }
        /// <summary>
        /// Checks if the specified "left" value is greater than or equal to the specified "right" value. 
        /// </summary>
        /// <param name="left">int on the left of &gt;=</param>
        /// <param name="right">int on the right of &gt;=</param>
        public static void IsGreaterThanOrEqualTo(int left, int right, string failureMessage)
        {
            if (!(left >= right))
                throw new ExpectationFailedException(failureMessage);
        }

        /// <summary>
        /// Checks if the specified "left" value is less than or equal to the specified "right" value.
        /// </summary>
        /// <param name="left">The value on the left side of the comparison.</param>
        /// <param name="right">The value on the right side of the comparison.</param>
        /// <param name="failureMessage">Optional message to include in the exception if the assertion fails.</param>
        public static void IsLessThanOrEqualTo(int left, int right, string? failureMessage = null)
        {
            if (!(left <= right))
            {
                throw new ExpectationFailedException(failureMessage ?? $"{left} is not less than or equal to {right}");
            }
        }

        /// <summary>
        /// Checks if the specified objects are the same using == (!=).
        /// </summary>
        /// <param name="expected"></param>
        /// <param name="actual"></param>
        public static void AreSame(object expected, object actual)
        {
            AreSame(expected, actual, string.Empty);
        }
        
        /// <summary>
        /// Checks if the specified objects are the same using == (!=).
        /// </summary>
        /// <param name="expected"></param>
        /// <param name="actual"></param>
        public static void AreSame(object expected, object actual, string failureMessage)
        {
            if (expected != actual)
            {
                if (!string.IsNullOrEmpty(failureMessage))
                    throw new ExpectationFailedException(failureMessage, ShouldHtmlEncodeExceptions);

                throw new ExpectationFailedException(expected.ToString()!, actual.ToString()!, ShouldHtmlEncodeExceptions);
            }
        }
        
        /// <summary>
        /// Asserts that the actual integer equals the expected value. Extension method form of <see cref="AreEqual(int, int, string)"/>.
        /// </summary>
        /// <param name="actual">The actual value.</param>
        /// <param name="expected">The expected value.</param>
        /// <param name="failureMessage">Optional message to include in the exception if values are not equal.</param>
        public static void IsEqualTo(this int actual, int expected, string failureMessage = "")
        {
            AreEqual(expected, actual, failureMessage);
        }

        /// <summary>
        /// Asserts that two integer values are equal. Throws an <see cref="ExpectationFailedException"/> if they are not.
        /// </summary>
        /// <param name="expected">The expected value.</param>
        /// <param name="actual">The actual value.</param>
        /// <param name="failureMessage">Optional message to include in the exception if values are not equal.</param>
        public static void AreEqual(int expected, int actual, string failureMessage = "")
        {
            if (expected != actual)
            {
                if (string.IsNullOrEmpty(failureMessage))
                {
                    throw new ExpectationFailedException(expected.ToString(), actual.ToString(), ShouldHtmlEncodeExceptions);
                }
                else
                {
                    throw new ExpectationFailedException(failureMessage);
                }
            }
        }
                        
        /// <summary>
        /// Asserts that the actual long equals the expected value. Extension method form of <see cref="AreEqual(long, long)"/>.
        /// </summary>
        /// <param name="actual">The actual value.</param>
        /// <param name="expected">The expected value.</param>
        public static void IsEqualTo(this long actual, long expected)
        {
            AreEqual(expected, actual);
        }

        /// <summary>
        /// Asserts that two long values are equal. Throws an <see cref="ExpectationFailedException"/> if they are not.
        /// </summary>
        /// <param name="expected">The expected value.</param>
        /// <param name="actual">The actual value.</param>
        public static void AreEqual(long expected, long actual)
        {
            AreEqual(expected, actual, "");
        }

        /// <summary>
        /// Asserts that two long values are equal. Throws an <see cref="ExpectationFailedException"/> if they are not.
        /// </summary>
        /// <param name="expected">The expected value.</param>
        /// <param name="actual">The actual value.</param>
        /// <param name="failureMessage">The message to include in the exception if values are not equal.</param>
        public static void AreEqual(long expected, long actual, string failureMessage)
        {
            if (expected != actual)
            {
                if (string.IsNullOrEmpty(failureMessage))
                {
                    throw new ExpectationFailedException(expected.ToString(), actual.ToString(), ShouldHtmlEncodeExceptions);
                }
                else
                {
                    throw new ExpectationFailedException(failureMessage);
                }
            }
        }

        /// <summary>
        /// Asserts that two byte arrays are equal in length and content. Throws an <see cref="ExpectationFailedException"/> if they differ.
        /// </summary>
        /// <param name="x">The first byte array.</param>
        /// <param name="y">The second byte array.</param>
        public static void AreEqual(byte[] x, byte[] y)
        {
            if (x.Length != y.Length)
            {
                throw new ExpectationFailedException("byte arrays are different lengths");
            }

            if (x == null && y != null)
            {
                throw new ExpectationFailedException("byte arrays are different");
            }

            if (x != null && y == null)
            {
                throw new ExpectationFailedException("byte arrays are different");
            }

            for (int i = 0; i < x!.Length; i++)
            {
                if (x[i] != y![i])
                {
                    throw new ExpectationFailedException("byte arrays are different");
                }
            }
        }

        /// <summary>
        /// Does an equality comparison using expected.Equals()
        /// </summary>
        /// <param name="expected">The expected value</param>
        /// <param name="actual">The actual value</param>
        public static void AreEqual(object expected, object actual)
        {
            AreEqual(expected, actual, "");
        }

        /// <summary>
        /// Asserts that the actual string equals the expected string. Extension method form of <see cref="AreEqual(string, string, string)"/>.
        /// </summary>
        /// <param name="actual">The actual string value.</param>
        /// <param name="expected">The expected string value.</param>
        /// <param name="failureMessage">Optional message to include in the exception if values are not equal.</param>
        public static void IsEqualTo(this string actual, string expected, string failureMessage = "")
        {
            AreEqual(expected, actual, failureMessage);
        }
        
        /// <summary>
        /// Does an equality comparison using expected.Equals()
        /// </summary>
        /// <param name="expected">The expected value</param>
        /// <param name="actual">The actual value</param>
        public static void AreEqual(string expected, string actual)
        {
            AreEqual(expected, actual, "");
        }

        /// <summary>
        /// Does an equality comparison using expected.Equals()
        /// </summary>
        /// <param name="expected">The expected value</param>
        /// <param name="actual">The actual value</param>
        /// <param name="failureMessage">The failureMessage to display if the comparison fails</param>
        public static void AreEqual(string expected, string actual, string failureMessage)
        {
            AreEqual((object)expected, (object)actual, failureMessage);
        }

        /// <summary>
        /// Checks if the specified objects are equal using the Equals() method.
        /// </summary>
        /// <param name="expected"></param>
        /// <param name="actual"></param>
        public static void AreEqual(object expected, object actual, string failureMessage)
        {
            if (((expected == null && actual != null) ||
                (actual == null && expected != null)) ||
                (expected != null && !expected.Equals(actual))
                )
            {
                if (string.IsNullOrEmpty(failureMessage))
                {
                    string expectString = expected == null! ? "null" : expected.ToString()!;
                    string actualString = actual == null! ? "null" : actual.ToString()!;
                    throw new ExpectationFailedException(expectString!, actualString!, ShouldHtmlEncodeExceptions);
                }
                else
                {
                    throw new ExpectationFailedException(failureMessage);
                }
            }
        }
		
        /// <summary>
        /// Throws an ExpectFailedException if the type doesn't 
        /// derive from the specified generic type T
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="objectToCheck"></param>
        public static void DerivesFromType<T>(this object objectToCheck)
        {
            DerivesFromType<T>(objectToCheck, string.Empty);
        }

        /// <summary>
        /// Throws an <see cref="ExpectationFailedException"/> if the object's type does not derive from type T.
        /// </summary>
        /// <typeparam name="T">The expected base type.</typeparam>
        /// <param name="objectToCheck">The object to check.</param>
        /// <param name="failureMessage">The message to include in the exception if the check fails.</param>
        public static void DerivesFromType<T>(this object objectToCheck, string failureMessage)
        {
            Type checkType = objectToCheck.GetType();
            if (!checkType.IsSubclassOf(typeof(T)))
            {
                if (string.IsNullOrEmpty(failureMessage))
                {
                    throw new ExpectationFailedException(typeof(T), objectToCheck, ShouldHtmlEncodeExceptions);
                }
                else
                {
                    throw new ExpectationFailedException(failureMessage);
                }
            }
        }

        /// <summary>
        /// Asserts that the current instance is of the specified generic type.
        /// Throws an excpetion if the assertion fails.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="objectToCheck"></param>
        public static void IsObjectOfType<T>(this object objectToCheck)
        {
            IsObjectOfType<T>(objectToCheck, $"{objectToCheck?.GetType()?.Name ?? "null"} is not of type {typeof(T).Name}");
        }

        /// <summary>
        /// Checks if the specified object is of type T using GetType().
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="objectToCheck"></param>
        public static void IsObjectOfType<T>(this object objectToCheck, string failureMessage)
        {
            if (objectToCheck?.GetType() != typeof(T))
            {
                if (string.IsNullOrEmpty(failureMessage))
                {
                    throw new ExpectationFailedException(typeof(T), objectToCheck!, ShouldHtmlEncodeExceptions);
                }

                throw new ExpectationFailedException(failureMessage);
            }
        }

        /// <summary>
        /// Asserts that the object is an instance of the specified generic type
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="objectToCheck"></param>
        /// <param name="failureMessage"></param>
        public static void IsInstanceOfType<T>(this object objectToCheck, string failureMessage = "")
        {
            if (!typeof(T).IsInstanceOfType(objectToCheck))
            {
                if (string.IsNullOrWhiteSpace(failureMessage))
                {
                    throw new ExpectationFailedException(typeof(T), objectToCheck, ShouldHtmlEncodeExceptions);
                }

                throw new ExpectationFailedException(failureMessage);
            }
        }

        /// <summary>
        /// Asserts that the string is null or empty. Throws an <see cref="ExpectationFailedException"/> if it has a value.
        /// </summary>
        /// <param name="value">The string to check.</param>
        public static void ShouldBeNullOrEmpty(this string value)
        {
            IsNullOrEmpty(value);
        }
        
        /// <summary>
        /// Asserts that the specified string is null or empty.  Throws
        /// an exception if the assertion fails.
        /// </summary>
        /// <param name="stringToCheck"></param>
        public static void IsNullOrEmpty(string stringToCheck)
        {
            IsNullOrEmpty(stringToCheck, string.Empty);
        }

        /// <summary>
        /// Asserts that the specified string is null or empty.  Throws
        /// an exception if the assertion fails.
        /// </summary>
        /// <param name="stringToCheck"></param>
        public static void IsNullOrEmpty(string stringToCheck, string failureMessage)
        {
            if (!string.IsNullOrEmpty(stringToCheck))
            {
                if (string.IsNullOrEmpty(failureMessage))
                    throw new ExpectationFailedException("null or empty string", stringToCheck);
                throw new ExpectationFailedException(failureMessage);
            }
        }

        /// <summary>
        /// Asserts that the specified string is not null or empty. Throws an <see cref="ExpectationFailedException"/> if it is.
        /// </summary>
        /// <param name="stringToCheck">The string to check.</param>
        public static void IsNotNullOrEmpty(string stringToCheck)
        {
            IsNotNullOrEmpty(stringToCheck, "");
        }

        /// <summary>
        /// Asserts that the specified string is not null or empty. Throws an <see cref="ExpectationFailedException"/> if it is.
        /// </summary>
        /// <param name="stringToCheck">The string to check.</param>
        /// <param name="failureMessage">The message to include in the exception if the assertion fails.</param>
        public static void IsNotNullOrEmpty(string stringToCheck, string failureMessage)
        {
            if (string.IsNullOrEmpty(stringToCheck))
            {
                if (string.IsNullOrEmpty(failureMessage))
                    throw new ExpectationFailedException("string with value", "null or empty string");
                throw new ExpectationFailedException(failureMessage);
            }
        }

        /// <summary>
        /// Checks if the specified object extends type T using the "is" operator.  The same as Extends&lt;T&gt;
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="objectToCheck"></param>
        public static void IsExtenderOfType<T>(object objectToCheck)
        {
            Extends<T>(objectToCheck);
        }

        /// <summary>
        /// Checks if the specified object extends type T using the "is" operator.
        /// </summary>
        /// <typeparam name="T">The type to be extended.</typeparam>
        /// <param name="objectToCheck">The object to check if it extends the specified type T.</param>
        public static void Extends<T>(object objectToCheck)
        {
            if (!(objectToCheck is T))
                throw new ExpectationFailedException($"{objectToCheck.GetType().Name} doesn't extend {typeof(T).Name}", ShouldHtmlEncodeExceptions);
        }

        /// <summary>
        /// Asserts that the int value is greater than the comparison value.
        /// </summary>
        /// <param name="valueToCheck">The value to check.</param>
        /// <param name="valueToCompareTo">The value to compare against.</param>
        /// <param name="message">Optional message to include in the exception if the assertion fails.</param>
        public static void ShouldBeGreaterThan(this int valueToCheck, int valueToCompareTo, string? message = null)
        {
            if (!(valueToCheck > valueToCompareTo))
            {
                if (!string.IsNullOrEmpty(message))
                {
                    throw new ExpectationFailedException(message);
                }

                throw new ExpectationFailedException($"value ({valueToCheck}) is not greater than ({valueToCompareTo})");
            }
        }

        /// <summary>
        /// Asserts that the long value is greater than the comparison value.
        /// </summary>
        /// <param name="valueToCheck">The value to check.</param>
        /// <param name="valueToCompareTo">The value to compare against.</param>
        /// <param name="message">Optional message to include in the exception if the assertion fails.</param>
        public static void ShouldBeGreaterThan(this long valueToCheck, long valueToCompareTo, string? message = null)
        {
            if (!(valueToCheck > valueToCompareTo))
            {
                if (!string.IsNullOrEmpty(message))
                {
                    throw new ExpectationFailedException(message);
                }

                throw new ExpectationFailedException($"value ({valueToCheck}) is not greater than ({valueToCompareTo})");
            }
        }

        /// <summary>
        /// Asserts that the ulong value is greater than the comparison value.
        /// </summary>
        /// <param name="valueToCheck">The value to check.</param>
        /// <param name="valueToCompareTo">The value to compare against.</param>
        /// <param name="message">Optional message to include in the exception if the assertion fails.</param>
        public static void ShouldBeGreaterThan(this ulong valueToCheck, ulong valueToCompareTo, string? message = null)
        {
            if (!(valueToCheck > valueToCompareTo))
            {
                if (!string.IsNullOrEmpty(message))
                {
                    throw new ExpectationFailedException(message);
                }

                throw new ExpectationFailedException($"value ({valueToCheck}) is not greater than ({valueToCompareTo})");
            }
        }

        /// <summary>
        /// Asserts that the uint value is greater than the comparison value.
        /// </summary>
        /// <param name="valueToCheck">The value to check.</param>
        /// <param name="valueToCompareTo">The value to compare against.</param>
        /// <param name="message">Optional message to include in the exception if the assertion fails.</param>
        public static void ShouldBeGreaterThan(this uint valueToCheck, uint valueToCompareTo, string? message = null)
        {
            if (!(valueToCheck > valueToCompareTo))
            {
                if (!string.IsNullOrEmpty(message))
                {
                    throw new ExpectationFailedException(message);
                }

                throw new ExpectationFailedException($"value ({valueToCheck}) is not greater than ({valueToCompareTo})");
            }
        }

        /// <summary>
        /// Asserts that the long value is greater than or equal to the comparison value.
        /// </summary>
        /// <param name="valueToCheck">The value to check.</param>
        /// <param name="valueToCompareTo">The value to compare against.</param>
        /// <param name="message">Optional message to include in the exception if the assertion fails.</param>
        public static void ShouldBeGreaterThanOrEqualTo(this long valueToCheck, long valueToCompareTo, string? message = null)
        {
            if (!(valueToCheck >= valueToCompareTo))
            {
                if (!string.IsNullOrEmpty(message))
                {
                    throw new ExpectationFailedException(message);
                }

                throw new ExpectationFailedException($"value ({valueToCheck}) is not greater than ({valueToCompareTo})");
            }
        }

        /// <summary>
        /// Asserts that the ulong value is greater than or equal to the comparison value.
        /// </summary>
        /// <param name="valueToCheck">The value to check.</param>
        /// <param name="valueToCompareTo">The value to compare against.</param>
        /// <param name="message">Optional message to include in the exception if the assertion fails.</param>
        public static void ShouldBeGreaterThanOrEqualTo(this ulong valueToCheck, ulong valueToCompareTo, string? message = null)
        {
            if (!(valueToCheck >= valueToCompareTo))
            {
                if (!string.IsNullOrEmpty(message))
                {
                    throw new ExpectationFailedException(message);
                }

                throw new ExpectationFailedException($"value ({valueToCheck}) is not greater than ({valueToCompareTo})");
            }
        }

        /// <summary>
        /// Asserts that the int value is greater than or equal to the comparison value.
        /// </summary>
        /// <param name="valueToCheck">The value to check.</param>
        /// <param name="valueToCompareTo">The value to compare against.</param>
        /// <param name="message">Optional message to include in the exception if the assertion fails.</param>
        public static void ShouldBeGreaterThanOrEqualTo(this int valueToCheck, int valueToCompareTo, string? message = null)
        {
            if (!(valueToCheck >= valueToCompareTo))
            {
                if (!string.IsNullOrEmpty(message))
                {
                    throw new ExpectationFailedException(message);
                }

                throw new ExpectationFailedException($"value ({valueToCheck}) is not greater than ({valueToCompareTo})");
            }
        }

        /// <summary>
        /// Asserts that the uint value is greater than or equal to the comparison value.
        /// </summary>
        /// <param name="valueToCheck">The value to check.</param>
        /// <param name="valueToCompareTo">The value to compare against.</param>
        /// <param name="message">Optional message to include in the exception if the assertion fails.</param>
        public static void ShouldBeGreaterThanOrEqualTo(this uint valueToCheck, uint valueToCompareTo, string? message = null)
        {
            if (!(valueToCheck >= valueToCompareTo))
            {
                if (!string.IsNullOrEmpty(message))
                {
                    throw new ExpectationFailedException(message);
                }

                throw new ExpectationFailedException($"value ({valueToCheck}) is not greater than ({valueToCompareTo})");
            }
        }
        
        /// <summary>
        /// Does a value equality check using <see cref="object.Equals(object)"/>.
        /// </summary>
        /// <param name="objectToCheck"></param>
        /// <param name="compareTo"></param>
        /// <param name="failureMessage"></param>
        /// <exception cref="ExpectationFailedException"></exception>
        public static void ShouldEqual(this object objectToCheck, object compareTo, string? failureMessage = null)
        {
            ShouldBeEqualTo(objectToCheck, compareTo, failureMessage);
        }

        /// <summary>
        /// Does a value equality check.
        /// </summary>
        /// <param name="objectToCheck"></param>
        /// <param name="compareTo"></param>
        /// <param name="failureMessage"></param>
        /// <exception cref="ExpectationFailedException"></exception>
        public static void ShouldBeEqualTo(this object objectToCheck, object compareTo, string? failureMessage = null)
        {
            if (!objectToCheck.Equals(compareTo))
            {
                if (!string.IsNullOrEmpty(failureMessage))
                {
                    throw new ExpectationFailedException(failureMessage);
                }

                throw new ExpectationFailedException($"Expected \r\n\r\n{objectToCheck?.ToString()}\r\n\r\n to equal\r\n\r\n{compareTo?.ToString()}");
            }
        }

        /// <summary>
        /// Asserts that the object is not equal to the comparison object using <see cref="object.Equals(object)"/>.
        /// Alias for <see cref="ShouldNotBeEqualTo"/>.
        /// </summary>
        /// <param name="objectToCheck">The object to check.</param>
        /// <param name="compareTo">The object to compare against.</param>
        /// <param name="failureMessage">Optional message to include in the exception if the assertion fails.</param>
        public static void ShouldNotEqual(this object objectToCheck, object compareTo, string? failureMessage = null)
        {
            ShouldNotBeEqualTo(objectToCheck, compareTo, failureMessage);
        }

        /// <summary>
        /// Asserts that the object is not equal to the comparison object using <see cref="object.Equals(object)"/>.
        /// </summary>
        /// <param name="objectToCheck">The object to check.</param>
        /// <param name="compareTo">The object to compare against.</param>
        /// <param name="failureMessage">Optional message to include in the exception if the assertion fails.</param>
        public static void ShouldNotBeEqualTo(this object objectToCheck, object compareTo, string? failureMessage = null)
        {
            if (objectToCheck.Equals(compareTo))
            {
                if (!string.IsNullOrEmpty(failureMessage))
                {
                    throw new ExpectationFailedException(failureMessage);
                }

                throw new ExpectationFailedException($"Expected {objectToCheck?.ToString()}.Equals({compareTo?.ToString()}) to be false");
            }
        }

        /// <summary>
        /// Asserts that the instance is exactly of type T (not a subclass). Throws an <see cref="ExpectationFailedException"/> otherwise.
        /// </summary>
        /// <typeparam name="T">The expected exact type.</typeparam>
        /// <param name="instance">The object to check.</param>
        /// <param name="failureMessage">Optional message to include in the exception if the assertion fails.</param>
        public static void ShouldBeOfType<T>(this object instance, string? failureMessage = null)
        {
            if (instance == null)
            {
                throw new ExpectationFailedException(failureMessage ?? $"instance was null and not of type {typeof(T).Name}");
            }
            Type type = instance.GetType();
            if (type != typeof(T))
            {
                throw new ExpectationFailedException(failureMessage ?? $"instance was null and not of type {typeof(T).Name}");
            }
        }

        /// <summary>
        /// Asserts that the instance is exactly of the specified type (not a subclass). Throws an <see cref="ExpectationFailedException"/> otherwise.
        /// </summary>
        /// <param name="instance">The object to check.</param>
        /// <param name="shouldBe">The expected exact type.</param>
        /// <param name="failureMessage">Optional message to include in the exception if the assertion fails.</param>
        public static void ShouldBeOfType(this object instance, Type shouldBe, string? failureMessage = null)
        {
            if (instance == null)
            {
                throw new ExpectationFailedException(failureMessage ?? $"instance was null and not of type {shouldBe.Name}");
            }
            Type type = instance.GetType();
            if (type != shouldBe)
            {
                throw new ExpectationFailedException(failureMessage ?? $"instance was null and not of type {shouldBe.Name}");
            }
        }

        /// <summary>
        /// Does a reference equality check.
        /// </summary>
        /// <param name="objectToCheck"></param>
        /// <param name="compareTo"></param>
        /// <param name="failureMessage"></param>
        /// <exception cref="ExpectationFailedException"></exception>
        public static void ShouldBe(this object objectToCheck, object compareTo, string? failureMessage = null)
        {
            if (objectToCheck != compareTo)
            {
                if (!string.IsNullOrEmpty(failureMessage))
                {
                    throw new ExpectationFailedException(failureMessage);
                }

                throw new ExpectationFailedException($"Expected {objectToCheck?.ToString()} == {compareTo?.ToString()}");
            }
        }

        /// <summary>
        /// Asserts that the two objects are not the same reference using the == operator.
        /// </summary>
        /// <param name="objectToCheck">The object to check.</param>
        /// <param name="compareTo">The object to compare against.</param>
        /// <param name="failureMessage">Optional message to include in the exception if the assertion fails.</param>
        public static void ShouldNotBe(this object objectToCheck, object compareTo, string? failureMessage = null)
        {
            if (objectToCheck == compareTo)
            {
                if (!string.IsNullOrEmpty(failureMessage))
                {
                    throw new ExpectationFailedException(failureMessage);
                }

                throw new ExpectationFailedException($"Expected {objectToCheck?.ToString()} != {compareTo?.ToString()}");
            }
        }

        /// <summary>
        /// Asserts that the nullable boolean value is true. Throws an <see cref="ExpectationFailedException"/> if false or null.
        /// </summary>
        /// <param name="valueToCheck">The nullable boolean value to check.</param>
        /// <param name="failureMessage">Optional message to include in the exception if the assertion fails.</param>
        public static void ShouldBeTrue(this bool? valueToCheck, string? failureMessage = null)
        {
            IsTrue(valueToCheck, failureMessage);
        }
        
        /// <summary>
        /// Asserts that the nullable boolean value is true. Throws an <see cref="ExpectationFailedException"/> if false or null.
        /// </summary>
        /// <param name="valueToCheck">The nullable boolean value to check.</param>
        /// <param name="failureMessage">Optional message to include in the exception if the assertion fails.</param>
        public static void IsTrue(this bool? valueToCheck, string? failureMessage = null)
        {
            if (valueToCheck != true)
            {
                if (!string.IsNullOrEmpty(failureMessage))
                {
                    throw new ExpectationFailedException(failureMessage);
                }

                throw new ExpectationFailedException("true", "false");
            }
        }
        
        /// <summary>
        /// Asserts that the object is null. Throws an <see cref="ExpectationFailedException"/> if it is not null.
        /// </summary>
        /// <param name="objectToCheck">The object to check.</param>
        /// <param name="failureMessage">Optional message to include in the exception if the assertion fails.</param>
        public static void ShouldBeNull(this object? objectToCheck, string? failureMessage = null)
        {
            IsNull(objectToCheck, failureMessage);
        }
        
        /// <summary>
        /// Throws an exception if the specified object is not null.
        /// </summary>
        /// <param name="objectToCheck">The object to verify for null. If this value is not null, an exception is thrown.</param>
        public static void IsNull(this object? objectToCheck)
        {
            IsNull(objectToCheck, $"{nameof(objectToCheck)} was not null as expected");
        }
        
        /// <summary>
        /// Throws an exception if the specified objectToCheck is null.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="objectToCheck"></param>
        /// <param name="failureMessage"></param>
        public static void IsNull(object? objectToCheck, string? failureMessage) 
        {
            if (objectToCheck != null)
            {
                if (!string.IsNullOrEmpty(failureMessage))
                {
                    throw new ExpectationFailedException(failureMessage);
                }

                throw new ExpectationFailedException("null", objectToCheck.GetType().Name,
                    ShouldHtmlEncodeExceptions);
            }
        }

        /// <summary>
        /// Asserts that the object is not null. Throws an <see cref="ExpectationFailedException"/> if it is null.
        /// </summary>
        /// <param name="objectToCheck">The object to check.</param>
        /// <param name="failureMessage">Optional message to include in the exception if the assertion fails.</param>
        public static void ShouldNotBeNull(this object objectToCheck, string? failureMessage = null)
        {
            IsNotNull(objectToCheck, failureMessage!);
        }

        /// <summary>
        /// Asserts that the object is not null. Throws an <see cref="ExpectationFailedException"/> if it is null.
        /// </summary>
        /// <param name="objectToCheck">The object to check.</param>
        public static void IsNotNull(this object objectToCheck)
        {
            IsNotNull(objectToCheck, string.Empty);
        }

        /// <summary>
        /// Asserts that the object is not null. Throws an <see cref="ExpectationFailedException"/> if it is null.
        /// </summary>
        /// <param name="objectToCheck">The object to check.</param>
        /// <param name="failureMessage">The message to include in the exception if the assertion fails.</param>
        public static void IsNotNull(this object objectToCheck, string failureMessage)
        {
            if (objectToCheck == null)
            {
                if (!string.IsNullOrEmpty(failureMessage))
                {
                    throw new ExpectationFailedException(failureMessage);
                }

                throw new ExpectationFailedException("object", "null", ShouldHtmlEncodeExceptions);
            }
        }

        /// <summary>
        /// Asserts that the string is not null or empty. Throws an <see cref="ExpectationFailedException"/> if blank.
        /// </summary>
        /// <param name="value">The string to check.</param>
        /// <param name="failureMessage">Optional message to include in the exception if the assertion fails.</param>
        public static void ShouldNotBeBlank(this string value, string? failureMessage = null)
        {
            IsNotBlank(value, failureMessage);
        }

        /// <summary>
        /// Asserts that the string is not null or empty. Throws an <see cref="ExpectationFailedException"/> if blank.
        /// </summary>
        /// <param name="value">The string to check.</param>
        /// <param name="failureMessage">Optional message to include in the exception if the assertion fails.</param>
        public static void IsNotBlank(this string value, string? failureMessage = null)
        {
            if (string.IsNullOrEmpty(value))
            {
                if (!string.IsNullOrEmpty(failureMessage))
                {
                    throw new ExpectationFailedException(failureMessage);
                }

                throw new ExpectationFailedException("any value", "[blank]");
            }
        }
        
        /// <summary>
        /// Throw an ExpectFailedException with the specified failureMessage
        /// </summary>
        /// <param name="failureMessage"></param>
        public static void Fail(string failureMessage = "Expect.Fail() was called to throw this exception.")
        {
            throw new ExpectationFailedException(failureMessage, ShouldHtmlEncodeExceptions);
        }
    }
}
