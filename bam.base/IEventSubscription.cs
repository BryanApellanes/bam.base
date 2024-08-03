using System;
using System.Reflection;
using System.Threading.Tasks;

namespace Bam
{
    public interface IEventSubscription
    {
        Delegate Delegate { get; set; }
        EventInfo EventInfo { get; set; }
        string EventName { get; set; }
        FieldInfo FieldInfo { get; set; }

        bool Equals(EventHandler handler);
        bool Equals(object obj);
        object? Invoke(params object[] args);
        Task<object?> InvokeAsync(params object[] args);
    }
}