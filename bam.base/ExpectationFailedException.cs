/*
	Copyright © Bryan Apellanes 2015  
*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Runtime.Serialization;

namespace Bam.Net
{
    public partial class ExpectationFailedException: Exception
    {        
        private const string defaultMessage = "Unexpected result: Expected <{0}>, Actual <{1}>";

        public ExpectationFailedException(SerializationInfo info, StreamingContext context)
            : base(info.GetString("Message"))
        {
        }

        public ExpectationFailedException(string message)
            : base(message)
        {             
        }

        public ExpectationFailedException(string message, bool htmlEncode) :
            this(htmlEncode ? HttpUtility.HtmlEncode(message) : message)
        { }

        public ExpectationFailedException(string expected, string actual)
            : this(expected, actual, false)
        {
            this.Expected = expected;
            this.Actual = actual;
        }

        public ExpectationFailedException(string expected, string actual, bool htmlEncode)
            : this(string.Format(htmlEncode ? HttpUtility.HtmlEncode(defaultMessage) : defaultMessage, expected,
                actual))
        {
            this.Expected = expected;
            this.Actual = actual;
        }

        public ExpectationFailedException(bool expected, bool actual)
            : this(expected.ToString(), actual.ToString())
        {
            this.Expected = expected;
            this.Actual = actual;
        }

        public ExpectationFailedException(bool expected, bool actual, bool htmlEncode)
            : this(expected.ToString(), actual.ToString(), htmlEncode)
        {
            this.Expected = expected;
            this.Actual = actual;
        }

        public ExpectationFailedException(Type expected, object actual)
            : this(expected.Name, actual == null ? "null" : actual.GetType().Name)
        {
            this.Expected = expected;
            this.Actual = actual?.GetType();
        }

        public ExpectationFailedException(Type expected, object actual, bool htmlEncode)
            : this(expected.Name, actual == null ? "null" : actual.GetType().Name, htmlEncode)
        {
            this.Expected = expected;
            this.Actual = actual?.GetType();
        }
        
        public object Expected { get; set; }
        public object Actual { get; set; }
    }
}
