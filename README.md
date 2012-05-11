teamcity-dashboard
==================

Teamcity REST interface merged to simple JSON feed with visuals to display builds &amp; breakers

Usage:
 * configure web.config settings for teamcity url+username/passwd (Remark: this user needs to be able to acces user's private details - role >= project admin required)
 * hook up the service in IIS
 * go to the url. It will try to find all non-archived projects, its build configs and which are failing and who is the possible breaker. Returns this info as JSON Feed