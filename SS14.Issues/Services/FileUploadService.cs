using Microsoft.AspNetCore.Components.Forms;
using MimeTypes;
using Serilog;

namespace SS14.Issues.Services;

public sealed class FileUploadService
{
    private readonly IConfiguration _configuration;

    public FileUploadService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public string GetUploadPath()
    {
        return _configuration["Uploads:UploadPath"];
    }

    public string GetFileName(Guid fileId, string mimeType)
    {
        var fileEnding = MimeTypeMap.GetExtension(mimeType);
        return $"{fileId}{fileEnding}";
    }
    
    public async Task UploadFile(IBrowserFile browserFile, Guid fileId)
    {
        var uploadPath = GetUploadPath();
        var fileName = GetFileName(fileId, browserFile.ContentType);
        
        await using FileStream fs = new( $"wwwroot/{uploadPath}/", FileMode.Create);
        await browserFile.OpenReadStream((1024 * 10000)).CopyToAsync(fs);
        Log.Information("Saved a file {filename}", fs.Name);
    }
}