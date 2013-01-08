teamcity-dashboard
==================

Teamcity REST interface merged to simple JSON feed with visuals to display builds &amp; breakers

USAGE
 * Configure web.config settings for teamcity url+username/passwd (Remark: this user needs to be able to acces user's private details - role >= project admin required)
 * Hook up the site in IIS (.Net 4.0)
 * Go to the url. 
 
The site only shows non-archived projects with at least one build config where ‘enable status widget’ is checked. When a build is broken it tries to determine the breakers email address and displays the Gravatar of this user.
 
About the font: Segoe UI
==================
The font is copyright by Microsoft. Please visit this link http://www.microsoft.com/typography/fonts/family.aspx?FID=331 to determine how you can get a licensed version. Fallback is Serif.
