using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.StaticFiles;

namespace CityInfo.API.Controllers;
[Route("api/files")]
[ApiController]
public class FilesController : ControllerBase
{
    private readonly FileExtensionContentTypeProvider _fileContentProvider;

    public FilesController(FileExtensionContentTypeProvider fileContentProvider)
    {
        _fileContentProvider = fileContentProvider ?? throw new ArgumentNullException(nameof(fileContentProvider));
    }

    [HttpGet("{fileId}")]
    public ActionResult GetFile(string fileId)
    {
        var pathToFile = "testimage.jpg";

        if (!System.IO.File.Exists(pathToFile))
        {
            return NotFound();
        }

        if (!_fileContentProvider.TryGetContentType(pathToFile, out var contentType))
        {
            contentType = "application/octect-stream";
        }

        var bytes = System.IO.File.ReadAllBytes(pathToFile);
        return File(bytes, contentType, Path.GetFileName(pathToFile));
    }
}
