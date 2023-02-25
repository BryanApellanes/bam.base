using System;
using System.Collections.Generic;
using System.Text;

namespace Bam.Net
{
    public interface IValueConverter<TData> : IValueConverter
    {
        TData ConvertBytesToObject(byte[] bytes);

        byte[] ConvertObjectToBytes(TData value);
    }

    public interface IValueConverter
    {
        string ConvertBytesToString(byte[] bytes);

        byte[] ConvertStringToBytes(string value);
    }
}
