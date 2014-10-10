CodeMetricsLoader
=================

Visual Studio provides maintainability index, cyclomatic complexity, class coupling, depth of inheritance
and lines of code metrics. These metrics can be exported to Excel, but often there is a need
for a deeper analysis such as indientifing the worst methods accross the large number of targets,
analyze code metrics trends or share the metrics accross multiple teams.

Microsoft provides command line power tool which generates metrics XML file for the
given target. Code metrics loader utility parses this file and loads it into code metrics warehousing
where the metrics can be queried or drive SSAS cube which in turn can be analyzed
by variety of different clients.

See Wiki for more details.

