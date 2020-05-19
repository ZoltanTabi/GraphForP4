using Microsoft.EntityFrameworkCore.Internal;
using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using System.Linq;
using System.IO;

namespace Persistence
{
    public class Service : IDisposable
    {
        private readonly P4Context context;
        
        public Service(P4Context context)
        {
            this.context = context;
        }

        public P4File SetP4File(P4File file)
        {
            if (file.FileName == null || file.FileName.Trim().Length == 0)
            {
                throw new ApplicationException("A fájl név megadása kötelező!");
            }
            if (file.Content == null)
            {
                throw new ApplicationException("A tartalom megadása kötelező!");
            }

            var hash = CreateHash(file.Content);
            if (hash != string.Empty)
            {
                if (!context.P4Files.Any(x => x.Hash == hash))
                {
                    file.FileName = Path.GetFileNameWithoutExtension(file.FileName);
                    file.Hash = hash;
                    file.CreatedDate = DateTime.Now;

                    context.Add(file);
                    context.SaveChanges();
                }
            }
            else
            {
                throw new ApplicationException("A hash képzése nem sikerült!");
            }    

            return file.Hash != String.Empty ? file : null;
        }

        public List<P4File> GetP4Files()
        {
            return context.P4Files.Select(x => new P4File
            {
                Id = x.Id,
                FileName = x.FileName,
                CreatedDate = x.CreatedDate
            }).ToList();
        }

        public P4File GetP4File(int id)
        {
            var file = context.P4Files.Find(id);
            if (file != null)
            {
                return file;
            }
            else
            {
                throw new ApplicationException($"A megadott azonosítóval nincs letárolt fájl! Id: {id}");
            }
        }

        private string CreateHash(byte[] content)
        {
            var result = string.Empty;

            using (SHA256 hashAlgorithm = SHA256.Create())
            {
                byte[] data = hashAlgorithm.ComputeHash(content);
                var stringBuilder = new StringBuilder();

                for (int i = 0; i < data.Length; i++)
                {
                    stringBuilder.Append(data[i].ToString("x2"));
                }

                result = stringBuilder.ToString();
            }

            return result;
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
        }
    }
}
