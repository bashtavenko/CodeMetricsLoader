using System;
using System.Globalization;
using System.Reflection;
using System.Xml.Linq;

using AutoMapper;
using CodeMetricsLoader.Data;

namespace CodeMetricsLoader
{
    public class AutoMapperConfig
    {
        public static void CreateMaps()
        {
            Mapper.CreateMap<XElement, Metrics>()
                .ConvertUsing(src => MapMetrics(src));
            
            Mapper.CreateMap<XElement, Target>()
                .ForMember(m => m.Name, opt => opt.MapFrom(src => GetStringAttribute(src, "Name")));

            Mapper.CreateMap<XElement, Data.Module>()
                .ForMember(m => m.Name, opt => opt.MapFrom(src => GetStringAttribute(src, "Name")))
                .ForMember(m => m.FileVersion, opt => opt.MapFrom(src => GetStringAttribute(src, "FileVersion")))
                .ForMember(m => m.AssemblyVersion, opt => opt.MapFrom(src => GetStringAttribute(src, "AssemblyVersion")))
                .ForMember(m => m.Metrics, opt => opt.MapFrom(src => MapMetrics(src.Element("Metrics"))));

            Mapper.CreateMap<XElement, Namespace>()
                .ForMember(m => m.Name, opt => opt.MapFrom(src => GetStringAttribute(src, "Name")))
                .ForMember(m => m.Metrics, opt => opt.MapFrom(src => MapMetrics(src.Element("Metrics"))));

            Mapper.CreateMap<XElement, Data.Type>()
                .ForMember(m => m.Name, opt => opt.MapFrom(src => GetStringAttribute(src, "Name")))
                .ForMember(m => m.Metrics, opt => opt.MapFrom(src => MapMetrics(src.Element("Metrics"))));

            Mapper.CreateMap<XElement, Member>()
                .ForMember(m => m.Name, opt => opt.MapFrom(src => GetStringAttribute(src, "Name")))                
                .ForMember(m => m.Metrics, opt => opt.MapFrom(src => MapMetrics(src.Element("Metrics"))));

            Mapper.CreateMap<Metrics, FactMetrics>()
                .ForMember(m => m.Module, opt => opt.Ignore())
                .ForMember(m => m.Type, opt => opt.Ignore())
                .ForMember(m => m.Namespace, opt => opt.Ignore())
                .ForMember(m => m.Member, opt => opt.Ignore());

            Mapper.CreateMap<Data.Module, DimModule>()
                .ForMember(m => m.Metrics, opt => opt.Ignore())
                .ForMember(m => m.Namespaces, opt => opt.Ignore());                

            Mapper.CreateMap<Data.Namespace, DimNamespace>()
                .ForMember(m => m.Types, opt => opt.Ignore())
                .ForMember(m => m.Metrics, opt => opt.Ignore());

            Mapper.CreateMap<Data.Type, DimType>()
                .ForMember(m => m.Members, opt => opt.Ignore())
                .ForMember(m => m.Metrics, opt => opt.Ignore());

            Mapper.CreateMap<Data.Member, DimMember>()
                .ForMember(m => m.Type, opt => opt.Ignore())
                .ForMember(m => m.Metrics, opt => opt.Ignore());

            Mapper.CreateMap<Data.Target, DimTarget>();                
        }

        private static Metrics MapMetrics(XElement elements)
        {
            Metrics metrics = new Metrics();
            foreach (var metricElement in elements.Elements("Metric"))
            {
                string name = GetStringAttribute(metricElement, "Name");
                int value = GetIntAttribute(metricElement, "Value");
                                
                PropertyInfo property = metrics.GetType().GetProperty(name);
                if (property == null)
                {
                    throw new LoaderException(string.Format("No {0} property", name));
                }

                property.SetValue(metrics, value);
            }

            return metrics;
        }        

        private static string GetStringAttribute(XElement element, string name)
        {
            return element.Attribute(name) != null ? element.Attribute(name).Value : string.Empty;            
        }

        private static int GetIntAttribute(XElement element, string name)
        {
            return int.Parse(GetStringAttribute(element, name), NumberStyles.AllowThousands);
        }

        private static int? GetNullableIntAttribute(XElement element, string name)
        {
            var value = GetStringAttribute(element, name);
            return !string.IsNullOrEmpty(value) ? Convert.ToInt32(value) : (int?)null;
        }
    }
}
