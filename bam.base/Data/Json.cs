using System.Diagnostics.CodeAnalysis;

namespace Bam.Data
{
    /// <summary>
    /// Represents one raw JSON text value. Instances are immutable. The value is carried as-is —
    /// no parsing or validation — because the type exists to distinguish JSON columns from plain
    /// string columns in the data pipeline (type translation, DDL rendering, and native jsonb
    /// parameter binding), not to model JSON structure.
    /// </summary>
    public sealed class Json : IEquatable<Json>
    {
        /// <summary>
        /// Creates a Json value from the specified raw JSON text.
        /// </summary>
        /// <param name="value">The raw JSON text.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
        public Json(string value)
        {
            if (value == null)
            {
                throw new ArgumentNullException(nameof(value));
            }
            this.Value = value;
        }

        /// <summary>
        /// Gets the raw JSON text.
        /// </summary>
        public string Value { get; }

        /// <summary>
        /// Gets a Json value holding an empty JSON array (<c>[]</c>).
        /// </summary>
        public static Json EmptyArray { get; } = new Json("[]");

        /// <summary>
        /// Gets a Json value holding an empty JSON object (<c>{}</c>).
        /// </summary>
        public static Json EmptyObject { get; } = new Json("{}");

        /// <summary>
        /// Gets the raw JSON text.
        /// </summary>
        public override string ToString()
        {
            return Value;
        }

        /// <summary>
        /// Converts raw JSON text to a Json value.
        /// </summary>
        /// <param name="value">The raw JSON text.</param>
        public static implicit operator Json(string value)
        {
            return new Json(value);
        }

        /// <summary>
        /// Converts a Json value to its raw JSON text.
        /// </summary>
        /// <param name="json">The Json value.</param>
        [return: NotNullIfNotNull(nameof(json))]
        public static implicit operator string?(Json? json)
        {
            return json?.Value;
        }

        /// <summary>
        /// Determines value equality: identical raw JSON text (ordinal comparison).
        /// </summary>
        public bool Equals(Json? other)
        {
            if (other is null)
            {
                return false;
            }
            return Value.Equals(other.Value, StringComparison.Ordinal);
        }

        /// <summary>
        /// Determines value equality: identical raw JSON text (ordinal comparison).
        /// </summary>
        public override bool Equals(object? obj)
        {
            return obj is Json other && Equals(other);
        }

        /// <summary>
        /// Determines value equality: identical raw JSON text (ordinal comparison).
        /// </summary>
        public static bool operator ==(Json? left, Json? right)
        {
            if (ReferenceEquals(left, right))
            {
                return true;
            }
            if (left is null || right is null)
            {
                return false;
            }
            return left.Equals(right);
        }

        /// <summary>
        /// Determines value inequality: differing raw JSON text (ordinal comparison).
        /// </summary>
        public static bool operator !=(Json? left, Json? right)
        {
            return !(left == right);
        }

        /// <summary>
        /// Gets a hash code consistent with value equality.
        /// </summary>
        public override int GetHashCode()
        {
            return Value.GetHashCode();
        }
    }
}
