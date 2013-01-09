teamcity-dashboard
========
Dashboard to display useful information from TeamCity and if available merges it with Sonar results. If somebody broke the build it tries to display their gravatar.

Installation
------
 * Compile project (.Net 4.0). This will automatically copy the web.example.config => web.config
 * Modify the web.config settings for your TeamCity and Sonar instance
	<add key="teamcity.baseUrl" value="http://your-teamcity-env.com"/>
	<add key="teamcity.username" value="a-valid-teamcity-username"/>
	<add key="teamcity.password" value="his-password"/>
	<add key="sonar.baseUrl" value="http://your-sonar-env.com"/>
	<add key="sonar.username" value="a-valid-teamcity-username"/>
	<add key="sonar.password" value="his-password"/> 
 * Hook up the site in IIS (.Net 4.0)
 * Visit the dashboard on the configured URL. 
 
Configuration
------
The site only shows non-archived projects which contain at least one build config where ‘enable status widget’ is checked. If you want to hide specific build steps (i.e. false positives) disable the 'status' widget in the specific build step. When a build is broken the dashboard tries to retrace which user broke the build by traversing back in history until it finds a succesful build. It then will display the user(s) gravatar of the configured email address.

Sonar integration
------
The sonar integration is done via configuration in TeamCity. Add the following parameter to your project configuration in TeamCity:
 * `sonar.project.key` containing the exact project key as found in Sonar

Project logo
------ 
If you want to display a logo in the interface you can also configure this in TeamCity. Add the following parameter to your project's configuration in TeamCity:
 * `dashboard.project.logo.url` containing the URL to a image of your application.
 
Retrieval of the users Gravatar
------
 * The dashboard can only find the email address of a user if the user has mapped its VCS username to its account. If you see thing in Teamcity like 'changes (4) crunchie84@github' it might not be linked. This can be configured in the user his settings in TeamCity.
 
About the font: Segoe UI
========
The font is copyright by Microsoft. Please visit this link http://www.microsoft.com/typography/fonts/family.aspx?FID=331 to determine how you can get a licensed version. Fallback is Serif.
