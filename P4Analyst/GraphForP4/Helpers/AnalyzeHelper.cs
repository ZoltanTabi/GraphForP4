using GraphForP4.Models;
using GraphForP4.Services;
using GraphForP4.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace GraphForP4.Helpers
{
    public class HeaderHelper
    {
        public int Use { get; set; }
        public int VariableUsefulSize { get; set; }
        public int VariablesSize { get; set; }
        public int Count { get; set; }
    };

    public static class AnalyzeHelper
    {
        public static void CreateCharts(this List<Analyzer> analyzers, out BarChartData readAndWriteChartData, out PieChartData useVariable, out PieChartData useful, out BarChartData headers)
        {
            var readAndWriteChartDataHelper = new Dictionary<string, List<Variable>>();
            var headersHelper = new Dictionary<string, HeaderHelper>();
            var usefulModified = 0;
            var unUsefulModified = 0;

            analyzers.ForEach(analyzer =>
            {
                analyzer.AllStructs.ForEach(structs =>
                {
                    foreach(var structPair in structs)
                    {
                        if (structPair.Value == null) continue;

                        foreach(var headerPair in structPair.Value.Headers)
                        {
                            var headerKey = $"{structPair.Key}.{headerPair.Key}";
                            if (!headersHelper.ContainsKey(headerKey))
                            {
                                headersHelper[headerKey] = new HeaderHelper();
                            }
                            var headerHelper = headersHelper[headerKey];
                            headerHelper.Use += headerPair.Value.Use;
                            ++headerHelper.Count;

                            foreach (var variable in headerPair.Value.Variables)
                            {
                                var variableSize = GetVariableSize(variable.Type);
                                headerHelper.VariablesSize += variableSize;

                                if (variable.Modified) ++unUsefulModified;
                                else if (variable.ModifiedAndUse > 0) ++usefulModified;

                                if (variable.Read + variable.Write == 0) continue;

                                headerHelper.VariableUsefulSize += variableSize;

                                var key = $"{structPair.Key}.{headerPair.Key}.{variable.Name}";
                                if (!readAndWriteChartDataHelper.ContainsKey(key))
                                {
                                    readAndWriteChartDataHelper[key] = new List<Variable>();
                                }
                                readAndWriteChartDataHelper[key].Add(variable);
                            }
                        }
                    }
                });
            });

            readAndWriteChartData = new BarChartData();
            useVariable = new PieChartData();
            useful = new PieChartData
            {
                Labels = new List<string> { "Felhasznált", "Nem használt fel" },
                Datas = new List<int> { usefulModified, unUsefulModified }
            };
            headers = new BarChartData();

            foreach (var variablePair in readAndWriteChartDataHelper)
            {
                var count = variablePair.Value.Count;
                readAndWriteChartData.Labels.Add(variablePair.Key);
                if (!readAndWriteChartData.DoubleDatas.ContainsKey("Olvasás"))
                {
                    readAndWriteChartData.DoubleDatas["Olvasás"] = new List<double>();
                    readAndWriteChartData.DoubleDatas["Írás"] = new List<double>();
                }

                var sumRead = variablePair.Value.Sum(x => x.Read);
                var sumWrite = variablePair.Value.Sum(x => x.Write);

                readAndWriteChartData.DoubleDatas["Olvasás"].Add(Math.Round(sumRead / (double)count, 2));
                readAndWriteChartData.DoubleDatas["Írás"].Add(Math.Round(sumWrite / (double)count, 2));

                useVariable.Labels.Add(variablePair.Key);
                useVariable.DoubleDatas.Add((sumRead + sumWrite) / (double)count);
            }

            foreach (var headerPair in headersHelper)
            {
                headers.Labels.Add(headerPair.Key);
                if (!headers.DoubleDatas.ContainsKey("Felhasználás"))
                {
                    headers.DoubleDatas["Felhasználás"] = new List<double>();
                    headers.DoubleDatas["Változóinak mérete"] = new List<double>();
                    headers.DoubleDatas["Felhasznált változóinak mérete"] = new List<double>();
                }

                var headerHelper = headerPair.Value;

                headers.DoubleDatas["Felhasználás"].Add(Math.Round(headerHelper.Use / (double)headerHelper.Count, 2));
                headers.DoubleDatas["Változóinak mérete"].Add(Math.Round(headerHelper.VariablesSize / (double)headerHelper.Count, 2));
                headers.DoubleDatas["Felhasznált változóinak mérete"].Add(Math.Round(headerHelper.VariableUsefulSize / (double)headerHelper.Count, 2));
            }
        }

        private static int GetVariableSize(string type)
        {
            if (type.Contains('<') && type.Contains('>'))
            {
                if (int.TryParse(Regex.Replace(FileHelper.GetMethod(type, type.First().ToString(), '<', '>', true), "<|>", String.Empty).Trim(), out int size))
                {
                    return size;
                }
                else return default;
            }
            else return default;
        }

        public static void DistinctGraphs(this List<Analyzer> analyzers, out List<List<AngularNode>> controlFlowGraphs, out List<List<AngularNode>> dataFlowGraphs)
        {
            controlFlowGraphs = new List<List<AngularNode>>();
            dataFlowGraphs = new List<List<AngularNode>>();

            foreach(var x in analyzers.GroupBy(x => x.Id))
            {
                var analyzer = x.FirstOrDefault();
                controlFlowGraphs.Add(GraphToAngular.Serialize(analyzer.ControlFlowGraph).ToList());
                dataFlowGraphs.Add(GraphToAngular.Serialize(analyzer.DataFlowGraph).ToList());
            }
        }
    }
}
