using Ganss.Xss;
using Volo.Abp.DependencyInjection;

namespace Informatique.Alumni.Magazine;

public class HtmlSanitizerService : ITransientDependency
{
    private readonly HtmlSanitizer _sanitizer;

    public HtmlSanitizerService()
    {
        _sanitizer = new HtmlSanitizer();
        // Add or remove allowed tags/attributes if needed
    }

    public string Sanitize(string html)
    {
        if (string.IsNullOrWhiteSpace(html)) return html;
        return _sanitizer.Sanitize(html);
    }
}
