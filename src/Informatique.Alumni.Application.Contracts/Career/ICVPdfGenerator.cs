using System;
using System.Threading.Tasks;

namespace Informatique.Alumni.Career;

public interface ICVPdfGenerator
{
    Task<byte[]> GeneratePdfAsync(CurriculumVitaeDto cv);
}
