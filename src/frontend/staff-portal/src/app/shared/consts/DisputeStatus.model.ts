export type DisputeStatus = 'NEW' | 'PROG' | 'UPD' | 'CONF' | 'REQH' | 'REQM' | 'ACCP' | 'REV' | 'CNLD' | 'CANC' | 'HEAR';

export const DisputeStatus = {    
    New: 'NEW' as DisputeStatus,
    InProgress: 'PROG' as DisputeStatus,
    DataUpdate: 'UPD' as DisputeStatus,
    Confirmed: 'CONF' as DisputeStatus,
    RequireCourtHearing: 'REQH' as DisputeStatus,
    RequireMoreInfo: 'REQM' as DisputeStatus,
    Accepted: 'ACCP' as DisputeStatus,
    Review: 'REV' as DisputeStatus,
    Concluded: 'CNLD' as DisputeStatus,
    Cancelled: 'CANC' as DisputeStatus,
    HearingScheduled: 'HEAR' as DisputeStatus
};