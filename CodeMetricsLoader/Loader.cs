using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Validation;
using System.Linq;
using System.Text;
using System.Xml.Linq;

using AutoMapper;
using CodeMetricsLoader.CodeCoverage;
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
        /// <param name="metricsElements"></param>
        /// <param name="codeCoverageElements"></param>
        /// <param name="useDateTime">Use both date and time</param>
        public void Load(XElement metricsElements, XElement codeCoverageElements, bool useDateTime)
        {
            if (metricsElements == null)
            {
                throw new ArgumentException("Must have metrics");
            }
            IList<Target> targets;
            List<Target> metricsTargets = MapMetricsXmlToEntities(metricsElements);
            
            if (codeCoverageElements != null)
            {
                var parser = new OpenCoverParser();
                List<Target> codeCoverageTargets = parser.Parse(codeCoverageElements);
                var merger = new Merger();
                targets = merger.Merge(metricsTargets, codeCoverageTargets, _logger);
            }
            else 
            {
                targets = metricsTargets;
            }
            
            _logger.Log("Saving to database...");
            SaveTargets(targets, useDateTime);
            _logger.Log("Done.");
        }

        /// <summary>
        /// Map XElements with metric xml to EF DTOs
        /// </summary>
        /// <param name="elements">metric xml nodes</param>        
        /// <returns>Collection of DTOs</returns>
        public List<Target> MapMetricsXmlToEntities(XElement elements)
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
        /// <param name="useDateTime">Use both date and time</param>
        public void SaveTargets(IList<Target> targets, bool useDateTime)
        {
            _context.Configuration.AutoDetectChangesEnabled = false;
            
            var dimDate = new DimDate();
            
            if (!useDateTime)
            {
                var date = dimDate.Date;
                dimDate = GetOrAddEntity(_context.Dates, dimDate, d => d.Date.Date == date);
            }
            FactMetrics factMetrics;
            foreach (var module in targets.SelectMany(m => m.Modules))
            {
                // TODO: this won't work if module is used in multiple projects
                if (HaveDataForThisDate(module.Name, dimDate.Date))
                {
                    _logger.Log(string.Format("Already have data for module {0} and this date", module.Name));
                    continue;
                }

                var dimModule = Mapper.Map<DimModule>(module);
                string moduleName = dimModule.Name;
                dimModule = GetOrAddEntity(_context.Modules, dimModule, m => m.Name == moduleName);
                factMetrics = Mapper.Map<FactMetrics>(module.Metrics);
                factMetrics.Module = dimModule;
                factMetrics.Date = dimDate;
                dimModule.Metrics.Add(factMetrics);
                foreach (var ns in module.Namespaces)
                {
                    var dimNamespace = Mapper.Map<DimNamespace>(ns);
                    string namespaceName = dimNamespace.Name;
                    dimNamespace = GetOrAddEntity(_context.Namespaces, dimNamespace, n => n.Name == namespaceName);
                    dimNamespace.Modules.Add(dimModule);
                    factMetrics = Mapper.Map<FactMetrics>(ns.Metrics);
                    factMetrics.Module = dimModule;
                    factMetrics.Namespace = dimNamespace;
                    factMetrics.Date = dimDate;
                    dimNamespace.Metrics.Add(factMetrics);
                    foreach (var type in ns.Types)
                    {
                        var dimType = Mapper.Map<DimType>(type);
                        string typeName = dimType.Name;
                        dimType = GetOrAddEntity(_context.Types, dimType, t => t.Name == typeName);
                        dimType.Namespaces.Add(dimNamespace);
                        factMetrics = Mapper.Map<FactMetrics>(type.Metrics);
                        factMetrics.Module = dimModule;
                        factMetrics.Namespace = dimNamespace;
                        factMetrics.Type = dimType;
                        factMetrics.Date = dimDate;
                        dimType.Metrics.Add(factMetrics);
                        foreach (var member in type.Members)
                        {
                            var dimMember = Mapper.Map<DimMember>(member);
                            string memberName = dimMember.Name;
                            string memberFileName = dimMember.File;
                            int? memberLine = dimMember.Line;
                            dimMember = GetOrAddEntity(_context.Members, dimMember, m => m.Name == memberName && m.File == memberFileName && m.Line == memberLine);
                            dimMember.Types.Add(dimType);
                            factMetrics = Mapper.Map<FactMetrics>(member.Metrics);
                            factMetrics.Module = dimModule;
                            factMetrics.Namespace = dimNamespace;
                            factMetrics.Type = dimType;
                            factMetrics.Member = dimMember;
                            factMetrics.Date = dimDate;
                            dimMember.Metrics.Add(factMetrics);
                        }
                    }
                }
            }
            try
            {
                _context.Configuration.AutoDetectChangesEnabled = true;
                _context.SaveChanges();
            }
            catch (DbEntityValidationException ex)
            {
                var sb = new StringBuilder("Failed to save\n");
                foreach (var entity in ex.EntityValidationErrors)
                {
                    sb.AppendLine(string.Format("Entity: {0}", entity.Entry.Entity));
                    foreach (var error in entity.ValidationErrors)
                    {
                        sb.AppendLine(string.Format("Property: {0}", error.PropertyName));
                        sb.AppendLine(string.Format("Error: {0}", error.ErrorMessage));
                    }
                    sb.AppendLine();
                }
                throw new ApplicationException(sb.ToString(), ex);
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
        private static T GetOrAddEntity<T>(DbSet<T> list, T src, Func<T, bool> where) where T : class
        {
            var srcFromDb = list.Local.FirstOrDefault(where); // .Local means we'll get unsaved entities

            if (srcFromDb == null)
            {
                srcFromDb = list.FirstOrDefault(where); // Maybe it was saved before
                if (srcFromDb == null)
                {
                    list.Add(src);
                    return src;
                }
                else
                {
                    return srcFromDb;
                }
            }
            else
            {
                return srcFromDb;
            }
        }

        private bool HaveDataForThisDate(string moduleName, DateTime date)
        {
            return _context.Metrics.Any(m => m.Date.Date == date && m.Module.Name.Equals(moduleName, StringComparison.InvariantCultureIgnoreCase));
        }
    }
}
