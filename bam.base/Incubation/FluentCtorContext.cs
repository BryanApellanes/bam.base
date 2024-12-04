using Bam.Incubation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bam.Services
{
    public class FluentCtorContext<I>
    {
        public FluentCtorContext(DependencyProvider dependencyProvider, string parameterName)
        {
            DependencyProvider = dependencyProvider;
            ParameterName = parameterName;
        }
        public DependencyProvider Use(object value)
        {
            DependencyProvider inc = DependencyProvider ?? new DependencyProvider();
            inc.SetCtorParam(typeof(I), ParameterName, value);
            return inc;
        }
        protected string ParameterName { get; set; }
        protected DependencyProvider DependencyProvider
        {
            get;
            set;
        }
    }
}
