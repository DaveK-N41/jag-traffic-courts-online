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


export interface CredentialRepresentation { 
    id?: string | null;
    type?: string | null;
    userLabel?: string | null;
    secretData?: string | null;
    credentialData?: string | null;
    priority?: number;
    createdDate?: number;
    value?: string | null;
    temporary?: boolean;
    device?: string | null;
    hashedSaltedValue?: string | null;
    salt?: string | null;
    hashIterations?: number;
    counter?: number;
    algorithm?: string | null;
    digits?: number;
    period?: number;
    config?: { [key: string]: string; } | null;
    additionalProperties?: { [key: string]: any; } | null;
}
