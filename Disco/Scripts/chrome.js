// wishlu browser extension logic
(function (w, d, n) {
    var $ = w["wishlu"] = {
        'w': w,
        'd': d,
        'n': n,
        'version': '1.0.0.0',
        'f': (function() {
            return {
                init: function (browser) {
                    //$.w.alert("wishlu.init called for browser: " + browser);
                    $.d.b = $.d.getElementsByTagName('BODY')[0];
                    // TODO: any other initialization
                },
                absurl: function (url) {
                    // Handle absolute URLs (with protocol-relative prefix)
                    // Example: //domain.com/file.png
                    if (url.search(/^\/\//) != -1) {
                        return $.w.location.protocol + url
                    }

                    // Handle absolute URLs (with explicit origin)
                    // Example: http://domain.com/file.png
                    if (url.search(/:\/\//) != -1) {
                        return url
                    }

                    // Handle absolute URLs (without explicit origin)
                    // Example: /file.png
                    if (url.search(/^\//) != -1) {
                        return $.w.location.origin + url
                    }

                    // Handle relative URLs
                    // Example: file.png
                    var base = $.w.location.href.match(/(.*\/)/)[0]
                    return base + url
                },
                check: function (img) {
                    var name = encodeURIComponent($.d.title);
                    var desc = "Description"; //encodeURIComponent($("meta[name='description']").attr('content'));
                    var imgurl = img.src;

                    $.f.popUp(imgurl, name, desc);
                },
                popUp: function (img, name, desc) {
                    var ww = 600;
                    var wh = 400;

                    $.w.open('http://wishlu.com/button/share?url=' + encodeURIComponent($.w.location.href) + '&image=' + encodeURIComponent($.f.absurl(img)) + '&name=' + name + '&description=' + desc, 'wishlu button - popup' + new Date().getTime(), 'status=no,height=' + wh + ",width=" + ww + ",resizable=yes,left=0,top=0,toolbar=no,menubar=no,scrollbars=yes,personalbar=no,location=no,directories=no");
                },
                warn: function () {
                    $.w.alert("There was an error adding this item to wishlu.");
                },
                rightClick: function (src) {
                    console.log("wishlu.rightClick called for: " + src);

                    // Chrome: trigged by bg.js in the background tab
                    var i, n, images;

                    // convert source of image we've right-clicked into an image so we can check it
                    images = $.d.getElementsByTagName('IMG');
                    for (i = 0, n = images.length; i < n; i = i + 1) {
                        if (images[i] && images[i].src && images[i].src === src) {
                            $.f.check(images[i]);
                            break;
                        }
                    }
                }
            }
        }())
    };
            
    //$.w.alert("chrome.js loaded via  chrome extension!!!");
   
    if (chrome && chrome.storage && chrome.storage.local) {
        $.f.init("cr");
    }
}(window, document, navigator));