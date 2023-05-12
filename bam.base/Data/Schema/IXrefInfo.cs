/*
	Copyright © Bryan Apellanes 2015  
*/
namespace Bam.Net.Data.Schema
{
    public interface IXrefInfo
    {
        string ListTableName { get; set; }
        string ParentTableName { get; set; }
        string XrefTableName { get; set; }

/*        string RenderAddToChildDaoCollection();
        string RenderXrefProperty();*/
    }
}