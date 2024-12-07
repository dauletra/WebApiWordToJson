using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.PixelFormats;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace WordToJsonApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DocumentController : ControllerBase
    {
        [HttpPost("convert")]
        public async Task<IActionResult> ConvertWordToJson(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest("No file uploaded.");

            try
            {
                using (var stream = new MemoryStream())
                {
                    await file.CopyToAsync(stream);
                    var jsonLines = ProcessWordDocument(stream);
                    var jsonResponse = JsonSerializer.Serialize(jsonLines, new JsonSerializerOptions { WriteIndented = true });
                    return Ok(jsonResponse);
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        private List<string> ProcessWordDocument(Stream fileStream)
        {
            var lines = new List<string>();

            using (WordprocessingDocument wordDoc = WordprocessingDocument.Open(fileStream, false))
            {
                Body body = wordDoc.MainDocumentPart.Document.Body;

                foreach (var element in body.Elements())
                {
                    if (element is Paragraph paragraph)
                    {
                        string text = GetParagraphText(paragraph, wordDoc.MainDocumentPart);
                        lines.Add(text);
                    }
                }
            }

            return lines;
        }

        private string GetParagraphText(Paragraph paragraph, MainDocumentPart mainPart)
        {
            string result = string.Empty;

            foreach (var run in paragraph.Elements<Run>())
            {
                foreach (var child in run.ChildElements)
                {
                    // Обработка текста с проверкой вертикального выравнивания
                    if (child is DocumentFormat.OpenXml.Wordprocessing.Text textElement)
                    {
                        // Определяем вертикальное выравнивание текста (надстрочный или подстрочный)
                        var verticalAlignment = run.RunProperties?.VerticalTextAlignment?.Val;

                        if (verticalAlignment != null && verticalAlignment.Value == VerticalPositionValues.Superscript)
                        {
                            result += $"<sup>{textElement.Text}</sup> ";
                        }
                        else if (verticalAlignment != null && verticalAlignment.Value == VerticalPositionValues.Subscript)
                        {
                            result += $"<sub>{textElement.Text}</sub> ";
                        }
                        else
                        {
                            result += textElement.Text + " ";
                        }
                    }
                    else if (child is Drawing drawing)
                    {
                        // Обработка изображения
                        result += "<img src=\"" + GetImageBase64(drawing, mainPart) + "\" />";
                    }
                }
            }

            return result.Trim();
        }

        private string GetImageBase64(Drawing drawing, MainDocumentPart mainPart)
        {
            // Поиск изображения, связанного с текущим Drawing элементом
            var blip = drawing.Descendants<DocumentFormat.OpenXml.Drawing.Blip>().FirstOrDefault();
            if (blip != null && mainPart.GetPartById(blip.Embed) is ImagePart imagePart)
            {
                using (var stream = imagePart.GetStream())
                {
                    using (Image<Rgba32> image = Image.Load<Rgba32>(stream))
                    {
                        using (var ms = new MemoryStream())
                        {
                            image.Save(ms, new PngEncoder());
                            return "data:image/png;base64," + Convert.ToBase64String(ms.ToArray());
                        }
                    }
                }
            }

            return string.Empty;
        }
    }
}
