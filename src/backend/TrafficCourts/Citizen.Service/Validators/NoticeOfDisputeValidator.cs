﻿using FluentValidation;
using FluentValidation.Validators;
using TrafficCourts.Citizen.Service.Models.Disputes;
using TrafficCourts.Domain.Models;

namespace TrafficCourts.Citizen.Service.Validators
{
    /// <summary>
    /// Fluent Validator for Notice Of Dispute Model
    /// </summary>
    public class NoticeOfDisputeValidator : AbstractValidator<NoticeOfDispute>
    {
        private const string PhoneNumberRegex = @"\(?\d{3}\)?-? *\d{3}-? *-?\d{4}";

        public NoticeOfDisputeValidator()
        {
            RuleFor(_ => _.TicketNumber).NotEmpty().MaximumLength(12);
            RuleFor(_ => _.IssuedTs).NotEmpty();
            RuleFor(_ => _.DisputantSurname).NotEmpty();
            RuleFor(_ => _.DisputantGivenName1).NotEmpty();
            RuleFor(_ => _.DisputantBirthdate).NotEmpty();
            RuleFor(_ => _.DriversLicenceNumber).MaximumLength(20);
            RuleFor(_ => _.DriversLicenceProvince).MaximumLength(30);
            RuleFor(_ => _.AddressLine1).NotEmpty();
            RuleFor(_ => _.AddressCity).NotEmpty();
            RuleFor(_ => _.AddressProvince).MaximumLength(30);
            RuleFor(_ => _.PostalCode).MaximumLength(6);
            RuleFor(_ => _.HomePhoneNumber).Matches(PhoneNumberRegex);
            RuleFor(_ => _.WorkPhoneNumber).Matches(PhoneNumberRegex);
            RuleFor(_ => _.EmailAddress).EmailAddress();
#pragma warning disable CS0618 // Type or member is obsolete
            RuleFor(_ => _.EmailAddress).EmailAddress(EmailValidationMode.Net4xRegex); // TODO: change validator
#pragma warning restore CS0618 // Type or member is obsolete
            
            RuleFor(_ => _.TicketId)
                .NotEmpty();

            RuleFor(_ => _.WitnessNo).InclusiveBetween(0,99);
            RuleFor(_ => _.RequestCourtAppearanceYn)
                .NotEmpty();

            RuleFor(_ => _.DisputantOcrIssues)
                .NotEmpty()
                .When(DisputantDetectedOcrIssues)
                .WithMessage("'Disputant Ocr Issues Description' is required since the disputant detected ocr issues");
            
            RuleFor(_ => _.ContactTypeCd).NotEmpty();
            
            RuleFor(_ => _.ContactLawFirmNm)
                .NotEmpty()
                .When(ContactTypeIsLawyer)
                .WithMessage("'Contact Law Firm Name' is required since contact type is Lawyer.");
            
            RuleFor(_ => _.ContactGiven1Nm)
                .NotEmpty()
                .When(ContactTypeIsLawyerOrOther)
                .WithMessage("'Contact Given Name' is required since contact type is not 'Individual on Ticket'");
            
            RuleFor(_ => _.ContactSurnameNm).NotEmpty()
                .When(ContactTypeIsLawyerOrOther)
                .WithMessage("'Contact Surame' is required since contact type is not 'Individual on Ticket'");

            // Validation rules for Legal Representation
           RuleFor(_ => _.LawFirmName)
                .NotNull()
                .When(WillBeRepresentedByLawyer)
                .WithMessage("'Law Firm Name' is required since disputant selected to be represented by lawyer");
            
            RuleFor(_ => _.LawyerSurname)
                .NotEmpty()
                .When(WillBeRepresentedByLawyer)
                .WithMessage("'Lawyer Surname' is required since disputant selected to be represented by lawyer");
            
            RuleFor(_ => _.LawyerGivenName1)
                .NotEmpty()
                .When(WillBeRepresentedByLawyer)
                .WithMessage("'Lawyer Given Name' is required since disputant selected to be represented by lawyer");
            
            RuleFor(_ => _.LawyerEmail)
                .NotEmpty()
                .EmailAddress()
                .When(WillBeRepresentedByLawyer)
                .WithMessage("'Lawyer Email' is required and must be a proper email address since disputant selected to be represented by lawyer");
            
            RuleFor(_ => _.LawyerPhoneNumber)
                .NotEmpty()
                .Matches(PhoneNumberRegex)
                .When(WillBeRepresentedByLawyer)
                .WithMessage("'Lawyer phone number' is required and must be a proper phone number since disputant selected to be represented by lawyer");
            
            RuleFor(_ => _.LawyerAddress).NotEmpty()
                .When(WillBeRepresentedByLawyer)
                .WithMessage("'Lawyer Address' is required since disputant selected to be represented by lawyer");

            // Validation rules for Disputed Counts
            RuleFor(_ => _.DisputeCounts)
                .Cascade(CascadeMode.Stop)
                .NotNull()
                .NotEmpty()
                .Must(_ => _!.Count > 0 && _.Count < 4)
                .WithMessage("At least 1 and maximum 3 Disputed Counts required");

            // Legal Representation Validators
            RuleForEach(_ => _.DisputeCounts).SetValidator(new DisputeCountValidator());

            RuleFor(_ => _.TimeToPayReason)
                .NotEmpty()
                .When(HasRequestedTimeToPay)
                .WithMessage("'Time To Pay Reason' cannot be null since 'Request Time To Pay' is selected for at least one of the counts");

            RuleFor(_ => _.FineReductionReason)
                .NotEmpty()
                .When(HasRequestedReduction)
                .WithMessage("'Fine Reduction Reason' cannot be null since 'Request Reduction' is selected for at least one of the counts");
        }

        private bool ContactTypeIsLawyer(NoticeOfDispute value)
        {
            return value.ContactTypeCd == DisputeContactTypeCd.LAWYER;
        }

        private bool ContactTypeIsLawyerOrOther(NoticeOfDispute value)
        {
            return value.ContactTypeCd == DisputeContactTypeCd.LAWYER || value.ContactTypeCd == DisputeContactTypeCd.OTHER;
        }

        private bool DisputantDetectedOcrIssues(NoticeOfDispute value)
        {
            return value.DisputantDetectedOcrIssues == DisputeDisputantDetectedOcrIssues.Y;
        }

        private bool WillBeRepresentedByLawyer(NoticeOfDispute value)
        {
            return value.RepresentedByLawyer == DisputeRepresentedByLawyer.Y;
        }

        private bool HasRequestedReduction(NoticeOfDispute value)
        {
            return value.DisputeCounts != null && value.DisputeCounts.Any(count => count.RequestReduction == DisputeCountRequestReduction.Y);
        }

        private bool HasRequestedTimeToPay(NoticeOfDispute value)
        {
            return value.DisputeCounts != null && value.DisputeCounts.Any(count => count.RequestTimeToPay == DisputeCountRequestTimeToPay.Y);
        }
    }
}
