package ca.bc.gov.open.jag.tco.oracledataapi.mapper;

import java.util.List;

import org.mapstruct.Mapper;
import org.mapstruct.Mapping;
import org.mapstruct.factory.Mappers;

/**
 * Maps from ORDS lookup tables to OracleDataAPI
 */
@Mapper
public interface LookupMapper {

	LookupMapper INSTANCE = Mappers.getMapper(LookupMapper.class);

	/** Map from ORDS Statute to OracleDataAPI Statute */
	@Mapping(source = "statId", target = "id")
	@Mapping(source = "actCd", target = "actCode")
	@Mapping(source = "statSectionTxt", target = "sectionText")
	@Mapping(source = "statSubSectionTxt", target = "subsectionText")
	@Mapping(source = "statParagraphTxt", target = "paragraphText")
	@Mapping(source = "statSubParagraphTxt", target = "subparagraphText")
	@Mapping(source = "statCode", target = "code")
	@Mapping(source = "statShortDescriptionTxt", target = "shortDescriptionText")
	@Mapping(source = "statDescriptionTxt", target = "descriptionText")
	ca.bc.gov.open.jag.tco.oracledataapi.model.Statute convertStatute(ca.bc.gov.open.jag.tco.oracledataapi.ords.occam.api.model.Statute statute);
	List<ca.bc.gov.open.jag.tco.oracledataapi.model.Statute> convertStatutes(List<ca.bc.gov.open.jag.tco.oracledataapi.ords.occam.api.model.Statute> statuteList);

	/** Map from ORDS Language to OracleDataAPI Language */
	@Mapping(source = "cdlnLanguageCd", target = "code")
	@Mapping(source = "cdlnLanguageDsc", target = "description")
	ca.bc.gov.open.jag.tco.oracledataapi.model.Language convertLanguage(ca.bc.gov.open.jag.tco.oracledataapi.ords.occam.api.model.Language language);
	List<ca.bc.gov.open.jag.tco.oracledataapi.model.Language> convertLanguages(List<ca.bc.gov.open.jag.tco.oracledataapi.ords.occam.api.model.Language> languages);
	
	/** Map from ORDS Agency to OracleDataAPI Agency */
	@Mapping(source = "agenId", target = "id")
	@Mapping(source = "agenAgencyNm", target = "name")
	@Mapping(source = "cdatAgencyTypeCd", target = "typeCode")
	ca.bc.gov.open.jag.tco.oracledataapi.model.Agency convertAgency(ca.bc.gov.open.jag.tco.oracledataapi.ords.occam.api.model.Agency agency);
	List<ca.bc.gov.open.jag.tco.oracledataapi.model.Agency> convertAgencies(List<ca.bc.gov.open.jag.tco.oracledataapi.ords.occam.api.model.Agency> cthList);
	
	/** Map from ORDS Province to OracleDataAPI Province */
	@Mapping(source = "ctryId", target = "ctryId")
	@Mapping(source = "provSeqNo", target = "provSeqNo")
	@Mapping(source = "provNm", target = "provNm")
	@Mapping(source = "provAbbreviationCd", target = "provAbbreviationCd")
	List<ca.bc.gov.open.jag.tco.oracledataapi.model.Province> convertProvinces(List<ca.bc.gov.open.jag.tco.oracledataapi.ords.occam.api.model.Province> provinces);
	
	/** Map from ORDS Country to OracleDataAPI Country */
	@Mapping(source = "ctryId", target = "ctryId")
	@Mapping(source = "ctryLongNm", target = "ctryLongNm")
	List<ca.bc.gov.open.jag.tco.oracledataapi.model.Country> convertCountries(List<ca.bc.gov.open.jag.tco.oracledataapi.ords.occam.api.model.Country> countries);

}
