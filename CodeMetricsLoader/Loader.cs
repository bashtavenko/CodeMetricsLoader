using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using AutoMapper;
using CodeMetricsLoader.Data;

namespace CodeMetricsLoader
{
    public class Loader
    {
        private readonly LoaderContext _context;        
        private readonly ILogger _logger;        

        /// <summary>
        /// Initializes a new instance of the <see cref="Loader"/> class
        /// </summary>
        /// <param name="context">Entity Framework context</param>
        /// <param name="logger">logger</param>
        public Loader(LoaderContext context, ILogger logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// Load metrics from xml and save them to database
        /// </summary>
        /// <param name="elements">Root node of the metrics tree</param>
        /// <param name="tag">Optional build or repository tag</param>
        public void Load(XElement elements, string tag)
        {
            List<Target> targets = MapXmlToEntities(elements);
            _logger.Log("Saving to database...");
            SaveTargets(targets, tag);
            _logger.Log("Done.");
        }

        /// <summary>
        /// Map XElments with metric xml to EF DTOs
        /// </summary>
        /// <param name="elements">metric xml nodes</param>
        /// <param name="tag">optional tag</param>
        /// <returns>Collection of DTOs</returns>
        public List<Target> MapXmlToEntities(XElement elements)
        {   
            if (elements == null || elements.Elements().Count() != 1)
            {
                throw new LoaderException("Invalid xml"); 
            }
            
            AutoMapperConfig.CreateMaps();

            var targets = new List<Target>();
            foreach (var targetElement in elements.Descendants("Target"))
            {                
                var target = Mapper.Map<Target>(targetElement);                

                targets.Add(target);

                foreach (var moduleElement in targetElement.Descendants("Module"))
                {             
                    var module = Mapper.Map<Module>(moduleElement);                    
                    target.Modules.Add(module);

                    foreach (var namespaceElement in moduleElement.Descendants("Namespace"))
                    {
                        var ns = Mapper.Map<Data.Namespace>(namespaceElement);                        
                        module.Namespaces.Add(ns);

                        foreach (var typeElement in namespaceElement.Descendants("Type"))
                        {
                            var type = Mapper.Map<Data.Type>(typeElement);                            
                            ns.Types.Add(type);

                            foreach (var memberElement in typeElement.Descendants("Member"))
                            {
                                var member = Mapper.Map<Member>(memberElement);                                
                                type.Members.Add(member);
                            }
                        }
                    }
                }
            }

            return targets;
        }

        /// <summary>
        /// Save DTOs to database
        /// </summary>
        /// <param name="targets">DTOs to save</param>
        public void SaveTargets(List<Target> targets, string tag)
        {
            var dimDate = new DimDate();            
            foreach (var target in targets)
            {
                var dimRun = new DimRun { Tag = tag, Target = target.Name };                            
                foreach (var module in target.Modules)
                {
                    dimRun = Mapper.Map<Data.Module, DimRun>(module, dimRun);
                    dimRun = GetEntityFromDbOrSource(dimRun);
                    InsertMetrics(module.Metrics, dimRun, dimDate);                    
                    
                    foreach (var ns in module.Namespaces)
                    {
                        dimRun = Mapper.Map<DimRun, DimRun>(dimRun, new DimRun());
                        dimRun = Mapper.Map<Data.Namespace, DimRun>(ns, dimRun);
                        dimRun = GetEntityFromDbOrSource(dimRun);
                        InsertMetrics(ns.Metrics, dimRun, dimDate);                    

                        foreach (var type in ns.Types)
                        {
                            dimRun = Mapper.Map<DimRun, DimRun>(dimRun, new DimRun());
                            dimRun = Mapper.Map<Data.Type, DimRun>(type, dimRun);
                            dimRun = GetEntityFromDbOrSource(dimRun);
                            InsertMetrics(type.Metrics, dimRun, dimDate);             

                            foreach (var member in type.Members)
                            {
                                dimRun = Mapper.Map<DimRun, DimRun>(dimRun, new DimRun());
                                dimRun = Mapper.Map<Data.Member, DimRun>(member, dimRun);
                                dimRun = GetEntityFromDbOrSource(dimRun);
                                InsertMetrics(member.Metrics, dimRun, dimDate);                                        
                            }
                        }
                    }
                }
            }
        }


        private DimRun GetEntityFromDbOrSource(DimRun dimRun)
        {
            var dimRunDb = _context.Runs
                .Where(r => r.Tag == dimRun.Tag &&
                            r.Module == dimRun.Module &&
                            r.Namespace == dimRun.Namespace &&
                            r.Type == dimRun.Type &&
                            r.Member == dimRun.Member)
                            .Take(2);

            return (dimRunDb != null && dimRunDb.Count() == 1) ? dimRunDb.First() : dimRun;            
        }

        private void InsertMetrics(Metrics metrics, DimRun dimRun, DimDate dimDate)
        {
            FactMetrics factMetrics = Mapper.Map<FactMetrics>(metrics);
            factMetrics.Run = dimRun;
            factMetrics.Date = dimDate;
            _context.Metrics.Add(factMetrics);
            _context.SaveChanges();
        }
    }
}
