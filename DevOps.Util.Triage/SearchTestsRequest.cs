﻿using DevOps.Util.DotNet;
using DevOps.Util.Triage;
using Microsoft.EntityFrameworkCore;
using Octokit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace DevOps.Util.Triage
{
    public class SearchTestsRequest : ISearchRequest
    {
        public string? Name { get; set; }

        public async Task<List<ModelTestResult>> GetResultsAsync(
            TriageContext context,
            SearchBuildsRequest searchBuildsRequest,
            bool includeBuild,
            bool includeTestRun,
            int buildLimit)
        {
            var lastBuildId = "";
            var buildCount = 0;
            var list = new List<ModelTestResult>();
            await foreach (var testResult in EnumerateResultsAsync(context, searchBuildsRequest, includeBuild, includeTestRun).ConfigureAwait(false))
            {
                list.Add(testResult);
                if (lastBuildId != testResult.ModelBuildId)
                {
                    lastBuildId = testResult.ModelBuildId;
                    buildCount++;
                    if (buildCount >= buildLimit)
                    {
                        break;
                    }
                }
            }

            return list;
        }

        private async IAsyncEnumerable<ModelTestResult> EnumerateResultsAsync(
            TriageContext context,
            SearchBuildsRequest searchBuildsRequest,
            bool includeBuild,
            bool includeTestRun)
        {
            IQueryable<ModelTestResult> query = context.ModelTestResults;
            query = searchBuildsRequest.FilterBuilds(query);
            query = query.OrderByDescending(x => x.ModelBuild.BuildNumber);

            if (includeBuild)
            {
                query = query.Include(x => x.ModelBuild).ThenInclude(x => x.ModelBuildDefinition);
            }

            if (includeTestRun)
            {
                query = query.Include(x => x.ModelTestRun);
            }

            // TODO: use standard search function
            Regex? nameRegex = null;
            if (Name is object)
            {
                nameRegex = new Regex(Name, RegexOptions.Compiled | RegexOptions.IgnoreCase);
            }

            var partition = 100;
            var skip = 0;
            do
            {
                var partitionQuery = query.Skip(skip).Take(partition);
                skip += partition;

                var partitionList = await partitionQuery.ToListAsync().ConfigureAwait(false);
                if (partitionList.Count == 0)
                {
                    break;
                }

                foreach (var result in partitionList)
                {
                    if (nameRegex is null || nameRegex.IsMatch(result.TestFullName))
                    {
                        yield return result;
                    }
                }
            }
            while (true);
        }

        public string GetQueryString()
        {
            var builder = new StringBuilder();
            if (!string.IsNullOrEmpty(Name))
            {
                Append($"name:\"{Name}\"");
            }

            return builder.ToString();

            void Append(string message)
            {
                if (builder.Length != 0)
                {
                    builder.Append(" ");
                }

                builder.Append(message);
            }
        }

        public void ParseQueryString(string userQuery)
        {
            if (!userQuery.Contains(":"))
            {
                Name = userQuery.Trim('"');
                return;
            }

            foreach (var tuple in DotNetQueryUtil.TokenizeQueryPairs(userQuery))
            {
                switch (tuple.Name.ToLower())
                {
                    case "name":
                        Name = tuple.Value.Trim('"');
                        break;
                    default:
                        throw new Exception($"Invalid option {tuple.Name}");
                }
            }
        }
    }
}
