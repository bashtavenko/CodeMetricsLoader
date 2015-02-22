using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Validation;
using System.Linq;
using System.Text;
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
        /// <param name="useDateTime">Use both date and time</param>
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
        /// <param name="useDateTime">Use both date and time</param>
        public void SaveTargets(List<Target> targets, string tag, bool useDateTime)
        {
            var dimDate = new DimDate();

            if (HaveDataForThisDate(tag, dimDate.Date))
            {
                _logger.Log("Already have data for this tag and date");
                return;
            }

            if (!useDateTime)            
            {
                dimDate = GetOrAddEntity(_context.Dates, dimDate, d => d.Date.Date == dimDate.Date);
            }
            FactMetrics factMetrics;
            foreach (var target in targets)
            {
                var dimTarget = Mapper.Map<DimTarget>(target);
                dimTarget.Tag = tag;
                dimTarget = GetOrAddEntity(_context.Targets, dimTarget, t => t.Tag == tag && t.FileName == target.FileName);
                foreach (var module in target.Modules)
                {
                    var dimModule = Mapper.Map<DimModule>(module);
                    dimModule = GetOrAddEntity(_context.Modules, dimModule, m => m.Name == dimModule.Name);
                    dimModule.Targets.Add(dimTarget);
                    factMetrics = Mapper.Map<FactMetrics>(module.Metrics);
                    InsertMetrics(factMetrics, dimDate, dimModule, null, null, null);                                                                      
                    foreach (var ns in module.Namespaces)
                    {
                        var dimNamespace = Mapper.Map<DimNamespace>(ns);
                        dimNamespace = GetOrAddEntity(_context.Namespaces, dimNamespace, n => n.Name == dimNamespace.Name);
                        dimNamespace.Modules.Add(dimModule);
                        factMetrics = Mapper.Map<FactMetrics>(ns.Metrics);
                        InsertMetrics(factMetrics, dimDate, dimModule, dimNamespace, null, null);
                        foreach (var type in ns.Types)
                        {
                            var dimType = Mapper.Map<DimType>(type);
                            dimType = GetOrAddEntity(_context.Types, dimType, t => t.Name == dimType.Name);
                            dimType.Namespaces.Add(dimNamespace);
                            factMetrics = Mapper.Map<FactMetrics>(type.Metrics);
                            InsertMetrics(factMetrics, dimDate, dimModule, dimNamespace, dimType, null);                            
                            foreach (var member in type.Members)
                            {
                                var dimMember = Mapper.Map<DimMember>(member);
                                dimMember = GetOrAddEntity(_context.Members, dimMember, m => m.Name == dimMember.Name);
                                dimMember.Types.Add(dimType);
                                factMetrics = Mapper.Map<FactMetrics>(member.Metrics);
                                factMetrics.Member = dimMember;
                                InsertMetrics(factMetrics, dimDate, dimModule, dimNamespace, dimType, dimMember);                                                
                            }                    
                        }
                    }
                }
            }
            try
            {
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
        
        private void InsertMetrics(FactMetrics factMetrics, DimDate date, DimModule module, DimNamespace ns, DimType type, DimMember member)
        {
            factMetrics.Date = date;
            factMetrics.Module = module;
            factMetrics.Namespace = ns;
            factMetrics.Type = type;
            factMetrics.Member = member;
            _context.Metrics.Add(factMetrics);
        }

        private bool HaveDataForThisDate(string tag, DateTime date)
        {
            return _context.Targets
                .SelectMany(s => s.Modules, (t, m) => new {Tag = t.Tag, Module = m})
                .Where(w => w.Tag.Equals(tag, StringComparison.InvariantCultureIgnoreCase))
                .SelectMany(x => x.Module.Metrics)
                .Any(w => w.Date.DateTime == date);
        }
    }
}
