//package ca.bc.gov.open.jag.tco.oracledataapi.repository.impl.h2;
//
//import org.springframework.beans.factory.annotation.Qualifier;
//import org.springframework.boot.autoconfigure.condition.ConditionalOnProperty;
//import org.springframework.data.jpa.repository.JpaRepository;
//
//import ca.bc.gov.open.jag.tco.oracledataapi.model.JJDisputeRemark;
//import ca.bc.gov.open.jag.tco.oracledataapi.repository.JJDisputeRemarkRepository;
//
//@ConditionalOnProperty(name = "repository.jjdispute", havingValue = "h2", matchIfMissing = false)
//@Qualifier("jjDisputeRemarkRepository")
//public interface JJDisputeRemarkRepositoryImpl extends JJDisputeRemarkRepository, JpaRepository<JJDisputeRemark, Long> {
//
//}
