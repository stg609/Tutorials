using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;
using Microsoft.Net.Http.Headers;

namespace FileUpload.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class StreamingController : ControllerBase
    {
        private readonly ILogger<StreamingController> _logger;
        private readonly string[] _permittedExtensions = { ".txt" };
        private readonly string _targetFilePath = "C:\\tempFildUpload";
        private readonly long _fileSizeLimit = 100 * 1048576;

        private static readonly FormOptions _defaultFormOptions = new FormOptions();

        public StreamingController(ILogger<StreamingController> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// 演示 straming 方式
        /// </summary>
        /// <param name="files"></param>
        /// <returns></returns>
        [HttpPost]
        [DisableFormValueModelBinding]
        public async Task<IActionResult> UploadAsync()
        {
            if (!MultipartRequestHelper.IsMultipartContentType(Request.ContentType))
            {
                ModelState.AddModelError("File",
                    $"The request couldn't be processed (Error 1).");
                // Log error

                return BadRequest(ModelState);
            }

            var boundary = MultipartRequestHelper.GetBoundary(
                MediaTypeHeaderValue.Parse(Request.ContentType),
                _defaultFormOptions.MultipartBoundaryLengthLimit);
            var reader = new MultipartReader(boundary, HttpContext.Request.Body);
            var section = await reader.ReadNextSectionAsync();

            while (section != null)
            {
                var hasContentDispositionHeader =
                    ContentDispositionHeaderValue.TryParse(
                        section.ContentDisposition, out var contentDisposition);

                if (hasContentDispositionHeader)
                {
                    // This check assumes that there's a file
                    // present without form data. If form data
                    // is present, this method immediately fails
                    // and returns the model error.
                    if (!MultipartRequestHelper
                        .HasFileContentDisposition(contentDisposition))
                    {
                        ModelState.AddModelError("File",
                            $"The request couldn't be processed (Error 2).");
                        // Log error

                        return BadRequest(ModelState);
                    }
                    else
                    {
                        // Don't trust the file name sent by the client. To display
                        // the file name, HTML-encode the value.
                        var trustedFileNameForDisplay = WebUtility.HtmlEncode(
                                contentDisposition.FileName.Value);
                        var trustedFileNameForFileStorage = Path.GetRandomFileName();

                        // **WARNING!**
                        // In the following example, the file is saved without
                        // scanning the file's contents. In most production
                        // scenarios, an anti-virus/anti-malware scanner API
                        // is used on the file before making the file available
                        // for download or for use by other systems. 
                        // For more information, see the topic that accompanies 
                        // this sample.

                        var streamedFileContent = await FileHelpers.ProcessStreamedFile(
                            section, contentDisposition, ModelState,
                            _permittedExtensions, _fileSizeLimit);

                        if (!ModelState.IsValid)
                        {
                            return BadRequest(ModelState);
                        }

                        using (var targetStream = System.IO.File.Create(
                            Path.Combine(_targetFilePath, trustedFileNameForFileStorage)))
                        {
                            await targetStream.WriteAsync(streamedFileContent);

                            _logger.LogInformation(
                                "Uploaded file '{TrustedFileNameForDisplay}' saved to " +
                                "'{TargetFilePath}' as {TrustedFileNameForFileStorage}",
                                trustedFileNameForDisplay, _targetFilePath,
                                trustedFileNameForFileStorage);
                        }
                    }
                }

                // Drain any remaining section body that hasn't been consumed and
                // read the headers for the next section.
                section = await reader.ReadNextSectionAsync();
            }

            return Created(nameof(StreamingController), null);
        }

        [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
        public class DisableFormValueModelBindingAttribute : Attribute, IResourceFilter
        {
            public void OnResourceExecuting(ResourceExecutingContext context)
            {
                var factories = context.ValueProviderFactories;
                factories.RemoveType<FormValueProviderFactory>();
                factories.RemoveType<FormFileValueProviderFactory>();
                factories.RemoveType<JQueryFormValueProviderFactory>();
            }

            public void OnResourceExecuted(ResourceExecutedContext context)
            {
            }
        }

        public static class MultipartRequestHelper
        {
            // Content-Type: multipart/form-data; boundary="----WebKitFormBoundarymx2fSWqWSd0OxQqq"
            // The spec at https://tools.ietf.org/html/rfc2046#section-5.1 states that 70 characters is a reasonable limit.
            public static string GetBoundary(MediaTypeHeaderValue contentType, int lengthLimit)
            {
                var boundary = HeaderUtilities.RemoveQuotes(contentType.Boundary).Value;

                if (string.IsNullOrWhiteSpace(boundary))
                {
                    throw new InvalidDataException("Missing content-type boundary.");
                }

                //注意这里的boundary.Length指的是boundary=---------------------------99614912995中，等号后面---------------------------99614912995字符串的长度，
                //也就是section分隔符的长度，上面也说了这个长度一般不会超过70个字符是比较合理的
                if (boundary.Length > lengthLimit)
                {
                    throw new InvalidDataException(
                        $"Multipart boundary length limit {lengthLimit} exceeded.");
                }

                return boundary;
            }

            public static bool IsMultipartContentType(string contentType)
            {
                return !string.IsNullOrEmpty(contentType)
                       && contentType.IndexOf("multipart/", StringComparison.OrdinalIgnoreCase) >= 0;
            }

            //如果section是表单键值对section，那么本方法返回true
            public static bool HasFormDataContentDisposition(ContentDispositionHeaderValue contentDisposition)
            {
                // Content-Disposition: form-data; name="key";
                return contentDisposition != null
                    && contentDisposition.DispositionType.Equals("form-data")
                    && string.IsNullOrEmpty(contentDisposition.FileName.Value)
                    && string.IsNullOrEmpty(contentDisposition.FileNameStar.Value);
            }

            public static bool HasFileContentDisposition(ContentDispositionHeaderValue contentDisposition)
            {
                // Content-Disposition: form-data; name="myfile1"; filename="Misc 002.jpg"
                return contentDisposition != null
                    && contentDisposition.DispositionType.Equals("form-data")
                    && (!string.IsNullOrEmpty(contentDisposition.FileName.Value)
                        || !string.IsNullOrEmpty(contentDisposition.FileNameStar.Value));
            }
        }

        public static class FileHelpers
        {
            // If you require a check on specific characters in the IsValidFileExtensionAndSignature
            // method, supply the characters in the _allowedChars field.
            private static readonly byte[] _allowedChars = { };

            // For more file signatures, see the File Signatures Database (https://www.filesignatures.net/)
            // and the official specifications for the file types you wish to add.
            private static readonly Dictionary<string, List<byte[]>> _fileSignature = new Dictionary<string, List<byte[]>>
            {
                { ".gif", new List<byte[]> { new byte[] { 0x47, 0x49, 0x46, 0x38 } } },
                { ".png", new List<byte[]> { new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A } } },
                { ".jpeg", new List<byte[]>
                    {
                        new byte[] { 0xFF, 0xD8, 0xFF, 0xE0 },
                        new byte[] { 0xFF, 0xD8, 0xFF, 0xE2 },
                        new byte[] { 0xFF, 0xD8, 0xFF, 0xE3 },
                    }
                },
                { ".jpg", new List<byte[]>
                    {
                        new byte[] { 0xFF, 0xD8, 0xFF, 0xE0 },
                        new byte[] { 0xFF, 0xD8, 0xFF, 0xE1 },
                        new byte[] { 0xFF, 0xD8, 0xFF, 0xE8 },
                    }
                },
                { ".zip", new List<byte[]>
                    {
                        new byte[] { 0x50, 0x4B, 0x03, 0x04 },
                        new byte[] { 0x50, 0x4B, 0x4C, 0x49, 0x54, 0x45 },
                        new byte[] { 0x50, 0x4B, 0x53, 0x70, 0x58 },
                        new byte[] { 0x50, 0x4B, 0x05, 0x06 },
                        new byte[] { 0x50, 0x4B, 0x07, 0x08 },
                        new byte[] { 0x57, 0x69, 0x6E, 0x5A, 0x69, 0x70 },
                    }
                },
            };

            public static async Task<byte[]> ProcessStreamedFile(
            MultipartSection section, ContentDispositionHeaderValue contentDisposition,
            ModelStateDictionary modelState, string[] permittedExtensions, long sizeLimit)
            {
                try
                {
                    using (var memoryStream = new MemoryStream())
                    {
                        await section.Body.CopyToAsync(memoryStream);

                        // Check if the file is empty or exceeds the size limit.
                        if (memoryStream.Length == 0)
                        {
                            modelState.AddModelError("File", "The file is empty.");
                        }
                        else if (memoryStream.Length > sizeLimit)
                        {
                            var megabyteSizeLimit = sizeLimit / 1048576;
                            modelState.AddModelError("File",
                            $"The file exceeds {megabyteSizeLimit:N1} MB.");
                        }
                        else if (!IsValidFileExtensionAndSignature(
                            contentDisposition.FileName.Value, memoryStream,
                            permittedExtensions))
                        {
                            modelState.AddModelError("File",
                                "The file type isn't permitted or the file's " +
                                "signature doesn't match the file's extension.");
                        }
                        else
                        {
                            return memoryStream.ToArray();
                        }
                    }
                }
                catch (Exception ex)
                {
                    modelState.AddModelError("File",
                        "The upload failed. Please contact the Help Desk " +
                        $" for support. Error: {ex.HResult}");
                    // Log the exception
                }

                return Array.Empty<byte>();
            }

            private static bool IsValidFileExtensionAndSignature(string fileName, Stream data, string[] permittedExtensions)
            {
                if (string.IsNullOrEmpty(fileName) || data == null || data.Length == 0)
                {
                    return false;
                }

                var ext = Path.GetExtension(fileName).ToLowerInvariant();

                if (string.IsNullOrEmpty(ext) || !permittedExtensions.Contains(ext))
                {
                    return false;
                }

                data.Position = 0;

                using (var reader = new BinaryReader(data))
                {
                    if (ext.Equals(".txt") || ext.Equals(".csv") || ext.Equals(".prn"))
                    {
                        if (_allowedChars.Length == 0)
                        {
                            // Limits characters to ASCII encoding.
                            for (var i = 0; i < data.Length; i++)
                            {
                                if (reader.ReadByte() > sbyte.MaxValue)
                                {
                                    return false;
                                }
                            }
                        }
                        else
                        {
                            // Limits characters to ASCII encoding and
                            // values of the _allowedChars array.
                            for (var i = 0; i < data.Length; i++)
                            {
                                var b = reader.ReadByte();
                                if (b > sbyte.MaxValue ||
                                    !_allowedChars.Contains(b))
                                {
                                    return false;
                                }
                            }
                        }

                        return true;
                    }

                    // Uncomment the following code block if you must permit
                    // files whose signature isn't provided in the _fileSignature
                    // dictionary. We recommend that you add file signatures
                    // for files (when possible) for all file types you intend
                    // to allow on the system and perform the file signature
                    // check.
                    /*
                    if (!_fileSignature.ContainsKey(ext))
                    {
                        return true;
                    }
                    */

                    // File signature check
                    // --------------------
                    // With the file signatures provided in the _fileSignature
                    // dictionary, the following code tests the input content's
                    // file signature.
                    var signatures = _fileSignature[ext];
                    var headerBytes = reader.ReadBytes(signatures.Max(m => m.Length));

                    return signatures.Any(signature =>
                        headerBytes.Take(signature.Length).SequenceEqual(signature));
                }
            }
        }
    }
}
