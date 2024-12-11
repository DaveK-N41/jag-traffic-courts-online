package ca.bc.gov.open.jag.tco.oracledataapi.service.impl;

import java.util.List;

import org.slf4j.Logger;
import org.slf4j.LoggerFactory;
import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.data.redis.core.RedisTemplate;

import ca.bc.gov.open.jag.tco.oracledataapi.model.Agency;
import ca.bc.gov.open.jag.tco.oracledataapi.model.Country;
import ca.bc.gov.open.jag.tco.oracledataapi.model.Language;
import ca.bc.gov.open.jag.tco.oracledataapi.model.Province;
import ca.bc.gov.open.jag.tco.oracledataapi.model.Statute;
import ca.bc.gov.open.jag.tco.oracledataapi.ords.occam.api.handler.ApiException;
import ca.bc.gov.open.jag.tco.oracledataapi.service.LookupService;
import io.swagger.v3.core.util.Json;

public abstract class BaseLookupService implements LookupService {

	protected static Logger log = LoggerFactory.getLogger(LookupService.class);

	private static final String STATUTES = "Statutes";
	private static final String LANGUAGES = "Languages";
	private static final String AGENCIES = "Agencies";
	private static final String PROVINCES = "Provinces";
	private static final String COUNTRIES = "Countries";

	@Autowired
	private RedisTemplate<String, String> redis;

	@Override
	public void refresh() {
	    log.debug("Refreshing code tables in redis.");

	    try {
	        log.debug("  refreshing Statutes...");
	        redis.opsForValue().set(STATUTES, Json.pretty(getStatutes()));
	    } catch (Exception e) {
	        log.error("Could not update Statutes in redis", e);
	    }

	    try {
	        log.debug("  refreshing Languages...");
	        redis.opsForValue().set(LANGUAGES, Json.pretty(getLanguages()));
	    } catch (Exception e) {
	        log.error("Could not update Languages in redis", e);
	    }

	    try {
	        log.debug("  refreshing Agencies...");
	        redis.opsForValue().set(AGENCIES, Json.pretty(getAgencies()));
	    } catch (Exception e) {
	        log.error("Could not update Agencies in redis", e);
	    }

	    try {
	        log.debug("  refreshing Provinces...");
	        redis.opsForValue().set(PROVINCES, Json.pretty(getProvinces()));
	    } catch (Exception e) {
	        log.error("Could not update Provinces in redis", e);
	    }

	    try {
	        log.debug("  refreshing Countries...");
	        redis.opsForValue().set(COUNTRIES, Json.pretty(getCountries()));
	    } catch (Exception e) {
	        log.error("Could not update Countries in redis", e);
	    }

	    log.debug("Code tables in redis refreshed.");
	}

	@Override
	public abstract List<Statute> getStatutes() throws ApiException;

	@Override
	public abstract List<Language> getLanguages() throws ApiException;
	
	@Override
	public abstract List<Agency> getAgencies() throws ApiException;
	
	@Override
	public abstract List<Province> getProvinces() throws ApiException;
	
	@Override
	public abstract List<Country> getCountries() throws ApiException;

}
