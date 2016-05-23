using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

using AutoMapper;
using CodeMetricsLoader.CodeCoverage;
using CodeMetricsLoader.Data;

namespace CodeMetricsLoader
{
    public class Loader
    {
        private readonly ILogger _logger;
        private readonly IMetricsRepository _repository;
        
        /// <summary>
        /// Initializes a new instance of the <see cref="Loader"/> class
        /// </summary>
        /// <param name="repository">Database repository</param>
        /// <param name="logger">logger</param>
        public Loader(IMetricsRepository repository, ILogger logger)
        {
            _logger = logger;
            _repository = repository;
            AutoMapperConfig.CreateMaps();
        }

        /// <summary>
        /// Load metrics from xml and save them to database
        /// </summary>
        /// <param name="metricsElements"></param>
        /// <param name="codeCoverageElements"></param>
        /// <param name="useDateTime">Use both date and time</param>
        /// <param name="branch">Optional branch name</param>
        public void Load(XElement metricsElements, XElement codeCoverageElements, bool useDateTime, string branch = null)
        {
            if (metricsElements == null)
            {
                throw new ArgumentException("Must have metrics");
            }
            IList<Target> targets;
            List<Target> metricsTargets = MapMetricsXmlToEntities(metricsElements);
            
            if (codeCoverageElements != null)
            {
                var parser = new CodeCoverageConverter();
                List<Target> codeCoverageTargets = parser.Convert(codeCoverageElements);
                var merger = new Merger();
                targets = merger.Merge(metricsTargets, codeCoverageTargets, _logger);
            }
            else 
            {
                targets = metricsTargets;
            }

            _logger.Log("Saving to database...");            
            _repository.SaveTargets(targets, useDateTime, branch);            
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
    }
}
