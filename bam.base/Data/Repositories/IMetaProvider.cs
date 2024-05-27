/*
	Copyright © Bryan Apellanes 2015  
*/
using System;
namespace Bam.Data.Repositories
{
	public interface IMetaProvider
	{
		Meta GetMeta(object data);
	}
}
