/**
 * VTC Staff API
 * Violation Ticket Centre Staff API
 *
 * The version of the OpenAPI document: v1
 * 
 *
 * NOTE: This class is auto generated by OpenAPI Generator (https://openapi-generator.tech).
 * https://openapi-generator.tech
 * Do not edit the class manually.
 */


export interface ViolationTicketCount { 
    createdBy?: string | null;
    createdTs?: string;
    modifiedBy?: string | null;
    modifiedTs?: string;
    /**
     * ID
     */
    id?: string;
    count?: number;
    description?: string | null;
    actRegulation?: string | null;
    fullSection?: string | null;
    section?: string | null;
    subsection?: string | null;
    paragraph?: string | null;
    subparagraph?: string | null;
    ticketedAmount?: number | null;
    isAct?: boolean | null;
    isRegulation?: boolean | null;
    additionalProperties?: { [key: string]: any; } | null;
}

