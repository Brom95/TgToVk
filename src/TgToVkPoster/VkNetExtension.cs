using System.Net;
using VkNet;
using VkNet.Model.Attachments;
using VkNet.Model.RequestParams;
using System.Net.Http.Headers;
using System.Text;

public static class VkNetExtension
{
    private static async Task<string> UploadFile(string serverUrl, string file, string fileExtension)
    {
        // Получение массива байтов из файла
        var data = GetBytes(file);

        // Создание запроса на загрузку файла на сервер
        using (var client = new HttpClient())
        {
            var requestContent = new MultipartFormDataContent();
            var content = new ByteArrayContent(data);
            content.Headers.ContentType = MediaTypeHeaderValue.Parse("multipart/form-data");
            requestContent.Add(content, "file", $"file.{fileExtension}");

            var response = client.PostAsync(serverUrl, requestContent).Result;
            return Encoding.Default.GetString(await response.Content.ReadAsByteArrayAsync());
        }
    }
    private static byte[] GetBytes(string filePath)
    {
        return File.ReadAllBytes(filePath);
    }

    public static void AddAttachment(this VkApi api, ref WallPostParams wallparams, string path)
    {
        var ownerId = (wallparams.OwnerId > 0) ? wallparams.OwnerId : wallparams.OwnerId * -1;
        var uploadServer = api.Photo.GetWallUploadServer(ownerId);
        var content = UploadFile(uploadServer.UploadUrl, path, Path.GetExtension(path).Remove(0, 1)).GetAwaiter().GetResult();
        var photoUploadResult = api.Photo.SaveWallPhoto(
            content,
            (ulong?)ownerId,
            ((wallparams.OwnerId < 0)) ? (ulong?)ownerId : null
            );
        if (wallparams.Attachments is not null)
            wallparams.Attachments.Concat(photoUploadResult);
        else
            wallparams.Attachments = photoUploadResult;
    }
}