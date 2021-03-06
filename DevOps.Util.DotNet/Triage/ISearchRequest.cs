﻿using DevOps.Util.DotNet;
using DevOps.Util.DotNet.Triage;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace DevOps.Util.DotNet.Triage
{
    public interface ISearchRequest
    {
        string GetQueryString();
        void ParseQueryString(string userQuery);
    }

    public interface ISearchQueryRequest<T> : ISearchRequest
    {
        IQueryable<T> Filter(IQueryable<T> query);
    }
}
