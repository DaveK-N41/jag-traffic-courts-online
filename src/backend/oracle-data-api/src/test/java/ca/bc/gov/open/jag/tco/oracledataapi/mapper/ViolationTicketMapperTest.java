package ca.bc.gov.open.jag.tco.oracledataapi.mapper;

import static org.junit.jupiter.api.Assertions.assertEquals;

import org.junit.jupiter.api.BeforeEach;
import org.junit.jupiter.api.Test;
import org.junit.jupiter.api.TestInstance;
import ca.bc.gov.open.jag.tco.oracledataapi.model.ContactType;
import ca.bc.gov.open.jag.tco.oracledataapi.model.Dispute;
import ca.bc.gov.open.jag.tco.oracledataapi.model.DisputeStatus;
import ca.bc.gov.open.jag.tco.oracledataapi.model.DisputeStatusType;
import ca.bc.gov.open.jag.tco.oracledataapi.model.ViolationTicket;

@TestInstance(TestInstance.Lifecycle.PER_CLASS)
public class ViolationTicketMapperTest {

	private ViolationTicketMapperImpl violationTicketMapperImpl;

	@BeforeEach
	void setUp() {
        violationTicketMapperImpl = new ViolationTicketMapperImpl();
	}

	@Test
	void testMapToViolationTicket_shouldReturnSplitOutLawyerAddress() {
		Dispute testDispute = new Dispute();
		ViolationTicket testViolationTicket = new ViolationTicket();
		DisputeStatusType statusType = new DisputeStatusType();
		testDispute.setDisputeStatusType(statusType);
		testDispute.setStatus(DisputeStatus.NEW);
		testDispute.setViolationTicket(testViolationTicket);
		testDispute.setContactTypeCd(ContactType.INDIVIDUAL);
		// Lawyer Address string having 300 characters (max allowed length for lawyerAddress field)
		testDispute.setLawyerAddress("Lorem ipsum dolor sit amet, consectetur adipiscing elit. Fusce accumsan nulla quam, non aliquam erat porttitor eu. "
				+ "Vivamus ornare ante nec eros luctus, non tincidunt ipsum interdum. Aliquam felis felis, ullamcorper non rutrum sit amet, sollicitudin sit amet lectus. "
				+ "Quisque non massa dolor porttitor.");

		ca.bc.gov.open.jag.tco.oracledataapi.ords.occam.api.model.ViolationTicket result = violationTicketMapperImpl.convertDisputeToViolationTicketDto(testDispute);

		// Should return split out lawyer address string into 3 different fields each having 100 characters
		assertEquals("Lorem ipsum dolor sit amet, consectetur adipiscing elit. Fusce accumsan nulla quam, non aliquam erat", result.getDispute().getLawFirmAddrLine1Txt());
		assertEquals(" porttitor eu. Vivamus ornare ante nec eros luctus, non tincidunt ipsum interdum. Aliquam felis feli", result.getDispute().getLawFirmAddrLine2Txt());
		assertEquals("s, ullamcorper non rutrum sit amet, sollicitudin sit amet lectus. Quisque non massa dolor porttitor.", result.getDispute().getLawFirmAddrLine3Txt());
	}
}
