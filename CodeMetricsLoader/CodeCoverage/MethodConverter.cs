using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace CodeMetricsLoader.CodeCoverage
{
    /// <summary>
    /// Converts OpenCover types and members into a string compatible with metrics PowerTool format.
    /// System.Void Test.AutoMapperConfig::CreateMaps() => CreateMaps() : void
    /// </summary>
    public class MethodConverter
    {
        private string ReturnTypeName { get; set; }
        private string TypeName { get; set; }
        private string MemberName { get; set; }
        private List<string> Parameters { get; set; }
        private MemberType MemberType { get; set; }

        public MethodConverter()
        {
            Parameters = new List<string>();
        }

        public string Convert(string input)
        {
            ReturnTypeName = null;
            TypeName = null;
            MemberName = null;
            Parameters.Clear();
            MemberType = MemberType.Unknown;

            // System.String Test.AutoMapperConfig::ShortenText(System.String)
            var match = (new Regex("^(.*)(?= .*::)")).Match(input);
            if (!match.Success)
            {
                throw new ArgumentException("Cannnot find return type");
            }
            var rawReturnType = match.Value;
            ReturnTypeName = ConvertType(rawReturnType);

            match = (new Regex("(?<=^.* )(.*)(?=::)")).Match(input);
            if (!match.Success)
            {
                throw new ArgumentException("Cannnot find class name");
            }
            var rawClassName = match.Value;
            TypeName = ConvertType(rawClassName);

            match = (new Regex("(?<=::).*")).Match(input);
            if (!match.Success)
            {
                throw new ArgumentException("Cannnot find member name");
            }
            var rawMemberWithParams = match.Value;
            
            // Parameters
            match = (new Regex("(?<=\\().*(?=\\))")).Match(rawMemberWithParams);
            if (!match.Success)
            {
                throw new ArgumentException("Cannnot find member parameters");
            }
            var rawParams = match.Value.Split(',');
            foreach (var rawParam in rawParams)
            {
                Parameters.Add(ConvertType(rawParam));
            }

            // Figure out member type
            // set_WebServiceTrafficId(System.String)
            // .ctor(Data.IStoreRepository)
            match = (new Regex(".*(?=\\()")).Match(rawMemberWithParams);
            if (!match.Success)
            {
                throw new ArgumentException("Cannnot find member name");
            }

            var rawMemberName = match.Value;

            if (rawMemberName.IndexOf(".ctor", System.StringComparison.Ordinal) >= 0)
            {
                MemberType = MemberType.Constructor;
            }
            else
            {
                var setMatch = (new Regex("(?<=set_).*(?=\\()")).Match(rawMemberWithParams);
                var getMatch = (new Regex("(?<=get_).*(?=\\()")).Match(rawMemberWithParams);
                if (getMatch.Success)
                {
                    MemberType = MemberType.Getter;
                    MemberName = getMatch.Value;
                }
                else if (setMatch.Success)
                {
                    MemberType = MemberType.Setter;
                    MemberName = setMatch.Value;
                }
                else
                {
                    MemberType = MemberType.Method;
                    MemberName = rawMemberName;
                }

                MemberName = ConvertType(MemberName);
            }

            string paramsWithCommas = string.Join(", ", Parameters);
            string result;
            switch (MemberType)
            {
                case MemberType.Constructor:
                    result =  string.Format("{0}({1})", TypeName, paramsWithCommas);
                    break;
                case MemberType.Method:
                    result = string.Format("{0}({1}) : {2}", MemberName, paramsWithCommas, ReturnTypeName);
                    break;
                case MemberType.Getter:
                    result = string.Format("{0}.get() : {1}", MemberName, ReturnTypeName);
                    break;
                case MemberType.Setter:
                    result = string.Format("{0}.set({1}) : {2}", MemberName, paramsWithCommas, ReturnTypeName);
                    break;
                default:
                    throw new ArgumentException();
            }

            return result;
        }

        public string ConvertType(string input)
        {
            if (string.IsNullOrEmpty(input))
            {
                return input;
            }
            string output = null;
            OpenCoverType openCoverType = OpenCoverType.Parse(input);
            switch (openCoverType.Member)
            {
                case "Void":
                    output = "void";
                    break;
                case "Boolean":
                    output = "bool";
                    break;
                case "Byte":
                    output = "byte";
                    break;
                case "String":
                    output = "string";
                    break;
                case "Char":
                    output = "char";
                    break;
                case "Int32":
                case "Int64":
                    output = "int";
                    break;
                case "Decimal":
                    output = "decimal";
                    break;
            }
            if (output != null)
            {
                return output;
            }
            else
            {
                if (openCoverType.IsGeneric)
                {
                    var convertedMemberType = ConvertType(openCoverType.MemberType);

                    // System.Nullable<int> n => int? n
                    if (openCoverType.IsGeneric && openCoverType.Namespace == "System" && openCoverType.Member == "Nullable")
                    {
                        output = string.Format("{0}?", convertedMemberType);
                    }
                    else
                    {
                        output = string.Format("{0}<{1}>", openCoverType.Member, convertedMemberType);    
                    }
                }
                else
                {
                    output = openCoverType.Member;
                }
            }

            return output;
        }
    }

    enum MemberType
    {
        Method,
        Getter,
        Setter,
        Constructor,
        Unknown
    }
}