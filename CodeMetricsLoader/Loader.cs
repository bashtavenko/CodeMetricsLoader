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
            List<Target> targets = MapXmlToEntities(elements, tag);
            _logger.Log("Saving to database...");
            SaveTargets(targets);
            _logger.Log("Done.");
        }

        /// <summary>
        /// Map XElments with metric xml to EF DTOs
        /// </summary>
        /// <param name="elements">metric xml nodes</param>
        /// <param name="tag">optional tag</param>
        /// <returns>Collection of DTOs</returns>
        public List<Target> MapXmlToEntities(XElement elements, string tag)
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
        public void SaveTargets(List<Target> targets)
        {
            var date = new Date();
            date.Targets = targets;
            _context.Dates.Add(date);
            _context.SaveChanges();
        }
    }
}
