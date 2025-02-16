namespace Bam.Data
{
    public interface IDataTypeTranslator
    {
        DataTypes EnumFromType(Type type);
        Type TypeFromDbDataType(string dbDataType);
        Type TypeFromDataType(DataTypes dataType);
        DataTypes TranslateDataType(string sqlDataType);
    }
}
