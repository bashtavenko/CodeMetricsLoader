using System;
using System.Text.RegularExpressions;

namespace CodeMetricsLoader.CodeCoverage
{
    /// <summary>
    /// Parses open cover types and members
    /// </summary>
    public class OpenCoverType
    {
        public string Member { get; private set; }
        public string MemberType { get; private set; }
        public string Namespace { get; private set; }
        public bool IsGeneric { get { return !string.IsNullOrEmpty(MemberType); } }

        public static OpenCoverType Parse (string fullName)
        {
            if (string.IsNullOrEmpty(fullName))
            {
                throw new ArgumentException("fullName");
            }
            
            var result = new OpenCoverType();
            
            // System.Collections.Generic.IList`1<ViewModels.FinanceOption>
            var regex = new Regex("\\..[^.]*[`]");
            var match = regex.Match(fullName);
            if (match.Success)
            {
                result.Namespace = fullName.Substring(0, match.Index);
                string rawMember = match.Value; // // .IList`
                result.Member = (new Regex("(?<=.).*(?=`)")).Match(rawMember).Value; // IList
                result.MemberType = (new Regex("(?<=<).*(?=>)").Match(fullName)).Value; //ViewModels.FinanceOption
            }
            else
            {
                int index = fullName.LastIndexOf('.');
                if (index == -1)
                {
                    result.Member = fullName;
                }
                else
                {
                    result.Namespace = fullName.Substring(0, index);
                    var rawMember = fullName.Substring(index + 1);    

                    // It can be an inner class AutoMapperConfig/EnvironmentTypeResolver
                    index = rawMember.IndexOf("/");
                    if (index == -1)
                    {
                        result.Member = rawMember;
                    }
                    else
                    {
                        result.Member = rawMember.Substring(index + 1);
                    }
                }
            }
            return result;
        }
    }
}