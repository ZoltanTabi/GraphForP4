using GraphForP4.Models;
using System;
using System.Collections.Generic;
using System.Text;
using GraphForP4.Helpers;
using GraphForP4.Extensions;
using System.Linq;
using System.Text.RegularExpressions;

namespace GraphForP4.Services
{
    public static class Analyzer
    {
        const string HEADER = "header ";
        const string STRUCT = "struct "; 

        public static List<Struct> GetVariables(string input)
        {
            var cleanInput = FileHelper.InputClean(input);

            List<Header> headers = new List<Header>();
            GetBlocks(cleanInput, HEADER).ForEach(block =>
            {
                var header = new Header
                {
                    Name = block.SubStringByEndChar(0, '{').Replace('{', ' ').Trim()
                };
                GetDeclarationBlock(block, header.Name).ForEach(variable =>
                {
                    var declaration = variable.Split(" ", StringSplitOptions.RemoveEmptyEntries);
                    if (declaration.Length != 2) throw new ApplicationException("Érvénytelen változó!");

                    header.Variables.Add(new Variable(declaration[0].Trim(), declaration[1].Trim()));
                });
                headers.Add(header);
            });

            List<Struct> structs = new List<Struct>();
            GetBlocks(cleanInput, STRUCT).ForEach(block =>
            {
                var _struct = new Struct
                {
                    Name = block.SubStringByEndChar(0, '{').Replace('{', ' ').Trim()
                };
                GetDeclarationBlock(block, _struct.Name).ForEach(variable =>
                {
                    var declaration = variable.Split(" ", StringSplitOptions.RemoveEmptyEntries);
                    if (declaration.Length != 2) throw new ApplicationException("Érvénytelen változó!");

                    var variableType = declaration[0].Trim();
                    if (structs.Any(x => x.Name == variableType))
                    {
                        var childStruct = structs.Find(x => x.Name == variableType);
                        _struct.Structs.Add(declaration[1].Trim(), childStruct);
                    }
                    else if (headers.Any(x => x.Name == variableType))
                    {
                        var header = headers.Find(x => x.Name == variableType);
                        _struct.Headers.Add(declaration[1].Trim(), header);
                    }
                    else
                    {
                        _struct.Variables.Add(new Variable(variableType, declaration[1].Trim()));
                    }
                });

                structs.Add(_struct);
            });

            return structs;
        }

        private static List<string> GetBlocks(string input, string key)
        {
            List<string> blocks = new List<string>();
            input.AllIndexesOf(key).ForEach(start =>
            {
                blocks.Add(input.SubStringByEndChar(start, '}').Replace(key, String.Empty).Trim());
            });

            return blocks;
        }

        private static List<String> GetDeclarationBlock(string block, string name)
        {
            return Regex.Replace(FileHelper.GetMethod(block, name), @"{|}", String.Empty).Trim().Split(";", StringSplitOptions.RemoveEmptyEntries).ToList();
        }
    }
}
