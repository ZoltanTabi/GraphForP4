using GraphForP4.ViewModels;
using Persistence;

namespace AngularApp.Extensions
{
    public static class FileConverter
    {
        public static P4File ToP4File(this FileData file)
        {
            return new P4File
            {
                FileName = file.Name,
                Content = System.Text.Encoding.ASCII.GetBytes(file.Content)
            };
        }

        public static FileData ToFileData(this P4File file)
        {
            return new FileData
            {
                Id = file.Id,
                Name = file.FileName,
                CreateDate = file.CreatedDate,
                Content = file.Content != null && file.Content.Length > 0 ? System.Text.Encoding.ASCII.GetString(file.Content) : string.Empty
            };
        }
    }
}
