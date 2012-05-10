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
      $.get("data", function (data) {
        var $containerEl = $('div[data-id="container"]');
        $containerEl.empty();    //remove current elements
        $containerEl.removeClass('masonry');
        $containerEl.removeAttr('style');

        $.each(data, function (index, projectDetails) {
          //project
          var failingBuildConfig = null;
          for (idx in projectDetails.BuildConfigs) {
            if (!projectDetails.BuildConfigs[idx].CurrentBuildIsSuccesfull) {
              failingBuildConfig = projectDetails.BuildConfigs[idx];
              break;
            }
          }
          //create project element
          var $project = $("<div>", { 
            'class': "project " + (failingBuildConfig == null ? "success" : "failing"),
            'id' : projectDetails.Id
          });
          $project.append('<h1>' + projectDetails.Name + '</h1>');

          //if not building - calculate gravatar hash
          if (failingBuildConfig != null) {
            //append details of project which is borked
            $project.append('<p>' + failingBuildConfig.Name + '</p>');

            //append image of build breaker
            for(idx in failingBuildConfig.PossibleBuildBreakerEmailAddresses){
              var emailHash = $().crypt({
                method: 'md5',
                source: failingBuildConfig.PossibleBuildBreakerEmailAddresses[idx]
              });
              $project.append('<figure><img src="http://www.gravatar.com/avatar/' + emailHash + '" class="build-breaker"/><figcaption>Build Breaker?</figcaption></figure>');
            }
          }
          $containerEl.append($project);
        });

        //now masonry the whole lot
        $containerEl.masonry({
          itemSelector : '.project',
          isFitWidth: true,
  //        isAnimated: true,
  //        animationOptions: {
  //          duration: 750,
  //          easing: 'linear',
  //          queue: false
  //        } 
        });  
      });
    };
    
    //hit it 
    window.setInterval(loadData, 30 * 1000);//refresh every 30 secs
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
    
    img.build-breaker
    {
      width: 80px;
      height: 80px;
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
    }

    .project > h1 {
      text-shadow: 1px 1px 2px #000;
      font-weight: bold;
      font-size: 1.1em;
      text-transform: uppercase;
    }

    .project > p {
      font-weight: normal;
      font-size: 0.7em;
      color: #bbb;
    }

    .project.success
     {
      background: #407A39;
    }    
    .project.failing{
      background: #AF0E01;
      border-color: #650801;
    }
       
    .wrapper{
      width: 90%;
      margin: auto;
    }
  </style>
</head>
<body>
  <div class="wrapper" data-id="container">
    

    <!--<script type="text/javascript" src="http://teamcity.q42.net/externalStatus.html?js=1"></script>-->
  </div>
</body>
</html>