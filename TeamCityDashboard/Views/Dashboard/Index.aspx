<%@ Page Language="C#" Inherits="System.Web.Mvc.ViewPage" %>

<!doctype html>
<title>Q42 Continuous Integration</title>

<meta name="apple-touch-fullscreen" content="yes">
<meta name="apple-mobile-web-app-capable" content="yes">
<meta name="viewport" content="user-scalable=no,initial-scale=1.0">

<link rel="apple-touch-icon" href="images/q42.png">
<link rel="stylesheet" href="css/styles.css">

<script type="text/javascript" src="scripts/jquery.min.js"></script>
<script type="text/javascript" src="scripts/jquery.crypt.js"></script>
<script type="text/javascript" src="scripts/jquery.timeago.js"></script>
<script type="text/javascript" src="scripts/jquery.masonry.min.js"></script>
<script type="text/javascript" src="https://www.google.com/jsapi"></script>
<script type="text/javascript" src="scripts/metro-grid.js"></script>

<script>
    /*settings for timeago textual tweaks*/
    jQuery.timeago.settings.strings = {
        prefixAgo: null,
        prefixFromNow: null,
        suffixAgo: "ago",
        suffixFromNow: "from now",
        seconds: "± 1 minute",
        minute: "± 1 minute",
        minutes: "%d minutes",
        hour: "1 hour",
        hours: "%d hours",
        day: "a day",
        days: "%d days",
        month: "a month",
        months: "%d months",
        year: "a year",
        years: "%d years",
        wordSeparator: " ",
        numbers: []
    };

    //load the required charts
    var chartsApiLoadedDfd = $.Deferred();
    google.load("visualization", "1", { packages: ["corechart"] });
    google.setOnLoadCallback(function () {
        chartsApiLoadedDfd.resolve();
    });

    var lastStr = '';

    function loadData(layout) {
        $.getJSON("data").done(function (data) {
            var str = JSON.stringify(data);
            if (str == lastStr) return; // nothing changed
            lastStr = str;

            var $buildConfigsContainer = $('#projectsContainer');

            $.each(data, function (_, project) {
                var name = project.Name;
                var id = project.Id;
                var lastBuildDate = project.LastBuildDate.substr(6, 13);

                var $oldItem = $('#' + id);
                if ($oldItem.length > 0 && $oldItem.attr('data-last-builddate') == lastBuildDate)
                    return;//skip this one, its the same

                //okay; it is new or different; start creation of element
                var $a = $('<div href="' + project.Url + '" id=' + project.Id + ' class="item" data-last-builddate="' + lastBuildDate + '">');

                var $text = $('<div class="item-text">');
                var $container = $('<div class="details"/>');
                var $extraText = $('<div class=extra-text>');
                $a.append($text);

                //first part - add icon if available (always)
                if (project.IconUrl != null) {
                  $text.prepend('<img src="' + project.IconUrl + '" class="logo" />');
                }

                $text.append($container);

                var failingSteps = project.BuildConfigs.filter(function (s) { return !s.CurrentBuildIsSuccesfull });
                if (failingSteps.length) {
                    $a.addClass('failing');
                    $container.append('<p class=large>' + name + '</p>');

                    var allBreakers = [];
                    var $breakers = $('<div class=item-images>');

                    $.each(failingSteps, function (_, step) {
                      $container.append('<p id=' + step.Id + ' class=small>'
                                     + '<a href="' + step.Url + '">' + step.Name + '</a></p>');

                      allBreakers = allBreakers.concat(step.PossibleBuildBreakerEmailAddresses);
                    });

                    //make unique list of breakers
                    var uniqueBreakers = $.grep(allBreakers, function (v, k) {
                        return $.inArray(v, allBreakers) === k;
                    });

                    //append all breaker images to the container
                    $.each(uniqueBreakers, function (_, email) {
                        var emailHash = $().crypt({ method: 'md5', source: email });
                        var url = 'http://www.gravatar.com/avatar/' + emailHash + '?s=500';

                        $breakers.append('<img src=' + url + ' class='
                                 + (uniqueBreakers.length > 1 ? 'half-size' : 'full-size')
                                 + ' alt="' + email + '" title="Possibly broken by: ' + email + '">');
                    });

                    var amountOfRows = Math.ceil(uniqueBreakers.length / 2);
                    if (uniqueBreakers.length > 1) {
                        //we have small images, fix the height of the container and optionally add spacer image
                        $breakers.height(amountOfRows * 125);//images are 125px height
                        if (amountOfRows % 2 > 0) {
                            /* we have an uneven amount of rows and small images */
                            $breakers.addClass('images-uneven-rows');
                        }

                        if (uniqueBreakers.length % 2 == 1)
                            $breakers.append('<img src=images/transparent.gif class=half-size>');
                    }

                    //put the breaking peope images on top inside the project element if there are any
                    if (uniqueBreakers.length > 0)
                        $a.prepend($breakers);
                }
                else {
                    $a.addClass('successful')
                    $container.append('<p class=large>' + name + '</p>');

                    if (project.CoverageGraph != null && project.CoverageGraph.length > 0) {
                        $a.append($extraText);//we have extra info to animate
                        var chartElementId = 'chart_' + project.Id;
                        $extraText.append('<h3>Code coverage</h3><div class="chart" id="' + chartElementId + '"/>');

                        //create the graph
                        setTimeout(function () {
                            var $chartContainer = $extraText.find('.chart');

                            var dataTable = new google.visualization.DataTable();
                            dataTable.addColumn('date', 'Report Date');
                            dataTable.addColumn('number', 'Coverage');
                            $.each(project.CoverageGraph, function (i, dataRow) {
                                dataTable.addRow([new Date(parseInt(dataRow[0].substr(6))), dataRow[1]]);
                            });

                            var options = {
                                width: 250,
                                height: 85,
                                backgroundColor: '#363',
                                colors: ['#fff'],
                                legend: { position: 'none' },
                                pointSize: 5,
                                hAxis: {
                                    textPosition: 'none',
                                    baselineColor: '#363',
                                    gridlines: {
                                        color: '#363'
                                    }
                                },
                                vAxis: {
                                    textPosition: 'none',
                                    format: '#,##%',
                                    baselineColor: '#363',
                                    gridlines: {
                                        color: '#363'
                                    }
                                },
                                chartArea: {
                                    top: 3,
                                    left: 10,
                                    width: 230,
                                    height: 79
                                },
                            };

                            chartsApiLoadedDfd.then(function () {
                                var chart = new google.visualization.LineChart($chartContainer[0]);
                                chart.draw(dataTable, options);
                            });
                        }, 0);

                    }

                    //append last build information to item box
                    var buildDate = new Date(parseInt(lastBuildDate));
                    $container.append('<p class="small last-build-date"><span title="' + buildDate.toISOString() + '">' + buildDate.toISOString() + '</span></p>');
                }

                //add or re-add element
                if ($oldItem.length == 1) {
                    $oldItem.fadeOut(400, function () {
                        if (masonryInitialized) {
                            $buildConfigsContainer.masonry('remove', $oldItem);
                        }

                        var newEl = $buildConfigsContainer.prepend($a);

                        if (masonryInitialized) {
                            newEl.masonry('reload');
                        }
                    });
                }
                else {
                    var newEl = $buildConfigsContainer.append($a);
                    if (masonryInitialized) {
                        newEl.masonry('reload');
                    }
                }

                //now try if it can be smaller - depends on being attached to the DOM
                if ($a.hasClass('successful')) {
                    //now activate the timeago ticker
                    $a.find('.last-build-date span').timeago();

                    $a.width(120);
                    var overflows = $a.find('.item-text .details p')[0].scrollWidth > $a.find('.item-text .details p')[0].clientWidth;
                    var hasStatistics = $a.find('.item-text .statistics-container').length;
                    var hasChart = $a.find('.extra-text .chart').length;
                    var textOverflowsIcon = $a.find('.item-text .logo').length && ( $a.find('.item-text .small').position().top + $a.find('.item-text .small').height() >= 80);

                    if (overflows || hasStatistics || hasChart || textOverflowsIcon) {
                        $a.width(250);
                    }
                }
            });

            layout();
        });

        //after initial callback to layout() we provide a stub function because masonry will be initialized
        window.setTimeout(loadData.bind(this, function () { }), 10 * 1000);
    };

    function loadEvents(layout) {
        $.getJSON("pushevents").done(function (data) {
            var $eventsContainer = $('#pushMessagesContainer .items');
            var $currentEvents = $eventsContainer.find('.event');

            var $mappedNewEventsIds = $.map(data, function (pushEvent, i) {
                return "pushevent_" + pushEvent.EventId;
            });

            // fadeout all items which are the oldest and surplus of 5 (when adding the new items)
            var $expiredPushEvents = $currentEvents.filter(function (idx){
                return $.inArray(this.id, $mappedNewEventsIds) == -1;//if not found then we are going to remove it
            });

            //this array will contain the promises that all fadeouts are done
            var fadeOuts = $.map($expiredPushEvents, function (expiredEvent, i) {
                return $(expiredEvent).fadeOut(400, function () {
                    $(this).remove();
                }).promise();
            });

            $.when.apply($, fadeOuts).then(function () {
                $.each(data, function (idx, pushEvent) {
                    var eventId = "pushevent_" + pushEvent.EventId;
                    if (document.getElementById(eventId) != null)
                        return;

                    //create new element
                    var $a = $('<div href="#" id="' + eventId + '" class="item event">');
                    //$a.hide();
                    var $text = $('<div class="item-text">');
                    var $container = $('<div class="details"/>');
                    $a.append($text);
                    var created = new Date(parseInt(pushEvent.Created.substr(6)));
                    var formatted = "" + (created.getHours() < 10 ? "0" + created.getHours() : "" + created.getHours());
                    formatted += ':' + (created.getMinutes() < 10 ? "0" + created.getMinutes() : "" + created.getMinutes());

                    $text.append('<img src="http://www.gravatar.com/avatar/' + pushEvent.ActorGravatarId + '?s=500" class="pusher" title="' + pushEvent.ActorUsername + '"/>');
                    $text.append($container);
                    $container.append('<p class=large>' + pushEvent.RepositoryName + ' - ' + formatted + '</p>');
                    $container.append('<div class="event-info"><p class="small">' + pushEvent.BranchName + ' (' + pushEvent.AmountOfCommits + ' commits)</p></div>');
                    

                    //simple animation
                    $a.fadeOut(0, function () {
                        $eventsContainer.prepend($a);
                        $a.fadeIn(700, function () {
                        });
                    });
                });
            });

        });
        window.setTimeout(loadEvents.bind(this, layout), 10 * 1000);
    }

    /*
    easy function / ticker which automatically reloads the dashboard after a new deploy
    */
    var currentVersion = "";
    function versionTicker() {
        $.getJSON("version").done(function (data) {
            if (currentVersion == "") {
                console.log('current dashboard version', data);
                currentVersion = data;
            }
            else if (currentVersion != data) {
                window.location.reload();
            }
        });
        window.setTimeout(versionTicker, 30 * 1000);
    }
</script>

<div id="title">
  <h2>Actual Continuous Integration Status</h2>
</div>

<div id="projectsContainer">
</div>


<div id="pushMessagesContainer">
    <h2>Pushes to GitHub</h2>
    <div class="items"></div>
</div>

<script>
    window.grid = new MetroGrid();
    grid.init($('#projectsContainer'));
    grid.animate();

    var masonryInitialized = false;

    loadData(function () {
        var $container = $('#projectsContainer');
        $container.masonry({
            itemSelector: '.item',
            gutterWidth: 10,
            columnWidth: 120,
            isResizable: true,
            isAnimated: true
        });
        masonryInitialized = true;
    });

    loadEvents(function () {
        //we do not use masonry on the push events for now
    });

    versionTicker();
</script>