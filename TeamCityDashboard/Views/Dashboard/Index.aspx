<%@ Page Language="C#" Inherits="System.Web.Mvc.ViewPage" %>

<!doctype html>
<title>Q42 Continuous Integration</title>

<meta name="apple-touch-fullscreen" content="yes">
<meta name="apple-mobile-web-app-capable" content="yes">
<meta name="viewport" content="user-scalable=no,initial-scale=1.0">

<link rel="apple-touch-icon" href="images/q42.png">
<link rel="stylesheet" href="css/styles.css">

<script src="scripts/jquery.min.js"></script>
<script src="scripts/jquery.crypt.js"></script>
<script src="scripts/jquery.timeago.js"></script>
<script src="scripts/metro-grid.js"></script>

<script>
    var lastStr = '';

    function loadData(layout) {
        $.getJSON("data").done(function (data) {
            // // Random bugs!
            // var p = data[Math.floor(Math.random() * data.length)];
            // console.log(p);
            // p.BuildConfigs[Math.floor(Math.random() * p.BuildConfigs.length)].CurrentBuildIsSuccesfull = false;

            var str = JSON.stringify(data);
            if (str == lastStr) return; // nothing changed
            lastStr = str;

            var $failing = $('#failing');
            var $successful = $('#successful');
            $failing.find('.item').remove();
            $successful.find('.item').remove();

            // WARNING: mutating array!
            data.sort(function (x, y) {
                return x.BuildConfigs.length == y.BuildConfigs.length
                    ? x.Name < y.Name ? -1 : 0
                    : x.BuildConfigs.length > y.BuildConfigs.length ? -1 : 1;
            });

            $.each(data, function (_, project) {
                var name = project.Name;

                var $a = $('<a href="' + project.Url + '" id=' + project.Id + ' class="item">');

                var $text = $('<div class="item-text">');
                var $extraText = $('<div class=extra-text>');
                $a.append($text);

                var failingSteps = project.BuildConfigs.filter(function (s) { return !s.CurrentBuildIsSuccesfull });
                if (failingSteps.length) {
                    $a.addClass('failing');
                    $text.append('<p><span class=large>' + name + '</p>');

                    var allBreakers = [];

                    $.each(failingSteps, function (_, step) {
                        $text.append('<p id=' + step.Id + ' class=small>'
                                     + '<a href="' + step.Url + '">' + step.Name + '</a></p>');

                        var $breakers = $('<div class=item-images>');
                        var breakers = step.PossibleBuildBreakerEmailAddresses;
                        $.each(breakers, function (_, email) {
                            var emailHash = $().crypt({ method: 'md5', source: email });
                            var url = 'http://www.gravatar.com/avatar/' + emailHash + '?s=500';

                            if (allBreakers.indexOf(email) >= 0) return;
                            allBreakers.push(email);

                            $breakers
                              .append('<img src=' + url + ' class='
                                     + (failingSteps.length > 1 || breakers.length > 1 ? 'half-size' : 'full-size')
                                     + ' alt="' + email + '" title="' + email + '">');
                        })
                        if (breakers.length % 2 == 1 && breakers.length > 1)
                            $breakers
                              .append('<img src=images/transparent.gif class=half-size>');

                        $a.prepend($breakers);
                    });
                }
                else {
                    $a.addClass('successful')
                    $text.append('<p class=large>' + name + '</p>');

                    if (project.Statistics != null) {
                        $a.append($extraText);//we have extra info to animate

                        //add statistics to animation
                        $text.append(
                            '<div class="statistics-container">'+
                            '<p class="small"><span class="statistic LinesOfCode">Lines of code <span class="value">' + project.Statistics.NonCommentingLinesOfCode + '</span></span></p>' +
                            '<p class="small"><span class="statistic CodeCoveragePercentage">Test coverage <span class="value">' + project.Statistics.CodeCoveragePercentage + '%</span></span></p>' +
                            '</div>'
                            );

                        $extraText.append('<div class="statistic PercentageComments">Comments <span class="value">' + project.Statistics.CommentLinesPercentage + '%</span></div>');
                        $extraText.append('<div class="statistic AmountOfUnitTests">Amount of unit Tests <span class="value">' + project.Statistics.AmountOfUnitTests + '</span></div>');
                        $extraText.append('<div class="statistic CyclomaticComplexityClass">Average class complexity <span class="value">' + project.Statistics.CyclomaticComplexityClass + '</span></div>');
                        $extraText.append('<div class="statistic CyclomaticComplexityFunction">Average func complexity <span class="value">' + project.Statistics.CyclomaticComplexityFunction + '</span></div>');
                    }
                    else {
                        //append buildstep information to animation + summary
                        var buildDate = new Date(parseInt(project.LastBuildDate.substr(6)));
                        $text.append('<p class="small last-build-date">Last build <em>' + $.timeago(buildDate.toISOString()) + '</em></p>');
                     }
                }

                //last part - add icon if available
                if (project.IconUrl != null) {
                    $text.append('<img src="' + project.IconUrl + '" class="logo" />');
                }

                //now append the project to the correct column
                if (failingSteps.length)
                    $failing.find('.column-container').append($a);
                else
                    $successful.find('.column-container').append($a);
            });

            layout();
        });
        window.setTimeout(loadData.bind(this, layout), 10 * 1000);
    };

    function loadEvents() {
        $.getJSON("pushevents").done(function (data) {
            var $eventsContainer = $('#events .column-container');
            var $currentEvents = $eventsContainer.find('.event');
            //TODO remove this when real animation is done.
            var newTotal = data.length + $currentEvents.length;
            if (newTotal > 5) {
                //remove the amount of too much items

                $.each($currentEvents, function (idx, currentEvent) {
                    var $evt = $(currentEvent);
                    $evt.fadeOut(400, function () {
                        $evt.remove();
                    });

                    //now enough?
                    newTotal--;
                    if (newTotal == 5) return false;
                });
                //$eventsContainer.empty();
                //clear current content for now (will be: if more then 5 remove oldest entry in iteration)
            }
            
            $.each(data, function (idx, pushEvent) {
                if (idx > 4) return false;//more then enough elements

                //create new element
                var $a = $('<a href="#" id="" class="item event">');
                //$a.hide();
                var $text = $('<div class="item-text">');
                $a.append($text);
                var created = new Date(parseInt(pushEvent.Created.substr(6)));
                var formatted = "" + (created.getHours() < 10 ? "0" + created.getHours() : "" + created.getHours());
                formatted += ':' + (created.getMinutes() < 10 ? "0" + created.getMinutes() : "" + created.getMinutes());

                $text.append('<p class=large>' + formatted + ' - ' + pushEvent.RepositoryName + '</p>');
                $text.append('<div class="event-info"><p class="small">' + pushEvent.ActorUsername + ' pushed ' + pushEvent.AmountOfCommits + ' commits to branch <em>' + pushEvent.BranchName + '</em></p></div>');
                $text.append('<img src="http://www.gravatar.com/avatar/' + pushEvent.ActorGravatarId + '?s=500" class="pusher"/>');

                //simple animation
                $a.fadeOut(0, function () {
                    $eventsContainer.append($a);
                    $a.fadeIn(700, function () {
                    });
                });
            });
        });
        window.setTimeout(loadEvents.bind(this), 10 * 1000);
    }

    //copy from http://www.sitepoint.com/html5-full-screen-api/
    var pfx = ["webkit", "moz", "ms", "o", ""];
    function RunPrefixMethod(obj, method) {
        var p = 0, m, t;
        while (p < pfx.length && !obj[m]) {
            m = method;
            if (pfx[p] == "") {
                m = m.substr(0, 1).toLowerCase() + m.substr(1);
            }
            m = pfx[p] + m;
            t = typeof obj[m];
            if (t != "undefined") {
                pfx = [pfx[p]];
                return (t == "function" ? obj[m]() : obj[m]);
            }
            p++;
        }
    }
</script>

<div class="title">
    <h1>Q42 Continuous Integration</h1>
</div>

<div class="grid">
    <div class="group-container">
        <div class="group" id="failing">
            <h2>Failing</h2>
            <div class="column-container"></div>
        </div>
        <div class="group" id="successful">
            <h2>Successful</h2>
            <div class="column-container"></div>
        </div>
    </div>
    <div class="group-container">
        <div class="group" id="events">
            <div class="column-container">
                <%--
                    <a href="#" class="item event" style="height: 120px; width: 250px;">
                    <div class="item-text">
                        <p class="large">14:41 - master at Q42/Q42.nl</p>
                        <div class="event-info">
                            <p class="small">crunchie84 pushed 2 commits to master at Q42/Q42.nl at 14:51</p>
                        </div>
                        <img src="http://www.gravatar.com/avatar/30eeda7130e6999a87749cafe01e1feb" class="pusher">
                    </div>
                </a>--%>
            </div>
        </div>
    </div>
</div>

<script>
    window.grid = new MetroGrid();
    grid.init($('.grid'));

    loadData(function () {
        grid.layout();
        grid.animate();
    });
    loadEvents();
</script>
