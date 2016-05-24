using System;
using System.Collections.Generic;
using System.Data.Entity.Validation;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using AutoMapper;
using Dapper;

namespace CodeMetricsLoader.Data
{
    public class MetricsRepository : IMetricsRepository
    {
        private readonly LoaderContext _context;
        private readonly ILogger _logger;
        private readonly SqlConnection _connection;
        
        public MetricsRepository(string connectionString, ILogger logger)
        {
            if (connectionString == null)
            {
                throw new ArgumentNullException(nameof(connectionString));
            }
            _context = new LoaderContext(connectionString);
            _connection = new SqlConnection(connectionString);
            _logger = logger;
        }

        /// <summary>
        /// Save DTOs to database
        /// </summary>
        /// <param name="targets">DTOs to save</param>
        /// <param name="useDateTime">Use both date and time</param>
        /// <param name="branch">Optional branch name</param>
        public void SaveTargets(IList<Target> targets, bool useDateTime, string branch)
        {
            var dimDate = new DimDate();
            int dateId = GetOrInsertDate(dimDate);
            
            int? branchId;
            if (string.IsNullOrEmpty(branch) || branch.Equals("master", StringComparison.InvariantCultureIgnoreCase))
            {
                branchId = null;
            }
            else
            {
                branchId = CreateReadOrDeleteBranch(branch);
            }

            FactMetrics factMetrics;
            foreach (var module in targets.SelectMany(m => m.Modules))
            {
                if (HaveDataForThisDate(branchId, module.Name, dateId))
                {
                    string branchNameToDisplay = branch ?? "master";
                    _logger.Log($"Already have data for branch '{branchNameToDisplay}', module {module.Name} and this date.");
                    continue;
                }

                var dimModule = Mapper.Map<DimModule>(module);
                int moduleId = GetOrInsertModule(dimModule);
                factMetrics = Mapper.Map<FactMetrics>(module.Metrics);
                factMetrics.BranchId = branchId;
                factMetrics.ModuleId = moduleId;
                factMetrics.DateId = dateId;
                InsertMetrics(factMetrics);
                foreach (var ns in module.Namespaces)
                {
                    var dimNamespace = Mapper.Map<DimNamespace>(ns);
                    int namespaceId = GetOrInsertNamespace(dimNamespace);
                    InsertModuleNamespace(moduleId, namespaceId);
                    factMetrics = Mapper.Map<FactMetrics>(ns.Metrics);
                    factMetrics.BranchId = branchId;
                    factMetrics.ModuleId = moduleId;
                    factMetrics.NamespaceId = namespaceId;
                    factMetrics.DateId = dateId;
                    InsertMetrics(factMetrics);
                    foreach (var type in ns.Types)
                    {
                        var dimType = Mapper.Map<DimType>(type);
                        int typeId = GetOrInsertType(dimType);
                        InsertNamespaceType(namespaceId, typeId);
                        factMetrics = Mapper.Map<FactMetrics>(type.Metrics);
                        factMetrics.BranchId = branchId;
                        factMetrics.ModuleId = moduleId;
                        factMetrics.NamespaceId = namespaceId;
                        factMetrics.TypeId = typeId;
                        factMetrics.DateId = dateId;
                        InsertMetrics(factMetrics);
                        foreach (var member in type.Members)
                        {
                            var dimMember = Mapper.Map<DimMember>(member);
                            int memberId = GetOrInsertMember(dimMember);
                            InsertTypeMember(typeId, memberId);
                            factMetrics = Mapper.Map<FactMetrics>(member.Metrics);
                            factMetrics.BranchId = branchId;
                            factMetrics.ModuleId = moduleId;
                            factMetrics.NamespaceId = namespaceId;
                            factMetrics.TypeId = typeId;
                            factMetrics.MemberId = memberId;
                            factMetrics.DateId = dateId;
                            InsertMetrics(factMetrics);
                        }
                    }
                }
            }
        }

        public int CreateReadOrDeleteBranch(string branchName)
        {
            const int maxNumberOfBranches = 10;

            var branch = _context.Branches.FirstOrDefault(f => f.Name.Equals(branchName, StringComparison.InvariantCultureIgnoreCase));
            if (branch != null)
            {
                return branch.BranchId;
            }

            // We don't want stale branches. An alternative for deletion branches during insert would be a separate SQL job
            // although it would have to make deletes in more than one table.
            if (_context.Branches.Count() > maxNumberOfBranches)
            {
                var branchesToDelete = _context.Branches
                    .OrderBy(s => s.CreatedDate)
                    .Take(_context.Branches.Count() - maxNumberOfBranches + 1);

                _context.Branches.RemoveRange(branchesToDelete);
            }

            var newBranch = new DimBranch { Name = branchName, CreatedDate = DateTime.Now };
            _context.Branches.Add(newBranch);
            SaveChanges();
            return newBranch.BranchId;
        }

        private void SaveChanges()
        {
            try
            {
                _context.SaveChanges();
            }
            catch (DbEntityValidationException ex)
            {
                var sb = new StringBuilder("Failed to save\n");
                foreach (var entity in ex.EntityValidationErrors)
                {
                    sb.AppendLine(string.Format("Entity: {0}", entity.Entry.Entity));
                    foreach (var error in entity.ValidationErrors)
                    {
                        sb.AppendLine(string.Format("Property: {0}", error.PropertyName));
                        sb.AppendLine(string.Format("Error: {0}", error.ErrorMessage));
                    }
                    sb.AppendLine();
                }
                throw new ApplicationException(sb.ToString(), ex);
            }
        }

        private void InsertMetrics(FactMetrics metrics)
        {
            const string sqlTemplate = @"INSERT INTO dbo.FactMetrics
                                           (BranchId
                                           ,DateId
                                           ,ModuleId
                                           ,NamespaceId
                                           ,TypeId
                                           ,MemberId
                                           ,MaintainabilityIndex
                                           ,CyclomaticComplexity
                                           ,ClassCoupling
                                           ,DepthOfInheritance
                                           ,LinesOfCode
                                           ,CodeCoverage)
                                     VALUES
                                           (@branchId
		                                    ,@dateId
			                                ,@moduleId
			                                ,@namespaceId
			                                ,@typeId
			                                ,@memberId
			                                ,@maintainabilityIndex
                                            ,@cyclomaticComplexity
                                            ,@classCoupling
                                            ,@depthOfInheritance
                                            ,@linesOfCode
                                            ,@codeCoverage)";
            _connection.Execute(sqlTemplate, new
            {
                metrics.BranchId,
                metrics.DateId,
                metrics.ModuleId,
                metrics.NamespaceId,
                metrics.TypeId,
                metrics.MemberId,
                metrics.MaintainabilityIndex,
                metrics.CyclomaticComplexity,
                metrics.ClassCoupling,
                metrics.DepthOfInheritance,
                metrics.LinesOfCode,
                metrics.CodeCoverage
            });
        }

        private int GetOrInsertDate(DimDate date)
        {
            var id = _connection.ExecuteScalar<int?>("SELECT DateId FROM dbo.DimDate WHERE Date = @date", new {date.Date});
            if (id.HasValue)
            {
                return id.Value;
            }
            else
            {
                const string sqlTemplate = @"INSERT INTO dbo.DimDate
                                           (DateTime
                                           ,Year
                                           ,YearString
                                           ,Month
                                           ,MonthString                                           
                                           ,WeekOfYear
                                           ,Date
                                           ,DayOfYear
                                           ,DayOfMonth
                                           ,DayOfWeek)                                           
                                     VALUES
                                           (@dateTime
		                                    ,@year
			                                ,@yearString
			                                ,@month
			                                ,@monthString			                                
			                                ,@weekOfYear
                                            ,@date
                                            ,@dayOfYear
                                            ,@dayOfMonth
                                            ,@dayOfWeek);
                                     SELECT SCOPE_IDENTITY();";
                id = _connection.ExecuteScalar<int>(sqlTemplate, new
                {
                    date.DateTime,
                    date.Year,
                    date.YearString,
                    date.Month,
                    date.MonthString,
                    date.WeekOfYear,
                    date.Date,
                    date.DayOfYear,
                    date.DayOfMonth,
                    date.DayOfWeek
                });
                return id ?? -1;
            }
        }

        private int GetOrInsertModule(DimModule module)
        {
            var id = _connection.ExecuteScalar<int?>("SELECT ModuleId FROM dbo.DimModule WHERE Name = @name", new { module.Name });
            if (id.HasValue)
            {
                return id.Value;
            }
            else
            {
                const string sqlTemplate = @"INSERT INTO dbo.DimModule (Name, AssemblyVersion,FileVersion) VALUES (@name, @assemblyVersion, @fileVersion);
                                             SELECT SCOPE_IDENTITY();";
                id = _connection.ExecuteScalar<int>(sqlTemplate, new {module.Name, module.AssemblyVersion, module.FileVersion});
                return id ?? -1;
            }
        }

        private int GetOrInsertNamespace(DimNamespace ns)
        {
            var id = _connection.ExecuteScalar<int?>("SELECT NamespaceId FROM dbo.DimNamespace WHERE Name = @name", new { ns.Name });
            if (id.HasValue)
            {
                return id.Value;
            }
            else
            {
                const string sqlTemplate = @"INSERT INTO dbo.DimNamespace (Name) VALUES (@name);
                                             SELECT SCOPE_IDENTITY();";
                id = _connection.ExecuteScalar<int>(sqlTemplate, new { ns.Name });
                return id ?? -1;
            }
        }

        private void InsertModuleNamespace(int moduleId, int namespaceId)
        {
            var id = _connection.ExecuteScalar<int?>("SELECT 1 FROM dbo.DimModuleNamespace WHERE ModuleId = @moduleId AND NamespaceId = @namespaceId", new { moduleId, namespaceId });
            if (id == null)
            {
                const string sqlTemplate = @"INSERT INTO dbo.DimModuleNamespace (ModuleId, NamespaceId) VALUES (@moduleId, @namespaceId)";
                _connection.ExecuteScalar<int>(sqlTemplate, new {moduleId, namespaceId});
            }
        }

        private int GetOrInsertType(DimType type)
        {
            var id = _connection.ExecuteScalar<int?>("SELECT TypeId FROM dbo.DimType WHERE Name = @name", new { type.Name });
            if (id.HasValue)
            {
                return id.Value;
            }
            else
            {
                const string sqlTemplate = @"INSERT INTO dbo.DimType (Name) VALUES (@name);
                                             SELECT SCOPE_IDENTITY();";
                id = _connection.ExecuteScalar<int>(sqlTemplate, new { type.Name });
                return id ?? -1;
            }
        }

        private void InsertNamespaceType(int namespaceId, int typeId)
        {
            var id = _connection.ExecuteScalar<int?>("SELECT 1 FROM dbo.DimNamespaceType WHERE NamespaceId = @namespaceId AND TypeId = @typeId", new { namespaceId, typeId });
            if (id == null)
            {
                const string sqlTemplate = @"INSERT INTO dbo.DimNamespaceType (NamespaceId, TypeId) VALUES (@namespaceId, @typeId)";
                _connection.ExecuteScalar<int>(sqlTemplate, new { namespaceId, typeId });
            }
        }

        private int GetOrInsertMember(DimMember member)
        {
            var id = _connection.ExecuteScalar<int?>("SELECT MemberId FROM dbo.DimMember WHERE Name = @name AND [File] = @file AND Line = @line", new { member.Name, member.File, member.Line });
            if (id.HasValue)
            {
                return id.Value;
            }
            else
            {
                const string sqlTemplate = @"INSERT INTO dbo.DimMember (Name, [File], Line) VALUES (@name, @file, @line);
                                             SELECT SCOPE_IDENTITY();";
                id = _connection.ExecuteScalar<int>(sqlTemplate, new { member.Name, member.File, member.Line });
                return id ?? -1;
            }
        }

        private void InsertTypeMember(int typeId, int memberId)
        {
            var id = _connection.ExecuteScalar<int?>("SELECT 1 FROM dbo.DimTypeMember WHERE TypeId = @typeId AND MemberId = @memberId", new { typeId, memberId });
            if (id == null)
            {
                const string sqlTemplate = @"INSERT INTO dbo.DimTypeMember (TypeId, MemberId) VALUES (@typeId, @memberId)";
                _connection.ExecuteScalar<int>(sqlTemplate, new { typeId, memberId });
            }
        }
        
        private bool HaveDataForThisDate(int? branchId, string moduleName, int dateId)
        {
            return _context.Metrics.Any(m => m.BranchId == branchId && m.DateId == dateId && m.Module.Name == moduleName);
        }

        public void Dispose()
        {
            _connection.Close();
            _context.Dispose();
        }
    }
}
