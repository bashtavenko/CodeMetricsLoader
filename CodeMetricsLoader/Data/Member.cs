﻿using System;
using System.Collections.Generic;
using System.IO;

namespace CodeMetricsLoader.Data
{
    public class Member : Node
    {
        public Metrics Metrics { get; set; }
        public override IList<Node> Children { get { return new List<Node>(); }}
        public override string Key { get; }
        public string File { get; set; }
        public int? Line { get; set; }
        public string FileName { get { return string.IsNullOrEmpty(File) ? null : Path.GetFileName(File); } }
        public override bool CanBeMerged { get; set; } = false;

        public override int? Value
        {
            get { return Metrics.CodeCoverage; }
            set { Metrics.CodeCoverage = value; }
        }

        public Member(string name) : base (name)
        {
            Metrics = new Metrics();
            Key = "Member-" + Name;
        }
    }
}
