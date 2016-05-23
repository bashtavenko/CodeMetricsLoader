using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Validation;
using System.Linq;
using System.Text;
using AutoMapper;

namespace CodeMetricsLoader.Data
{
    public class MetricsRepository
    {
        private readonly LoaderContext _context;
        private readonly ILogger _logger;

        public MetricsRepository(LoaderContext context, ILogger logger)
        {
            _context = context;
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
            _context.Configuration.AutoDetectChangesEnabled = false;

            var dimDate = new DimDate();

            if (!useDateTime)
            {
                var date = dimDate.Date;
                dimDate = GetOrAddEntity(_context.Dates, dimDate, d => d.Date.Date == date);
            }

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
                if (HaveDataForThisDate(branchId, module.Name, dimDate.Date))
                {
                    string branchNameToDisplay = branch ?? "master";
                    _logger.Log($"Already have data for branch '{branchNameToDisplay}', module {module.Name} and this date.");
                    continue;
                }

                var dimModule = Mapper.Map<DimModule>(module);
                string moduleName = dimModule.Name;
                dimModule = GetOrAddEntity(_context.Modules, dimModule, m => m.Name == moduleName);
                factMetrics = Mapper.Map<FactMetrics>(module.Metrics);
                factMetrics.BranchId = branchId;
                factMetrics.Module = dimModule;
                factMetrics.Date = dimDate;
                _context.Metrics.Add(factMetrics);
                foreach (var ns in module.Namespaces)
                {
                    var dimNamespace = Mapper.Map<DimNamespace>(ns);
                    string namespaceName = dimNamespace.Name;
                    dimNamespace = GetOrAddEntity(_context.Namespaces, dimNamespace, n => n.Name == namespaceName);
                    dimNamespace.Modules.Add(dimModule);
                    factMetrics = Mapper.Map<FactMetrics>(ns.Metrics);
                    factMetrics.BranchId = branchId;
                    factMetrics.Module = dimModule;
                    factMetrics.Namespace = dimNamespace;
                    factMetrics.Date = dimDate;
                    _context.Metrics.Add(factMetrics);
                    foreach (var type in ns.Types)
                    {
                        var dimType = Mapper.Map<DimType>(type);
                        string typeName = dimType.Name;
                        dimType = GetOrAddEntity(_context.Types, dimType, t => t.Name == typeName);
                        dimType.Namespaces.Add(dimNamespace);
                        factMetrics = Mapper.Map<FactMetrics>(type.Metrics);
                        factMetrics.BranchId = branchId;
                        factMetrics.Module = dimModule;
                        factMetrics.Namespace = dimNamespace;
                        factMetrics.Type = dimType;
                        factMetrics.Date = dimDate;
                        _context.Metrics.Add(factMetrics);
                        foreach (var member in type.Members)
                        {
                            var dimMember = Mapper.Map<DimMember>(member);
                            string memberName = dimMember.Name;
                            string memberFileName = dimMember.File;
                            int? memberLine = dimMember.Line;
                            dimMember = GetOrAddEntity(_context.Members, dimMember, m => m.Name == memberName && m.File == memberFileName && m.Line == memberLine);
                            dimMember.Types.Add(dimType);
                            factMetrics = Mapper.Map<FactMetrics>(member.Metrics);
                            factMetrics.BranchId = branchId;
                            factMetrics.Module = dimModule;
                            factMetrics.Namespace = dimNamespace;
                            factMetrics.Type = dimType;
                            factMetrics.Member = dimMember;
                            factMetrics.Date = dimDate;
                            _context.Metrics.Add(factMetrics);
                        }
                    }
                }
            }
            try
            {
                _context.Configuration.AutoDetectChangesEnabled = true;
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
            _context.SaveChanges();
            return newBranch.BranchId;
        }

        /// <summary>
        /// Gets existing entity from db or adds one
        /// </summary>
        /// <typeparam name="T">Dimension (type, module, namespace, etc)</typeparam>
        /// <param name="list">List of these entities from the context</param>
        /// <param name="src">Entity itself</param>
        /// <param name="where">Where clause used to search for this entity</param>
        /// <returns>Newly added entity of entity from database</returns>
        private static T GetOrAddEntity<T>(DbSet<T> list, T src, Func<T, bool> where) where T : class
        {
            var srcFromDb = list.Local.FirstOrDefault(where); // .Local means we'll get unsaved entities

            if (srcFromDb == null)
            {
                srcFromDb = list.FirstOrDefault(where); // Maybe it was saved before
                if (srcFromDb == null)
                {
                    list.Add(src);
                    return src;
                }
                else
                {
                    return srcFromDb;
                }
            }
            else
            {
                return srcFromDb;
            }
        }

        private bool HaveDataForThisDate(int? branchId, string moduleName, DateTime date)
        {
            return _context.Metrics.Any(m => m.BranchId == branchId && m.Date.Date == date && m.Module.Name.Equals(moduleName, StringComparison.InvariantCultureIgnoreCase));
        }
    }
}
