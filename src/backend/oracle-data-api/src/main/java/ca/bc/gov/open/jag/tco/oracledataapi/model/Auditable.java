package ca.bc.gov.open.jag.tco.oracledataapi.model;

import java.util.Date;

import org.springframework.data.annotation.CreatedBy;
import org.springframework.data.annotation.CreatedDate;
import org.springframework.data.annotation.LastModifiedBy;
import org.springframework.data.annotation.LastModifiedDate;

import io.swagger.v3.oas.annotations.media.Schema;
import lombok.Getter;
import lombok.Setter;

/**
 * An abstract Auditable class that auto-populates <code>createdBy</code>, <code>createdTs</code>, <code>modifiedBy</code>, and
 * <code>modifiedTs</code> fields. Classes need only to extend this class to add auditing fields to a model object.
 */
@Getter
@Setter
public abstract class Auditable<U> {

	/** The username of the individual (or system) who created this record. */
	@CreatedBy
	private U createdBy;

	/** The timestamp this record was created. This should always be in UTC date-time (ISO 8601) format */
	@CreatedDate
	private Date createdTs;

	/** The username of the individual (or system) who modified this record. */
	@LastModifiedBy
	@Schema(nullable = true)
	private U modifiedBy;

	/** The timestamp this record was last modified. This should always be in UTC date-time (ISO 8601) format*/
	@LastModifiedDate
	@Schema(nullable = true)
	private Date modifiedTs;

}
