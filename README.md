CodeMetricsLoader
=================

Visual Studio provides maintainability index, cyclomatic complexity, class coupling, depth of inheritance and lines of code metrics. These metrics can be exported to Excel, but often there is a need to share the metrics and being able to see the trends, such as how has maintainability index been changing during last week or how it is different between different code branches.

Microsoft provides command line power tool which generates metrics XML file for the given target. This utility loads this file into warehousing-like database where the metrics can be queried or drive SSAS cube which in turn can be analyzed by variety of different clients.

