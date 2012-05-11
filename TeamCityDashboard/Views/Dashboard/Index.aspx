<%@ Page Language="C#" Inherits="System.Web.Mvc.ViewPage" %>
<html>
<head>
  <title>Q42 Continouos Integration Status</title>
  <!--<meta http-equiv="refresh" content="30">-->
  <script src="https://ajax.googleapis.com/ajax/libs/jquery/1.7.1/jquery.min.js"></script>
  <script src="Scripts/jquery.crypt.js"></script>
  <script src="Scripts/jquery.masonry.min.js"></script>
  
  <script type="text/javascript">
    function loadData(){
      console.log("going to reload data");
      $.get("data", function (data) {
        var $containerEl = $('div[data-id="container"]');
        $containerEl.empty();    //remove current elements
        $containerEl.masonry({
          itemSelector: '.project',
          isFitWidth: true,
          isAnimated: true,
          animationOptions: {
            duration: 750,
            easing: 'linear',
            queue: false,
            isAnimatedFromBottom: true
          }
        });

        $.each(data, function (index, projectDetails) {
          //create project element
          var $project = $("<div>", {
            'class': "project",
            'id': projectDetails.Id
          });
          $project.append('<h1><a href="' + projectDetails.Url + '">' + projectDetails.Name + '</a></h1>');

          //project
          for (idx in projectDetails.BuildConfigs) {
            if (!projectDetails.BuildConfigs[idx].CurrentBuildIsSuccesfull) {
              $project.addClass("failing");
              var failingBuildConfig = projectDetails.BuildConfigs[idx];

              //append failing build details
              $failingBuildEl = $("<div>", { 'class': "failing-build", 'id': failingBuildConfig.Id });
              $failingBuildEl.append('<h2><a href="' + failingBuildConfig.Url + '">' + failingBuildConfig.Name + '</a></h2>');

              var $breakersEl = $("<div>", { 'class': "build-breakers" });
              for (idx in failingBuildConfig.PossibleBuildBreakerEmailAddresses) {
                var emailHash = $().crypt({
                  method: 'md5',
                  source: failingBuildConfig.PossibleBuildBreakerEmailAddresses[idx]
                });
                $breakersEl.append('<figure class="build-breaker"><img src="http://www.gravatar.com/avatar/' + emailHash + '" /><figcaption>Build Breaker?</figcaption></figure>');
              }
              $failingBuildEl.append($breakersEl);

              //now add failing build to project div
              $project.append($failingBuildEl);
            }
          }

          $containerEl.append($project);
        });

        //now masonry the whole lot
        $containerEl.masonry('reload');
      });
      
      window.setTimeout(loadData, 30 * 1000);//reload ourselves in 30 seconds
    };
    
    //hit it 
    loadData();
  </script>
  <style type="text/css">
    body {
      background-color: #000;
      font-family: Tahoma;
      color: #FFF;
    }

    a {
      text-decoration: none;
      color: #FFF;
    }
    
    figure
    {
      margin: 0;
    }
        
    .project {
      -moz-border-radius: 20px;
      -webkit-border-radius: 20px;
      -khtml-border-radius: 20px;
      border-radius: 20px;
      border: 4px solid #1c4c16;
      padding: 8px;
      margin: 10px;
      float: left;
      width: 270px;
      background: #407A39; /*default = green*/
    }

    .project > h1 {
      text-shadow: 1px 1px 2px #000;
      font-weight: bold;
      font-size: 1.1em;
      text-transform: uppercase;
    }
    .project .failing-build h2 a:before
    {
      content: "Broken: ";
    }    
    .project .failing-build h2 {
      font-weight: normal;
      font-size: 0.7em;
    }
    .project .failing-build h2 a{
      color: #bbb;
    }
   
    .project.failing{
      background: #AF0E01;
      border-color: #650801;
    }

       
    .build-breakers
    {
      clear: both;
    }
       
    .build-breaker
    {
      float: left;
      width: 80px;
      overflow: hidden;
      margin-right: 0.5em;
    }

    .build-breaker img
    {
      width: 80px;
      height: 80px;
    }
   
    .build-breaker figcaption
    {
      font-size: 0.75em;
    }    

    .wrapper{
      width: 90%;
      margin: auto;
    }
  </style>
</head>
<body>
  <div class="wrapper" data-id="container" />
</body>
</html>