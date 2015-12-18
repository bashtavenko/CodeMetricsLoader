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
            Mapper.CreateMap<XElement, Metrics>().ConvertUsing(MapMetrics);
            Mapper.CreateMap<XElement, Target>().ConvertUsing(MapTarget);
            Mapper.CreateMap<XElement, Data.Module>().ConvertUsing(MapModule);
            Mapper.CreateMap<XElement, Namespace>().ConvertUsing(MapNamespace);
            Mapper.CreateMap<XElement, Data.Type>().ConvertUsing(MapType);
            Mapper.CreateMap<XElement, Member>().ConvertUsing(MapMember);

            Mapper.CreateMap<Metrics, FactMetrics>()
                .ForMember(m => m.Module, opt => opt.Ignore())
                .ForMember(m => m.Type, opt => opt.Ignore())
                .ForMember(m => m.Namespace, opt => opt.Ignore())
                .ForMember(m => m.Member, opt => opt.Ignore())
                .ForMember(m => m.CodeCoverage, opt => opt.MapFrom(src => src.CodeCoverage ?? 0));
            
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
                .ForMember(m => m.Types, opt => opt.Ignore())
                .ForMember(m => m.File, opt => opt.MapFrom(src => src.FileName))
                .ForMember(m => m.Metrics, opt => opt.Ignore());
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
                    throw new LoaderException($"No {name} property");
                }

                property.SetValue(metrics, value);
            }

            return metrics;
        }        

        public static string GetStringAttribute(XElement element, string name)
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

        private static Target MapTarget(XElement element)
        {
            return new Target(GetStringAttribute(element, "Name"));
        }

        private static Data.Module MapModule(XElement element)
        {
            return new Data.Module(GetStringAttribute(element, "Name"))
            {
                FileVersion = GetStringAttribute(element, "FileVersion"),
                AssemblyVersion = GetStringAttribute(element, "AssemblyVersion"),
                Metrics = MapMetrics(element.Element("Metrics"))
            };
        }

        private static Namespace MapNamespace(XElement element)
        {
            return new Namespace(GetStringAttribute(element, "Name"))
            {
                Metrics = MapMetrics(element.Element("Metrics"))
            };
        }

        private static Data.Type MapType(XElement element)
        {
            return new Data.Type(GetStringAttribute(element, "Name"))
            {
                Metrics = MapMetrics(element.Element("Metrics"))
            };
        }

        private static Member MapMember(XElement element)
        {
            return new Member(GetStringAttribute(element, "Name"))
            {
                File = GetStringAttribute(element, "File"),
                Line = GetNullableIntAttribute(element, "Line"),
                Metrics = MapMetrics(element.Element("Metrics"))
            };
        }
    }
}
