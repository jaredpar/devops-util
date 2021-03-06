﻿using System.Collections.Generic;
using System.Linq;
using Mono.Options;

namespace Runfo
{
    internal sealed class GetFromHelixOptionSet : OptionSet
    {
        internal string? JobId { get; set; }

        internal List<string> WorkItems { get; set; } = new List<string>();

        internal string? DownloadDir { get; set; }

        internal string? Token { get; set; }

        internal bool IgnoreDumps { get; set; }

        internal GetFromHelixOptionSet()
        {
            Add("j|jobid=", "helix job id to download items from.", j => JobId = j);
            Add("o|output=", "output directory to download to.", d => DownloadDir = d);
            Add("n|no-dumps", "don't download dump files if any.", nd => IgnoreDumps = nd is object);
            Add("w|workitems=", "Accepted values:\n empty: first workitem.\n list: workitem name(s) separated by comma.\n all: download all workitems.", w => WorkItems = w.Split(",").ToList());
        }
    }
}