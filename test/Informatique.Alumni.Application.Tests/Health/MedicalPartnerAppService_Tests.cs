using System;
using System.Linq;
using System.Threading.Tasks;
using Shouldly;
using Volo.Abp.Application.Dtos;
using Xunit;

namespace Informatique.Alumni.Health;

public class MedicalPartnerAppService_Tests : AlumniApplicationTestBase<AlumniApplicationTestModule>
{
    private readonly IMedicalPartnerAppService _medicalPartnerAppService;

    public MedicalPartnerAppService_Tests()
    {
        _medicalPartnerAppService = GetRequiredService<IMedicalPartnerAppService>();
    }

    [Fact]
    public async Task Should_Get_List_Of_Medical_Partners()
    {
        // Act
        var result = await _medicalPartnerAppService.GetListAsync(new GetMedicalPartnersInput());

        // Assert
        result.Count.ShouldBeGreaterThan(0);
    }

    [Fact]
    public async Task Should_Filter_By_Text_In_Category()
    {
        // Arrange
        // Assuming seeded data has "El Ezaby Pharmacy" with Category "Pharmacy"
        var input = new GetMedicalPartnersInput { FilterText = "Pharmacy" };

        // Act
        var result = await _medicalPartnerAppService.GetListAsync(input);

        // Assert
        result.ShouldContain(x => x.Name.Contains("El Ezaby"));
        result.ShouldContain(x => x.Category == "Pharmacy");
    }

    [Fact]
    public async Task Should_Filter_By_City()
    {
        // Arrange
        var input = new GetMedicalPartnersInput { City = "Cairo" };

        // Act
        var result = await _medicalPartnerAppService.GetListAsync(input);

        // Assert
        result.ShouldAllBe(x => x.City == "Cairo");
    }
}
