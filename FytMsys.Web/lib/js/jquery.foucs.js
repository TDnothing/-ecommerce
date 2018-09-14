﻿; (function ($) {
    $.extend({
        'foucs': function (con) {
            var $container = $('#index_b_hero')
                , $imgs = $container.find('li.hero')
            , $leftBtn = $container.find('a.prev')
            , $rightBtn = $container.find('a.next')
            , config = {
                interval: con && con.interval || 3500,
                animateTime: con && con.animateTime || 500,
                direction: con && (con.direction === 'right'),
                _imgLen: $imgs.length,
                _imgWidth : $imgs.width()
            }
            , i = 0
            , getNextIndex = function (y) { return i + y >= config._imgLen ? i + y - config._imgLen : i + y; }
            , getPrevIndex = function (y) { return i - y < 0 ? config._imgLen + i - y : i - y; }
            , silde = function (d,j) {
            	if(j!=20){
            		i=j;
            	}            	
            	$imgs.eq((d ? getPrevIndex(2) : getNextIndex(2))).css('left', (d ? '-' + (config._imgWidth - 200) * 2 + 'px' : '' + (config._imgWidth - 200) * 2 + 'px'))
                $imgs.stop(true,true).animate({
                    'left': (d ? '+' : '-') + '=' + (config._imgWidth - 200) + 'px'
                }, config.animateTime);
                i = d ? getPrevIndex(1) : getNextIndex(1);
                $(".helper span").removeClass('cur').eq(i).addClass("cur");
            }
            , s = setInterval(function () { silde(config.direction,20); }, config.interval);
            $imgs.eq(i).css('left', 0).end().eq(i + 1).css('left', '' + (config._imgWidth - 200) + 'px').end().eq(i - 1).css('left', '-' + (config._imgWidth-200) + 'px');
            $container.find('.hero-wrap').add($leftBtn).add($rightBtn).hover(function () { clearInterval(s); }, function () { s = setInterval(function () { silde(config.direction,20); }, config.interval); });

            
            for(var g=0; g<$imgs.length; g++){
            	if(g==0){ $(".helper").append('<span class="cur"></span>'); }
            	else{$(".helper").append('<span></span>');}
            }
//          $(".helper span").click(function(){
//          	$(this).addClass("cur").siblings().removeClass('cur');
//          	if ($(':animated').length === 0) {
//                  silde(false,$(this).index());
//              }
//          });
            
            $leftBtn.click(function () {
                if ($(':animated').length === 0) {
                    silde(true,20);
                }
            });
            $rightBtn.click(function () {
                if ($(':animated').length === 0) {
                    silde(false,20);
                }
            });
        }
    });
}(jQuery));