using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using CodeMetricsLoader.Data;
using Type = CodeMetricsLoader.Data.Type;

namespace CodeMetricsLoader.CodeCoverage
{
    public class OpenCoverParser
    {
        private readonly MethodConverter _methodConverter;

        public OpenCoverParser()
        {
            _methodConverter = new MethodConverter();
        }

        public List<Target> Parse(XElement session)
        {
            if (session == null)
            {
                throw new LoaderException("Invalid xml");
            }
            
            var targets = new List<Target>();
            // Each module becomes target in this context
            foreach (var moduleElement in session.Descendants("Module")
                .Where(w => w.Attribute("skippedDueTo") == null)) // Can get "<Module skippedDueTo="Filter"  ..." 
            {
                var target = new Target
                {
                    Name = GetDescendantValue(moduleElement, "FullName")
                };
                targets.Add(target);
                var module = new Module
                {
                    Name = target.FileName,
                    FileVersion = "0.0.0.0",
                    AssemblyVersion = "0.0.0.0",
                    Metrics = ParseSummary(moduleElement)
                };
                
                target.Modules.Add(module);
                
                foreach (var classElement in moduleElement.Descendants("Class")
                    .Where(w => w.Attribute("skippedDueTo") == null && GetDescendantValue(w, "FullName") != "<Module>")) // some kind of dummy class
                {
                    string rawClassName = GetDescendantValue(classElement, "FullName");
                    OpenCoverType openCoverType = OpenCoverType.Parse(rawClassName);
                    var cs = new Type
                    {
                        Name = openCoverType.Member,
                        Metrics = ParseSummary(classElement)
                    };

                    Namespace ns = module.Namespaces.Find(f => f.Name == openCoverType.Namespace);
                    if (ns == null)
                    {
                       ns = new Namespace {Name = openCoverType.Namespace}; 
                       module.Namespaces.Add(ns);
                    }

                    ns.Types.Add(cs);

                    foreach (var methodElement in classElement.Descendants("Method"))
                    {
                        var rawMember = GetDescendantValue(methodElement, "Name");
                        var method = new Member
                        {
                            Name = _methodConverter.Convert(rawMember),
                            Metrics = ParseSummary(methodElement)
                        };
                        cs.Members.Add(method);
                    }
                }

                // Need to update namespace stats based on the types we found
                module.Namespaces.ForEach(n => n.UpdateMetricsFromTypes());
            }

            return targets;
        }

        private static string GetDescendantValue(XElement element, string name)
        {
            return element.Descendants(name).First().Value;
        }

        private Metrics ParseSummary(XElement summaryParent)
        {
            if (summaryParent == null)
            {
                throw new ArgumentException("Invalid Summary element");
            }

            var summary = summaryParent.Descendants("Summary").First();
            if (summary == null)
            {
                throw new ArgumentException("No Summary element");
            }

            var coverage =  (int)Math.Round(decimal.Parse(summary.Attribute("branchCoverage").Value));
            return new Metrics {CodeCoverage = coverage};
        }
    }
}
