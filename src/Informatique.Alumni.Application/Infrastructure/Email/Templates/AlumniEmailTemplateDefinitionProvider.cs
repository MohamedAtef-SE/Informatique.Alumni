using Volo.Abp.TextTemplating;

namespace Informatique.Alumni.Infrastructure.Email.Templates;

public class AlumniEmailTemplateDefinitionProvider : TemplateDefinitionProvider
{
    public override void Define(ITemplateDefinitionContext context)
    {
        context.Add(
            new TemplateDefinition("StandardLayout")
                .WithVirtualFilePath("/Infrastructure/Email/Templates/StandardEmail.tpl", isInlineLocalized: true)
        );
    }
}
