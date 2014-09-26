using CommandLine;
using CommandLine.Text;

namespace CodeMetricsLoader
{
    public class LoaderConfiguration
    {
        [Option('f', "file", Required = true, HelpText = "Xml file with metrics")]
        public string FilePath { get; set; }

        [Option('c', "connection", Required = false, HelpText = "Connection string")]
        public string ConnectionString { get; set; }

        [Option('t', "tag", Required = false, HelpText = "Target tag (repo name, branch name, etc")]
        public string Tag { get; set; }

        [ParserState]
        public IParserState LastParserState { get; set; }

        [HelpOption]
        public string GetUsage()
        {
            return HelpText.AutoBuild(this, (HelpText current) => HelpText.DefaultParsingErrorsHandler(this, current));
        }        
    }
}
