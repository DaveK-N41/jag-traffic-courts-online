/**
 * Traffic Court Online Citizen Api
 * An API for creating violation ticket disputes
 *
 * The version of the OpenAPI document: v1
 * 
 *
 * NOTE: This class is auto generated by OpenAPI Generator (https://openapi-generator.tech).
 * https://openapi-generator.tech
 * Do not edit the class manually.
 */
import { DisputedCount } from './disputedCount.model';
import { LegalRepresentation } from './legalRepresentation.model';


/**
 * Represents a violation ticket notice of dispute.
 */
export interface NoticeOfDispute { 
    /**
     * The violation ticket number.
     */
    ticket_number?: string | null;
    /**
     * The provincial court hearing location named on the violation ticket.
     */
    provincial_court_hearing_location?: string | null;
    /**
     * The date and time the violation ticket was issue. Time must only be hours and minutes.
     */
    issued_date?: string | null;
    /**
     * The surname or corporate name.
     */
    surname?: string | null;
    /**
     * The given names or corporate name continued.
     */
    given_names?: string | null;
    /**
     * The disputant\'s birthdate.
     */
    birthdate?: string | null;
    /**
     * The drivers licence number. Note not all jurisdictions will use numeric drivers licence numbers.
     */
    drivers_licence_number?: string | null;
    /**
     * The province or state the drivers licence was issued by.
     */
    drivers_licence_province?: string | null;
    /**
     * The mailing address of the disputant.
     */
    address?: string | null;
    /**
     * The mailing address city of the disputant.
     */
    city?: string | null;
    /**
     * The mailing address province of the disputant.
     */
    province?: string | null;
    /**
     * The mailing address postal code or zip code of the disputant.
     */
    postal_code?: string | null;
    /**
     * The disputant\'s home phone number.
     */
    home_phone_number?: string | null;
    /**
     * The disputant\'s work phone number.
     */
    work_phone_number?: string | null;
    /**
     * The disputant\'s email address.
     */
    email_address?: string | null;
    /**
     * The count dispute details.
     */
    disputed_counts?: Array<DisputedCount> | null;
    /**
     * The disputant intends to be represented by a lawyer at the hearing.
     */
    represented_by_lawyer?: boolean;
    legal_representation?: LegalRepresentation;
    /**
     * The disputant requires spoken language interpreter. The language name is indicated in this field.
     */
    interpreter_language?: string | null;
    /**
     * The number of witnesses that the disputant intends to call.
     */
    number_of_witness?: number;
    /**
     * The reason that disputant declares for requesting a fine reduction.
     */
    fine_reduction_reason?: string | null;
    /**
     * The reason that disputant declares for requesting more time to pay the amount on the violation ticket.
     */
    time_to_pay_reason?: string | null;
    /**
     * Identifier for whether the citizen has detected any issues with the OCR ticket result or not.
     */
    disputant_detected_ocr_issues?: boolean;
    /**
     * The description of the issue with OCR ticket if the citizen has detected any.
     */
    disputant_ocr_issues_description?: string | null;
    /**
     * The unique identifier for the Violation Ticket (OCR or looked up) for this dispute.
     */
    ticket_id?: string | null;
}

