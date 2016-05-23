using System;
using System.Collections.Generic;

namespace CodeMetricsLoader.Data
{
    public interface IMetricsRepository : IDisposable
    {
        /// <summary>
        /// Save DTOs to database
        /// </summary>
        /// <param name="targets">DTOs to save</param>
        /// <param name="useDateTime">Use both date and time</param>
        /// <param name="branch">Optional branch name</param>
        void SaveTargets(IList<Target> targets, bool useDateTime, string branch);

        int CreateReadOrDeleteBranch(string branchName);
    }
}