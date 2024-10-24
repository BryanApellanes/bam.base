﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Bam.Data;

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
