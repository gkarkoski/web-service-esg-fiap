using EsgTrainingManagement.Api.Enums;

namespace EsgTrainingManagement.Api.Models;

public class JobRole
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public Department Department { get; set; }
    public string Description { get; set; } = "";
    public ICollection<TrainingRequirement> TrainingRequirements { get; set; } = [];
}

public class Employee
{
    public Guid Id { get; set; }
    public string Name { get; set; } = "";
    public string EmployeeNumber { get; set; } = "";
    public string Email { get; set; } = "";
    public DateTime HireDate { get; set; }
    public bool IsActive { get; set; }
    public int JobRoleId { get; set; }
    public JobRole JobRole { get; set; } = null!;
}

public class Training
{
    public int Id { get; set; }
    public string Title { get; set; } = "";
    public string Description { get; set; } = "";
    public EsgPillar EsgPillar { get; set; }
    public int DurationHours { get; set; }
    public int ValidityMonths { get; set; }
}

public class TrainingRequirement
{
    public int JobRoleId { get; set; }
    public int TrainingId { get; set; }
    public bool IsMandatory { get; set; } = true;
    public int DaysAfterHire { get; set; }
    public JobRole JobRole { get; set; } = null!;
    public Training Training { get; set; } = null!;
}

public class TrainingRecord
{
    public int Id { get; set; }
    public Guid EmployeeId { get; set; }
    public int TrainingId { get; set; }
    public DateTime CompletedAt { get; set; }
    public DateTime? ExpiresAt { get; set; }
    public decimal? Score { get; set; }
    public string CertificateCode { get; set; } = "";
    public Employee Employee { get; set; } = null!;
    public Training Training { get; set; } = null!;
}