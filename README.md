Code Metrics Loader
=================

Visual Studio provides maintainability index, cyclomatic complexity, class coupling, depth of inheritance
and lines of code metrics. These metrics can be exported to Excel, but often there is a need
for a deeper analysis such as indientifing the worst methods accross the large number of targets,
analyze code metrics trends or share the metrics accross multiple teams.

Microsoft provides command line power tool which generates metrics XML file for the
given target. Code metrics loader utility parses this file and loads it into code metrics warehousing
where the metrics can be queried or drive SSAS cube which in turn can be analyzed
by variety of different clients.

#Database
If you have database create permissions the new database will be initialized during first code run,
otherwise database tables should be created manually. The best way to get the latest database schema
is to install EF Power Tool. Then right-click on LoaderContext file, View Entity Model SQL.

Here's the current schema
![Database schema](/../screenshots/CodeMetricsWarehouse.png?raw=true "Database schema")

#How to use it
First grab Microsoft Metrics Power Tool and install it.

Then generate metrics xml for some dll with

metrics.exe /f:myfile.dll /o:metrics.xml

Run code metrics loader utility with

CodeMetricsLoader.exe --f metrics.xml --t myrepo-master

You can pass connection string in --c parameter. If you don't
then LocalDb will be used (LocalDb)\v11.0

#A few helpful queries

-- Top 10 worst methods
select top 10 dm.Name, fm.MaintainabilityIndex, fm.CyclomaticComplexity, fm.ClassCoupling,     fm.DepthOfInheritance, fm.LinesOfCode
 from FactMetrics fm
join DimDate dd on dd.DateId = fm.DateId
join DimMember dm on dm.MemberId = fm.MemberId
join DimType dt on dt.TypeId = dm.TypeId
join DimNamespace dn on dn.NamespaceId = dt.NamespaceId
join DimModule dmo on dmo.ModuleId = dn.NamespaceId
where dmo.Name like '%WebServices.dll
and dd.DayOfMonth = 7
order by fm.MaintainabilityIndex


-- Module over time
select top 10 dd.[Date], fm.MaintainabilityIndex, fm.CyclomaticComplexity, fm.ClassCoupling,  fm.DepthOfInheritance, fm.LinesOfCode
from FactMetrics fm
join DimDate dd on dd.DateId = fm.DateId
join DimModule dmo on dmo.ModuleId = fm.ModuleId
where dmo.Name like '%Account.dll'
order by dd.[Date] desc
