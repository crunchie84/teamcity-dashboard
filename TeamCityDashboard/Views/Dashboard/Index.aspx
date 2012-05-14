<%@ Page Language="C#" Inherits="System.Web.Mvc.ViewPage" %>
<!doctype html>
<title>Q42 Continouos Integration</title>

<meta name=apple-touch-fullscreen content=yes>
<meta name=apple-mobile-web-app-capable content=yes>
<meta name=viewport content="user-scalable=no,initial-scale=1.0">

<link rel=apple-touch-icon href=images/q42.png>
<link rel=apple-touch-startup-image href=images/black.png>

<link rel=stylesheet href=css/styles.css>

<script src="scripts/jquery.min.js"></script>
<script src="scripts/jquery.crypt.js"></script>
<script src="scripts/metro-grid.js"></script>

<script>
var lastStr = '';
function loadData(layout)
{
  $.getJSON("data").done(function (data)
  {
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
    data.sort(function (x, y)
    {
      return x.BuildConfigs.length == y.BuildConfigs.length
          ? x.Name < y.Name ? -1 : 0
          : x.BuildConfigs.length > y.BuildConfigs.length ? -1 : 1;
    });

    $.each(data, function (_, project)
    {
      var name = project.Name;

      // Top secret!!1!
      // if (name.indexOf("hilips") >= 0)
      //   name = "Jeweetwel";

      var $a = $('<a href="' + project.Url + '" id=' + project.Id + ' class="item">');
      var $text = $('<div class="item-text">');
      var $extraText = $('<div class=extra-text>');
      $a
        .append($text)
        .append($extraText);

      var failingSteps = project.BuildConfigs.filter(function (s) { return !s.CurrentBuildIsSuccesfull });

      if (failingSteps.length)
      {
        $a.addClass('failing');
        $text.append('<p><span class=large>' + name + '</p>');

        var allBreakers = [];

        $.each(failingSteps, function (_, step)
        {
          $extraText.append('<p id=' + step.Id + ' class=small>'
                       + '<a href="' + step.Url + '">' + step.Name + '</a></p>');

          var $breakers = $('<div class=item-images>');
          var breakers = step.PossibleBuildBreakerEmailAddresses;
          $.each(breakers, function (_, email)
          {
            var emailHash = $().crypt({ method: 'md5', source: email });
            var url = 'http://www.gravatar.com/avatar/' + emailHash + '?s=250';

            if (allBreakers.indexOf(email) >= 0) return;
            allBreakers.push(email);

            $breakers
              .append('<img src=' + url + ' class='
                     + (breakers.length > 1 ? 'half-size' : 'full-size')
                     + '>');
          })
          if (breakers.length % 2 == 1 && breakers.length > 1)
            $breakers
              .append('<img src=images/transparent.gif class=half-size>');

          $a.prepend($breakers);
        });
      }
      else
      {
        $a.addClass('successful')
        $text
          .append('<p class=large>' + name + '</p>')
          .append('<p class=small>' + project.BuildConfigs.length + ' build '
                  + (project.BuildConfigs.length > 1 ? 'steps' : 'step')
                  + '</p>');

        $.each(project.BuildConfigs, function (_, step)
        {
          $extraText.append('<p id=' + step.Id + ' class=small>'
                       + '<a href="' + step.Url + '">' + step.Name + '</a></p>');
        });
      }


      if (failingSteps.length)
      //if (project.BuildConfigs.length > 1)
        $failing.find('.column-container').append($a);
      else
        $successful.find('.column-container').append($a);
    });

    layout();
  });
  
  window.setTimeout(loadData.bind(this, layout), 5 * 1000);
};

</script>

<div class=title>
  <h1>Q42 Continous Integration</h1>
</div>

<div class=grid>
  <div class=group-container>
    <div class=group id=failing>
      <h2>Failing</h2>
      <div class=column-container></div>
    </div>
    <div class=group id=successful>
      <h2>Successful</h2>
      <div class=column-container></div>
    </div>
  </div>
</div>

<script>
window.grid = new MetroGrid();
grid.init($('.grid'));

loadData(function ()
  {
    grid.layout();
    grid.animate();
  });
</script>

