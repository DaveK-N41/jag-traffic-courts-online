﻿using AutoMapper;
using TrafficCourts.Citizen.Service.Models.Disputes;
using TrafficCourts.Messaging.MessageContracts;

namespace TrafficCourts.Citizen.Service.Mappings;

public class NoticeOfDisputeToMessageContractMappingProfile : Profile
{
    public NoticeOfDisputeToMessageContractMappingProfile()
    {
        CreateMap<NoticeOfDispute, SubmitNoticeOfDispute>()
            .ForMember(dest => dest.DisputeCounts, opt => opt.MapFrom(src => src.DisputeCounts));
        CreateMap<Models.Disputes.DisputeCount, Messaging.MessageContracts.DisputeCount>();

        CreateMap<SubmitNoticeOfDispute, NoticeOfDispute>()
            .ForMember(dest => dest.ViolationTicket, opt => opt.MapFrom(src => src.ViolationTicket))
            .ForMember(dest => dest.AppearanceLessThan14Days, opt => opt.MapFrom(src => src.AppearanceLessThan14DaysYn == Domain.Models.DisputeAppearanceLessThan14DaysYn.Y ? true : false));

        CreateMap<Messaging.MessageContracts.DisputeCount, Models.Disputes.DisputeCount>();
        CreateMap<Messaging.MessageContracts.ViolationTicket, Models.Tickets.ViolationTicket>()
            .ForMember(dest => dest.Counts, opt => opt.MapFrom(src => src.ViolationTicketCounts));
        CreateMap<Messaging.MessageContracts.TicketCount, Models.Tickets.ViolationTicketCount>()
            .ForMember(dest => dest.Section, opt => opt.MapFrom(src => src.Section));

        CreateMap<Models.Tickets.ViolationTicket, Messaging.MessageContracts.ViolationTicket>()
            .ForMember(dest => dest.ViolationTicketCounts, opt => opt.MapFrom(src => src.Counts));
        CreateMap<Models.Tickets.ViolationTicketCount, Messaging.MessageContracts.TicketCount>()
            .ForMember(dest => dest.Section, opt => opt.MapFrom(src => src.Section));

        CreateMap<DisputantContactInformation, Messaging.MessageContracts.DisputeUpdateContactRequest>()
            .ForMember(dest => dest.ContactType, opt => opt.MapFrom(src => src.ContactTypeCd));
        CreateMap<Messaging.MessageContracts.DisputeUpdateContactRequest, Messaging.MessageContracts.DisputeUpdateRequest>();

        CreateMap<Dispute, Messaging.MessageContracts.DisputeUpdateRequest>()
            .ForMember(dest => dest.RepresentedByLawyer, opt => opt.MapFrom(src => src.RepresentedByLawyer == Domain.Models.DisputeRepresentedByLawyer.Y ? true : false))
            .ForMember(dest => dest.InterpreterRequired, opt => opt.MapFrom(src => src.InterpreterRequired == Domain.Models.DisputeInterpreterRequired.Y ? true : false))
            .ForMember(dest => dest.DriversLicenceNumber, opt => opt.MapFrom(src => src.DriversLicenceNumber))
            .ForMember(dest => dest.DriversLicenceIssuedCountryId, opt => opt.MapFrom(src => src.DriversLicenceCountryId))
            .ForMember(dest => dest.DriversLicenceIssuedProvinceSeqNo, opt => opt.MapFrom(src => src.DriversLicenceProvinceSeqNo))
            .ForMember(dest => dest.DriversLicenceProvince, opt => opt.MapFrom(src => src.DriversLicenceProvince))
            .ForMember(dest => dest.RequestCourtAppearance, opt => opt.MapFrom(src => src.RequestCourtAppearanceYn))
            .ForMember(dest => dest.DisputeCounts, opt => opt.MapFrom(src => src.DisputeCounts))
            .ForMember(dest => dest.ContactType, opt => opt.MapFrom(src => src.ContactTypeCd));

        CreateMap<Models.Disputes.DisputeCount, Domain.Models.DisputeCount>();
    }
}
