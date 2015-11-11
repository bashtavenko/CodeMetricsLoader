using CommandLine;
using CommandLine.Text;

namespace CodeMetricsLoader
{
    public class LoaderConfiguration
    {
        [Option('m', "metrics", Required = true, HelpText = "Xml file with metrics")]
        public string MetricsFilePath { get; set; }

        [Option('r', "coverage", Required = false, HelpText = "Xml file with code coverage")]
        public string CodeCoverageFilePath { get; set; }
        
        [Option('c', "connection", Required = false, HelpText = "Connection string")]
        public string ConnectionString { get; set; }

        [Option('d', "datetime", Required = false, DefaultValue = false, HelpText = "Use date and time for date dimension")]
        public bool UseDateAndTime { get; set; }

        [Option('b', "branch", Required = false, HelpText = "Branch name if different from 'master'")]
        public string Branch { get; set; }

        [ParserState]
        public IParserState LastParserState { get; set; }

        [HelpOption]
        public string GetUsage()
        {
            return HelpText.AutoBuild(this, (HelpText current) => HelpText.DefaultParsingErrorsHandler(this, current));
        }        
    }
}
