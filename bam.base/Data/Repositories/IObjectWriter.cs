using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Bam.Data.Repositories
{
    public interface IObjectWriter//: IObjectPersisterDirectoryProvider
    {
        void Enqueue(object data);
        void Enqueue(Type type, object data);
        Task WriteAsync(object data);
        Task WriteAsync(Type type, object data);
        Task WritePropertyAsync(PropertyInfo prop, object propertyValue, object parentData);
        bool Delete(object data, Type type = null);

        event EventHandler WriteObjectStarted;
        event EventHandler WriteObjectComplete;
        
        event EventHandler WriteObjectFailed;
        event EventHandler WriteObjectPropertiesFailed;
        event EventHandler DeleteFailed;
    }
}
