using System.Globalization;
using System.Text;

namespace Bam.Data
{
    /// <summary>
    /// Represents an embedding value as an ordered list of float components and owns the
    /// pgvector text-literal round-trip (<c>[v0,v1,...]</c>). Instances are immutable.
    /// </summary>
    public sealed class Vector
    {
        private readonly float[] _values;

        /// <summary>
        /// Creates a Vector from the specified components.
        /// </summary>
        /// <param name="values">The vector components, in order.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="values"/> is null.</exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="values"/> is empty.</exception>
        public Vector(IEnumerable<float> values)
        {
            if (values == null)
            {
                throw new ArgumentNullException(nameof(values));
            }
            _values = values.ToArray();
            if (_values.Length == 0)
            {
                throw new ArgumentException("A Vector requires at least one component.", nameof(values));
            }
        }

        /// <summary>
        /// Gets the vector components, in order.
        /// </summary>
        public IReadOnlyList<float> Values => _values;

        /// <summary>
        /// Gets the number of components (the vector's dimension).
        /// </summary>
        public int Dimensions => _values.Length;

        /// <summary>
        /// Renders the pgvector text literal for this vector, e.g. <c>[0.1,0.2,0.3]</c>.
        /// Components are formatted with the round-trip format specifier ("R") and the
        /// invariant culture so locale decimal separators never corrupt the literal.
        /// </summary>
        public override string ToString()
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.Append('[');
            for (int i = 0; i < _values.Length; i++)
            {
                if (i > 0)
                {
                    stringBuilder.Append(',');
                }
                stringBuilder.Append(_values[i].ToString("R", CultureInfo.InvariantCulture));
            }
            stringBuilder.Append(']');
            return stringBuilder.ToString();
        }

        /// <summary>
        /// Parses a pgvector text literal (e.g. <c>[0.1,0.2,0.3]</c>) into a Vector.
        /// </summary>
        /// <param name="value">The literal to parse.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
        /// <exception cref="FormatException">Thrown when the literal is not bracketed or a component is not a valid float.</exception>
        public static Vector Parse(string value)
        {
            if (value == null)
            {
                throw new ArgumentNullException(nameof(value));
            }
            string trimmed = value.Trim();
            if (trimmed.Length < 3 || trimmed[0] != '[' || trimmed[^1] != ']')
            {
                throw new FormatException($"'{value}' is not a pgvector literal: expected a bracketed, comma-delimited list of floats.");
            }
            string[] parts = trimmed[1..^1].Split(',');
            float[] components = new float[parts.Length];
            for (int i = 0; i < parts.Length; i++)
            {
                if (!float.TryParse(parts[i], NumberStyles.Float, CultureInfo.InvariantCulture, out components[i]))
                {
                    throw new FormatException($"'{parts[i]}' is not a valid vector component in '{value}'.");
                }
            }
            return new Vector(components);
        }

        /// <summary>
        /// Parses a pgvector text literal and validates that it has the expected dimension.
        /// </summary>
        /// <param name="value">The literal to parse.</param>
        /// <param name="expectedDimensions">The dimension the literal must have.</param>
        /// <exception cref="FormatException">Thrown when the parsed dimension does not equal <paramref name="expectedDimensions"/>.</exception>
        public static Vector Parse(string value, int expectedDimensions)
        {
            Vector vector = Parse(value);
            if (vector.Dimensions != expectedDimensions)
            {
                throw new FormatException($"Expected a vector of dimension {expectedDimensions} but '{value}' has dimension {vector.Dimensions}.");
            }
            return vector;
        }

        /// <summary>
        /// Converts a float array to a Vector.
        /// </summary>
        /// <param name="values">The vector components, in order.</param>
        public static implicit operator Vector(float[] values)
        {
            return new Vector(values);
        }

        /// <summary>
        /// Determines structural equality: same dimension and identical components in order.
        /// </summary>
        public override bool Equals(object? obj)
        {
            if (obj is Vector other)
            {
                return _values.SequenceEqual(other._values);
            }
            return false;
        }

        /// <summary>
        /// Gets a hash code consistent with structural equality.
        /// </summary>
        public override int GetHashCode()
        {
            HashCode hashCode = new HashCode();
            foreach (float component in _values)
            {
                hashCode.Add(component);
            }
            return hashCode.ToHashCode();
        }
    }
}
