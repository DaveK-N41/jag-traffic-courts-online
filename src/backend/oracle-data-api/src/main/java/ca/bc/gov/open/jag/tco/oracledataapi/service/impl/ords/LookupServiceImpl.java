package ca.bc.gov.open.jag.tco.oracledataapi.service.impl.ords;

import java.util.List;

import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.boot.autoconfigure.condition.ConditionalOnProperty;
import org.springframework.stereotype.Service;

import ca.bc.gov.open.jag.tco.oracledataapi.mapper.LookupMapper;
import ca.bc.gov.open.jag.tco.oracledataapi.model.Agency;
import ca.bc.gov.open.jag.tco.oracledataapi.model.Country;
import ca.bc.gov.open.jag.tco.oracledataapi.model.Language;
import ca.bc.gov.open.jag.tco.oracledataapi.model.Province;
import ca.bc.gov.open.jag.tco.oracledataapi.model.Statute;
import ca.bc.gov.open.jag.tco.oracledataapi.ords.occam.api.LookupValuesApi;
import ca.bc.gov.open.jag.tco.oracledataapi.ords.occam.api.handler.ApiException;
import ca.bc.gov.open.jag.tco.oracledataapi.service.impl.BaseLookupService;

@Service
@ConditionalOnProperty(name = "repository.lookup", havingValue = "ords", matchIfMissing = true)
public class LookupServiceImpl extends BaseLookupService {

	@Autowired
	private LookupValuesApi lookupValuesApi;

	@Override
	public List<Statute> getStatutes() throws ApiException {
		// Get all statutes from the ORDS webclient service and convert them to OracleDataApi using Mapstruct
		List<ca.bc.gov.open.jag.tco.oracledataapi.ords.occam.api.model.Statute> statutes = lookupValuesApi.statutesList().getStatuteCodeValues();
		return LookupMapper.INSTANCE.convertStatutes(statutes);
	}

	@Override
	public List<Language> getLanguages() throws ApiException {
		List<ca.bc.gov.open.jag.tco.oracledataapi.ords.occam.api.model.Language> languages = lookupValuesApi.languagesList().getLanguageCodeValues();
		return LookupMapper.INSTANCE.convertLanguages(languages);
	}
	
	@Override
	public List<Agency> getAgencies() throws ApiException {
		List<ca.bc.gov.open.jag.tco.oracledataapi.ords.occam.api.model.Agency> cthAgencies = lookupValuesApi.agenciesList().getAgencyCodeValues();
		return LookupMapper.INSTANCE.convertAgencies(cthAgencies);
	}
	
	@Override
	public List<Province> getProvinces() throws ApiException {
		List <ca.bc.gov.open.jag.tco.oracledataapi.ords.occam.api.model.Province> provinces = lookupValuesApi.provincesList().getProvinceCodeValues();
		return LookupMapper.INSTANCE.convertProvinces(provinces);
	}
	
	@Override
	public List<Country> getCountries() throws ApiException {
		List <ca.bc.gov.open.jag.tco.oracledataapi.ords.occam.api.model.Country> countries = lookupValuesApi.countriesList().getCountryCodeValues();
		return LookupMapper.INSTANCE.convertCountries(countries);
	}
}
