using AngularApp.Controllers;
using AngularApp.Extensions;
using GraphForP4.Extensions;
using GraphForP4.Models;
using GraphForP4.Services;
using GraphForP4.ViewModels;
using GraphForP4.Helpers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using Xunit;
using System.Threading.Tasks;
using Persistence;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;

namespace Test
{
    public class ControllerTest : IDisposable
    {
        private readonly Mock<IHttpContextAccessor> mockHttpContextAccessor;
        private readonly Mock<ISession> mockSession;
        private readonly DefaultHttpContext context;

        private readonly P4Context p4Context;
        private readonly List<P4File> files;

        public ControllerTest()
        {
            mockHttpContextAccessor = new Mock<IHttpContextAccessor>();
            mockSession = new Mock<ISession>();
            context = new DefaultHttpContext();
            var fakeTenantId = "test";
            context.Request.Headers["Tenant-ID"] = fakeTenantId;
            mockHttpContextAccessor.Setup(_ => _.HttpContext).Returns(context);
            mockHttpContextAccessor.Setup(_ => _.HttpContext.Session).Returns(mockSession.Object);

            var options = new DbContextOptionsBuilder<P4Context>()
            .UseInMemoryDatabase("ControllerTest")
            .Options;

            p4Context = new P4Context(options);
            p4Context.Database.EnsureCreated();

            files = new List<P4File>
            {
                new P4File { Id = 1, FileName = "demo1", Content = Encoding.ASCII.GetBytes("Teszteles fajl!"), Hash = CreateHash(Encoding.ASCII.GetBytes("Teszteles fajl!")), CreatedDate = DateTime.Now },
                new P4File { Id = 2, FileName = "demo2", Content = Encoding.ASCII.GetBytes("Ez egy teszt!"), Hash = CreateHash(Encoding.ASCII.GetBytes("Ez egy teszt!")), CreatedDate = DateTime.Now }
            };

            p4Context.P4Files.AddRange(files);
            p4Context.SaveChanges();
        }

        #region GraphController

        [Theory]
        [InlineData(1)]
        [InlineData(2)]
        public void FileUploadTest(int no)
        {
            var graphController = new GraphController(mockHttpContextAccessor.Object);
            var file = new FileData { Name = "Valami", Content = System.IO.File.ReadAllText($@"..\..\..\..\AngularApp\Files\demo{no}.txt") };
            var result = graphController.FileUpload(file);

            var objectResult = Assert.IsType<OkObjectResult>(result);
            var model = Assert.IsAssignableFrom<FileData>(objectResult.Value);
            Assert.Equal(file, model);
        }

        [Fact]
        public void FileUploadWithOutContentTest()
        {
            var graphController = new GraphController(mockHttpContextAccessor.Object);
            var file = new FileData();
            var result = graphController.FileUpload(file);

            var objectResult = Assert.IsType<BadRequestObjectResult>(result);
            var model = Assert.IsAssignableFrom<string>(objectResult.Value);
            Assert.Equal("Üres fájl!", model);
        }

        [Fact]
        public void FileUploadWithApplyElseIfTest()
        {
            var graphController = new GraphController(mockHttpContextAccessor.Object);
            var file = new FileData { Name = "Valami", Content = System.IO.File.ReadAllText(@"..\..\..\..\AngularApp\Files\applyElseIf.txt") };
            var result = graphController.FileUpload(file);

            var objectResult = Assert.IsType<BadRequestObjectResult>(result);
            var model = Assert.IsAssignableFrom<string>(objectResult.Value);
            Assert.Equal("Nem megengedett nyelvi elem! (else if)", model);
        }

        [Fact]
        public void FileUploadWithActionIfTest()
        {
            var graphController = new GraphController(mockHttpContextAccessor.Object);
            var file = new FileData { Name = "Valami", Content = System.IO.File.ReadAllText(@"..\..\..\..\AngularApp\Files\actionIf.txt") };
            var result = graphController.FileUpload(file);

            var objectResult = Assert.IsType<BadRequestObjectResult>(result);
            var model = Assert.IsAssignableFrom<string>(objectResult.Value);
            Assert.Equal("Nem megengedett nyelvi elem! (Akción belüli elágazás)", model);
        }

        [Theory]
        [InlineData(1)]
        [InlineData(2)]
        public void GetControlFlowGraphTest(int no)
        {
            var graphController = new GraphController(mockHttpContextAccessor.Object);
            var file = System.IO.File.ReadAllText($@"..\..\..\..\AngularApp\Files\demo{no}.txt");
            var controlFlowGraph = P4ToGraph.ControlFlowGraph(ref file);

            var value = Encoding.ASCII.GetBytes(controlFlowGraph.ToJson());
            mockSession.Setup(_ => _.TryGetValue(Key.ControlFlowGraph.ToString("g"), out value)).Returns(true);
            var result = graphController.GetGraph(Key.ControlFlowGraph.ToString("g"));

            var objectResult = Assert.IsType<OkObjectResult>(result);
            var model = Assert.IsAssignableFrom<IEnumerable<ViewNode>>(objectResult.Value).ToList();
            var viewNodes = controlFlowGraph.Serialize().ToList();

            CompareViewNodeLists(viewNodes, model);
        }

        [Theory]
        [InlineData(1)]
        [InlineData(2)]
        public void GetDataFlowGraphTest(int no)
        {
            var graphController = new GraphController(mockHttpContextAccessor.Object);
            var file = System.IO.File.ReadAllText($@"..\..\..\..\AngularApp\Files\demo{no}.txt");
            var controlFlowGraph = P4ToGraph.ControlFlowGraph(ref file);
            var dataFlowGraph = P4ToGraph.DataFlowGraph(file, controlFlowGraph);

            var value = Encoding.ASCII.GetBytes(dataFlowGraph.ToJson());
            mockSession.Setup(_ => _.TryGetValue(Key.DataFlowGraph.ToString("g"), out value)).Returns(true);
            var result = graphController.GetGraph(Key.DataFlowGraph.ToString("g"));

            var objectResult = Assert.IsType<OkObjectResult>(result);
            var model = Assert.IsAssignableFrom<IEnumerable<ViewNode>>(objectResult.Value).ToList();
            var viewNodes = dataFlowGraph.Serialize().ToList();

            CompareViewNodeLists(viewNodes, model);
        }

        private void CompareViewNodeLists(List<ViewNode> viewNodes, List<ViewNode> model)
        {
            for (var i = 0; i < viewNodes.Count; ++i)
            {
                Assert.Equal(viewNodes[i].Id, model[i].Id);
                Assert.Equal(viewNodes[i].FillColor, model[i].FillColor);
                Assert.Equal(viewNodes[i].FontColor, model[i].FontColor);
                Assert.Equal(viewNodes[i].Number, model[i].Number);
                Assert.Equal(viewNodes[i].ParentId, model[i].ParentId);
                Assert.Equal(viewNodes[i].Shape, model[i].Shape);
                Assert.Equal(viewNodes[i].SubGraph, model[i].SubGraph);
                Assert.Equal(viewNodes[i].Text, model[i].Text);
                Assert.Equal(viewNodes[i].Tooltip, model[i].Tooltip);
                for (var j = 0; j < viewNodes[i].Edges.Count; ++j)
                {
                    Assert.Equal(viewNodes[i].Edges[j].ArrowType, model[i].Edges[j].ArrowType);
                    Assert.Equal(viewNodes[i].Edges[j].Child, model[i].Edges[j].Child);
                    Assert.Equal(viewNodes[i].Edges[j].Color, model[i].Edges[j].Color);
                    Assert.Equal(viewNodes[i].Edges[j].Parent, model[i].Edges[j].Parent);
                    Assert.Equal(viewNodes[i].Edges[j].Style, model[i].Edges[j].Style);

                }
            }
        }

        [Fact]
        public void GetGraphWithNotExistKeyTest()
        {
            var graphController = new GraphController(mockHttpContextAccessor.Object);
            var result = graphController.GetGraph("TestData");

            var objectResult = Assert.IsType<BadRequestObjectResult>(result);
            var model = Assert.IsAssignableFrom<string>(objectResult.Value);
            Assert.Equal("Érvénytelen behívás!", model);
        }

        [Fact]
        public void GetGraphWithoutFileUploadTest()
        {
            var graphController = new GraphController(mockHttpContextAccessor.Object);
            var result = graphController.GetGraph(Key.ControlFlowGraph.ToString("g"));

            var objectResult = Assert.IsType<BadRequestObjectResult>(result);
            var model = Assert.IsAssignableFrom<string>(objectResult.Value);
            Assert.Equal("Kérem töltsön fel először fájlt!", model);
        }

        #endregion

        #region AnalyzerController

        [Theory]
        [InlineData(1)]
        [InlineData(2)]
        public void GetStructsTest(int no)
        {
            var analyzerController = new AnalyzerController(mockHttpContextAccessor.Object);
            var file = new FileData { Name = "Valami", Content = System.IO.File.ReadAllText($@"..\..\..\..\AngularApp\Files\demo{no}.txt") };

            var value = Encoding.ASCII.GetBytes(JsonSerializer.Serialize(file));
            mockSession.Setup(_ => _.TryGetValue(Key.File.ToString("g"), out value)).Returns(true);
            var result = analyzerController.GetStructs();

            var objectResult = Assert.IsType<OkObjectResult>(result);
            var model = Assert.IsAssignableFrom<List<Struct>>(objectResult.Value);
            var structs = Analyzer.GetStructs(file.Content);

            for (var i = 0; i < structs.Count; ++i)
            {
                CompareStuct(structs[i], model[i]);
            }
        }

        private void CompareStuct(Struct a, Struct b)
        {
            Assert.Equal(a.Name, b.Name);
            CompareVariableList(a.Variables, b.Variables);
            foreach(var headerPair in a.Headers)
            {
                Assert.True(b.Headers.ContainsKey(headerPair.Key));
                var bHeader = b.Headers[headerPair.Key];
                Assert.Equal(headerPair.Value.Name, bHeader.Name);
                CompareVariableList(headerPair.Value.Variables, bHeader.Variables);
            }

            foreach(var structPair in a.Structs)
            {
                Assert.True(b.Structs.ContainsKey(structPair.Key));
                CompareStuct(structPair.Value, b.Structs[structPair.Key]);
            }
        }

        private void CompareVariableList(List<Variable> a, List<Variable> b)
        {
            for(var i = 0; i < a.Count; ++i)
            {
                Assert.Equal(a[i].Name, b[i].Name);
                Assert.Equal(a[i].Type, b[i].Type);
            }
        }

        [Fact]
        public void GetStructsWithoutFileUploadTest()
        {
            var analyzerController = new AnalyzerController(mockHttpContextAccessor.Object);

            var result = analyzerController.GetStructs();

            var objectResult = Assert.IsType<BadRequestObjectResult>(result);
            var model = Assert.IsAssignableFrom<string>(objectResult.Value);
            Assert.Equal("Kérem töltsön fel először fájlt!", model);
        }

        [Theory]
        [InlineData(1)]
        [InlineData(2)]
        public void UpdateTest(int no)
        {
            var analyzerController = new AnalyzerController(mockHttpContextAccessor.Object);

            var file = new FileData { Name = "Valami", Content = System.IO.File.ReadAllText($@"..\..\..\..\AngularApp\Files\demo{no}.txt") };
            var value = Encoding.ASCII.GetBytes(JsonSerializer.Serialize(file));
            mockSession.Setup(_ => _.TryGetValue(Key.File.ToString("g"), out value)).Returns(true);

            var content = file.Content;

            var controlFlowGraph = P4ToGraph.ControlFlowGraph(ref content);
            var controlValue = Encoding.ASCII.GetBytes(controlFlowGraph.ToJson());
            mockSession.Setup(_ => _.TryGetValue(Key.ControlFlowGraph.ToString("g"), out controlValue)).Returns(true);

            var dataFlowGraph = P4ToGraph.DataFlowGraph(content, controlFlowGraph);
            var dataValue = Encoding.ASCII.GetBytes(dataFlowGraph.ToJson());
            mockSession.Setup(_ => _.TryGetValue(Key.DataFlowGraph.ToString("g"), out dataValue)).Returns(true);

            var analyzeDatas = new List<AnalyzeData>();

            for (var i = 0; i < 5; ++i)
            {
                var structs = Analyzer.GetStructs(file.Content);
                var _struct = structs.FirstOrDefault(x => x.Name == "headers_t");
                Random random = new Random();
                var randomNumber = random.Next(0, 2);

                var ethernet = _struct.Headers["ethernet"];
                if (randomNumber == 1)
                {
                    ethernet.Valid = true;
                    ethernet.Variables.ForEach(x => x.IsInitialize = true);
                }

                randomNumber = random.Next(0, 2);

                var ipv4 = _struct.Headers["ipv4"];
                if (randomNumber == 1)
                {
                    ipv4.Valid = true;
                    ipv4.Variables.ForEach(x => x.IsInitialize = true);
                }

                analyzeDatas.Add(new AnalyzeData
                {
                    Id = i,
                    StartState = structs,
                    EndState = structs
                });
            }

            var result = analyzerController.Update(analyzeDatas);

            var objectResult = Assert.IsType<OkObjectResult>(result);
            var model = Assert.IsAssignableFrom<CalculatedData>(objectResult.Value);

            var analyzers = new List<Analyzer>();

            analyzeDatas.ForEach(x =>
            {
                analyzers.Add(new Analyzer(controlFlowGraph.ToJson(), dataFlowGraph.ToJson(), x, file.Content));
            });

            Parallel.ForEach(analyzers, (analyzer) =>
            {
                analyzer.Analyze();
                analyzer.FinishOperations();
            });

            analyzers.DistinctGraphs(out List<List<ViewNode>> controlFlowGraphs, out List<List<ViewNode>> dataFlowGraphs);
            analyzers.CreateCharts(out BarChartData readAndWriteChartData, out PieChartData useVariable, out PieChartData useful, out BarChartData headers);

            var calculateData = new CalculatedData
            {
                ControlFlowGraphs = controlFlowGraphs,
                DataFlowGraphs = dataFlowGraphs,
                ReadAndWriteChartData = readAndWriteChartData,
                UseVariable = useVariable,
                Useful = useful,
                Headers = headers,
                File = file
            };

            Assert.Equal(calculateData.File.Name, model.File.Name);
            Assert.Equal(calculateData.File.Content, model.File.Content);
            CompareBarChartData(calculateData.ReadAndWriteChartData, model.ReadAndWriteChartData);
            CompareBarChartData(calculateData.Headers, model.Headers);
            ComparePieChartData(calculateData.UseVariable, model.UseVariable);
            ComparePieChartData(calculateData.Useful, model.Useful);
        }

        private void CompareBarChartData(BarChartData a, BarChartData b)
        {
            for (var i = 0; i < a.Labels.Count; ++i)
            {
                Assert.Equal(a.Labels[i], b.Labels[i]);
            }

            foreach(var keyValuePair in a.DoubleDatas)
            {
                Assert.True(b.DoubleDatas.ContainsKey(keyValuePair.Key));
                var bValue = b.DoubleDatas[keyValuePair.Key];
                var aValue = keyValuePair.Value;

                for(var i = 0; i < aValue.Count; ++i)
                {
                    Assert.Equal(aValue[i], bValue[i]);
                }
            }
        }

        private void ComparePieChartData(PieChartData a, PieChartData b)
        {
            for(var i = 0; i < a.Labels.Count; ++i)
            {
                Assert.Equal(a.Labels[i], b.Labels[i]);
            }

            for (var i = 0; i < a.Datas.Count; ++i)
            {
                Assert.Equal(a.Datas[i], b.Datas[i]);
            }

            for (var i = 0; i < a.DoubleDatas.Count; ++i)
            {
                Assert.Equal(a.DoubleDatas[i], b.DoubleDatas[i]);
            }
        }

        [Fact]
        public void UpdateTestWithoutFileUploadTest()
        {
            var analyzerController = new AnalyzerController(mockHttpContextAccessor.Object);

            var result = analyzerController.Update(null);

            var objectResult = Assert.IsType<BadRequestObjectResult>(result);
            var model = Assert.IsAssignableFrom<string>(objectResult.Value);
            Assert.Equal("Kérem töltsön fel először fájlt!", model);
        }

        #endregion

        #region FileController

        [Theory]
        [InlineData(1)]
        [InlineData(2)]
        public void GetP4FileTest(int no)
        {
            var fileController = new FileController(mockHttpContextAccessor.Object, p4Context);

            var result = fileController.GetFile(no);

            var objectResult = Assert.IsType<OkObjectResult>(result);
            var model = Assert.IsAssignableFrom<FileData>(objectResult.Value);
            var file = files[no-1].ToFileData();
            Assert.Equal(file.Id, model.Id);
            Assert.Equal(file.Content, model.Content);
            Assert.Equal(file.CreateDate, model.CreateDate);
            Assert.Equal(file.Name, model.Name);
        }

        [Theory]
        [InlineData(3)]
        [InlineData(4)]
        public void GetP4FileWithNotExistingIdTest(int no)
        {
            var fileController = new FileController(mockHttpContextAccessor.Object, p4Context);

            var result = fileController.GetFile(no);

            var objectResult = Assert.IsType<BadRequestObjectResult>(result);
            var model = Assert.IsAssignableFrom<string>(objectResult.Value);
            Assert.Equal($"A megadott azonosítóval nincs letárolt fájl! Id: {no}", model);
        }

        [Fact]

        public void GetP4FilesTest()
        {
            var fileController = new FileController(mockHttpContextAccessor.Object, p4Context);

            var result = fileController.GetFiles();

            var objectResult = Assert.IsType<OkObjectResult>(result);
            var model = Assert.IsAssignableFrom<List<FileData>>(objectResult.Value);
            var matchFiles = new List<FileData>();
            files.ForEach(x => matchFiles.Add(x.ToFileData()));

            for(var i = 1; i < 3; ++i)
            {
                var match = matchFiles.FirstOrDefault(x => x.Id == i);
                var compare = model.FirstOrDefault(x => x.Id == i);
                Assert.Equal(match.Name, compare.Name);
                Assert.Equal(match.CreateDate, compare.CreateDate);
                Assert.NotEqual(match.Content, compare.Content);
                Assert.Equal(string.Empty, compare.Content);
            }
        }

        [Theory]
        [InlineData("Valami", "Ez a valami lehetne akar semmi!")]
        [InlineData("Valami2", "Ez a valami lehetne akar semmi!")]
        public void SetP4FileTest(string name, string content)
        {
            var fileController = new FileController(mockHttpContextAccessor.Object, p4Context);

            var file = new FileData { Name = name, Content = content };

            var result = fileController.UploadFileToDataBase(file);

            var objectResult = Assert.IsType<OkObjectResult>(result);
            var model = Assert.IsAssignableFrom<FileData>(objectResult.Value);
            Assert.Equal(content, model.Content);
            Assert.Equal(name, model.Name);
        }

        [Fact]
        public void SetP4FileWithoutNameTest()
        {
            var fileController = new FileController(mockHttpContextAccessor.Object, p4Context);

            var file = new FileData { Content = "valami" };

            var result = fileController.UploadFileToDataBase(file);

            var objectResult = Assert.IsType<BadRequestObjectResult>(result);
            var model = Assert.IsAssignableFrom<string>(objectResult.Value);
            Assert.Equal("A fájl név megadása kötelező!", model);
        }

        [Theory]
        [InlineData("Teszteles fajl!")]
        [InlineData("Ez egy teszt!")]
        public void SetP4FileWithExistContentTest(string content)
        {
            var fileController = new FileController(mockHttpContextAccessor.Object, p4Context);

            var file = new FileData { Name = "valami", Content = content };

            var result = fileController.UploadFileToDataBase(file);

            var objectResult = Assert.IsType<BadRequestObjectResult>(result);
            var model = Assert.IsAssignableFrom<string>(objectResult.Value);
            Assert.Equal("Ez a fájl már feltöltésre került!", model);
        }

        #endregion

        private string CreateHash(byte[] content)
        {
            using SHA256 hashAlgorithm = SHA256.Create();
            byte[] data = hashAlgorithm.ComputeHash(content);
            var stringBuilder = new StringBuilder();

            for (int i = 0; i < data.Length; i++)
            {
                stringBuilder.Append(data[i].ToString("x2"));
            }

            return stringBuilder.ToString();
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                p4Context.Database.EnsureDeleted();
                p4Context.Dispose();
            }
        }
    }
}
