using System;
using Xunit;
using Persistence;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Text;
using System.Security.Cryptography;
using System.IO;
using System.Linq;

namespace Test
{
    public class PersistenceTest : IDisposable
    {
        private readonly P4Context context;
        private readonly List<P4File> files;

        public PersistenceTest()
        {
            var options = new DbContextOptionsBuilder<P4Context>()
                .UseInMemoryDatabase("P4Test")
                .Options;

            context = new P4Context(options);
            context.Database.EnsureCreated();

            var file1 = ReadFile(@"..\..\..\..\AngularApp\Files\demo1.txt");
            var file2 = ReadFile(@"..\..\..\..\AngularApp\Files\demo2.txt");

            files = new List<P4File>
            {
                new P4File { Id = 1, FileName = "demo1.txt", Content = file1.Item1, Hash = file1.Item2, CreatedDate = Convert.ToDateTime("2019.12.11") },
                new P4File { Id = 2, FileName = "demo2.txt", Content = file2.Item1, Hash = file2.Item2, CreatedDate = DateTime.Now}
            };

            context.P4Files.AddRange(files);
            context.SaveChanges();
        }

        [Fact]
        public void SetP4FileTest()
        {
            using var service = new Service(context);
            var f = ReadFile(@"..\..\..\..\AngularApp\Files\demo3.txt");
            var p4File = new P4File { Id = 3, FileName = "demo3.txt", Content = f.Item1, Hash = f.Item2, CreatedDate = Convert.ToDateTime("2019.11.23") };
            var file = service.SetP4File(p4File);

            Assert.Equal(p4File.Id, file.Id);
            Assert.Equal(Path.GetFileNameWithoutExtension(p4File.FileName), file.FileName);
            Assert.Equal(p4File.Hash, file.Hash);
            Assert.Equal(p4File.Content, file.Content);
            Assert.NotEqual(p4File.CreatedDate, file.CreatedDate);

            var f1 = ReadFile(@"..\..\..\..\AngularApp\Files\demo1.txt");
            var p4File1 = new P4File { FileName = "demo8.txt", Content = f1.Item1, Hash = f1.Item2, CreatedDate = DateTime.Now };
            var file1 = service.SetP4File(p4File1);

            Assert.Null(file1);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        public void GetP4FileTest(int no)
        {
            using var service = new Service(context);
            var file = service.GetP4File(files[no].Id);

            Assert.Equal(files[no].Id, file.Id);
            Assert.Equal(files[no].FileName, file.FileName);
            Assert.Equal(files[no].Hash, file.Hash);
        }

        [Fact]
        public void GetP4FileInvalidIdTest()
        {
            using var service = new Service(context);
            var ex = Record.Exception(() => service.GetP4File(3));

            Assert.IsType<ApplicationException>(ex);
            Assert.Equal("A megadott azonosítóval nincs letárolt fájl! Id: 3", ex.Message);
        }

        [Fact]
        public void GetP4FilesTest()
        {
            using var service = new Service(context);
            var files = service.GetP4Files().OrderBy(x => x.Id).ToList();

            Assert.Equal(files[0].Id, this.files[0].Id);
            Assert.Equal(files[0].FileName, this.files[0].FileName);
            Assert.Null(files[0].Hash);
            Assert.Null(files[0].Content);

            Assert.Equal(files[1].Id, this.files[1].Id);
            Assert.Equal(files[1].FileName, this.files[1].FileName);
            Assert.Null(files[1].Hash);
            Assert.Null(files[1].Content);
        }

        private Tuple<byte[], string> ReadFile(string fileName)
        {
            byte[] content = File.ReadAllBytes(fileName);

            Tuple<byte[], string> result = new Tuple<byte[], string>(content, CreateHash(content));
            return result;
        }

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
                context.Database.EnsureDeleted(); 
                context.Dispose();
            }
        }

    }
}
