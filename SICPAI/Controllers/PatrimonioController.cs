using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Hosting;
using System.IO;
using System;


namespace SICPAI.Controllers
{
    public class PatrimonioController : Controller
    {
        private readonly IWebHostEnvironment _env;

        public PatrimonioController(IWebHostEnvironment env)
        {
            _env = env;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Create()
        {
            return View();
        }

        public IActionResult Detail()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequestSizeLimit(10_000_000)] // 10 MB maximum request size
        public async Task<IActionResult> UploadDocument(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest(new { success = false, error = "No file provided" });

            // Server-side validation
            var allowedExtensions = new[] { ".pdf", ".jpg", ".jpeg", ".png" };
            var allowedContentTypes = new[] { "application/pdf", "image/jpeg", "image/png", "image/jpg" };
            var maxFileBytes = 5_000_000; // 5 MB

            var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!allowedExtensions.Contains(ext))
            {
                return BadRequest(new { success = false, error = "Tipo de archivo no permitido." });
            }

            if (!allowedContentTypes.Contains(file.ContentType.ToLowerInvariant()))
            {
                return BadRequest(new { success = false, error = "Tipo de contenido no permitido." });
            }

            if (file.Length > maxFileBytes)
            {
                return BadRequest(new { success = false, error = "El archivo excede el tamaño máximo permitido (5 MB)." });
            }

            var uploadsRoot = Path.Combine(_env.WebRootPath ?? "wwwroot", "uploads");
            if (!Directory.Exists(uploadsRoot))
                Directory.CreateDirectory(uploadsRoot);

            var fileName = $"{Guid.NewGuid()}{ext}";
            var filePath = Path.Combine(uploadsRoot, fileName);

            await using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            var relativePath = $"/uploads/{fileName}";

            return Json(new { success = true, url = relativePath, name = file.FileName });
        }
    }
}
