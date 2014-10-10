using System;
using System.IO;
using System.Xml.Linq;

using CodeMetricsLoader.Data;

namespace CodeMetricsLoader
{
    class Program
    {
        static void Main(string[] args)
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
                XElement elements;
                using (StreamReader sr = new StreamReader(config.FilePath))
                { 
                    string xml = sr.ReadToEnd();
                    elements = XElement.Parse(xml);
                }

                loader.Load(elements, config.Tag, false);
            }
            catch (Exception ex)
            {
                logger.Log(ex.Message);
            }            
        }
    }
}
