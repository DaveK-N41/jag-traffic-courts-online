package ca.bc.gov.open.jag.tco.keycloakuserinitializer.service;

import java.util.List;

import org.keycloak.admin.client.Keycloak;
import org.keycloak.admin.client.resource.UsersResource;
import org.keycloak.representations.idm.UserRepresentation;
import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.stereotype.Service;

import ca.bc.gov.open.jag.tco.keycloakuserinitializer.config.KeycloakConfig;
import ca.bc.gov.open.jag.tco.keycloakuserinitializer.idir.api.UsersApi;
import ca.bc.gov.open.jag.tco.keycloakuserinitializer.idir.api.model.Environment;
import ca.bc.gov.open.jag.tco.keycloakuserinitializer.idir.api.model.EnvironmentIdirUsersGet200Response;
import ca.bc.gov.open.jag.tco.keycloakuserinitializer.idir.api.model.EnvironmentIdirUsersGet200ResponseDataInner;
import ca.bc.gov.open.jag.tco.keycloakuserinitializer.model.TcoUser;
import ca.bc.gov.open.jag.tco.keycloakuserinitializer.util.TcoUserDataLoader;

@Service
public class KeycloakService {
	
	private final Keycloak keycloak;
	
	// Delegate, OpenAPI generated client
	private final UsersApi usersApi;
	
	@Autowired
	private TcoUserDataLoader userLoader;
	
	public KeycloakService (Keycloak keycloak, UsersApi usersApi) {
		this.keycloak = keycloak;
		this.usersApi = usersApi;
	}
	
	public UserRepresentation getUser(String userName) {
        UsersResource usersResource = getInstance();
        List<UserRepresentation> userResult = usersResource.search(userName, true);

        if (userResult != null && !userResult.isEmpty()) {
			return userResult.get(0);
		}
        return null;
    }
	
	public EnvironmentIdirUsersGet200ResponseDataInner getIdirUser(String userName) {
		EnvironmentIdirUsersGet200Response response = usersApi.environmentIdirUsersGet(Environment.DEV, null, null, userName, null);
		List<EnvironmentIdirUsersGet200ResponseDataInner> data = response.getData();
		if (data != null && !data.isEmpty()) {
			getTcoUsers();
			return data.get(0);
		}
        return null;
	}
	
	public List<TcoUser> getTcoUsers() {

        List<TcoUser> userResult = userLoader.getTcoUsers();

        if (userResult != null && !userResult.isEmpty()) {
        	for (TcoUser tcoUser : userResult) {
				System.out.println(tcoUser.toString());
			}
			return userResult;
		}
        return null;
    }

	public UsersResource getInstance(){
        return keycloak.realm(KeycloakConfig.realm).users();
    }
}
