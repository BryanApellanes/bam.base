/*
	Copyright © Bryan Apellanes 2015  
*/

namespace Bam.Data.Repositories
{
	/// <summary>
	/// Specifies that a property should be used as the 
	/// primary key for an object
	/// </summary>
	[AttributeUsage(AttributeTargets.Property)]
	public class KeyAttribute: Attribute
	{
	}
}
