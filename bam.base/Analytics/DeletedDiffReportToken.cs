/*
	Copyright © Bryan Apellanes 2015  
*/

using System.Xml.Serialization;

namespace Bam.Analytics
{
    [Serializable]
    [XmlInclude(typeof(DiffReportToken))]
    public class DeletedDiffReportToken: DiffReportToken
    {
        public override DiffType Type
        {
            get
            {
                return DiffType.Deleted;
            }
        }
    }
}
