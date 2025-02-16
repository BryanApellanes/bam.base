using System.Reflection;
using Bam.DependencyInjection;
using Bam.Services;

namespace Bam
{
    public class ValueTransformerPipelineFactory
    {
        public ValueTransformerPipelineFactory(ServiceRegistry serviceRegistry)
        {
            this.ServiceRegistry = serviceRegistry;
        }

        public ServiceRegistry ServiceRegistry { get; set; }

        public ValueTransformerPipeline<T> Create<T>(string commaSeparatedTransformerNames)
        {
            return Create<T>(ServiceRegistry, commaSeparatedTransformerNames);
        }
        
        public ValueTransformerPipeline<T> Create<T>(params string[] transformerNames)
        {
            return Create<T>(ServiceRegistry, transformerNames);
        }

        public ValueTransformerPipeline<T> Create<T>(Assembly assembly, params string[] transformerNames)
        {
            return Create<T>(ServiceRegistry, assembly, transformerNames);
        }

        public static ValueTransformerPipeline<T> Create<T>(ServiceRegistry serviceRegistry, string commaSepartedTransformerNames)
        {
            return Create<T>(serviceRegistry, commaSepartedTransformerNames.DelimitSplit(","));
        }

        public static ValueTransformerPipeline<T> Create<T>(ServiceRegistry serviceRegistry, params string[] transformerNames)
        {
            return Create<T>(serviceRegistry, Assembly.GetExecutingAssembly(), transformerNames);
        }

        public static ValueTransformerPipeline<T> Create<T>(ServiceRegistry serviceRegistry, Assembly assembly, params string[] transformerNames)
        {
            List<Type> transformerTypes = assembly
                .GetTypes()
                .Where(type => type.HasCustomAttributeOfType<PipelineFactoryTransformerNameAttribute>())
                .ToList();

            Dictionary<string, ConstructorInfo> namedTransformerTypes = new Dictionary<string, ConstructorInfo>();
            //HashSet<string> selectedTransformerNames = new HashSet<string>(transformerNames);
            foreach(Type type in transformerTypes)
            {
                PipelineFactoryTransformerNameAttribute nameAttribute = type.GetCustomAttributeOfType<PipelineFactoryTransformerNameAttribute>();
                ConstructorInfo? ctor = type.GetConstructors().FirstOrDefault(c => c.HasCustomAttributeOfType<PipelineFactoryConstructorAttribute>());
                if (ctor == null)
                {
                    ctor = type.GetConstructor(Type.EmptyTypes);
                }
                if (ctor != null)
                {           
                    namedTransformerTypes.Add(nameAttribute.Name, ctor);
                }
                
            }
            
            ValueTransformerPipeline<T> pipeline = new ValueTransformerPipeline<T>();
            foreach(string name in transformerNames)
            {
                ConstructorInfo ctor = namedTransformerTypes[name];
                ParameterInfo[] ctorParameterInfos = ctor.GetParameters();
                IValueTransformer<byte[], byte[]> transformer = (IValueTransformer<byte[], byte[]>) serviceRegistry.Construct(ctor.DeclaringType, ctorParameterInfos.Select(p => p.ParameterType).ToArray());
                if (transformer != null)
                {
                    pipeline.Add(transformer);
                }
            }
            return pipeline;
        }
    }
}
