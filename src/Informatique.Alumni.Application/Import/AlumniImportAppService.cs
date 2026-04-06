using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using ClosedXML.Excel;
using Informatique.Alumni.Profiles;
using Microsoft.AspNetCore.Identity;
using Volo.Abp;
using Volo.Abp.Application.Services;
using Volo.Abp.Content;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Guids;
using Volo.Abp.Identity;
using Informatique.Alumni.Branches;
using Informatique.Alumni.Profiles;
using System.Globalization;
using Volo.Abp.Data;
using Volo.Abp.BackgroundJobs;

namespace Informatique.Alumni.Import;

public class AlumniImportAppService : ApplicationService, IAlumniImportAppService
{
    private readonly IdentityUserManager _userManager;
    private readonly IRepository<AlumniProfile, Guid> _profileRepository;
    private readonly IRepository<College, Guid> _collegeRepository;
    private readonly IRepository<Branch, Guid> _branchRepository;
    private readonly IRepository<Major, Guid> _majorRepository;
    private readonly IGuidGenerator _guidGenerator;
    private readonly IBackgroundJobManager _backgroundJobManager;

    public AlumniImportAppService(
        IdentityUserManager userManager,
        IRepository<AlumniProfile, Guid> profileRepository,
        IRepository<College, Guid> collegeRepository,
        IRepository<Branch, Guid> branchRepository,
        IRepository<Major, Guid> majorRepository,
        IGuidGenerator guidGenerator,
        IBackgroundJobManager backgroundJobManager)
    {
        _userManager = userManager;
        _profileRepository = profileRepository;
        _collegeRepository = collegeRepository;
        _branchRepository = branchRepository;
        _majorRepository = majorRepository;
        _guidGenerator = guidGenerator;
        _backgroundJobManager = backgroundJobManager;
    }

    public async Task<AlumniImportResultDto> ImportExcelAsync(IRemoteStreamContent stream)
    {
        var result = new AlumniImportResultDto();

        if (stream == null || stream.GetStream() == null)
        {
            throw new UserFriendlyException("No file was uploaded.");
        }

        using var memoryStream = new MemoryStream();
        await stream.GetStream().CopyToAsync(memoryStream);
        memoryStream.Position = 0;

        using var workbook = new XLWorkbook(memoryStream);
        // Remove this:
        // var worksheet = workbook.Worksheets.FirstOrDefault();

        // Replace with this:
        if (!workbook.TryGetWorksheet("Alumni Import Template", out var worksheet))
        {
            // Alternatively, if the sheet name might vary, you can grab it by index: 
            // var worksheet = workbook.Worksheet(2); // 1-based index in ClosedXML
            throw new UserFriendlyException("The 'Alumni Import Template' sheet was not found in the uploaded Excel file.");
        }

        if (worksheet == null)
        {
            throw new UserFriendlyException("The uploaded Excel file does not contain any sheets.");
        }

        // Use AllContents flag to only iterate rows that have actual cell values,
        // NOT rows that only have data validation (which bloats the range to 1000+ rows).
        var rows = worksheet.RowsUsed(XLCellsUsedOptions.AllContents).Skip(1); // Skip header row
        
        // Pre-fetch mappings for quick lookup
        var colleges = await _collegeRepository.GetListAsync();
        var collegeMap = colleges.Where(c => !string.IsNullOrWhiteSpace(c.Name))
                                 .GroupBy(c => c.Name.Trim().ToLowerInvariant()) // Group by safe name
                                 .ToDictionary(g => g.Key, g => g.First().Id);   // Take the first one if duplicates exist

        var branches = await _branchRepository.GetListAsync();
        var branchMap = branches.Where(b => !string.IsNullOrWhiteSpace(b.Name))
                                .GroupBy(b => b.Name.Trim().ToLowerInvariant())
                                .ToDictionary(g => g.Key, g => g.First().Id);
        
        var majors = await _majorRepository.GetListAsync();
        var majorMap = majors.Where(m => !string.IsNullOrWhiteSpace(m.Name))
                             .GroupBy(m => m.Name.Trim().ToLowerInvariant())
                             .ToDictionary(g => g.Key, g => g.First().Id);

        foreach (var row in rows)
        {
            int rowNumber = row.RowNumber();

            try
            {
                var firstName = row.Cell(1).GetString()?.Trim();
                var lastName = row.Cell(2).GetString()?.Trim();
                var email = row.Cell(3).GetString()?.Trim();
                // Mobile number may be stored as a number by Excel → scientific notation guard
                var mobileNumber = ReadExcelLongAsString(row.Cell(4));
                var graduationYearStr = row.Cell(5).GetString()?.Trim();
                var collegeName = row.Cell(6).GetString()?.Trim();
                var jobTitle = row.Cell(7).GetString()?.Trim();
                // National ID: same scientific notation guard (14-digit number stored as double)
                var nationalId = ReadExcelLongAsString(row.Cell(8));
                
                var branchName = row.Cell(9).GetString()?.Trim();
                var majorName = row.Cell(10).GetString()?.Trim();
                var genderStr = row.Cell(11).GetString()?.Trim();
                var birthDateStr = row.Cell(12).GetString()?.Trim();
                var studentId = ReadExcelLongAsString(row.Cell(13));

                // Stop processing at the first completely empty row (template has 1000 validation rows)
                if (string.IsNullOrWhiteSpace(firstName) && string.IsNullOrWhiteSpace(lastName) && string.IsNullOrWhiteSpace(email))
                {
                    break;
                }

                result.TotalRecords++;

                // Validations
                if (string.IsNullOrWhiteSpace(firstName) || string.IsNullOrWhiteSpace(lastName) || string.IsNullOrWhiteSpace(email))
                {
                    result.ErrorMessages.Add($"Row {rowNumber}: First Name, Last Name, and Email are required.");
                    result.FailureCount++;
                    continue;
                }

                if (string.IsNullOrWhiteSpace(nationalId) || nationalId.Length < 14)
                {
                    result.ErrorMessages.Add($"Row {rowNumber}: National ID is required and must be at least 14 characters.");
                    result.FailureCount++;
                    continue;
                }
                
                if (string.IsNullOrWhiteSpace(mobileNumber))
                {
                    mobileNumber = "+201000000000"; // Dummy fallback if empty
                }

                int.TryParse(graduationYearStr, out int gradYear);
                if (gradYear == 0) gradYear = DateTime.Now.Year;

                // Check for existing user by email
                var existingUser = await _userManager.FindByEmailAsync(email);
                if (existingUser != null)
                {
                    result.ErrorMessages.Add($"Row {rowNumber}: Email '{email}' already exists in the system. Skipped.");
                    result.FailureCount++;
                    continue;
                }

                // Create User
                var userId = _guidGenerator.Create();
                var userName = $"{firstName}{lastName}{new Random().Next(100, 999)}".Replace(" ", "");
                var user = new Volo.Abp.Identity.IdentityUser(userId, userName, email);
                user.Name = firstName;
                user.Surname = lastName;
                user.SetIsActive(true);
                user.SetEmailConfirmed(true);

                var generatedPassword = $"Alumni@{_guidGenerator.Create().ToString("N").Substring(0, 6)}!";
                var createResult = await _userManager.CreateAsync(user, generatedPassword);
                if (!createResult.Succeeded)
                {
                    result.ErrorMessages.Add($"Row {rowNumber}: Failed to create user. {string.Join(", ", createResult.Errors.Select(e => e.Description))}");
                    result.FailureCount++;
                    continue;
                }

                await _backgroundJobManager.EnqueueAsync(new SendImportedAlumniWelcomeEmailJobArgs 
                { 
                    Email = email, 
                    Name = firstName, 
                    Password = generatedPassword 
                });

                await _userManager.AddToRoleAsync(user, "alumni");

                // Create Profile
                var profile = new AlumniProfile(
                    _guidGenerator.Create(),
                    userId,
                    mobileNumber,
                    nationalId
                );
                
                profile.UpdateBasicInfo(mobileNumber, jobTitle, jobTitle);
                
                var primaryMobile = new ContactMobile(_guidGenerator.Create(), profile.Id, mobileNumber, true);
                profile.AddMobile(primaryMobile);
                profile.SetPrimaryMobile(primaryMobile.Id);

                var primaryEmail = new ContactEmail(_guidGenerator.Create(), profile.Id, email, true);
                profile.AddEmail(primaryEmail);
                profile.SetPrimaryEmail(primaryEmail.Id);
                
                if (!string.IsNullOrWhiteSpace(branchName) && branchMap.TryGetValue(branchName.ToLowerInvariant(), out Guid branchId))
                {
                    profile.SetBranchId(branchId);
                }

                Guid? recognizedMajorId = null;
                if (!string.IsNullOrWhiteSpace(majorName) &&
                    majorMap.TryGetValue(majorName.Trim().ToLowerInvariant(), out Guid majorId))
                {
                    recognizedMajorId = majorId;
                }

                if (!string.IsNullOrWhiteSpace(collegeName) &&
                    collegeMap.TryGetValue(collegeName.Trim().ToLowerInvariant(), out Guid collegeId))
                {
                    var education = new Education(_guidGenerator.Create(), profile.Id, "Ain Shams University", "Bachelor", gradYear);
                    education.SetAcademicDetails(1, collegeId, recognizedMajorId, null);
                    profile.AddEducation(education);
                }
                // Optional: Store extended properties securely on the Profile ExtraProperties
                if (!string.IsNullOrWhiteSpace(studentId))
                {
                    profile.SetProperty("StudentId", studentId);
                }
                if (!string.IsNullOrWhiteSpace(genderStr))
                {
                    profile.SetProperty("Gender", genderStr);
                }
                if (!string.IsNullOrWhiteSpace(birthDateStr) && DateTime.TryParse(birthDateStr, out DateTime parsedDate))
                {
                    profile.SetProperty("BirthDate", parsedDate);
                }

                await _profileRepository.InsertAsync(profile);
                result.SuccessCount++;
            }
            catch (Exception ex)
            {
                result.ErrorMessages.Add($"Row {rowNumber}: An unexpected error occurred - {ex.Message}");
                result.FailureCount++;
            }
        }

        return result;
    }

    public async Task<IRemoteStreamContent> GetImportTemplateAsync()
    {
        var branches = await _branchRepository.GetListAsync();
        var colleges = await _collegeRepository.GetListAsync();
        var majors = await _majorRepository.GetListAsync();

        using var workbook = new ClosedXML.Excel.XLWorkbook();
        
        // 1. Data Lookup Sheet (Hidden)
        var lookupSheet = workbook.Worksheets.Add("DataLookup");
        lookupSheet.Visibility = ClosedXML.Excel.XLWorksheetVisibility.Hidden;
        
        lookupSheet.Cell(1, 1).Value = "Branches";
        for (int i = 0; i < branches.Count; i++) lookupSheet.Cell(i + 2, 1).Value = branches[i].Name;
        
        lookupSheet.Cell(1, 2).Value = "Colleges";
        for (int i = 0; i < colleges.Count; i++) lookupSheet.Cell(i + 2, 2).Value = colleges[i].Name;
        
        lookupSheet.Cell(1, 3).Value = "Majors";
        for (int i = 0; i < majors.Count; i++) lookupSheet.Cell(i + 2, 3).Value = majors[i].Name;
        
        lookupSheet.Cell(1, 4).Value = "Years";
        var currentYear = DateTime.Now.Year;
        var startYear = 1960;
        var yearDiff = (currentYear + 3) - startYear;
        for (int i = 0; i <= yearDiff; i++) lookupSheet.Cell(i + 2, 4).Value = (startYear + i).ToString();
        
        // 2. Main Template Sheet
        var sheet = workbook.Worksheets.Add("Alumni Import Template");
        var headers = new[] { 
            "First Name*", "Last Name*", "Email*", "Mobile Number", "Graduation Year", 
            "College Name", "Job Title", "National ID*", "Branch Name*", "Major Name", 
            "Gender", "Birthdate", "Student ID" 
        };

        for (int i = 0; i < headers.Length; i++)
        {
            var cell = sheet.Cell(1, i + 1);
            cell.Value = headers[i];
            cell.Style.Font.Bold = true;
            cell.Style.Fill.BackgroundColor = ClosedXML.Excel.XLColor.AirForceBlue;
            cell.Style.Font.FontColor = ClosedXML.Excel.XLColor.White;
        }

        // 3. Add Data Validation
        var dataRows = 1000;
        
        if (colleges.Count > 0)
        {
            var r = sheet.Range(2, 6, dataRows + 1, 6).CreateDataValidation();
            r.List($"'DataLookup'!$B$2:$B${colleges.Count + 1}", true);
        }

        if (branches.Count > 0)
        {
            var r = sheet.Range(2, 9, dataRows + 1, 9).CreateDataValidation();
            r.List($"'DataLookup'!$A$2:$A${branches.Count + 1}", true);
        }
        
        if (majors.Count > 0)
        {
            var r = sheet.Range(2, 10, dataRows + 1, 10).CreateDataValidation();
            r.List($"'DataLookup'!$C$2:$C${majors.Count + 1}", true);
        }
        
        // Graduation Year Validation (Col E / 5)
        var yearRange = sheet.Range(2, 5, dataRows + 1, 5).CreateDataValidation();
        yearRange.List($"'DataLookup'!$D$2:$D${yearDiff + 2}", true);
        
        var genderVal = sheet.Range(2, 11, dataRows + 1, 11).CreateDataValidation();
        genderVal.List("\"Male,Female\"", true);

        // 4. Format numeric-looking text columns as Text to prevent Excel scientific notation
        // Mobile Number (col D=4), National ID (col H=8), Student ID (col M=13)
        var textFormat = "@"; // "@" is the Excel number format code for Text
        sheet.Range(2, 4, dataRows + 1, 4).Style.NumberFormat.Format = textFormat;  // Mobile
        sheet.Range(2, 8, dataRows + 1, 8).Style.NumberFormat.Format = textFormat;  // National ID
        sheet.Range(2, 13, dataRows + 1, 13).Style.NumberFormat.Format = textFormat; // Student ID

        sheet.Columns().AdjustToContents();

        var memoryStream = new MemoryStream();
        workbook.SaveAs(memoryStream);
        memoryStream.Position = 0;

        return new RemoteStreamContent(memoryStream, "AlumniImportTemplate.xlsx", "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");
    }

    /// <summary>
    /// Reads a cell that may contain a long number (e.g., 14-digit National ID or mobile) stored
    /// by Excel as a double in scientific notation. Using GetString() returns "2.88E+13"; this
    /// method correctly returns the full integer string "28811172103093".
    /// Uses decimal to avoid the precision loss that occurs when casting double → long for numbers
    /// larger than 2^53 (15+ digits).
    /// </summary>
    private static string? ReadExcelLongAsString(ClosedXML.Excel.IXLCell cell)
    {
        if (cell.IsEmpty()) return null;

        if (cell.DataType == ClosedXML.Excel.XLDataType.Number)
        {
            // Use decimal (28-digit precision) to avoid floating-point truncation
            var raw = cell.GetValue<double>();
            var asDecimal = (decimal)raw;
            return decimal.Truncate(asDecimal).ToString("F0");
        }

        return cell.GetString()?.Trim();
    }
}
