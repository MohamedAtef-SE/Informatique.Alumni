
Act as the **abp-expert**. I need a **Full HTTP Cycle Audit** for all APIs in the `Informatique.Alumni` solution.

Please scan `src/Informatique.Alumni.Application` and `src/Informatique.Alumni.HttpApi` and cross-reference them with the **abp-expert** rules.

For EACH Application Service method, verify the "Full Cycle" compliance:

1.  **Security (Rule 4):** Does the method have the `[Authorize]` attribute with a specific permission? (Check `*Permissions.cs` to ensure the string exists).
2.  **Performance (Rule 3):**
    * Does it use `WithDetailsAsync` or `Include` for related data?
    * Are there any loops (`foreach`) performing DB calls (N+1 violations)?
3.  **Architecture (Rule 2 & 6):**
    * Are Controllers "thin" (no business logic)?
    * Are DTOs used strictly (no Entities returned)?
4.  **Testing (Rule 7):** Does a corresponding `IntegrationTest` exist in the `test/` folder for this service?

**Output Format:**
Please generate a **Markdown Table** with the following columns:
| Service Name | Method | Security Check 🔐 | Performance Check 🚀 | Testing Check 🧪 | Violation/Action Needed |
|---|---|---|---|---|---|
| JobPostAppService | GetListAsync | ✅ | ❌ (Missing WithDetails) | ❌ (Missing Test) | Refactor to use repository.WithDetailsAsync |

Start the audit now.