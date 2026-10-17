using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SufiChain.SufiPlatform.AspNetCore.Mvc.Controllers;
using SufiChain.SufiPlatform.FileManager.FileItems;

namespace SufiChain.SufiPlatform.FileManager.Controllers;

/// <summary>Public Quick Share endpoint: <c>/s/{token}</c> (sufi-chain#436).</summary>
[ApiExplorerSettings(IgnoreApi = true)]
[Route("s")]
public class FileShareController : SufiControllerBase
{
    private readonly IFileItemAppService _fileItemAppService;

    public FileShareController(IFileItemAppService fileItemAppService)
    {
        _fileItemAppService = fileItemAppService;
    }

    [HttpGet("{token}")]
    [AllowAnonymous]
    public virtual async Task<IActionResult> GetAsync(string token)
    {
        var result = await _fileItemAppService.GetSharedContentAsync(token);
        Response.Headers.CacheControl = "no-store";
        if (result.Status == FileShareLinkStatus.Valid && result.Content?.Content != null)
        {
            Response.Headers.ContentDisposition = FileDownloadHeaders.Attachment(result.Content.FileName);
            return File(result.Content.Content, FileDownloadHeaders.NormalizeContentType(result.Content.MimeType));
        }

        var expired = result.Status is FileShareLinkStatus.Expired or FileShareLinkStatus.Revoked;
        return new ContentResult
        {
            StatusCode = expired ? (int)HttpStatusCode.Gone : (int)HttpStatusCode.NotFound,
            ContentType = "text/html; charset=utf-8",
            Content = SharePage(expired)
        };
    }

    protected virtual string SharePage(bool expired)
    {
        var fa = expired ? "این پیوند منقضی یا لغو شده است." : "این پیوند معتبر نیست.";
        var en = expired ? "This link has expired or was revoked." : "This link is not valid.";
        return "<!doctype html><html lang=\"fa\" dir=\"rtl\"><head><meta charset=\"utf-8\"><meta name=\"viewport\" content=\"width=device-width,initial-scale=1\">"
               + "<meta name=\"robots\" content=\"noindex\"><title>Sufi</title>"
               + "<style>body{font-family:system-ui,Tahoma,sans-serif;display:flex;min-height:100vh;align-items:center;justify-content:center;margin:0;background:#f6f7f9;color:#1f2937}"
               + "main{background:#fff;padding:2rem 2.5rem;border-radius:12px;box-shadow:0 2px 12px rgba(0,0,0,.08);text-align:center;max-width:28rem}"
               + "p{margin:.5rem 0}p[lang=en]{color:#6b7280}</style></head><body><main>"
               + $"<p>{WebUtility.HtmlEncode(fa)}</p><p lang=\"en\" dir=\"ltr\">{WebUtility.HtmlEncode(en)}</p></main></body></html>";
    }
}