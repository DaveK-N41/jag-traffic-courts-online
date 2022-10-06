package ca.bc.gov.open.jag.tco.oracledataapi.model;

import javax.persistence.Column;
import javax.persistence.Entity;
import javax.persistence.GeneratedValue;
import javax.persistence.Id;
import javax.persistence.Table;
import io.swagger.v3.oas.annotations.media.Schema;
import lombok.Getter;
import lombok.NoArgsConstructor;
import lombok.Setter;

//mark class as an Entity
@Entity
//defining class name as Table name
@Table
@Getter
@Setter
@NoArgsConstructor
public class FileHistory extends Auditable<String> {
	
	/**
	 * Primary key
	 */
	@Schema(description = "ID", accessMode = Schema.AccessMode.READ_ONLY)
	@Id
	@GeneratedValue
	private Long fileHistoryId;
		
    /**
     * The violation ticket number.
     */
    @Column(length = 50)
    @Schema(nullable = false)
    private String ticketNumber;
    
    /**
	 * description
	 */
	@Column(length = 500)
	@Schema(nullable = true)
	private String description;    
}