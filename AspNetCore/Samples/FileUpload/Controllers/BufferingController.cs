using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace FileUpload.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class BufferingController : ControllerBase
    {
        private readonly ILogger<BufferingController> _logger;

        public BufferingController(ILogger<BufferingController> logger)
        {
            _logger = logger;
        }

        /*
         * 演示 buffering 方式 （也就是 Asp.Net Core 会把整个文件都一次性先读取到内存、磁盘中：小于 64k，在内存中。如果超过 64kb，则就会放在磁盘的临时文件中）。
         * 这种方式要避免上传的文件内容很大，避免消耗光内存，磁盘。
         * 
         * 这种方式使用起来最简单，只需要把 IFormFile 作为入参即可
         */

        [HttpPost]
        public async Task<IActionResult> UploadAsync(List<IFormFile> files)
        {
            long size = files.Sum(f => f.Length);

            foreach (var formFile in files)
            {
                if (formFile.Length > 0)
                {
                    var filePath = Path.GetTempFileName();

                    using (var stream = System.IO.File.Create(filePath))
                    {
                        await formFile.CopyToAsync(stream);
                    }
                }
            }

            // Process uploaded files
            // Don't rely on or trust the FileName property without validation.

            return Ok(new { count = files.Count, size });
        }
    }
}
