# Insomnia Collections

This folder contains [Insomnia](https://insomnia.rest/) collections for the TCO project.

When exporting to update GitHub, DO NOT export private environments in your export. Exporting private environments
will export credentials



## OrdsDataService

The `Base Environment` will set the `base_url` for both `tco` and `occam`. These depend on the `wsgw_host` and `db_name`
parameters defined in each environment specific environment.  Here is a template for the `dev` environment. You will
need to substitue the correct username and password values. Ensure to create your environment as a `Private environment`.

```json
{
	"tco": {
		"username": "username",
		"password": "password"
	},
	"occam": {
		"username": "username",
		"password": "password"
	},
	"wsgw_host": "wsgw.dev.jag.gov.bc.ca",
	"db_name": "devj"
}
```

The collection is organized into folders. Authentication settings are set at the top `occam` and `tco` folder levels.
Requests below inherit the authtication.  Each folder has also has an `After-response` script to ensure the response
is either a 200, or 304 if request header `If-None-Match` was supplied.

```
.
├── occam
│   ├── v1
│   └── v2
├── tco
│   └── v1
│   └── v2
```
