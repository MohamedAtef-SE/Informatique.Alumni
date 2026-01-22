using System;

namespace Informatique.Alumni.EmploymentFair;

public enum JobType
{
    FullTime = 0,
    PartTime = 1,
    Remote = 2,
    Freelance = 3,
    Internship = 4,
    Contract = 5
}

public enum JobInterests
{
    SoftwareDevelopment = 0,
    DataAnalysis = 1,
    ProjectManagement = 2,
    Marketing = 3,
    Sales = 4,
    HumanResources = 5,
    Accounting = 6,
    Design = 7,
    Consulting = 8,
    Other = 99
}

public enum MilitaryStatus
{
    Exempted = 0,
    Completed = 1,
    Postponed = 2,
    NotApplicable = 3
}

public enum MaritalStatus
{
    Single = 0,
    Married = 1,
    Divorced = 2,
    Widowed = 3
}
