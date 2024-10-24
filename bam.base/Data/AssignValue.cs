/*
	Copyright © Bryan Apellanes 2015  
*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Bam.Data
{
    /// <summary>
    /// Statement used to assign a value to a variable or parameter
    /// </summary>
    public class AssignValue: IParameterInfo
    {
        public AssignValue(string columnName, object? value, Func<string, string>? columnNameFormatter = null)
        {
            this.ColumnName = columnName;
            this.Value = value;
            this._number = new int?();
            this.ColumnNameFormatter = columnNameFormatter ?? (Func<string, string>)((c) => c);
			this.ParameterPrefix = "@";
        }

		public Func<string, string> ColumnNameFormatter { get; set; }
		public string ParameterPrefix { get; set; }
        public string ColumnName
        {
            get;
            set;
        }

        int? _number;
        public int? Number
        {
            get => _number;
            set => _number = value;
        }

        public int? SetNumber(int? value)
        {
            _number = value;
            return ++value;
        }

        public object? Value
        {
            get;
            set;
        }

        public string Operator
        {
            get => "=";
            set { }
        }        

        public override string ToString()
        {
            return $"{ColumnNameFormatter(ColumnName)} {this.Operator} {ParameterPrefix}{ColumnName}{Number} ";
        }

        public static IEnumerable<AssignValue> FromDynamic(dynamic obj, Func<string, string>? columnNameFormatter = null)
        {
            Args.ThrowIfNull(obj, "obj");
            Type type = obj.GetType();
            foreach(PropertyInfo prop in type.GetProperties())
            {
                yield return new AssignValue(prop.Name, prop.GetValue(obj), columnNameFormatter);
            }
        }

        public static IEnumerable<AssignValue> FromDictionary<TKey, TValue>(Dictionary<TKey, TValue> dictionary, Func<string, string>? columnNameFormatter = null) where TKey : notnull
        {
            Args.ThrowIfNull(dictionary);
            foreach(TKey key in dictionary.Keys)
            {
                yield return new AssignValue(key.ToString(), dictionary[key]?.ToString(), columnNameFormatter);
            }
        }
    }
}
