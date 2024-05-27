/*
	Copyright © Bryan Apellanes 2015  
*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bam.Data
{
	public interface IAddable 
	{
		void Add(object value);
        void Clear(IDatabase db = null);
	}
}
