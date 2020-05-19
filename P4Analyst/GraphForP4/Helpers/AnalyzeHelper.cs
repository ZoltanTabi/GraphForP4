using GraphForP4.Models;
using GraphForP4.Services;
using GraphForP4.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GraphForP4.Helpers
{
    public static class AnalyzeHelper
    {
        public static BarChartData BarChartData(this List<Analyzer> analyzers)
        {
            var helper = new Dictionary<string, List<Variable>>();

            analyzers.ForEach(analyzer =>
            {
                analyzer.AllStructs.ForEach(structs =>
                {
                    foreach(var structPair in structs)
                    {
                        if (structPair.Value == null) continue;

                        var key = structPair.Key;

                        foreach(var headerPair in structPair.Value.Headers)
                        {
                            key += "." + headerPair.Key;
                            foreach (var variable in headerPair.Value.Variables)
                            {
                                if (variable.Read + variable.Write == 0) continue;

                                key += "." + variable.Name;
                                if (helper.ContainsKey(key))
                                {
                                    helper[key].Add(variable);
                                }
                                else
                                {
                                    helper.Add(key, new List<Variable> { variable });
                                }
                            }
                        }
                    }
                });
            });

            var barChartData = new BarChartData();

            foreach (var variablePair in helper)
            {
                var count = variablePair.Value.Count;
                barChartData.Labels.Add(variablePair.Key);
                barChartData.Reads.Add(variablePair.Value.Sum(x => x.Read) / (long)count);
                barChartData.Writes.Add(variablePair.Value.Sum(x => x.Write) / (long)count);
            }

            return barChartData;
        }
    }
}
