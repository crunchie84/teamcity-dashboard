/*!
* Windows 8 Metro Grid implementation by Tom Lokhorst
*
*/
function MetroGrid() {
}

MetroGrid.prototype = {
  init: function ($grid) {
    this.$grid = $grid;

    // dummy group for space
    //this.layout();

    //$(window).bind('resize', this.layout.bind(this));
  },

  animate: function () {
    var $items = this.$grid.find('.item:has(.item-text)');
    var $item = $($items[Math.floor(Math.random() * $items.length)]);

    if ($item != null) {
      this.animateStep($item).then(function (step) {
        if (step)
          setTimeout(this.animateStep.bind(this, $item, step), (Math.random() * 2 + 3) * 1000);
      }.bind(this));
    }

    setTimeout(this.animate.bind(this), (Math.random() * 3 + 2) * 1000);//2 tot 5 seconds between animations
  },

  animateStep: function ($item, gotoStep) {
    return $.Deferred(function (dfd) {
      var step = gotoStep || $item.data('step') || 'uninitialized';

      //if somehow we are triggert twice at the same time we bail out the second time
      if (step == 'animating') return;
      $item.data('step', 'animating');


      if (step == 'uninitialized') {
        //hard position the extra text outside of the box and make it visible
        $itemText = $item.find('.item-text');
        var mtop = $itemText.position()
          ? $item.height() - $itemText.position().top - $itemText.height() - 10
          : 0;
        mtop = Math.floor(mtop / 120) * 120;
        $item.find('.extra-text')
          .css({ marginTop: mtop })
          .show();

        step = 'start';
      }

      if (step == 'start') {
        //if we have multiple images they will round-robin fade in fade out and change position
        var $images = $item.find('.item-images img');
        if (!$item.hasClass('failing') || $images.length < 2) {
          step = 'up';
        }
        else {
          var fst = Math.floor(Math.random() * $images.length);
          var snd = (fst + 1) % $images.length;

          var $fst = $($images[fst]);
          var $snd = $($images[snd]);

          $fst.animate({ opacity: 0, }, 'slow', function () {
            $snd.animate({ opacity: 0, }, 'slow', function () {
              $fst.swap($snd);

              $snd.animate({ opacity: 1, }, 'slow', function () {
                $fst.animate({ opacity: 1, }, 'slow', function () {
                  setTimeout(function () {
                    $item.data('step', 'up');
                    dfd.resolve();
                  }, 1000);
                });
              });
            });
          });
        }
      }

      if (step == '?') {
        if (!$item.hasClass('failing'))
          step = 'up';
      }

      //now animate the extra-text portion to top 
      if (step == 'up') {
        if (!$item.find('.extra-text').length) {
          //we dont animate up or down, ready with animation, ready for next cycle
          $item.data('step', 'start');
          dfd.resolve();
          return;
        }

        //130 = images should not display margin
        $item.children()
          .animate({ top: -(($item.hasClass('failing') && $item.find('.item-images img').length) ? 130 : 120) }, 'slow', function () {
            setTimeout(function () {
              dfd.resolve('down');
            }.bind(this), 1000);
          }.bind(this));
      }

      //now animate back to the bottom part
      if (step == 'down') {
        $item.children()
          .animate({ top: 0 }, 'slow', function () {
            setTimeout(function () {
              //ready for next animation cycle
              $item.data('step', 'start');
              dfd.resolve();
            }.bind(this), 1000);
          }.bind(this));
      }
    }.bind(this)).promise();
  },
};

if (!Function.prototype.bind) {
  Function.prototype.bind = function (oThis) {
    if (typeof this !== "function") {
      // closest thing possible to the ECMAScript 5 internal IsCallable function
      throw new TypeError("Function.prototype.bind - what is trying to be bound is not callable");
    }

    var aArgs = Array.prototype.slice.call(arguments, 1),
        fToBind = this,
        fNOP = function () { },
        fBound = function () {
          return fToBind.apply(this instanceof fNOP
                                 ? this
                                 : oThis || window,
                               aArgs.concat(Array.prototype.slice.call(arguments)));
        };

    fNOP.prototype = this.prototype;
    fBound.prototype = new fNOP();

    return fBound;
  };
}

jQuery.fn.swap = function (b) {
  b = jQuery(b)[0];
  var a = this[0];

  var t = a.parentNode.insertBefore(document.createTextNode(""), a);
  b.parentNode.insertBefore(a, b);
  t.parentNode.insertBefore(b, t);
  t.parentNode.removeChild(t);

  return this;
};

