(function (w,d,n) {
    // check if we already have wishlu defined, is so, don't initialize again
    wb = window.wb || {};
    if (wb.version) {
        wb.doButton();
        return;
    }

    wb.w = w;
    wb.d = d;
    wb.n = n;

    // define wishlu version
    wb.version = '1.0.0.0.';
        
    // regex
    wb.pattern = {};
    wb.pattern.youtube = "youtube\.com";
    wb.pattern.ytimg = "i\.ytimg\.com";

    if (!window.jQuery) {
        var jq = document.createElement('script');
        jq.type = 'text/javascript';
        jq.src = "https://code.jquery.com/jquery-2.1.1.min.js";
        document.getElementsByTagName('head')[0].appendChild(jq);

        // load jQuery async
        //(function () {
        //    function loadScripts() {
        //        var s = document.createElement('script');
        //        s.type = 'text/javascript';
        //        s.async = true;
        //        s.src = "https://code.jquery.com/jquery-2.1.1.min.js";
        //        var x = document.getElementsByTagName('script')[0];
        //        x.parentNode.insertBefore(s, x);
        //    }
        //    window.attachEvent ? window.attachEvent('onload', loadScripts) : window.addEventListener('load', loadScripts, false);
        //})();
    }
    else {
        window.$ = window.jQuery;
    }
                
    //alert("address.js loaded via script_tag!!!");

    // hook for wishlu purchase button on shopify product page
    wb.purchase = function () {
        alert("clicked!");
    };
        
    wb.doButton = function () {
        //var imgCount = 0;

        $("html, body").animate({ scrollTop: 0 }, 'fast');

        // import wishlu bookmarklet stylesheet
        $('<style type="text/css">@import url("//assets.wishlu.com/css/bookmarklet.css");</style>').appendTo("head");

        if ($("#wishlu_curtain").length == 0) {

            // background mask (curtain), setup container, toggle button
            $('body').append('<div id="wishlu_curtain"><div id="wbcontainer"><div id="headerbar"><div id="wl_logo"></div><div id="button-toggle"><span id="close">Close</span><span id="count"></span></div></div><ul id="imagelist"></ul></div></div>');

            // toggle button click handler
            $("#button-toggle").click(function () {
                $("#wishlu_curtain, #wbcontainer").remove();
            })

            // grab images
            $("img").each(function () { addImage($(this)); });

            // find background images and grab them
            $("*:visible").each(function () {
                var sudo = $(this);
                var img = sudo.css("background-image");

                if (img !== "none") {
                    addImage(sudo);
                }
            });

            // grab youtube videos
            /*$("a").each(function () {
                var src = $(this).attr("href");

                if (src.match(wishlu.pattern.youtube)) {
                    var vid = src.split("#")[0].split("?")[1].split("v")[0].split('=')[1];
                    
                    console.log("adding video");
                    addVideo("http://i.ytimg.com/vi/" + vid + "/hqdefault.jpg");
                }
            });*/

            // update total image counter
            //$("#count").text(imgCount);
        }
    };
        
    wb.imgCount = 0;
    function addImage(img) {
        var imgurl = img.attr("src");        
        var over = false;
        console.log("Adding image: " + imgurl);
                
        if (imgurl === undefined) {
            var bgurl = img.css('background-image').slice(4, -1);
            console.log("found background-image: " + bgurl);
            imgurl = bgurl;
        }

        imgurl.replace(" ", "%20");
        imgurl = imgurl.replace("'", "").replace('"', '');
        imgurl = encodeURI(imgurl);
        imgurl = imgurl.replace("%22", "");

        if (imgurl.match(wb.pattern.ytimg)) {
            console.log("found ytimg");
            imgurl = imgurl.replace("/default.", "/hqdefault.");
            img.attr("src", imgurl);
            console.log("HQ youtube thumb: " + imgurl);
            over = true;
        }
        
        var name = encodeURIComponent($("title").text());
        var desc = encodeURIComponent($("meta[name='description']").attr('content'));
                
        //var beginLiTag = "<li><a id='imglink_" + imgCount + "' onclick='return wishlu.popUp(\"" + imgurl + "\",\"" + name + "\",\"" + desc + "\");' href='";
        var beginLiTag = "<li><a id='imglink_" + wb.imgCount + "' href='#'>";
        var beginImageTag = "<img src=";
        var middleImageTag = " style='margin-top:";
        var endImageTag = "px'>";
        var endLiTag = "</a></li>";
        var imageWidth = img.width();
        var imageHeight = img.height();

        if (over) {
            imageWidth = 480;
            imageHeight = 360;
        }

        var containData = imgurl.indexOf('data:');
                        
        /*Check whether image big enough*/
        if (imageWidth > 150 && imageHeight > 200 && containData === -1) {
            console.log("actually adding ")
            /*Calculate margin to vertically center*/
            if (imageWidth > imageHeight) {
                var calculatedMargin = (200 - (200 * (imageHeight / imageWidth))) * 0.5;
            }
            else {
                calculatedMargin = 0;
            }

            var finalLink = beginLiTag + beginImageTag + imgurl + middleImageTag + calculatedMargin + endImageTag + "<span>" + imageWidth + " x " + imageHeight + "</span>" + endLiTag;
            $('#imagelist').append(finalLink).attr("src",imgurl);
            $("#imglink_" + wb.imgCount).click(function () {
                wb.popUp(imgurl, name, desc);
            })
            wb.imgCount++;
        }
    }

    function addVideo(img) {
        var imgurl = img;
        console.log(imgurl);

        imgurl.replace(" ", "%20");
                
        var name = encodeURIComponent($("title").text());
        var desc = encodeURIComponent($("meta[name='description']").attr('content'));

        //var beginLiTag = "<li><a id='imglink_" + imgCount + "' onclick='return wishlu.popUp(\"" + imgurl + "\",\"" + name + "\",\"" + desc + "\");' href='";
        var beginLiTag = "<li><a id='imglink_" + wb.imgCount + "' href='#'>";
        var beginImageTag = "<img src='";
        var middleImageTag = "' style='margin-top:";
        var endImageTag = "px'>";
        var endLiTag = "</a></li>";
        var imageWidth = img.width();
        var imageHeight = img.height();

        var containData = imgurl.indexOf('data:');

        /*Check whether image big enough*/
        if (imageWidth > 150 && imageHeight > 200 && containData === -1) {
            /*Calculate margin to vertically center*/
            if (imageWidth > imageHeight) {
                var calculatedMargin = (200 - (200 * (imageHeight / imageWidth))) * 0.5;
            }
            else {
                calculatedMargin = 0;
            }

            var finalLink = beginLiTag + beginImageTag + imgurl + middleImageTag + calculatedMargin + endImageTag + "<span>" + imageWidth + " x " + imageHeight + "</span>" + endLiTag;
            $('#imagelist').append(finalLink);
            $("#imglink_" + wb.imgCount).click(function () {
                wb.popUp(imgurl, name, desc);
            })
            wb.imgCount++;
        }
    }

    // utility function (TODO: add credit to author)
    function toAbsoluteURL(url) {
        // Handle absolute URLs (with protocol-relative prefix)
        // Example: //domain.com/file.png
        if (url.search(/^\/\//) != -1) {
            return window.location.protocol + url;
        }

        // Handle absolute URLs (with explicit origin)
        // Example: http://domain.com/file.png
        if (url.search(/:\/\//) != -1) {
            return url;
        }

        // Handle absolute URLs (without explicit origin)
        // Example: /file.png
        if (url.search(/^\//) != -1) {
            return window.location.origin + url;
        }

        // Handle relative URLs
        // Example: file.png
        var base = window.location.href.match(/(.*\/)/)[0]
        return base + url;
    };
        
    wb.popUp = function (img, name, desc) {
        ww = 600;
        wh = 400;

        img = toAbsoluteURL(img);        
        img = encodeURIComponent(img);
        img = img.replace('"', '').replace("'", "");

        wb.w.open('//www.wishlu.com/button/share?url=' + encodeURIComponent(window.location.href) + '&image=' + img + '&name=' + name + '&description=' + desc, 'wishlu button - popup', 'status=no,height=' + wh + ",width=" + ww + ",resizable=yes,left=0,top=0,toolbar=no,menubar=no,scrollbars=yes,personalbar=no,location=no,directories=no");

        return false;
    };
        
    window.wb = wb;
    wb.doButton();

}(window, document, navigator));