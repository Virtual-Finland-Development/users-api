# Terms of service 

The users-api, when used as part of the [Access Finland MVP](https://github.com/Virtual-Finland-Development/access-finland) application, is subject to the terms of service of the app. The requirement is that the user has to accept the terms of service before accessing the relevant data. It is also required that the service keeps track of which users have accepted the terms and which version of the terms they have accepted. 

## Terms of service version references management

The terms of service are provided and managed by the Access Finland MVP application, but it uses the users-api as the service that keeps track of the accepted terms. This means that the users-api has to have some reference to which terms of service versions exists so that it can validate which version the user has accepted. 

The versions are stored in the users-api database in the `TermsOfServices` table and is managed using the admin function script `update-terms-of-service`. The script performs a syncronization between the database and the a json-file that contains the terms of service versions. The json-file is located at [../Data/terms-of-services.json](../Data/terms-of-services.json) and it contains a list of terms of service versions, where each version has the following properties:

- `version`: the sematic version of the terms of service, used as key
- `url`: the url to the terms of service document
- `description`: a short description of the terms of service version
- `action`: the action that is performed when the syncronization script is run, can be `UPDATE` (default) or `REMOVE`. If the field is ommitted, the default action `UPDATE` is used.

## Enforcement of the terms of service

As it is required that the user has accepted the terms of service before accessing the relevant data, the users-api has to enforce this requirement. This is done as a part of the authentication process better described in the [API Security Features](./README.security.md) document.

## Using the api from third-party applications

The users-api provided ToS-subjected relevant data is accessed through the dataspace gateway service, which means that in theory it would be possible to access the user data from third-party applications as well (in a controlled manner). If this is the case, the third-party application has to inform the user that the data is not accessible before accepting the Access Finland MVP terms of service and that the terms can only be accepted through the Access Finland MVP application.