package ca.bc.gov.open.jag.tco.oracledataapi.model;

import java.util.Date;

import javax.persistence.Column;
import javax.persistence.Entity;
import javax.persistence.EnumType;
import javax.persistence.Enumerated;
import javax.persistence.FetchType;
import javax.persistence.Id;
import javax.persistence.ManyToOne;
import javax.persistence.Table;
import javax.persistence.Temporal;
import javax.persistence.TemporalType;
import javax.validation.constraints.Max;
import javax.validation.constraints.Min;
import javax.validation.constraints.Size;

import com.fasterxml.jackson.annotation.JsonBackReference;

import io.swagger.v3.oas.annotations.media.Schema;
import lombok.Getter;
import lombok.NoArgsConstructor;
import lombok.Setter;

//mark class as an Entity
/**
 * @author 237563
 * 
 * Represents JJ Written Reasons for each count
 *
 */
@Entity
//defining class name as Table name
@Table
@Getter
@Setter
@NoArgsConstructor
public class JJDisputedCount extends Auditable<String> {

	@Schema(description = "ID", accessMode = Schema.AccessMode.READ_ONLY)
	@Id
    private Long id;

	/**
	 * Represents the disputant plea on count.
	 */
	@Column
	private Plea plea;

	/**
	 * The count number.
	 */
	@Column
	@Min(1) @Max(3)
	private int count;

	/**
	 * The disputant is requesting time to pay the ticketed amount.
	 */
	@Column
	@Schema(nullable = false)
	@Enumerated(EnumType.STRING)
	private YesNo requestTimeToPay;

	/**
	 * The disputant is requesting a reduction of the ticketed amount.
	 */
	@Column
	@Schema(nullable = false)
	@Enumerated(EnumType.STRING)
	private YesNo requestReduction;

	/**
	 * Does the want to appear in court?
	 */
	@Column
	@Schema(nullable = false)
	@Enumerated(EnumType.STRING)
	private YesNo appearInCourt;
	
	/**
	 * The description of the offence including the statute and act. 
	 * Example: 92.1(1) MVA - Fail to stop resulting in pursuit
	 */
	@Column
	@Schema(nullable = true)
    private String description;
	
	/**
     * The due date for the offence to be paid.
     */
    @Column
    @Schema(nullable = true)
    @Temporal(TemporalType.TIMESTAMP)
    private Date dueDate;
    
    /**
	 * The original fine amount from reconciled ticket data.
	 */
	@Column
	@Schema(nullable = true)
    private Float ticketedFineAmount;
	
	/**
	 * The amount that JJ may enter to overwrite the ticketed fine amount.
	 */
	@Column
	@Schema(nullable = true)
	private Float LesserOrGreaterAmount;
	
	/**
	 * JJ's decision whether to include surcharge in the calculated fine or not.
	 * Surcharge is always 15% of the original fine amount.
	 */
	@Column
	@Schema(nullable = false)
	@Enumerated(EnumType.STRING)
	private YesNo includesSurcharge;
	
	/**
     * Revised due date of the original due date for the offence to be paid.
     */
    @Column
    @Schema(nullable = true)
    @Temporal(TemporalType.TIMESTAMP)
    private Date revisedDueDate;
    
    /**
	 * The final fine amount to be paid by the disputant.
	 */
	@Column
	@Schema(nullable = true)
    private Float totalFineAmount;
	
	/**
	 * JJ's comments that will be shared to disputant.
	 */
	@Size(max = 500)
	@Column(length = 500)
	@Schema(nullable = true, maxLength = 500)
	private String comments;

	@JsonBackReference
	@ManyToOne(targetEntity=JJDispute.class, fetch = FetchType.LAZY)
	@Schema(hidden = true)
	private JJDispute jjDispute;

}
