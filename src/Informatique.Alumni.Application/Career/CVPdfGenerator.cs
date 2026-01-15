using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using PuppeteerSharp;
using Volo.Abp.DependencyInjection;

using PuppeteerSharp.Media;

namespace Informatique.Alumni.Career;

public class CVPdfGenerator : ICVPdfGenerator, ITransientDependency
{
    public CVPdfGenerator() { }

    public async Task<byte[]> GeneratePdfAsync(CurriculumVitaeDto cv)
    {
        // Setup Puppeteer
        var browserFetcher = new BrowserFetcher();
        await browserFetcher.DownloadAsync();
        
        await using var browser = await Puppeteer.LaunchAsync(new LaunchOptions
        {
            Headless = true,
            Args = new[] { "--no-sandbox" }
        });
        
        await using var page = await browser.NewPageAsync();
        
        var html = BuildHtml(cv);
        await page.SetContentAsync(html);
        
        var pdfData = await page.PdfDataAsync(new PdfOptions
        {
            Format = PaperFormat.A4,
            PrintBackground = true
        });
        
        return pdfData;
    }

    private string BuildHtml(CurriculumVitaeDto cv)
    {
        var sb = new StringBuilder();
        sb.Append("<html><head><style>body { font-family: Arial, sans-serif; } .section { margin-top: 20px; } .title { font-weight: bold; border-bottom: 1px solid #ccc; }</style></head><body>");
        sb.Append($"<h1>Curriculum Vitae</h1>");
        sb.Append($"<div class='section'><div class='title'>Summary</div><p>{cv.Summary ?? "No summary provided"}</p></div>");

        // Education
        if (cv.Educations.Count > 0)
        {
            sb.Append("<div class='section'><div class='title'>Education</div><ul>");
            foreach (var edu in cv.Educations)
            {
                sb.Append($"<li>{edu.Degree} at {edu.Institution} ({edu.StartDate:yyyy} - {edu.EndDate?.ToString("yyyy") ?? "Present"})</li>");
            }
            sb.Append("</ul></div>");
        }

        // Skills
        if (cv.Skills.Count > 0)
        {
            sb.Append("<div class='section'><div class='title'>Skills</div><p>");
            sb.Append(string.Join(", ", cv.Skills.ConvertAll(x => $"{x.Name} ({x.ProficiencyLevel ?? "N/A"})")));
            sb.Append("</p></div>");
        }

        sb.Append("</body></html>");
        return sb.ToString();
    }
}
