package ca.bc.gov.open.jag.tco.oracledataapi.service;

import java.util.List;

import ca.bc.gov.open.jag.tco.oracledataapi.model.Agency;
import ca.bc.gov.open.jag.tco.oracledataapi.model.Country;
import ca.bc.gov.open.jag.tco.oracledataapi.model.Language;
import ca.bc.gov.open.jag.tco.oracledataapi.model.Province;
import ca.bc.gov.open.jag.tco.oracledataapi.model.Statute;
import ca.bc.gov.open.jag.tco.oracledataapi.ords.occam.api.handler.ApiException;

public interface LookupService {

	public void refresh();

	public List<Statute> getStatutes() throws ApiException;

	public List<Language> getLanguages() throws ApiException;
	
	public List<Agency> getAgencies() throws ApiException;
	
	public List<Province> getProvinces() throws ApiException;

	public List<Country> getCountries() throws ApiException;

}
