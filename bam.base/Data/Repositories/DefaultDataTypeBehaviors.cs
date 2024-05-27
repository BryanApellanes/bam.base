/*
	Copyright © Bryan Apellanes 2015  
*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bam.Data.Repositories
{
	/// <summary>
	/// Enumerates the desired behaviors if a data type is not explicitly specified when managing
	/// schema object properties.  This is relevant when DataTypes.Default is encountered.
	/// </summary>
	public enum DefaultDataTypeBehaviors
	{
		Invalid,
		Exclude,
		IncludeAsString,
		IncludeAsByteArray		
	}
}
