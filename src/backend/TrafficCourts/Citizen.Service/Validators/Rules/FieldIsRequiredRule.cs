using TrafficCourts.Citizen.Service.Models.Tickets;

namespace TrafficCourts.Citizen.Service.Validators.Rules;

/// <summary>Validates a field is not blank.</summary>
public class FieldIsRequiredRule : ValidationRule
{

    /// <summary>Validates a field is not blank.</summary>
    public FieldIsRequiredRule(OcrViolationTicket.Field field) : base(field)
    {
    }

    public override void Run()
    {
        if (Field.Value is null)
        {
            Field.ValidationErrors.Add(String.Format(ValidationMessages.FieldIsBlankError, Field.TagName));
        }
    }
}