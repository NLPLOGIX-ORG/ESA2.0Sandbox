# ESA2.0Sandbox

## Azure AD B2C: Register app, Web UI, by using the Azure portal
**Reference:**  https://docs.microsoft.com/en-us/azure/active-directory/develop/quickstart-register-app 
1.	Sign in to the Azure portal.
2.	Use the Directory + subscription filter in the top menu to select the tenant containing Azure AD B2C.
3.	Search for and select Azure AD B2C.
4.	Under Manage, select App registrations > New registration.
5.	When the Register an application page appears, enter your application's registration information:
    - Enter a Name for your application, for example ESA Authentication Web UI. Users of your app might see this name, and you can change it later.
    - Choose the supported account types for your application. In this case, we need local user accounts and organizational directory (Azure AD) so select “Accounts in any identity provider or organizational directory (for authenticating users with user flows)”.
    - For Redirect URI, add the type of application and the URI destination that will accept returned token responses after successful authentication. 
    - For local development, select Web and type in https://localhost:{VSConfiguredPort#}/signin-oidc.
    - For Azure deployed application, select Web and type in (https://{SiteName}.azurewebsites.net/signin-oidc).
    - For testing and viewing token, select Web and type in https://jwt.ms.  NOTE: This is used outside of the application. You can run the policy and select this endpoint to just see the token value (claims).
    - For Permissions, make sure “Grant admin consent to openid and offline_access permissions” is checked
    - Select Register.
6.	Under Manage, select Authentication and then add the following information:
    - In the Web section (Add a platform, if needed), add additional Redirect URI(s).
    - Under Implicit grant and hybrid flows, select ID tokens.
    - Select Save.
7.	Under Manage, select Certifications & secrets and then add the following information:
    - Select “New client secret”
    - Add a description for your client secret, for example ESAAuthenticationWebUISecret
    - Select a duration
    - Select Add
    - Record the secret's value for use in the web application. This secret value is never displayed again after you leave this page.

## Azure AD B2C: Register app, Web API, by using the Azure portal
**Reference:**  https://docs.microsoft.com/en-us/azure/active-directory/develop/quickstart-configure-app-expose-web-apis 
1.	Sign in to the Azure portal.
2.	Use the Directory + subscription filter in the top menu to select the tenant containing Azure AD B2C.
3.	Search for and select Azure AD B2C.
4.	Under Manage, select App registrations > New registration.
5.	When the Register an application page appears, enter your application's registration information:
    - Enter a Name for your application, for example ESA Authentication Api. Users of your app might see this name, and you can change it later.
    - Choose the supported account types for your application. In this case, we need local user accounts and organizational directory (Azure AD) so select “Accounts in any identity provider or organizational directory (for authenticating users with user flows)”.
    - For Redirect URI, Web APIs do not need to register a redirect URI because no user is interactively signed in.
    - For Permissions, make sure “Grant admin consent to openid and offline_access permissions” is checked
    - Select Register.
6.	Under Manage, select Expose an API > Add a scope
    - If prompted, accept the proposed application ID URI (https://{tenantDomain}/{applicationId}) by selecting Save and Continue.
    - Specify the following:
    - Enter scope name (ex. access_as_user)
    - Enter Admin consent display name (ex. Access service as a user.)
    - Enter Admin consent description (ex. Accesses the web API as a user.)
    - Keep the State value set to Enabled
    - Select Add scope

## Azure AD B2C: Add Permissions to Web UI App Registration to Access Your Web API App Registration
***Prerequisite:***
- [Azure AD B2C: Register app, Web UI, by using the Azure portal](#azure-ad-b2c-register-app-web-ui-by-using-the-azure-portal)
- [Azure AD B2C: Register app, Web API, by using the Azure portal](#azure-ad-b2c-register-app-web-api-by-using-the-azure-portal)

**Reference:** https://docs.microsoft.com/en-us/azure/active-directory/develop/quickstart-configure-app-access-web-apis#add-permissions-to-access-your-web-api 
1.	Sign in to the Azure portal.
2.	Use the Directory + subscription filter in the top menu to select the tenant containing Azure AD B2C.
3.	Search for and select Azure AD B2C.
4.	Under Manage, select App registrations then select the Web UI you registered as part of the prerequisites
5.	Select API permissions > Add a permission > My APIs
6.	Select the web API you registered as part of the prerequisites > Select Delegated permissions.
7.	Under Select permissions, expand the resource whose scope(s) you defined for your web API, and select the permission(s) the Web UI app should have on behalf of the signed-in user.  
If you used the Web API registration above, you should see access_as_user; select access_as_user.
8.	Select Add permissions to complete the process.
After adding permissions to your API, you should see the selected permissions under Configured permissions.  For the permissions you added, if you see “Not granted for…” then an Admin (could be yourself) will need to “Grant admin consent for …”. 
 
    ![Alt Text](ReadmeFiles/AADB2C_ProtectedWebApi_Configuration.png)

## Create Web UI Project using the mvc template to connect to Azure AD B2C and downstream APIs

***Prerequisite:***
- It's recommended to have .NET 5 framework installed

dotnet new mvc -au IndividualB2C --aad-b2c-instance "{B2C_Instance}" -ssp "{B2C_SignUpSignInPolicy}" -S "/signout/{B2C_SignUpSignInPolicy}" -rp "{B2C_PasswordResetPolicy}" -ep "{B2C_ProfileEditPolicy}" --client-id "{B2C_ApplicationId_RegisteredWebApp}" --domain "{Qualified_B2C_Domain}" --callback-path "/signin-oidc" --called-api-url "{ApiUrl}" --called-api-scopes "{ApiScopes}" -o "{WebAppName}"

The following modifications need to be made in order to run the application.

### Project File: {WebAppName}.csproj
	Make sure the package references are using the latest stable versions (as of 3/15/2021)
	<ItemGroup>
		<PackageReference Include="Microsoft.AspNetCore.Authentication.JwtBearer" Version="5.0.3" NoWarn="NU1605" />
		<PackageReference Include="Microsoft.AspNetCore.Authentication.OpenIdConnect" Version="5.0.3" NoWarn="NU1605" />
		<PackageReference Include="Microsoft.Identity.Web" Version="1.8.0" />
		<PackageReference Include="Microsoft.Identity.Web.UI" Version="1.8.0" />
	</ItemGroup>

### App Configuration File: appsettings.json
	Update "ClientSecret" with the secret value created in the App Registerstration for {B2C_ApplicationId_RegisteredWebApp}

### Startup.cs
	You will need to ensure this is added: using Microsoft.AspNetCore.Mvc.Authorization;
	
	In the method, ConfigureServices, the templated code needs to be replaced with the following:
	
	var initialScopes = Configuration.GetValue<string>("DownstreamApi:Scopes")?.Split(' ');

	services.AddMicrosoftIdentityWebAppAuthentication(Configuration, "AzureAdB2C")
        .EnableTokenAcquisitionToCallDownstreamApi(initialScopes)
	    .AddDownstreamWebApi("DownstreamApi", Configuration.GetSection("DownstreamApi"))
	    .AddInMemoryTokenCaches();

	services.AddControllersWithViews(options =>
	{
        var policy = new AuthorizationPolicyBuilder()
            .RequireAuthenticatedUser()
            .Build();
        options.Filters.Add(new AuthorizeFilter(policy));
	}).AddMicrosoftIdentityUI();

	services.AddRazorPages();
	
### HomeController.cs
In method, Index(), change the line

    using var response = await _downstreamWebApi.CallWebApiForUserAsync("DownstreamApi").ConfigureAwait(false);
    
**To**

    using var response = await _downstreamWebApi.CallWebApiForUserAsync("DownstreamApi", options =>
    {
        options.HttpMethod = HttpMethod.Get;
        options.RelativePath = "weatherforecast"; // Since we are using the sample Api
    }).ConfigureAwait(false);

## Create Web Api Project using the webapi template to connect to Azure AD B2C and validate scope access

***Prerequisite:***
- It's recommended to have .NET 5 framework installed

dotnet new webapi -au IndividualB2C --aad-b2c-instance "{B2C_Instance}" -ssp "{B2C_SignUpSignInPolicy}" --client-id "{B2C_ApplicationId_RegisteredWebApi}" --domain "{Qualified_B2C_Domain}" -o "{WebApiName}"

The following modifications need to be made in order to run the application.

### Project File: {WebApiName}.csproj
	Make sure the package references are using the latest stable versions (as of 3/15/2021)
	<ItemGroup>
		<PackageReference Include="Microsoft.AspNetCore.Authentication.JwtBearer" Version="5.0.3" NoWarn="NU1605" />
		<PackageReference Include="Microsoft.AspNetCore.Authentication.OpenIdConnect" Version="5.0.3" NoWarn="NU1605" />
		<PackageReference Include="Microsoft.Identity.Web" Version="1.8.0" />
	</ItemGroup>
	
### Properties: adjust application port settings, if applicable
* launchSettings.json - adjust applicationUrl for your development; this should match to the value {ApiUrl} (for development purposes)

### Configuration File: appsettings.json
* Add "Scopes" key to "AzureAdB2C"
    "Scopes": "access_as_user" // Should match to the scopes configured in the Web App {ApiScopes}

### WeatherForecast Controller: /Controllers/WeatherForecastController.cs
* Replace using Microsoft.Identity.Web.Resource with Microsoft.Identity.Web
* Under [Authorize] add [AuthorizeForScopes(ScopeKeySection = "AzureAdB2C:Scopes")] 
* Remove line static readonly string[] scopeRequiredByApi = new string[] { "access_as_user" };
* Remove line HttpContext.VerifyUserHasAnyAcceptedScope(scopeRequiredByApi);
