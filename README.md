teamcity-dashboard
==================

Teamcity REST interface merged to simple JSON feed with visuals to display builds &amp; breakers

USAGE
 * Configure web.config settings for teamcity url+username/passwd (Remark: this user needs to be able to acces user's private details - role >= project admin required)
 * Hook up the site in IIS (.Net 4.0)
 * Go to the url. 
 
The site will try to find all non-archived projects and their (widget visible) build configs. It will determine which are failing and tries to retrieve the email addresses of the possible build breakers.
 
NOTES
 * In the site we reference the Segoe UI fonts. Due to copyright issues these are not supplied in
 this Git repository. Fallback is Serif.