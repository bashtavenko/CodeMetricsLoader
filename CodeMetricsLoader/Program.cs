using System;
using System.IO;
using System.Xml.Linq;

using CodeMetricsLoader.Data;

namespace CodeMetricsLoader
{
    public class Program
    {
        public static void Main(string[] args)
        {            
            var logger = new Logger();

            LoaderConfiguration config = new LoaderConfiguration();
            if (!CommandLine.Parser.Default.ParseArguments(args, config))
            {                
                return;
            }                        

            LoaderContext context;
            if (!string.IsNullOrEmpty(config.ConnectionString))
            {                
                context = new LoaderContext(config.ConnectionString);
            }
            else
            {
                context = new LoaderContext();
            }

            var loader = new Loader(context, logger);
                        
            try
            {
                XElement metricsElements = null, codeCoverageElements = null;
                if (!string.IsNullOrEmpty(config.MetricsFilePath))
                {
                    metricsElements = GetXml(config.MetricsFilePath);
                }

                if (!string.IsNullOrEmpty(config.CodeCoverageFilePath))
                {
                    if (File.Exists(config.CodeCoverageFilePath))
                    {
                        codeCoverageElements = GetXml(config.CodeCoverageFilePath);
                    }
                    else
                    {
                        // Code coverage is optional
                        logger.Log($"Could not find code coverage file at '{config.CodeCoverageFilePath}'.");
                    }
                }

                loader.Load(metricsElements, codeCoverageElements, false, config.Branch);
            }
            catch (Exception ex)
            {
                logger.Log(ex.Message);
            }            
        }

        private static XElement GetXml(string filePath)
        {
            XElement elements;
            using (var sr = new StreamReader(filePath))
            {
                string xml = sr.ReadToEnd();
                elements = XElement.Parse(xml);
            }
            return elements;
        }
    }
}
