using System;
using System.Collections.Generic;
using System.Data.Entity;
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
            AutoMapperConfig.CreateMaps();
        }

        /// <summary>
        /// Load metrics from xml and save them to database
        /// </summary>
        /// <param name="elements">Root node of the metrics tree</param>
        /// <param name="tag">Optional build or repository tag</param>
        public void Load(XElement elements, string tag, bool useDateTime)
        {
            List<Target> targets = MapXmlToEntities(elements);
            _logger.Log("Saving to database...");
            SaveTargets(targets, tag, useDateTime);
            _logger.Log("Done.");
        }

        /// <summary>
        /// Map XElements with metric xml to EF DTOs
        /// </summary>
        /// <param name="elements">metric xml nodes</param>        
        /// <returns>Collection of DTOs</returns>
        public List<Target> MapXmlToEntities(XElement elements)
        {   
            if (elements == null || elements.Elements().Count() != 1)
            {
                throw new LoaderException("Invalid xml"); 
            }

            string version = elements.FirstAttribute.Value;
            if (version != "12.0")
            {
                throw new LoaderException(string.Format("Version '{0}' of metrics xml is not supported", version));
            }        

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
        /// <param name="tag">Tag name</param>
        public void SaveTargets(List<Target> targets, string tag, bool useDateTime)
        {
            var dimDate = new DimDate();
            if (!useDateTime)            
            {
                dimDate = GetOrAddEntity<DimDate>(_context.Dates, dimDate, delegate(DimDate d) { return d.Date == dimDate.Date;});
            }
            FactMetrics factMetrics;
            foreach (var target in targets)
            {
                var dimTarget = Mapper.Map<DimTarget>(target);
                dimTarget.Tag = tag;
                dimTarget = GetOrAddEntity<DimTarget>(_context.Targets, dimTarget, delegate(DimTarget t) { return t.Tag == tag && t.FileName == target.FileName; });
                foreach (var module in target.Modules)
                {
                    var dimModule = Mapper.Map<DimModule>(module);
                    dimModule.TargetId = dimTarget.TargetId;
                    dimModule = GetOrAddEntity<DimModule>(_context.Modules, dimModule, delegate(DimModule m) { return m.Name == dimModule.Name && m.TargetId == dimModule.TargetId; });
                    factMetrics = Mapper.Map<FactMetrics>(module.Metrics);
                    factMetrics.Module = dimModule;
                    InsertMetrics(factMetrics, dimDate);                                                                      
                    foreach (var ns in module.Namespaces)
                    {
                        var dimNamespace = Mapper.Map<DimNamespace>(ns);
                        dimNamespace.ModuleId = dimModule.ModuleId;
                        dimNamespace = GetOrAddEntity<DimNamespace>(_context.Namespaces, dimNamespace, delegate(DimNamespace n) { return n.Name == dimNamespace.Name && n.ModuleId == dimNamespace.ModuleId; });
                        factMetrics = Mapper.Map<FactMetrics>(ns.Metrics);
                        factMetrics.Namespace = dimNamespace;
                        InsertMetrics(factMetrics, dimDate);
                        foreach (var type in ns.Types)
                        {
                            var dimType = Mapper.Map<DimType>(type);
                            dimType.NamespaceId = dimNamespace.NamespaceId;                            
                            dimType = GetOrAddEntity<DimType>(_context.Types, dimType, delegate(DimType t) { return t.Name == dimType.Name && t.NamespaceId == dimType.NamespaceId; });
                            factMetrics = Mapper.Map<FactMetrics>(type.Metrics);
                            factMetrics.Type = dimType;
                            InsertMetrics(factMetrics, dimDate);                            
                            foreach (var member in type.Members)
                            {
                                var dimMember = Mapper.Map<DimMember>(member);
                                dimMember.TypeId = dimType.TypeId;
                                dimMember = GetOrAddEntity<DimMember>(_context.Members, dimMember, delegate(DimMember m) { return m.Name == dimMember.Name && m.TypeId == dimMember.TypeId; });
                                factMetrics = Mapper.Map<FactMetrics>(member.Metrics);
                                factMetrics.Member = dimMember;
                                InsertMetrics(factMetrics, dimDate);                                                
                            }                    
                        }
                    }
                }
            }
        }      

        /// <summary>
        /// Gets existing entity from db or adds one
        /// </summary>
        /// <typeparam name="T">Dimension (type, module, namespace, etc)</typeparam>
        /// <param name="list">List of these entities from the context</param>
        /// <param name="src">Entity itself</param>
        /// <param name="where">Where clause used to search for this entity</param>
        /// <returns>Newly added entity of entity from database</returns>
        private T GetOrAddEntity<T>(DbSet<T> list, T src, Func<T, bool> where) where T : class
        {
            IEnumerable<T> srcFromDb = list
                .Where(where)
                .Take(2);

            if (srcFromDb != null && srcFromDb.Count() == 1)
            {
                return srcFromDb.First();
            }
            else
            {
                list.Add(src);
                _context.SaveChanges();
                return src;
            }
        }
                
        /// <summary>
        /// Inserts fact for the give date. It is assumed that facMetrics's corresponsing member (Type, Module, Namespace) has
        /// already been set.
        /// </summary>
        /// <param name="factMetrics">Fact metric to insert</param>
        /// <param name="dimDate">Date dimension</param>
        private void InsertMetrics(FactMetrics factMetrics, DimDate dimDate)
        {
            factMetrics.Date = dimDate;
            _context.Metrics.Add(factMetrics);
            _context.SaveChanges();
        }
    }
}
