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
                    metricsElements = GetXml(config.MetricsFilePath, logger);
                }

                if (!string.IsNullOrEmpty(config.CodeCoverageFilePath))
                {
                    if (File.Exists(config.CodeCoverageFilePath))
                    {
                        codeCoverageElements = GetXml(config.CodeCoverageFilePath, logger);
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

        private static XElement GetXml(string filePath, Logger logger)
        {
            XElement elements = null;
            StreamReader sr = null;
            try
            {
                sr = new StreamReader(filePath);
                string xml = sr.ReadToEnd();
                elements = XElement.Parse(xml);
            }
            catch (Exception ex)
            {
                logger.Log($"Failed to read XML file {filePath} - {ex.Message}");
            }
            finally
            {
                sr?.Dispose();
            }
            return elements;
        }
    }
}
