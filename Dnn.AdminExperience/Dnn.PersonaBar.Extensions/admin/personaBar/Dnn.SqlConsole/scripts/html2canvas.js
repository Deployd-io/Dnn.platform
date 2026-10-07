var logger = (typeof logger !== 'undefined' && logger) || console;
/*
  html2canvas 0.4.1 <http://html2canvas.hertzen.com>
  Copyright (c) 2013 Niklas von Hertzen

  Released under MIT License
*/

(function(window, document, undefined){

"use strict";

var _html2canvas = {},
previousElement,
computedCSS,
html2canvas;

_html2canvas.Util = {};

_html2canvas.Util.log = function(a) {
logger.error(`>>> Entering log(a=${a})`, { "event.action": "log", "logrite.a": a });
  if (_html2canvas.logging && window.console && window.console.log) {
    window.console.log(a);
	logger.warn(`<<< Exiting log(a=${a})`, { "event.action": "log", "logrite.a": a });
  }
};

_html2canvas.Util.trimText = (function(isNative){
  return function(input) {
    return isNative ? isNative.apply(input) : ((input || '') + '').replace( /^\s+|\s+$/g , '' );
  };
})(String.prototype.trim);

_html2canvas.Util.asFloat = function(v) {
logger.error(`>>> Entering asFloat(v=${v})`, { "event.action": "asFloat", "logrite.v": v });
logger.warn(`<<< Exiting asFloat(v=${v})`, { "event.action": "asFloat", "logrite.v": v });
  return parseFloat(v);
};

(function() {
  // TODO: support all possible length values
  var TEXT_SHADOW_PROPERTY = /((rgba|rgb)\([^\)]+\)(\s-?\d+px){0,})/g;
  var TEXT_SHADOW_VALUES = /(-?\d+px)|(#.+)|(rgb\(.+\))|(rgba\(.+\))/g;
  _html2canvas.Util.parseTextShadows = function (value) {
    if (!value || value === 'none') {
      return [];
    }

    // find multiple shadow declarations
    var shadows = value.match(TEXT_SHADOW_PROPERTY),
      results = [];
    for (var i = 0; shadows && (i < shadows.length); i++) {
      var s = shadows[i].match(TEXT_SHADOW_VALUES);
      results.push({
        color: s[0],
        offsetX: s[1] ? s[1].replace('px', '') : 0,
        offsetY: s[2] ? s[2].replace('px', '') : 0,
        blur: s[3] ? s[3].replace('px', '') : 0
      });
    }
    return results;
  };
})();


_html2canvas.Util.parseBackgroundImage = function (value) {
    var whitespace = ' \r\n\t',
        method, definition, prefix, prefix_i, block, results = [],
        c, mode = 0, numParen = 0, quote, args;

    var appendResult = function(){
		logger.error('>>> Entering appendResult()', { "event.action": "appendResult" });
        if(method) {
            if(definition.substr( 0, 1 ) === '"') {
                definition = definition.substr( 1, definition.length - 2 );
            }
            if(definition) {
                args.push(definition);
            }
            if(method.substr( 0, 1 ) === '-' &&
                    (prefix_i = method.indexOf( '-', 1 ) + 1) > 0) {
                prefix = method.substr( 0, prefix_i);
                method = method.substr( prefix_i );
            }
            results.push({
                prefix: prefix,
                method: method.toLowerCase(),
                value: block,
                args: args
            });
        }
        args = []; //for some odd reason, setting .length = 0 didn't work in safari
        method =
            prefix =
            definition =
            block = '';
			logger.error(`appendResult(): method → ${method}`, { "event.action": "appendResult" });
    };

    appendResult();
    for(var i = 0, ii = value.length; i<ii; i++) {
        c = value[i];
        if(mode === 0 && whitespace.indexOf( c ) > -1){
            continue;
        }
        switch(c) {
            case '"':
                if(!quote) {
                    quote = c;
                }
                else if(quote === c) {
                    quote = null;
                }
                break;

            case '(':
                if(quote) { break; }
                else if(mode === 0) {
                    mode = 1;
                    block += c;
                    continue;
                } else {
                    numParen++;
                }
                break;

            case ')':
                if(quote) { break; }
                else if(mode === 1) {
                    if(numParen === 0) {
                        mode = 0;
                        block += c;
                        appendResult();
                        continue;
                    } else {
                        numParen--;
                    }
                }
                break;

            case ',':
                if(quote) { break; }
                else if(mode === 0) {
                    appendResult();
                    continue;
                }
                else if (mode === 1) {
                    if(numParen === 0 && !method.match(/^url$/i)) {
                        args.push(definition);
                        definition = '';
                        block += c;
                        continue;
                    }
                }
                break;
        }

        block += c;
        if(mode === 0) { method += c; }
        else { definition += c; }
    }
    appendResult();

    return results;
};

_html2canvas.Util.Bounds = function (element) {
logger.error(`>>> Entering Bounds(element=${element})`, { "event.action": "Bounds", "logrite.element": element });
  var clientRect, bounds = {};

  if (element.getBoundingClientRect){
    clientRect = element.getBoundingClientRect();
	logger.error(`Bounds(element=${element}): clientRect → ${clientRect}`, { "event.action": "Bounds", "logrite.element": element });

    // TODO add scroll position to bounds, so no scrolling of window necessary
    bounds.top = clientRect.top;
    bounds.bottom = clientRect.bottom || (clientRect.top + clientRect.height);
    bounds.left = clientRect.left;

    bounds.width = element.offsetWidth;
    bounds.height = element.offsetHeight;
  }

logger.warn(`<<< Exiting Bounds(element=${element})`, { "event.action": "Bounds", "logrite.element": element });
  return bounds;
};

// TODO ideally, we'd want everything to go through this function instead of Util.Bounds,
// but would require further work to calculate the correct positions for elements with offsetParents
_html2canvas.Util.OffsetBounds = function (element) {
logger.error(`>>> Entering OffsetBounds(element=${element})`, { "event.action": "OffsetBounds", "logrite.element": element });
  var parent = element.offsetParent ? _html2canvas.Util.OffsetBounds(element.offsetParent) : {top: 0, left: 0};

logger.warn(`<<< Exiting OffsetBounds(element=${element})`, { "event.action": "OffsetBounds", "logrite.element": element });
  return {
    top: element.offsetTop + parent.top,
    bottom: element.offsetTop + element.offsetHeight + parent.top,
    left: element.offsetLeft + parent.left,
    width: element.offsetWidth,
    height: element.offsetHeight
  };
};

function toPX(element, attribute, value ) {
	logger.error(`>>> Entering toPX(element=${element},attribute=${attribute},value=${value})`, { "event.action": "toPX", "logrite.element": element, "logrite.attribute": attribute, "logrite.value": value });
    var rsLeft = element.runtimeStyle && element.runtimeStyle[attribute],
        left,
        style = element.style;

    // Check if we are not dealing with pixels, (Opera has issues with this)
    // Ported from jQuery css.js
    // From the awesome hack by Dean Edwards
    // http://erik.eae.net/archives/2007/07/27/18.54.15/#comment-102291

    // If we're not dealing with a regular pixel number
    // but a number that has a weird ending, we need to convert it to pixels

    if ( !/^-?[0-9]+\.?[0-9]*(?:px)?$/i.test( value ) && /^-?\d/.test(value) ) {
        // Remember the original values
        left = style.left;

        // Put in the new values to get a computed value out
        if (rsLeft) {
            element.runtimeStyle.left = element.currentStyle.left;
        }
        style.left = attribute === "fontSize" ? "1em" : (value || 0);
        value = style.pixelLeft + "px";

		logger.error(`toPX(element=${element},attribute=${attribute},value=${value}): value → ${value}`, { "event.action": "toPX", "logrite.element": element, "logrite.attribute": attribute, "logrite.value": value });
        // Revert the changed values
        style.left = left;
        if (rsLeft) {
            element.runtimeStyle.left = rsLeft;
        }
    }

    if (!/^(thin|medium|thick)$/i.test(value)) {
        return Math.round(parseFloat(value)) + "px";
    }

	logger.warn(`<<< Exiting toPX(element=${element},attribute=${attribute},value=${value})`, { "event.action": "toPX", "logrite.element": element, "logrite.attribute": attribute, "logrite.value": value });
    return value;
}

function asInt(val) {
	logger.error(`>>> Entering asInt(val=${val})`, { "event.action": "asInt", "logrite.val": val });
	logger.warn(`<<< Exiting asInt(val=${val})`, { "event.action": "asInt", "logrite.val": val });
    return parseInt(val, 10);
}

function parseBackgroundSizePosition(value, element, attribute, index) {
	logger.error(`>>> Entering parseBackgroundSizePosition(value=${value},element=${element},attribute=${attribute},index=${index})`, { "event.action": "parseBackgroundSizePosition", "logrite.value": value, "logrite.element": element, "logrite.attribute": attribute, "logrite.index": index });
    value = (value || '').split(',');
    value = value[index || 0] || value[0] || 'auto';
    value = _html2canvas.Util.trimText(value).split(' ');

	logger.error(`parseBackgroundSizePosition(value=${value},element=${element},attribute=${attribute},index=${index}): value → ${value}`, { "event.action": "parseBackgroundSizePosition", "logrite.value": value, "logrite.element": element, "logrite.attribute": attribute, "logrite.index": index });
    if(attribute === 'backgroundSize' && (!value[0] || value[0].match(/cover|contain|auto/))) {
        //these values will be handled in the parent function
    } else {
        value[0] = (value[0].indexOf( "%" ) === -1) ? toPX(element, attribute + "X", value[0]) : value[0];
        if(value[1] === undefined) {
            if(attribute === 'backgroundSize') {
                value[1] = 'auto';
                return value;
            } else {
                // IE 9 doesn't return double digit always
                value[1] = value[0];
            }
        }
        value[1] = (value[1].indexOf("%") === -1) ? toPX(element, attribute + "Y", value[1]) : value[1];
    }
	logger.warn(`<<< Exiting parseBackgroundSizePosition(value=${value},element=${element},attribute=${attribute},index=${index})`, { "event.action": "parseBackgroundSizePosition", "logrite.value": value, "logrite.element": element, "logrite.attribute": attribute, "logrite.index": index });
    return value;
}

_html2canvas.Util.getCSS = function (element, attribute, index) {
	logger.error(`>>> Entering getCSS(element=${element},attribute=${attribute},index=${index})`, { "event.action": "getCSS", "logrite.element": element, "logrite.attribute": attribute, "logrite.index": index });
    if (previousElement !== element) {
      computedCSS = document.defaultView.getComputedStyle(element, null);
    }

    var value = computedCSS[attribute];

    if (/^background(Size|Position)$/.test(attribute)) {
        return parseBackgroundSizePosition(value, element, attribute, index);
    } else if (/border(Top|Bottom)(Left|Right)Radius/.test(attribute)) {
      var arr = value.split(" ");
	logger.error(`getCSS(element=${element},attribute=${attribute},index=${index}): arr → ${arr}`, { "event.action": "getCSS", "logrite.element": element, "logrite.attribute": attribute, "logrite.index": index });
      if (arr.length <= 1) {
          arr[1] = arr[0];
      }
	logger.warn(`<<< Exiting getCSS(element=${element},attribute=${attribute},index=${index})`, { "event.action": "getCSS", "logrite.element": element, "logrite.attribute": attribute, "logrite.index": index });
      return arr.map(asInt);
    }

  return value;
};

_html2canvas.Util.resizeBounds = function( current_width, current_height, target_width, target_height, stretch_mode ){
logger.error(`>>> Entering resizeBounds(current_width=${current_width},current_height=${current_height},target_width=${target_width},target_height=${target_height},stretch_mode=${stretch_mode})`, { "event.action": "resizeBounds", "logrite.current_width": current_width, "logrite.current_height": current_height, "logrite.target_width": target_width, "logrite.target_height": target_height, "logrite.stretch_mode": stretch_mode });
  var target_ratio = target_width / target_height,
    current_ratio = current_width / current_height,
    output_width, output_height;

  if(!stretch_mode || stretch_mode === 'auto') {
    output_width = target_width;
    output_height = target_height;
  } else if(target_ratio < current_ratio ^ stretch_mode === 'contain') {
    output_height = target_height;
    output_width = target_height * current_ratio;
  } else {
    output_width = target_width;
    output_height = target_width / current_ratio;
  }

logger.warn(`<<< Exiting resizeBounds(current_width=${current_width},current_height=${current_height},target_width=${target_width},target_height=${target_height},stretch_mode=${stretch_mode})`, { "event.action": "resizeBounds", "logrite.current_width": current_width, "logrite.current_height": current_height, "logrite.target_width": target_width, "logrite.target_height": target_height, "logrite.stretch_mode": stretch_mode });
  return {
    width: output_width,
    height: output_height
  };
};

function backgroundBoundsFactory( prop, el, bounds, image, imageIndex, backgroundSize ) {
	logger.error(`>>> Entering backgroundBoundsFactory(prop=${prop},el=${el},bounds=${bounds},image=${image},imageIndex=${imageIndex},backgroundSize=${backgroundSize})`, { "event.action": "backgroundBoundsFactory", "logrite.prop": prop, "logrite.el": el, "logrite.bounds": bounds, "logrite.image": image, "logrite.imageIndex": imageIndex, "logrite.backgroundSize": backgroundSize });
    var bgposition =  _html2canvas.Util.getCSS( el, prop, imageIndex ) ,
    topPos,
    left,
    percentage,
    val;

    if (bgposition.length === 1){
      val = bgposition[0];

      bgposition = [];

      bgposition[0] = val;
      bgposition[1] = val;
    }

    if (bgposition[0].toString().indexOf("%") !== -1){
      percentage = (parseFloat(bgposition[0])/100);
      left = bounds.width * percentage;
      if(prop !== 'backgroundSize') {
        left -= (backgroundSize || image).width*percentage;
      }
    } else {
      if(prop === 'backgroundSize') {
        if(bgposition[0] === 'auto') {
          left = image.width;
        } else {
          if (/contain|cover/.test(bgposition[0])) {
            var resized = _html2canvas.Util.resizeBounds(image.width, image.height, bounds.width, bounds.height, bgposition[0]);
			logger.error(`backgroundBoundsFactory(prop=${prop},el=${el},bounds=${bounds},image=${image},imageIndex=${imageIndex},backgroundSize=${backgroundSize}): resized → ${resized}`, { "event.action": "backgroundBoundsFactory", "logrite.prop": prop, "logrite.el": el, "logrite.bounds": bounds, "logrite.image": image, "logrite.imageIndex": imageIndex, "logrite.backgroundSize": backgroundSize });
            left = resized.width;
            topPos = resized.height;
          } else {
            left = parseInt(bgposition[0], 10);
          }
        }
      } else {
        left = parseInt( bgposition[0], 10);
      }
    }


    if(bgposition[1] === 'auto') {
      topPos = left / image.width * image.height;
    } else if (bgposition[1].toString().indexOf("%") !== -1){
      percentage = (parseFloat(bgposition[1])/100);
      topPos =  bounds.height * percentage;
      if(prop !== 'backgroundSize') {
        topPos -= (backgroundSize || image).height * percentage;
      }

    } else {
      topPos = parseInt(bgposition[1],10);
    }

	logger.warn(`<<< Exiting backgroundBoundsFactory(prop=${prop},el=${el},bounds=${bounds},image=${image},imageIndex=${imageIndex},backgroundSize=${backgroundSize})`, { "event.action": "backgroundBoundsFactory", "logrite.prop": prop, "logrite.el": el, "logrite.bounds": bounds, "logrite.image": image, "logrite.imageIndex": imageIndex, "logrite.backgroundSize": backgroundSize });
    return [left, topPos];
}

_html2canvas.Util.BackgroundPosition = function( el, bounds, image, imageIndex, backgroundSize ) {
	logger.error(`>>> Entering BackgroundPosition(el=${el},bounds=${bounds},image=${image},imageIndex=${imageIndex},backgroundSize=${backgroundSize})`, { "event.action": "BackgroundPosition", "logrite.el": el, "logrite.bounds": bounds, "logrite.image": image, "logrite.imageIndex": imageIndex, "logrite.backgroundSize": backgroundSize });
    var result = backgroundBoundsFactory( 'backgroundPosition', el, bounds, image, imageIndex, backgroundSize );
	logger.error(`BackgroundPosition(el=${el},bounds=${bounds},image=${image},imageIndex=${imageIndex},backgroundSize=${backgroundSize}): result → ${result}`, { "event.action": "BackgroundPosition", "logrite.el": el, "logrite.bounds": bounds, "logrite.image": image, "logrite.imageIndex": imageIndex, "logrite.backgroundSize": backgroundSize });
    return { left: result[0], top: result[1] };
};

_html2canvas.Util.BackgroundSize = function( el, bounds, image, imageIndex ) {
	logger.error(`>>> Entering BackgroundSize(el=${el},bounds=${bounds},image=${image},imageIndex=${imageIndex})`, { "event.action": "BackgroundSize", "logrite.el": el, "logrite.bounds": bounds, "logrite.image": image, "logrite.imageIndex": imageIndex });
    var result = backgroundBoundsFactory( 'backgroundSize', el, bounds, image, imageIndex );
	logger.error(`BackgroundSize(el=${el},bounds=${bounds},image=${image},imageIndex=${imageIndex}): result → ${result}`, { "event.action": "BackgroundSize", "logrite.el": el, "logrite.bounds": bounds, "logrite.image": image, "logrite.imageIndex": imageIndex });
    return { width: result[0], height: result[1] };
};

_html2canvas.Util.Extend = function (options, defaults) {
logger.error(`>>> Entering Extend(options=${options},defaults=${defaults})`, { "event.action": "Extend", "logrite.options": options, "logrite.defaults": defaults });
  for (var key in options) {
    if (options.hasOwnProperty(key)) {
      defaults[key] = options[key];
    }
  }
logger.warn(`<<< Exiting Extend(options=${options},defaults=${defaults})`, { "event.action": "Extend", "logrite.options": options, "logrite.defaults": defaults });
  return defaults;
};


/*
 * Derived from jQuery.contents()
 * Copyright 2010, John Resig
 * Dual licensed under the MIT or GPL Version 2 licenses.
 * http://jquery.org/license
 */
_html2canvas.Util.Children = function( elem ) {
logger.error(`>>> Entering Children(elem=${elem})`, { "event.action": "Children", "logrite.elem": elem });
  var children;
  try {
    children = (elem.nodeName && elem.nodeName.toUpperCase() === "IFRAME") ? elem.contentDocument || elem.contentWindow.document : (function(array) {
      var ret = [];
      if (array !== null) {
        (function(first, second ) {
          var i = first.length,
          j = 0;

          if (typeof second.length === "number") {
            for (var l = second.length; j < l; j++) {
              first[i++] = second[j];
            }
          } else {
            while (second[j] !== undefined) {
              first[i++] = second[j++];
            }
          }

          first.length = i;

          return first;
        })(ret, array);
      }
      return ret;
    })(elem.childNodes);

  } catch (ex) {
    _html2canvas.Util.log("html2canvas.Util.Children failed with exception: " + ex.message);
    children = [];
  }
  return children;
};

_html2canvas.Util.isTransparent = function(backgroundColor) {
logger.error(`>>> Entering isTransparent(backgroundColor=${backgroundColor})`, { "event.action": "isTransparent", "logrite.backgroundColor": backgroundColor });
logger.warn(`<<< Exiting isTransparent(backgroundColor=${backgroundColor})`, { "event.action": "isTransparent", "logrite.backgroundColor": backgroundColor });
  return (backgroundColor === "transparent" || backgroundColor === "rgba(0, 0, 0, 0)");
};
_html2canvas.Util.Font = (function () {

  var fontData = {};

  return function(font, fontSize, doc) {
    if (fontData[font + "-" + fontSize] !== undefined) {
      return fontData[font + "-" + fontSize];
    }

    var container = doc.createElement('div'),
    img = doc.createElement('img'),
    span = doc.createElement('span'),
    sampleText = 'Hidden Text',
    baseline,
    middle,
    metricsObj;

    container.style.visibility = "hidden";
    container.style.fontFamily = font;
    container.style.fontSize = fontSize;
    container.style.margin = 0;
    container.style.padding = 0;

    doc.body.appendChild(container);

    // http://probablyprogramming.com/2009/03/15/the-tiniest-gif-ever (handtinywhite.gif)
    img.src = "data:image/gif;base64,R0lGODlhAQABAIABAP///wAAACwAAAAAAQABAAACAkQBADs=";
    img.width = 1;
    img.height = 1;

    img.style.margin = 0;
    img.style.padding = 0;
    img.style.verticalAlign = "baseline";

    span.style.fontFamily = font;
    span.style.fontSize = fontSize;
    span.style.margin = 0;
    span.style.padding = 0;

    span.appendChild(doc.createTextNode(sampleText));
    container.appendChild(span);
    container.appendChild(img);
    baseline = (img.offsetTop - span.offsetTop) + 1;

    container.removeChild(span);
    container.appendChild(doc.createTextNode(sampleText));

    container.style.lineHeight = "normal";
    img.style.verticalAlign = "super";

    middle = (img.offsetTop-container.offsetTop) + 1;
    metricsObj = {
      baseline: baseline,
      lineWidth: 1,
      middle: middle
    };

    fontData[font + "-" + fontSize] = metricsObj;

    doc.body.removeChild(container);

    return metricsObj;
  };
})();

(function(){
  var Util = _html2canvas.Util,
    Generate = {};

  _html2canvas.Generate = Generate;

  var reGradients = [
  /^(-webkit-linear-gradient)\(([a-z\s]+)([\w\d\.\s,%\(\)]+)\)$/,
  /^(-o-linear-gradient)\(([a-z\s]+)([\w\d\.\s,%\(\)]+)\)$/,
  /^(-webkit-gradient)\((linear|radial),\s((?:\d{1,3}%?)\s(?:\d{1,3}%?),\s(?:\d{1,3}%?)\s(?:\d{1,3}%?))([\w\d\.\s,%\(\)\-]+)\)$/,
  /^(-moz-linear-gradient)\(((?:\d{1,3}%?)\s(?:\d{1,3}%?))([\w\d\.\s,%\(\)]+)\)$/,
  /^(-webkit-radial-gradient)\(((?:\d{1,3}%?)\s(?:\d{1,3}%?)),\s(\w+)\s([a-z\-]+)([\w\d\.\s,%\(\)]+)\)$/,
  /^(-moz-radial-gradient)\(((?:\d{1,3}%?)\s(?:\d{1,3}%?)),\s(\w+)\s?([a-z\-]*)([\w\d\.\s,%\(\)]+)\)$/,
  /^(-o-radial-gradient)\(((?:\d{1,3}%?)\s(?:\d{1,3}%?)),\s(\w+)\s([a-z\-]+)([\w\d\.\s,%\(\)]+)\)$/
  ];

  /*
 * TODO: Add IE10 vendor prefix (-ms) support
 * TODO: Add W3C gradient (linear-gradient) support
 * TODO: Add old Webkit -webkit-gradient(radial, ...) support
 * TODO: Maybe some RegExp optimizations are possible ;o)
 */
  Generate.parseGradient = function(css, bounds) {
	logger.error(`>>> Entering parseGradient(css=${css},bounds=${bounds})`, { "event.action": "parseGradient", "logrite.css": css, "logrite.bounds": bounds });
    var gradient, i, len = reGradients.length, m1, stop, m2, m2Len, step, m3, tl,tr,br,bl;

	logger.debug(`parseGradient(css=${css},bounds=${bounds}): gradient.y0 → ${gradient.y0}`, { "event.action": "parseGradient", "logrite.css": css, "logrite.bounds": bounds });
	logger.debug(`parseGradient(css=${css},bounds=${bounds}): gradient.y1 → ${gradient.y1}`, { "event.action": "parseGradient", "logrite.css": css, "logrite.bounds": bounds });
	logger.debug(`parseGradient(css=${css},bounds=${bounds}): gradient.x0 → ${gradient.x0}`, { "event.action": "parseGradient", "logrite.css": css, "logrite.bounds": bounds });
	logger.debug(`parseGradient(css=${css},bounds=${bounds}): gradient.x1 → ${gradient.x1}`, { "event.action": "parseGradient", "logrite.css": css, "logrite.bounds": bounds });
	logger.debug(`parseGradient(css=${css},bounds=${bounds}): gradient.type → ${gradient.type}`, { "event.action": "parseGradient", "logrite.css": css, "logrite.bounds": bounds });
    for(i = 0; i < len; i+=1){
      m1 = css.match(reGradients[i]);
      if(m1) {
        break;
      }
    }

    if(m1) {
      switch(m1[1]) {
        case '-webkit-linear-gradient':
        case '-o-linear-gradient':

          gradient = {
            type: 'linear',
            x0: null,
            y0: null,
            x1: null,
            y1: null,
            colorStops: []
          };

          // get coordinates
          m2 = m1[2].match(/\w+/g);
          if(m2){
            m2Len = m2.length;
            for(i = 0; i < m2Len; i+=1){
              switch(m2[i]) {
                case 'top':
                  gradient.y0 = 0;
                  gradient.y1 = bounds.height;
                  break;

                case 'right':
                  gradient.x0 = bounds.width;
                  gradient.x1 = 0;
                  break;

                case 'bottom':
                  gradient.y0 = bounds.height;
                  gradient.y1 = 0;
                  break;

                case 'left':
                  gradient.x0 = 0;
                  gradient.x1 = bounds.width;
                  break;
              }
            }
          }
          if(gradient.x0 === null && gradient.x1 === null){ // center
            gradient.x0 = gradient.x1 = bounds.width / 2;
          }
          if(gradient.y0 === null && gradient.y1 === null){ // center
            gradient.y0 = gradient.y1 = bounds.height / 2;
          }

          // get colors and stops
          m2 = m1[3].match(/((?:rgb|rgba)\(\d{1,3},\s\d{1,3},\s\d{1,3}(?:,\s[0-9\.]+)?\)(?:\s\d{1,3}(?:%|px))?)+/g);
          if(m2){
            m2Len = m2.length;
            step = 1 / Math.max(m2Len - 1, 1);
            for(i = 0; i < m2Len; i+=1){
              m3 = m2[i].match(/((?:rgb|rgba)\(\d{1,3},\s\d{1,3},\s\d{1,3}(?:,\s[0-9\.]+)?\))\s*(\d{1,3})?(%|px)?/);
              if(m3[2]){
                stop = parseFloat(m3[2]);
                if(m3[3] === '%'){
                  stop /= 100;
                } else { // px - stupid opera
                  stop /= bounds.width;
                }
              } else {
                stop = i * step;
              }
              gradient.colorStops.push({
                color: m3[1],
                stop: stop
              });
            }
          }
          break;

        case '-webkit-gradient':

          gradient = {
            type: m1[2] === 'radial' ? 'circle' : m1[2], // TODO: Add radial gradient support for older mozilla definitions
            x0: 0,
            y0: 0,
            x1: 0,
            y1: 0,
            colorStops: []
          };

          // get coordinates
          m2 = m1[3].match(/(\d{1,3})%?\s(\d{1,3})%?,\s(\d{1,3})%?\s(\d{1,3})%?/);
          if(m2){
            gradient.x0 = (m2[1] * bounds.width) / 100;
            gradient.y0 = (m2[2] * bounds.height) / 100;
            gradient.x1 = (m2[3] * bounds.width) / 100;
            gradient.y1 = (m2[4] * bounds.height) / 100;
          }

          // get colors and stops
          m2 = m1[4].match(/((?:from|to|color-stop)\((?:[0-9\.]+,\s)?(?:rgb|rgba)\(\d{1,3},\s\d{1,3},\s\d{1,3}(?:,\s[0-9\.]+)?\)\))+/g);
          if(m2){
            m2Len = m2.length;
            for(i = 0; i < m2Len; i+=1){
              m3 = m2[i].match(/(from|to|color-stop)\(([0-9\.]+)?(?:,\s)?((?:rgb|rgba)\(\d{1,3},\s\d{1,3},\s\d{1,3}(?:,\s[0-9\.]+)?\))\)/);
              stop = parseFloat(m3[2]);
              if(m3[1] === 'from') {
                stop = 0.0;
              }
              if(m3[1] === 'to') {
                stop = 1.0;
              }
              gradient.colorStops.push({
                color: m3[3],
                stop: stop
              });
            }
          }
          break;

        case '-moz-linear-gradient':

          gradient = {
            type: 'linear',
            x0: 0,
            y0: 0,
            x1: 0,
            y1: 0,
            colorStops: []
          };

          // get coordinates
          m2 = m1[2].match(/(\d{1,3})%?\s(\d{1,3})%?/);

          // m2[1] == 0%   -> left
          // m2[1] == 50%  -> center
          // m2[1] == 100% -> right

          // m2[2] == 0%   -> top
          // m2[2] == 50%  -> center
          // m2[2] == 100% -> bottom

          if(m2){
            gradient.x0 = (m2[1] * bounds.width) / 100;
            gradient.y0 = (m2[2] * bounds.height) / 100;
            gradient.x1 = bounds.width - gradient.x0;
            gradient.y1 = bounds.height - gradient.y0;
          }

          // get colors and stops
          m2 = m1[3].match(/((?:rgb|rgba)\(\d{1,3},\s\d{1,3},\s\d{1,3}(?:,\s[0-9\.]+)?\)(?:\s\d{1,3}%)?)+/g);
          if(m2){
            m2Len = m2.length;
            step = 1 / Math.max(m2Len - 1, 1);
            for(i = 0; i < m2Len; i+=1){
              m3 = m2[i].match(/((?:rgb|rgba)\(\d{1,3},\s\d{1,3},\s\d{1,3}(?:,\s[0-9\.]+)?\))\s*(\d{1,3})?(%)?/);
              if(m3[2]){
                stop = parseFloat(m3[2]);
                if(m3[3]){ // percentage
                  stop /= 100;
                }
              } else {
                stop = i * step;
              }
              gradient.colorStops.push({
                color: m3[1],
                stop: stop
              });
            }
          }
          break;

        case '-webkit-radial-gradient':
        case '-moz-radial-gradient':
        case '-o-radial-gradient':

          gradient = {
            type: 'circle',
            x0: 0,
            y0: 0,
            x1: bounds.width,
            y1: bounds.height,
            cx: 0,
            cy: 0,
            rx: 0,
            ry: 0,
            colorStops: []
          };

          // center
          m2 = m1[2].match(/(\d{1,3})%?\s(\d{1,3})%?/);
          if(m2){
            gradient.cx = (m2[1] * bounds.width) / 100;
            gradient.cy = (m2[2] * bounds.height) / 100;
          }

          // size
          m2 = m1[3].match(/\w+/);
          m3 = m1[4].match(/[a-z\-]*/);
          if(m2 && m3){
            switch(m3[0]){
              case 'farthest-corner':
              case 'cover': // is equivalent to farthest-corner
              case '': // mozilla removes "cover" from definition :(
                tl = Math.sqrt(Math.pow(gradient.cx, 2) + Math.pow(gradient.cy, 2));
                tr = Math.sqrt(Math.pow(gradient.cx, 2) + Math.pow(gradient.y1 - gradient.cy, 2));
                br = Math.sqrt(Math.pow(gradient.x1 - gradient.cx, 2) + Math.pow(gradient.y1 - gradient.cy, 2));
                bl = Math.sqrt(Math.pow(gradient.x1 - gradient.cx, 2) + Math.pow(gradient.cy, 2));
                gradient.rx = gradient.ry = Math.max(tl, tr, br, bl);
                break;
              case 'closest-corner':
                tl = Math.sqrt(Math.pow(gradient.cx, 2) + Math.pow(gradient.cy, 2));
                tr = Math.sqrt(Math.pow(gradient.cx, 2) + Math.pow(gradient.y1 - gradient.cy, 2));
                br = Math.sqrt(Math.pow(gradient.x1 - gradient.cx, 2) + Math.pow(gradient.y1 - gradient.cy, 2));
                bl = Math.sqrt(Math.pow(gradient.x1 - gradient.cx, 2) + Math.pow(gradient.cy, 2));
                gradient.rx = gradient.ry = Math.min(tl, tr, br, bl);
                break;
              case 'farthest-side':
                if(m2[0] === 'circle'){
                  gradient.rx = gradient.ry = Math.max(
                    gradient.cx,
                    gradient.cy,
                    gradient.x1 - gradient.cx,
                    gradient.y1 - gradient.cy
                    );
                } else { // ellipse

                  gradient.type = m2[0];

                  gradient.rx = Math.max(
                    gradient.cx,
                    gradient.x1 - gradient.cx
                    );
                  gradient.ry = Math.max(
                    gradient.cy,
                    gradient.y1 - gradient.cy
                    );
                }
                break;
              case 'closest-side':
              case 'contain': // is equivalent to closest-side
                if(m2[0] === 'circle'){
                  gradient.rx = gradient.ry = Math.min(
                    gradient.cx,
                    gradient.cy,
                    gradient.x1 - gradient.cx,
                    gradient.y1 - gradient.cy
                    );
                } else { // ellipse

                  gradient.type = m2[0];

                  gradient.rx = Math.min(
                    gradient.cx,
                    gradient.x1 - gradient.cx
                    );
                  gradient.ry = Math.min(
                    gradient.cy,
                    gradient.y1 - gradient.cy
                    );
                }
                break;

            // TODO: add support for "30px 40px" sizes (webkit only)
            }
          }

          // color stops
          m2 = m1[5].match(/((?:rgb|rgba)\(\d{1,3},\s\d{1,3},\s\d{1,3}(?:,\s[0-9\.]+)?\)(?:\s\d{1,3}(?:%|px))?)+/g);
          if(m2){
            m2Len = m2.length;
            step = 1 / Math.max(m2Len - 1, 1);
            for(i = 0; i < m2Len; i+=1){
              m3 = m2[i].match(/((?:rgb|rgba)\(\d{1,3},\s\d{1,3},\s\d{1,3}(?:,\s[0-9\.]+)?\))\s*(\d{1,3})?(%|px)?/);
              if(m3[2]){
                stop = parseFloat(m3[2]);
                if(m3[3] === '%'){
                  stop /= 100;
                } else { // px - stupid opera
                  stop /= bounds.width;
                }
              } else {
                stop = i * step;
              }
              gradient.colorStops.push({
                color: m3[1],
                stop: stop
              });
            }
          }
          break;
      }
    }

	logger.warn(`<<< Exiting parseGradient(css=${css},bounds=${bounds})`, { "event.action": "parseGradient", "logrite.css": css, "logrite.bounds": bounds });
    return gradient;
  };

  function addScrollStops(grad) {
	logger.error(`>>> Entering addScrollStops(grad=${grad})`, { "event.action": "addScrollStops", "logrite.grad": grad });
    return function(colorStop) {
      try {
        grad.addColorStop(colorStop.stop, colorStop.color);
      }
      catch(e) {
        Util.log(['failed to add color stop: ', e, '; tried to add: ', colorStop]);
      }
    };
  }

  Generate.Gradient = function(src, bounds) {
    if(bounds.width === 0 || bounds.height === 0) {
      return;
    }

    var canvas = document.createElement('canvas'),
    ctx = canvas.getContext('2d'),
    gradient, grad;

    canvas.width = bounds.width;
    canvas.height = bounds.height;

    // TODO: add support for multi defined background gradients
    gradient = _html2canvas.Generate.parseGradient(src, bounds);

    if(gradient) {
      switch(gradient.type) {
        case 'linear':
          grad = ctx.createLinearGradient(gradient.x0, gradient.y0, gradient.x1, gradient.y1);
          gradient.colorStops.forEach(addScrollStops(grad));
          ctx.fillStyle = grad;
          ctx.fillRect(0, 0, bounds.width, bounds.height);
          break;

        case 'circle':
          grad = ctx.createRadialGradient(gradient.cx, gradient.cy, 0, gradient.cx, gradient.cy, gradient.rx);
          gradient.colorStops.forEach(addScrollStops(grad));
          ctx.fillStyle = grad;
          ctx.fillRect(0, 0, bounds.width, bounds.height);
          break;

        case 'ellipse':
          var canvasRadial = document.createElement('canvas'),
            ctxRadial = canvasRadial.getContext('2d'),
            ri = Math.max(gradient.rx, gradient.ry),
            di = ri * 2;

          canvasRadial.width = canvasRadial.height = di;

          grad = ctxRadial.createRadialGradient(gradient.rx, gradient.ry, 0, gradient.rx, gradient.ry, ri);
          gradient.colorStops.forEach(addScrollStops(grad));

          ctxRadial.fillStyle = grad;
          ctxRadial.fillRect(0, 0, di, di);

          ctx.fillStyle = gradient.colorStops[gradient.colorStops.length - 1].color;
          ctx.fillRect(0, 0, canvas.width, canvas.height);
          ctx.drawImage(canvasRadial, gradient.cx - gradient.rx, gradient.cy - gradient.ry, 2 * gradient.rx, 2 * gradient.ry);
          break;
      }
    }

    return canvas;
  };

  Generate.ListAlpha = function(number) {
	logger.error(`>>> Entering ListAlpha(number=${number})`, { "event.action": "ListAlpha", "logrite.number": number });
    var tmp = "",
    modulus;

    do {
      modulus = number % 26;
      tmp = String.fromCharCode((modulus) + 64) + tmp;
      number = number / 26;
    }while((number*26) > 26);

	logger.warn(`<<< Exiting ListAlpha(number=${number})`, { "event.action": "ListAlpha", "logrite.number": number });
    return tmp;
  };

  Generate.ListRoman = function(number) {
	logger.error(`>>> Entering ListRoman(number=${number})`, { "event.action": "ListRoman", "logrite.number": number });
    var romanArray = ["M", "CM", "D", "CD", "C", "XC", "L", "XL", "X", "IX", "V", "IV", "I"],
    decimal = [1000, 900, 500, 400, 100, 90, 50, 40, 10, 9, 5, 4, 1],
    roman = "",
    v,
    len = romanArray.length;
	logger.error(`ListRoman(number=${number}): romanArray → ${romanArray}`, { "event.action": "ListRoman", "logrite.number": number });

    if (number <= 0 || number >= 4000) {
      return number;
    }

    for (v=0; v < len; v+=1) {
      while (number >= decimal[v]) {
        number -= decimal[v];
        roman += romanArray[v];
      }
    }

	logger.warn(`<<< Exiting ListRoman(number=${number})`, { "event.action": "ListRoman", "logrite.number": number });
    return roman;
  };
})();
function h2cRenderContext(width, height) {
logger.error(`>>> Entering h2cRenderContext(width=${width},height=${height})`, { "event.action": "h2cRenderContext", "logrite.width": width, "logrite.height": height });
  var storage = [];
  return {
    storage: storage,
    width: width,
    height: height,
    clip: function() {
	logger.error('>>> Entering clip()', { "event.action": "clip" });
      storage.push({
        type: "function",
        name: "clip",
        'arguments': arguments
      });
	logger.warn('<<< Exiting clip()', { "event.action": "clip" });
    },
    translate: function() {
	logger.error('>>> Entering translate()', { "event.action": "translate" });
      storage.push({
        type: "function",
        name: "translate",
        'arguments': arguments
      });
	logger.warn('<<< Exiting translate()', { "event.action": "translate" });
    },
    fill: function() {
	logger.error('>>> Entering fill()', { "event.action": "fill" });
      storage.push({
        type: "function",
        name: "fill",
        'arguments': arguments
      });
	logger.warn('<<< Exiting fill()', { "event.action": "fill" });
    },
    save: function() {
	logger.error('>>> Entering save()', { "event.action": "save" });
      storage.push({
        type: "function",
        name: "save",
        'arguments': arguments
      });
	logger.warn('<<< Exiting save()', { "event.action": "save" });
    },
    restore: function() {
	logger.error('>>> Entering restore()', { "event.action": "restore" });
      storage.push({
        type: "function",
        name: "restore",
        'arguments': arguments
      });
	logger.warn('<<< Exiting restore()', { "event.action": "restore" });
    },
    fillRect: function () {
	logger.error('>>> Entering fillRect()', { "event.action": "fillRect" });
      storage.push({
        type: "function",
        name: "fillRect",
        'arguments': arguments
      });
	logger.warn('<<< Exiting fillRect()', { "event.action": "fillRect" });
    },
    createPattern: function() {
	logger.error('>>> Entering createPattern()', { "event.action": "createPattern" });
      storage.push({
        type: "function",
        name: "createPattern",
        'arguments': arguments
      });
	logger.warn('<<< Exiting createPattern()', { "event.action": "createPattern" });
    },
    drawShape: function() {
	logger.error('>>> Entering drawShape()', { "event.action": "drawShape" });

      var shape = [];

      storage.push({
        type: "function",
        name: "drawShape",
        'arguments': shape
      });

      return {
        moveTo: function() {
		logger.error('>>> Entering moveTo()', { "event.action": "moveTo" });
          shape.push({
            name: "moveTo",
            'arguments': arguments
          });
		logger.warn('<<< Exiting moveTo()', { "event.action": "moveTo" });
        },
        lineTo: function() {
		logger.error('>>> Entering lineTo()', { "event.action": "lineTo" });
          shape.push({
            name: "lineTo",
            'arguments': arguments
          });
		logger.warn('<<< Exiting lineTo()', { "event.action": "lineTo" });
        },
        arcTo: function() {
		logger.error('>>> Entering arcTo()', { "event.action": "arcTo" });
          shape.push({
            name: "arcTo",
            'arguments': arguments
          });
		logger.warn('<<< Exiting arcTo()', { "event.action": "arcTo" });
        },
        bezierCurveTo: function() {
		logger.error('>>> Entering bezierCurveTo()', { "event.action": "bezierCurveTo" });
          shape.push({
            name: "bezierCurveTo",
            'arguments': arguments
          });
		logger.warn('<<< Exiting bezierCurveTo()', { "event.action": "bezierCurveTo" });
        },
        quadraticCurveTo: function() {
		logger.error('>>> Entering quadraticCurveTo()', { "event.action": "quadraticCurveTo" });
          shape.push({
            name: "quadraticCurveTo",
            'arguments': arguments
          });
		logger.warn('<<< Exiting quadraticCurveTo()', { "event.action": "quadraticCurveTo" });
        }
      };

	logger.warn('<<< Exiting drawShape()', { "event.action": "drawShape" });
    },
    drawImage: function () {
	logger.error('>>> Entering drawImage()', { "event.action": "drawImage", "logrite.ctx": ctx });
      storage.push({
        type: "function",
        name: "drawImage",
        'arguments': arguments
      });
	logger.warn('<<< Exiting drawImage()', { "event.action": "drawImage", "logrite.ctx": ctx });
    },
    fillText: function () {
	logger.error('>>> Entering fillText()', { "event.action": "fillText" });
      storage.push({
        type: "function",
        name: "fillText",
        'arguments': arguments
      });
	logger.warn('<<< Exiting fillText()', { "event.action": "fillText" });
    },
    setVariable: function (variable, value) {
	logger.error(`>>> Entering setVariable(variable=${variable},value=${value})`, { "event.action": "setVariable", "logrite.variable": variable, "logrite.value": value });
      storage.push({
        type: "variable",
        name: variable,
        'arguments': value
      });
	logger.warn(`<<< Exiting setVariable(variable=${variable},value=${value})`, { "event.action": "setVariable", "logrite.variable": variable, "logrite.value": value });
      return value;
    }
  };
}
_html2canvas.Parse = function (images, options) {
logger.error(`>>> Entering Parse(images=${images},options=${options})`, { "event.action": "Parse", "logrite.images": images, "logrite.options": options });
  window.scroll(0,0);

  var element = (( options.elements === undefined ) ? document.body : options.elements[0]), // select body by default
  numDraws = 0,
  doc = element.ownerDocument,
  Util = _html2canvas.Util,
  support = Util.Support(options, doc),
  ignoreElementsRegExp = new RegExp("(" + options.ignoreElements + ")"),
  body = doc.body,
  getCSS = Util.getCSS,
  pseudoHide = "___html2canvas___pseudoelement",
  hidePseudoElements = doc.createElement('style');

  hidePseudoElements.innerHTML = '.' + pseudoHide + '-before:before { content: "" !important; display: none !important; }' +
  '.' + pseudoHide + '-after:after { content: "" !important; display: none !important; }';

  body.appendChild(hidePseudoElements);

  images = images || {};

  function documentWidth () {
	logger.error('>>> Entering documentWidth()', { "event.action": "documentWidth" });
	logger.warn('<<< Exiting documentWidth()', { "event.action": "documentWidth" });
    return Math.max(
      Math.max(doc.body.scrollWidth, doc.documentElement.scrollWidth),
      Math.max(doc.body.offsetWidth, doc.documentElement.offsetWidth),
      Math.max(doc.body.clientWidth, doc.documentElement.clientWidth)
      );
  }

  function documentHeight () {
	logger.error('>>> Entering documentHeight()', { "event.action": "documentHeight" });
	logger.warn('<<< Exiting documentHeight()', { "event.action": "documentHeight" });
    return Math.max(
      Math.max(doc.body.scrollHeight, doc.documentElement.scrollHeight),
      Math.max(doc.body.offsetHeight, doc.documentElement.offsetHeight),
      Math.max(doc.body.clientHeight, doc.documentElement.clientHeight)
      );
  }

  function getCSSInt(element, attribute) {
	logger.error(`>>> Entering getCSSInt(element=${element},attribute=${attribute})`, { "event.action": "getCSSInt", "logrite.element": element, "logrite.attribute": attribute });
    var val = parseInt(getCSS(element, attribute), 10);
	logger.error(`getCSSInt(element=${element},attribute=${attribute}): val → ${val}`, { "event.action": "getCSSInt", "logrite.element": element, "logrite.attribute": attribute });
    return (isNaN(val)) ? 0 : val; // borders in old IE are throwing 'medium' for demo.html
  }

  function renderRect (ctx, x, y, w, h, bgcolor) {
	logger.error(`>>> Entering renderRect(ctx=${ctx},x=${x},y=${y},w=${w},h=${h},bgcolor=${bgcolor})`, { "event.action": "renderRect", "logrite.ctx": ctx, "logrite.x": x, "logrite.y": y, "logrite.w": w, "logrite.h": h, "logrite.bgcolor": bgcolor });
    if (bgcolor !== "transparent"){
      ctx.setVariable("fillStyle", bgcolor);
      ctx.fillRect(x, y, w, h);
      numDraws+=1;
	logger.warn(`<<< Exiting renderRect(ctx=${ctx},x=${x},y=${y},w=${w},h=${h},bgcolor=${bgcolor})`, { "event.action": "renderRect", "logrite.ctx": ctx, "logrite.x": x, "logrite.y": y, "logrite.w": w, "logrite.h": h, "logrite.bgcolor": bgcolor });
    }
  }

  function capitalize(m, p1, p2) {
	logger.error(`>>> Entering capitalize(m=${m},p1=${p1},p2=${p2})`, { "event.action": "capitalize", "logrite.m": m, "logrite.p1": p1, "logrite.p2": p2 });
    if (m.length > 0) {
	logger.warn(`<<< Exiting capitalize(m=${m},p1=${p1},p2=${p2})`, { "event.action": "capitalize", "logrite.m": m, "logrite.p1": p1, "logrite.p2": p2 });
      return p1 + p2.toUpperCase();
    }
  }

  function textTransform (text, transform) {
	logger.error(`>>> Entering textTransform(text=${text},transform=${transform})`, { "event.action": "textTransform", "logrite.text": text, "logrite.transform": transform });
    switch(transform){
      case "lowercase":
        return text.toLowerCase();
      case "capitalize":
        return text.replace( /(^|\s|:|-|\(|\))([a-z])/g, capitalize);
      case "uppercase":
        return text.toUpperCase();
      default:
		logger.warn(`<<< Exiting textTransform(text=${text},transform=${transform})`, { "event.action": "textTransform", "logrite.text": text, "logrite.transform": transform });
        return text;
    }
  }

  function noLetterSpacing(letter_spacing) {
	logger.error(`>>> Entering noLetterSpacing(letter_spacing=${letter_spacing})`, { "event.action": "noLetterSpacing", "logrite.letter_spacing": letter_spacing });
	logger.warn(`<<< Exiting noLetterSpacing(letter_spacing=${letter_spacing})`, { "event.action": "noLetterSpacing", "logrite.letter_spacing": letter_spacing });
    return (/^(normal|none|0px)$/.test(letter_spacing));
  }

  function drawText(currentText, x, y, ctx){
	logger.error(`>>> Entering drawText(currentText=${currentText},x=${x},y=${y},ctx=${ctx})`, { "event.action": "drawText", "logrite.currentText": currentText, "logrite.x": x, "logrite.y": y, "logrite.ctx": ctx });
    if (currentText !== null && Util.trimText(currentText).length > 0) {
      ctx.fillText(currentText, x, y);
      numDraws+=1;
	logger.warn(`<<< Exiting drawText(currentText=${currentText},x=${x},y=${y},ctx=${ctx})`, { "event.action": "drawText", "logrite.currentText": currentText, "logrite.x": x, "logrite.y": y, "logrite.ctx": ctx });
    }
  }

  function setTextVariables(ctx, el, text_decoration, color) {
	logger.error(`>>> Entering setTextVariables(ctx=${ctx},el=${el},text_decoration=${text_decoration},color=${color})`, { "event.action": "setTextVariables", "logrite.ctx": ctx, "logrite.el": el, "logrite.text_decoration": text_decoration, "logrite.color": color });
    var align = false,
    bold = getCSS(el, "fontWeight"),
    family = getCSS(el, "fontFamily"),
    size = getCSS(el, "fontSize"),
    shadows = Util.parseTextShadows(getCSS(el, "textShadow"));

    switch(parseInt(bold, 10)){
      case 401:
        bold = "bold";
        break;
      case 400:
        bold = "normal";
        break;
    }

    ctx.setVariable("fillStyle", color);
    ctx.setVariable("font", [getCSS(el, "fontStyle"), getCSS(el, "fontVariant"), bold, size, family].join(" "));
    ctx.setVariable("textAlign", (align) ? "right" : "left");

    if (shadows.length) {
      // TODO: support multiple text shadows
      // apply the first text shadow
      ctx.setVariable("shadowColor", shadows[0].color);
      ctx.setVariable("shadowOffsetX", shadows[0].offsetX);
      ctx.setVariable("shadowOffsetY", shadows[0].offsetY);
      ctx.setVariable("shadowBlur", shadows[0].blur);
    }

    if (text_decoration !== "none"){
	logger.warn(`<<< Exiting setTextVariables(ctx=${ctx},el=${el},text_decoration=${text_decoration},color=${color})`, { "event.action": "setTextVariables", "logrite.ctx": ctx, "logrite.el": el, "logrite.text_decoration": text_decoration, "logrite.color": color });
      return Util.Font(family, size, doc);
    }
  }

  function renderTextDecoration(ctx, text_decoration, bounds, metrics, color) {
	logger.error(`>>> Entering renderTextDecoration(ctx=${ctx},text_decoration=${text_decoration},bounds=${bounds},metrics=${metrics},color=${color})`, { "event.action": "renderTextDecoration", "logrite.ctx": ctx, "logrite.text_decoration": text_decoration, "logrite.bounds": bounds, "logrite.metrics": metrics, "logrite.color": color });
    switch(text_decoration) {
      case "underline":
        // Draws a line at the baseline of the font
        // TODO As some browsers display the line as more than 1px if the font-size is big, need to take that into account both in position and size
        renderRect(ctx, bounds.left, Math.round(bounds.top + metrics.baseline + metrics.lineWidth), bounds.width, 1, color);
        break;
      case "overline":
        renderRect(ctx, bounds.left, Math.round(bounds.top), bounds.width, 1, color);
        break;
      case "line-through":
        // TODO try and find exact position for line-through
        renderRect(ctx, bounds.left, Math.ceil(bounds.top + metrics.middle + metrics.lineWidth), bounds.width, 1, color);
        break;
    }
	logger.warn(`<<< Exiting renderTextDecoration(ctx=${ctx},text_decoration=${text_decoration},bounds=${bounds},metrics=${metrics},color=${color})`, { "event.action": "renderTextDecoration", "logrite.ctx": ctx, "logrite.text_decoration": text_decoration, "logrite.bounds": bounds, "logrite.metrics": metrics, "logrite.color": color });
  }

  function getTextBounds(state, text, textDecoration, isLast, transform) {
	logger.error(`>>> Entering getTextBounds(state=${state},text=${text},textDecoration=${textDecoration},isLast=${isLast},transform=${transform})`, { "event.action": "getTextBounds", "logrite.state": state, "logrite.text": text, "logrite.textDecoration": textDecoration, "logrite.isLast": isLast, "logrite.transform": transform });
    var bounds;
    if (support.rangeBounds && !transform) {
      if (textDecoration !== "none" || Util.trimText(text).length !== 0) {
        bounds = textRangeBounds(text, state.node, state.textOffset);
      }
      state.textOffset += text.length;
    } else if (state.node && typeof state.node.nodeValue === "string" ){
      var newTextNode = (isLast) ? state.node.splitText(text.length) : null;
	logger.error(`getTextBounds(state=${state},text=${text},textDecoration=${textDecoration},isLast=${isLast},transform=${transform}): newTextNode → ${newTextNode}`, { "event.action": "getTextBounds", "logrite.state": state, "logrite.text": text, "logrite.textDecoration": textDecoration, "logrite.isLast": isLast, "logrite.transform": transform });
      bounds = textWrapperBounds(state.node, transform);
      state.node = newTextNode;
	logger.error(`getTextBounds(state=${state},text=${text},textDecoration=${textDecoration},isLast=${isLast},transform=${transform}): bounds → ${bounds}`, { "event.action": "getTextBounds", "logrite.state": state, "logrite.text": text, "logrite.textDecoration": textDecoration, "logrite.isLast": isLast, "logrite.transform": transform });
    }
	logger.warn(`<<< Exiting getTextBounds(state=${state},text=${text},textDecoration=${textDecoration},isLast=${isLast},transform=${transform})`, { "event.action": "getTextBounds", "logrite.state": state, "logrite.text": text, "logrite.textDecoration": textDecoration, "logrite.isLast": isLast, "logrite.transform": transform });
    return bounds;
  }

  function textRangeBounds(text, textNode, textOffset) {
	logger.error(`>>> Entering textRangeBounds(text=${text},textNode=${textNode},textOffset=${textOffset})`, { "event.action": "textRangeBounds", "logrite.text": text, "logrite.textNode": textNode, "logrite.textOffset": textOffset });
    var range = doc.createRange();
    range.setStart(textNode, textOffset);
	logger.error(`textRangeBounds(text=${text},textNode=${textNode},textOffset=${textOffset}): range → ${range}`, { "event.action": "textRangeBounds", "logrite.text": text, "logrite.textNode": textNode, "logrite.textOffset": textOffset });
    range.setEnd(textNode, textOffset + text.length);
	logger.warn(`<<< Exiting textRangeBounds(text=${text},textNode=${textNode},textOffset=${textOffset})`, { "event.action": "textRangeBounds", "logrite.text": text, "logrite.textNode": textNode, "logrite.textOffset": textOffset });
    return range.getBoundingClientRect();
  }

  function textWrapperBounds(oldTextNode, transform) {
	logger.error(`>>> Entering textWrapperBounds(oldTextNode=${oldTextNode},transform=${transform})`, { "event.action": "textWrapperBounds", "logrite.oldTextNode": oldTextNode, "logrite.transform": transform });
    var parent = oldTextNode.parentNode,
    wrapElement = doc.createElement('wrapper'),
    backupText = oldTextNode.cloneNode(true);

    wrapElement.appendChild(oldTextNode.cloneNode(true));
    parent.replaceChild(wrapElement, oldTextNode);

    var bounds = transform ? Util.OffsetBounds(wrapElement) : Util.Bounds(wrapElement);
	logger.error(`textWrapperBounds(oldTextNode=${oldTextNode},transform=${transform}): bounds → ${bounds}`, { "event.action": "textWrapperBounds", "logrite.oldTextNode": oldTextNode, "logrite.transform": transform });
    parent.replaceChild(backupText, wrapElement);
	logger.warn(`<<< Exiting textWrapperBounds(oldTextNode=${oldTextNode},transform=${transform})`, { "event.action": "textWrapperBounds", "logrite.oldTextNode": oldTextNode, "logrite.transform": transform });
    return bounds;
  }

  function renderText(el, textNode, stack) {
	logger.error(`>>> Entering renderText(el=${el},textNode=${textNode},stack=${stack})`, { "event.action": "renderText", "logrite.el": el, "logrite.textNode": textNode, "logrite.stack": stack });
    var ctx = stack.ctx,
    color = getCSS(el, "color"),
    textDecoration = getCSS(el, "textDecoration"),
    textAlign = getCSS(el, "textAlign"),
    metrics,
    textList,
    state = {
      node: textNode,
      textOffset: 0
    };

    if (Util.trimText(textNode.nodeValue).length > 0) {
      textNode.nodeValue = textTransform(textNode.nodeValue, getCSS(el, "textTransform"));
      textAlign = textAlign.replace(["-webkit-auto"],["auto"]);

      textList = (!options.letterRendering && /^(left|right|justify|auto)$/.test(textAlign) && noLetterSpacing(getCSS(el, "letterSpacing"))) ?
      textNode.nodeValue.split(/(\b| )/)
      : textNode.nodeValue.split("");

      metrics = setTextVariables(ctx, el, textDecoration, color);

      if (options.chinese) {
        textList.forEach(function(word, index) {
          if (/.*[\u4E00-\u9FA5].*$/.test(word)) {
            word = word.split("");
            word.unshift(index, 1);
            textList.splice.apply(textList, word);
          }
        });
      }

      textList.forEach(function(text, index) {
        var bounds = getTextBounds(state, text, textDecoration, (index < textList.length - 1), stack.transform.matrix);
        if (bounds) {
          drawText(text, bounds.left, bounds.bottom, ctx);
          renderTextDecoration(ctx, textDecoration, bounds, metrics, color);
		logger.warn(`<<< Exiting renderText(el=${el},textNode=${textNode},stack=${stack})`, { "event.action": "renderText", "logrite.el": el, "logrite.textNode": textNode, "logrite.stack": stack });
        }
      });
    }
  }

  function listPosition (element, val) {
	logger.error(`>>> Entering listPosition(element=${element},val=${val})`, { "event.action": "listPosition", "logrite.element": element, "logrite.val": val });
    var boundElement = doc.createElement( "boundelement" ),
    originalType,
    bounds;
	logger.error(`listPosition(element=${element},val=${val}): boundElement → ${boundElement}`, { "event.action": "listPosition", "logrite.element": element, "logrite.val": val });

    boundElement.style.display = "inline";

    originalType = element.style.listStyleType;
    element.style.listStyleType = "none";

    boundElement.appendChild(doc.createTextNode(val));

    element.insertBefore(boundElement, element.firstChild);

    bounds = Util.Bounds(boundElement);
    element.removeChild(boundElement);
    element.style.listStyleType = originalType;
	logger.warn(`<<< Exiting listPosition(element=${element},val=${val})`, { "event.action": "listPosition", "logrite.element": element, "logrite.val": val });
    return bounds;
  }

  function elementIndex(el) {
	logger.error(`>>> Entering elementIndex(el=${el})`, { "event.action": "elementIndex", "logrite.el": el });
    var i = -1,
    count = 1,
    childs = el.parentNode.childNodes;

    if (el.parentNode) {
      while(childs[++i] !== el) {
        if (childs[i].nodeType === 1) {
          count++;
        }
      }
      return count;
    } else {
	logger.warn(`<<< Exiting elementIndex(el=${el})`, { "event.action": "elementIndex", "logrite.el": el });
      return -1;
    }
  }

  function listItemText(element, type) {
	logger.error(`>>> Entering listItemText(element=${element},type=${type})`, { "event.action": "listItemText", "logrite.element": element, "logrite.type": type });
    var currentIndex = elementIndex(element), text;
    switch(type){
      case "decimal":
        text = currentIndex;
		logger.error(`listItemText(element=${element},type=${type}): currentIndex → ${currentIndex}`, { "event.action": "listItemText", "logrite.element": element, "logrite.type": type });
        break;
      case "decimal-leading-zero":
        text = (currentIndex.toString().length === 1) ? currentIndex = "0" + currentIndex.toString() : currentIndex.toString();
        break;
      case "upper-roman":
        text = _html2canvas.Generate.ListRoman( currentIndex );
        break;
      case "lower-roman":
        text = _html2canvas.Generate.ListRoman( currentIndex ).toLowerCase();
        break;
      case "lower-alpha":
        text = _html2canvas.Generate.ListAlpha( currentIndex ).toLowerCase();
        break;
      case "upper-alpha":
        text = _html2canvas.Generate.ListAlpha( currentIndex );
        break;
    }

	logger.warn(`<<< Exiting listItemText(element=${element},type=${type})`, { "event.action": "listItemText", "logrite.element": element, "logrite.type": type });
    return text + ". ";
  }

  function renderListItem(element, stack, elBounds) {
	logger.error(`>>> Entering renderListItem(element=${element},stack=${stack},elBounds=${elBounds})`, { "event.action": "renderListItem", "logrite.element": element, "logrite.stack": stack, "logrite.elBounds": elBounds });
    var x,
    text,
    ctx = stack.ctx,
    type = getCSS(element, "listStyleType"),
    listBounds;

    if (/^(decimal|decimal-leading-zero|upper-alpha|upper-latin|upper-roman|lower-alpha|lower-greek|lower-latin|lower-roman)$/i.test(type)) {
      text = listItemText(element, type);
      listBounds = listPosition(element, text);
      setTextVariables(ctx, element, "none", getCSS(element, "color"));

      if (getCSS(element, "listStylePosition") === "inside") {
        ctx.setVariable("textAlign", "left");
        x = elBounds.left;
      } else {
        return;
      }

      drawText(text, x, listBounds.bottom, ctx);
    }
	logger.warn(`<<< Exiting renderListItem(element=${element},stack=${stack},elBounds=${elBounds})`, { "event.action": "renderListItem", "logrite.element": element, "logrite.stack": stack, "logrite.elBounds": elBounds });
  }

  function loadImage (src){
	logger.error(`>>> Entering loadImage(src=${src})`, { "event.action": "loadImage", "logrite.src": src });
    var img = images[src];
	logger.warn(`<<< Exiting loadImage(src=${src})`, { "event.action": "loadImage", "logrite.src": src });
	logger.debug(`loadImage(src=${src}): img.src → ${img.src}`, { "event.action": "loadImage", "logrite.src": src });
	logger.debug(`loadImage(src=${src}): img.crossOrigin → ${img.crossOrigin}`, { "event.action": "loadImage", "logrite.src": src });
	logger.error(`loadImage(src=${src}): img → ${img}`, { "event.action": "loadImage", "logrite.src": src });
    return (img && img.succeeded === true) ? img.img : false;
  }

  function clipBounds(src, dst){
	logger.error(`>>> Entering clipBounds(src=${src},dst=${dst})`, { "event.action": "clipBounds", "logrite.src": src, "logrite.dst": dst });
    var x = Math.max(src.left, dst.left),
    y = Math.max(src.top, dst.top),
    x2 = Math.min((src.left + src.width), (dst.left + dst.width)),
    y2 = Math.min((src.top + src.height), (dst.top + dst.height));
	logger.error(`clipBounds(src=${src},dst=${dst}): x → ${x}`, { "event.action": "clipBounds", "logrite.src": src, "logrite.dst": dst });

	logger.warn(`<<< Exiting clipBounds(src=${src},dst=${dst})`, { "event.action": "clipBounds", "logrite.src": src, "logrite.dst": dst });
    return {
      left:x,
      top:y,
      width:x2-x,
      height:y2-y
    };
  }

  function setZ(element, stack, parentStack){
	logger.error(`>>> Entering setZ(element=${element},stack=${stack},parentStack=${parentStack})`, { "event.action": "setZ", "logrite.element": element, "logrite.stack": stack, "logrite.parentStack": parentStack });
    var newContext,
    isPositioned = stack.cssPosition !== 'static',
    zIndex = isPositioned ? getCSS(element, 'zIndex') : 'auto',
    opacity = getCSS(element, 'opacity'),
    isFloated = getCSS(element, 'cssFloat') !== 'none';
	logger.debug(`setZ(element=${element},stack=${stack},parentStack=${parentStack}): newContext.opacity → ${newContext.opacity}`, { "event.action": "setZ", "logrite.element": element, "logrite.stack": stack, "logrite.parentStack": parentStack });
	logger.debug(`setZ(element=${element},stack=${stack},parentStack=${parentStack}): newContext.isPositioned → ${newContext.isPositioned}`, { "event.action": "setZ", "logrite.element": element, "logrite.stack": stack, "logrite.parentStack": parentStack });
	logger.debug(`setZ(element=${element},stack=${stack},parentStack=${parentStack}): newContext.isFloated → ${newContext.isFloated}`, { "event.action": "setZ", "logrite.element": element, "logrite.stack": stack, "logrite.parentStack": parentStack });

    // https://developer.mozilla.org/en-US/docs/Web/Guide/CSS/Understanding_z_index/The_stacking_context
    // When a new stacking context should be created:
    // the root element (HTML),
    // positioned (absolutely or relatively) with a z-index value other than "auto",
    // elements with an opacity value less than 1. (See the specification for opacity),
    // on mobile WebKit and Chrome 22+, position: fixed always creates a new stacking context, even when z-index is "auto" (See this post)

    stack.zIndex = newContext = h2czContext(zIndex);
    newContext.isPositioned = isPositioned;
    newContext.isFloated = isFloated;
    newContext.opacity = opacity;
    newContext.ownStacking = (zIndex !== 'auto' || opacity < 1);

    if (parentStack) {
      parentStack.zIndex.children.push(stack);
	logger.warn(`<<< Exiting setZ(element=${element},stack=${stack},parentStack=${parentStack})`, { "event.action": "setZ", "logrite.element": element, "logrite.stack": stack, "logrite.parentStack": parentStack });
    }
  }

  function renderImage(ctx, element, image, bounds, borders) {
logger.error(`>>> Entering renderImage(ctx=${ctx},element=${element},image=${image},bounds=${bounds},borders=${borders})`, { "event.action": "renderImage", "logrite.ctx": ctx, "logrite.element": element, "logrite.image": image, "logrite.bounds": bounds, "logrite.borders": borders });

    var paddingLeft = getCSSInt(element, 'paddingLeft'),
    paddingTop = getCSSInt(element, 'paddingTop'),
    paddingRight = getCSSInt(element, 'paddingRight'),
    paddingBottom = getCSSInt(element, 'paddingBottom');
	logger.error(`renderImage(ctx=${ctx},element=${element},image=${image},bounds=${bounds},borders=${borders}): paddingLeft → ${paddingLeft}`, { "event.action": "renderImage", "logrite.ctx": ctx, "logrite.element": element, "logrite.image": image, "logrite.bounds": bounds, "logrite.borders": borders });

	logger.warn(`<<< Exiting renderImage(ctx=${ctx},element=${element},image=${image},bounds=${bounds},borders=${borders})`, { "event.action": "renderImage", "logrite.ctx": ctx, "logrite.element": element, "logrite.image": image, "logrite.bounds": bounds, "logrite.borders": borders });
    drawImage(
      ctx,
      image,
      0, //sx
      0, //sy
      image.width, //sw
      image.height, //sh
      bounds.left + paddingLeft + borders[3].width, //dx
      bounds.top + paddingTop + borders[0].width, // dy
      bounds.width - (borders[1].width + borders[3].width + paddingLeft + paddingRight), //dw
      bounds.height - (borders[0].width + borders[2].width + paddingTop + paddingBottom) //dh
      );
  }

  function getBorderData(element) {
	logger.error(`>>> Entering getBorderData(element=${element})`, { "event.action": "getBorderData", "logrite.element": element });
    return ["Top", "Right", "Bottom", "Left"].map(function(side) {
      return {
        width: getCSSInt(element, 'border' + side + 'Width'),
        color: getCSS(element, 'border' + side + 'Color')
      };
    });
	logger.warn(`<<< Exiting getBorderData(element=${element})`, { "event.action": "getBorderData", "logrite.element": element });
  }

  function getBorderRadiusData(element) {
	logger.error(`>>> Entering getBorderRadiusData(element=${element})`, { "event.action": "getBorderRadiusData", "logrite.element": element });
    return ["TopLeft", "TopRight", "BottomRight", "BottomLeft"].map(function(side) {
	logger.warn(`<<< Exiting getBorderRadiusData(element=${element})`, { "event.action": "getBorderRadiusData", "logrite.element": element });
      return getCSS(element, 'border' + side + 'Radius');
    });
  }

  var getCurvePoints = (function(kappa) {

    return function(x, y, r1, r2) {
      var ox = (r1) * kappa, // control point offset horizontal
      oy = (r2) * kappa, // control point offset vertical
      xm = x + r1, // x-middle
      ym = y + r2; // y-middle
      return {
        topLeft: bezierCurve({
          x:x,
          y:ym
        }, {
          x:x,
          y:ym - oy
        }, {
          x:xm - ox,
          y:y
        }, {
          x:xm,
          y:y
        }),
        topRight: bezierCurve({
          x:x,
          y:y
        }, {
          x:x + ox,
          y:y
        }, {
          x:xm,
          y:ym - oy
        }, {
          x:xm,
          y:ym
        }),
        bottomRight: bezierCurve({
          x:xm,
          y:y
        }, {
          x:xm,
          y:y + oy
        }, {
          x:x + ox,
          y:ym
        }, {
          x:x,
          y:ym
        }),
        bottomLeft: bezierCurve({
          x:xm,
          y:ym
        }, {
          x:xm - ox,
          y:ym
        }, {
          x:x,
          y:y + oy
        }, {
          x:x,
          y:y
        })
      };
    };
  })(4 * ((Math.sqrt(2) - 1) / 3));

  function bezierCurve(start, startControl, endControl, end) {
logger.error(`>>> Entering bezierCurve(start=${start},startControl=${startControl},endControl=${endControl},end=${end})`, { "event.action": "bezierCurve", "logrite.start": start, "logrite.startControl": startControl, "logrite.endControl": endControl, "logrite.end": end });

    var lerp = function (a, b, t) {
	logger.error(`>>> Entering lerp(a=${a},b=${b},t=${t})`, { "event.action": "lerp", "logrite.a": a, "logrite.b": b, "logrite.t": t });
	logger.warn(`<<< Exiting lerp(a=${a},b=${b},t=${t})`, { "event.action": "lerp", "logrite.a": a, "logrite.b": b, "logrite.t": t });
      return {
        x:a.x + (b.x - a.x) * t,
        y:a.y + (b.y - a.y) * t
      };
    };

	logger.warn(`<<< Exiting bezierCurve(start=${start},startControl=${startControl},endControl=${endControl},end=${end})`, { "event.action": "bezierCurve", "logrite.start": start, "logrite.startControl": startControl, "logrite.endControl": endControl, "logrite.end": end });
    return {
      start: start,
      startControl: startControl,
      endControl: endControl,
      end: end,
      subdivide: function(t) {
		logger.error(`>>> Entering subdivide(t=${t})`, { "event.action": "subdivide", "logrite.t": t });
        var ab = lerp(start, startControl, t),
        bc = lerp(startControl, endControl, t),
        cd = lerp(endControl, end, t),
        abbc = lerp(ab, bc, t),
        bccd = lerp(bc, cd, t),
        dest = lerp(abbc, bccd, t);
		logger.error(`subdivide(t=${t}): ab → ${ab}`, { "event.action": "subdivide", "logrite.t": t });
		logger.warn(`<<< Exiting subdivide(t=${t})`, { "event.action": "subdivide", "logrite.t": t });
        return [bezierCurve(start, ab, abbc, dest), bezierCurve(dest, bccd, cd, end)];
      },
      curveTo: function(borderArgs) {
		logger.error(`>>> Entering curveTo(borderArgs=${borderArgs})`, { "event.action": "curveTo", "logrite.borderArgs": borderArgs });
        borderArgs.push(["bezierCurve", startControl.x, startControl.y, endControl.x, endControl.y, end.x, end.y]);
		logger.warn(`<<< Exiting curveTo(borderArgs=${borderArgs})`, { "event.action": "curveTo", "logrite.borderArgs": borderArgs });
      },
      curveToReversed: function(borderArgs) {
		logger.error(`>>> Entering curveToReversed(borderArgs=${borderArgs})`, { "event.action": "curveToReversed", "logrite.borderArgs": borderArgs });
        borderArgs.push(["bezierCurve", endControl.x, endControl.y, startControl.x, startControl.y, start.x, start.y]);
		logger.warn(`<<< Exiting curveToReversed(borderArgs=${borderArgs})`, { "event.action": "curveToReversed", "logrite.borderArgs": borderArgs });
      }
    };
  }

  function parseCorner(borderArgs, radius1, radius2, corner1, corner2, x, y) {
	logger.error(`>>> Entering parseCorner(borderArgs=${borderArgs},radius1=${radius1},radius2=${radius2},corner1=${corner1},corner2=${corner2},x=${x},y=${y})`, { "event.action": "parseCorner", "logrite.borderArgs": borderArgs, "logrite.radius1": radius1, "logrite.radius2": radius2, "logrite.corner1": corner1, "logrite.corner2": corner2, "logrite.x": x, "logrite.y": y });
    if (radius1[0] > 0 || radius1[1] > 0) {
      borderArgs.push(["line", corner1[0].start.x, corner1[0].start.y]);
      corner1[0].curveTo(borderArgs);
      corner1[1].curveTo(borderArgs);
    } else {
      borderArgs.push(["line", x, y]);
    }

    if (radius2[0] > 0 || radius2[1] > 0) {
      borderArgs.push(["line", corner2[0].start.x, corner2[0].start.y]);
	logger.warn(`<<< Exiting parseCorner(borderArgs=${borderArgs},radius1=${radius1},radius2=${radius2},corner1=${corner1},corner2=${corner2},x=${x},y=${y})`, { "event.action": "parseCorner", "logrite.borderArgs": borderArgs, "logrite.radius1": radius1, "logrite.radius2": radius2, "logrite.corner1": corner1, "logrite.corner2": corner2, "logrite.x": x, "logrite.y": y });
    }
  }

  function drawSide(borderData, radius1, radius2, outer1, inner1, outer2, inner2) {
	logger.error(`>>> Entering drawSide(borderData=${borderData},radius1=${radius1},radius2=${radius2},outer1=${outer1},inner1=${inner1},outer2=${outer2},inner2=${inner2})`, { "event.action": "drawSide", "logrite.borderData": borderData, "logrite.radius1": radius1, "logrite.radius2": radius2, "logrite.outer1": outer1, "logrite.inner1": inner1, "logrite.outer2": outer2, "logrite.inner2": inner2 });
    var borderArgs = [];

    if (radius1[0] > 0 || radius1[1] > 0) {
      borderArgs.push(["line", outer1[1].start.x, outer1[1].start.y]);
      outer1[1].curveTo(borderArgs);
    } else {
      borderArgs.push([ "line", borderData.c1[0], borderData.c1[1]]);
    }

    if (radius2[0] > 0 || radius2[1] > 0) {
      borderArgs.push(["line", outer2[0].start.x, outer2[0].start.y]);
      outer2[0].curveTo(borderArgs);
      borderArgs.push(["line", inner2[0].end.x, inner2[0].end.y]);
      inner2[0].curveToReversed(borderArgs);
    } else {
      borderArgs.push([ "line", borderData.c2[0], borderData.c2[1]]);
      borderArgs.push([ "line", borderData.c3[0], borderData.c3[1]]);
    }

    if (radius1[0] > 0 || radius1[1] > 0) {
      borderArgs.push(["line", inner1[1].end.x, inner1[1].end.y]);
      inner1[1].curveToReversed(borderArgs);
    } else {
      borderArgs.push([ "line", borderData.c4[0], borderData.c4[1]]);
    }

	logger.warn(`<<< Exiting drawSide(borderData=${borderData},radius1=${radius1},radius2=${radius2},outer1=${outer1},inner1=${inner1},outer2=${outer2},inner2=${inner2})`, { "event.action": "drawSide", "logrite.borderData": borderData, "logrite.radius1": radius1, "logrite.radius2": radius2, "logrite.outer1": outer1, "logrite.inner1": inner1, "logrite.outer2": outer2, "logrite.inner2": inner2 });
    return borderArgs;
  }

  function calculateCurvePoints(bounds, borderRadius, borders) {
logger.error(`>>> Entering calculateCurvePoints(bounds=${bounds},borderRadius=${borderRadius},borders=${borders})`, { "event.action": "calculateCurvePoints", "logrite.bounds": bounds, "logrite.borderRadius": borderRadius, "logrite.borders": borders });

    var x = bounds.left,
    y = bounds.top,
    width = bounds.width,
    height = bounds.height,

    tlh = borderRadius[0][0],
    tlv = borderRadius[0][1],
    trh = borderRadius[1][0],
    trv = borderRadius[1][1],
    brh = borderRadius[2][0],
    brv = borderRadius[2][1],
    blh = borderRadius[3][0],
    blv = borderRadius[3][1],

    topWidth = width - trh,
    rightHeight = height - brv,
    bottomWidth = width - brh,
    leftHeight = height - blv;

	logger.warn(`<<< Exiting calculateCurvePoints(bounds=${bounds},borderRadius=${borderRadius},borders=${borders})`, { "event.action": "calculateCurvePoints", "logrite.bounds": bounds, "logrite.borderRadius": borderRadius, "logrite.borders": borders });
    return {
      topLeftOuter: getCurvePoints(
        x,
        y,
        tlh,
        tlv
        ).topLeft.subdivide(0.5),

      topLeftInner: getCurvePoints(
        x + borders[3].width,
        y + borders[0].width,
        Math.max(0, tlh - borders[3].width),
        Math.max(0, tlv - borders[0].width)
        ).topLeft.subdivide(0.5),

      topRightOuter: getCurvePoints(
        x + topWidth,
        y,
        trh,
        trv
        ).topRight.subdivide(0.5),

      topRightInner: getCurvePoints(
        x + Math.min(topWidth, width + borders[3].width),
        y + borders[0].width,
        (topWidth > width + borders[3].width) ? 0 :trh - borders[3].width,
        trv - borders[0].width
        ).topRight.subdivide(0.5),

      bottomRightOuter: getCurvePoints(
        x + bottomWidth,
        y + rightHeight,
        brh,
        brv
        ).bottomRight.subdivide(0.5),

      bottomRightInner: getCurvePoints(
        x + Math.min(bottomWidth, width + borders[3].width),
        y + Math.min(rightHeight, height + borders[0].width),
        Math.max(0, brh - borders[1].width),
        Math.max(0, brv - borders[2].width)
        ).bottomRight.subdivide(0.5),

      bottomLeftOuter: getCurvePoints(
        x,
        y + leftHeight,
        blh,
        blv
        ).bottomLeft.subdivide(0.5),

      bottomLeftInner: getCurvePoints(
        x + borders[3].width,
        y + leftHeight,
        Math.max(0, blh - borders[3].width),
        Math.max(0, blv - borders[2].width)
        ).bottomLeft.subdivide(0.5)
    };
  }

  function getBorderClip(element, borderPoints, borders, radius, bounds) {
	logger.error(`>>> Entering getBorderClip(element=${element},borderPoints=${borderPoints},borders=${borders},radius=${radius},bounds=${bounds})`, { "event.action": "getBorderClip", "logrite.element": element, "logrite.borderPoints": borderPoints, "logrite.borders": borders, "logrite.radius": radius, "logrite.bounds": bounds });
    var backgroundClip = getCSS(element, 'backgroundClip'),
    borderArgs = [];

    switch(backgroundClip) {
      case "content-box":
      case "padding-box":
        parseCorner(borderArgs, radius[0], radius[1], borderPoints.topLeftInner, borderPoints.topRightInner, bounds.left + borders[3].width, bounds.top + borders[0].width);
		logger.error(`getBorderClip(element=${element},borderPoints=${borderPoints},borders=${borders},radius=${radius},bounds=${bounds}): backgroundClip → ${backgroundClip}`, { "event.action": "getBorderClip", "logrite.element": element, "logrite.borderPoints": borderPoints, "logrite.borders": borders, "logrite.radius": radius, "logrite.bounds": bounds });
        parseCorner(borderArgs, radius[1], radius[2], borderPoints.topRightInner, borderPoints.bottomRightInner, bounds.left + bounds.width - borders[1].width, bounds.top + borders[0].width);
        parseCorner(borderArgs, radius[2], radius[3], borderPoints.bottomRightInner, borderPoints.bottomLeftInner, bounds.left + bounds.width - borders[1].width, bounds.top + bounds.height - borders[2].width);
        parseCorner(borderArgs, radius[3], radius[0], borderPoints.bottomLeftInner, borderPoints.topLeftInner, bounds.left + borders[3].width, bounds.top + bounds.height - borders[2].width);
        break;

      default:
        parseCorner(borderArgs, radius[0], radius[1], borderPoints.topLeftOuter, borderPoints.topRightOuter, bounds.left, bounds.top);
        parseCorner(borderArgs, radius[1], radius[2], borderPoints.topRightOuter, borderPoints.bottomRightOuter, bounds.left + bounds.width, bounds.top);
        parseCorner(borderArgs, radius[2], radius[3], borderPoints.bottomRightOuter, borderPoints.bottomLeftOuter, bounds.left + bounds.width, bounds.top + bounds.height);
        parseCorner(borderArgs, radius[3], radius[0], borderPoints.bottomLeftOuter, borderPoints.topLeftOuter, bounds.left, bounds.top + bounds.height);
        break;
    }

	logger.warn(`<<< Exiting getBorderClip(element=${element},borderPoints=${borderPoints},borders=${borders},radius=${radius},bounds=${bounds})`, { "event.action": "getBorderClip", "logrite.element": element, "logrite.borderPoints": borderPoints, "logrite.borders": borders, "logrite.radius": radius, "logrite.bounds": bounds });
    return borderArgs;
  }

  function parseBorders(element, bounds, borders){
	logger.error(`>>> Entering parseBorders(element=${element},bounds=${bounds},borders=${borders})`, { "event.action": "parseBorders", "logrite.element": element, "logrite.bounds": bounds, "logrite.borders": borders });
    var x = bounds.left,
    y = bounds.top,
    width = bounds.width,
    height = bounds.height,
    borderSide,
    bx,
    by,
    bw,
    bh,
    borderArgs,
    // http://www.w3.org/TR/css3-background/#the-border-radius
    borderRadius = getBorderRadiusData(element),
    borderPoints = calculateCurvePoints(bounds, borderRadius, borders),
    borderData = {
      clip: getBorderClip(element, borderPoints, borders, borderRadius, bounds),
      borders: []
    };

    for (borderSide = 0; borderSide < 4; borderSide++) {

      if (borders[borderSide].width > 0) {
        bx = x;
        by = y;
        bw = width;
        bh = height - (borders[2].width);

        switch(borderSide) {
          case 0:
            // top border
            bh = borders[0].width;

            borderArgs = drawSide({
              c1: [bx, by],
              c2: [bx + bw, by],
              c3: [bx + bw - borders[1].width, by + bh],
              c4: [bx + borders[3].width, by + bh]
            }, borderRadius[0], borderRadius[1],
            borderPoints.topLeftOuter, borderPoints.topLeftInner, borderPoints.topRightOuter, borderPoints.topRightInner);
            break;
          case 1:
            // right border
            bx = x + width - (borders[1].width);
            bw = borders[1].width;

            borderArgs = drawSide({
              c1: [bx + bw, by],
              c2: [bx + bw, by + bh + borders[2].width],
              c3: [bx, by + bh],
              c4: [bx, by + borders[0].width]
            }, borderRadius[1], borderRadius[2],
            borderPoints.topRightOuter, borderPoints.topRightInner, borderPoints.bottomRightOuter, borderPoints.bottomRightInner);
            break;
          case 2:
            // bottom border
            by = (by + height) - (borders[2].width);
            bh = borders[2].width;

            borderArgs = drawSide({
              c1: [bx + bw, by + bh],
              c2: [bx, by + bh],
              c3: [bx + borders[3].width, by],
              c4: [bx + bw - borders[3].width, by]
            }, borderRadius[2], borderRadius[3],
            borderPoints.bottomRightOuter, borderPoints.bottomRightInner, borderPoints.bottomLeftOuter, borderPoints.bottomLeftInner);
            break;
          case 3:
            // left border
            bw = borders[3].width;

            borderArgs = drawSide({
              c1: [bx, by + bh + borders[2].width],
              c2: [bx, by],
              c3: [bx + bw, by + borders[0].width],
              c4: [bx + bw, by + bh]
            }, borderRadius[3], borderRadius[0],
            borderPoints.bottomLeftOuter, borderPoints.bottomLeftInner, borderPoints.topLeftOuter, borderPoints.topLeftInner);
            break;
        }

        borderData.borders.push({
          args: borderArgs,
          color: borders[borderSide].color
        });

      }
    }

	logger.warn(`<<< Exiting parseBorders(element=${element},bounds=${bounds},borders=${borders})`, { "event.action": "parseBorders", "logrite.element": element, "logrite.bounds": bounds, "logrite.borders": borders });
    return borderData;
  }

  function createShape(ctx, args) {
	logger.error(`>>> Entering createShape(ctx=${ctx},args=${args})`, { "event.action": "createShape", "logrite.ctx": ctx, "logrite.args": args });
    var shape = ctx.drawShape();
    args.forEach(function(border, index) {
      shape[(index === 0) ? "moveTo" : border[0] + "To" ].apply(null, border.slice(1));
    });
    return shape;
  }

  function renderBorders(ctx, borderArgs, color) {
	logger.error(`>>> Entering renderBorders(ctx=${ctx},borderArgs=${borderArgs},color=${color})`, { "event.action": "renderBorders", "logrite.ctx": ctx, "logrite.borderArgs": borderArgs, "logrite.color": color });
    if (color !== "transparent") {
      ctx.setVariable( "fillStyle", color);
      createShape(ctx, borderArgs);
      ctx.fill();
      numDraws+=1;
	logger.warn(`<<< Exiting renderBorders(ctx=${ctx},borderArgs=${borderArgs},color=${color})`, { "event.action": "renderBorders", "logrite.ctx": ctx, "logrite.borderArgs": borderArgs, "logrite.color": color });
    }
  }

  function renderFormValue (el, bounds, stack){
logger.error(`>>> Entering renderFormValue(el=${el},bounds=${bounds},stack=${stack})`, { "event.action": "renderFormValue", "logrite.el": el, "logrite.bounds": bounds, "logrite.stack": stack });

    var valueWrap = doc.createElement('valuewrap'),
    cssPropertyArray = ['lineHeight','textAlign','fontFamily','color','fontSize','paddingLeft','paddingTop','width','height','border','borderLeftWidth','borderTopWidth'],
    textValue,
    textNode;
	logger.error(`renderFormValue(el=${el},bounds=${bounds},stack=${stack}): valueWrap → ${valueWrap}`, { "event.action": "renderFormValue", "logrite.el": el, "logrite.bounds": bounds, "logrite.stack": stack });

    cssPropertyArray.forEach(function(property) {
      try {
        valueWrap.style[property] = getCSS(el, property);
      } catch(e) {
        // Older IE has issues with "border"
        Util.log("html2canvas: Parse: Exception caught in renderFormValue: " + e.message);
      }
    });

    valueWrap.style.borderColor = "black";
    valueWrap.style.borderStyle = "solid";
    valueWrap.style.display = "block";
    valueWrap.style.position = "absolute";

    if (/^(submit|reset|button|text|password)$/.test(el.type) || el.nodeName === "SELECT"){
      valueWrap.style.lineHeight = getCSS(el, "height");
    }

    valueWrap.style.top = bounds.top + "px";
    valueWrap.style.left = bounds.left + "px";

    textValue = (el.nodeName === "SELECT") ? (el.options[el.selectedIndex] || 0).text : el.value;
    if(!textValue) {
      textValue = el.placeholder;
    }

    textNode = doc.createTextNode(textValue);

    valueWrap.appendChild(textNode);
    body.appendChild(valueWrap);

    renderText(el, textNode, stack);
    body.removeChild(valueWrap);
	logger.warn(`<<< Exiting renderFormValue(el=${el},bounds=${bounds},stack=${stack})`, { "event.action": "renderFormValue", "logrite.el": el, "logrite.bounds": bounds, "logrite.stack": stack });
  }

  function drawImage (ctx) {
	logger.error(`>>> Entering drawImage(ctx=${ctx})`, { "event.action": "drawImage", "logrite.ctx": ctx });
    ctx.drawImage.apply(ctx, Array.prototype.slice.call(arguments, 1));
    numDraws+=1;
	logger.warn(`<<< Exiting drawImage(ctx=${ctx})`, { "event.action": "drawImage", "logrite.ctx": ctx });
  }

  function getPseudoElement(el, which) {
	logger.error(`>>> Entering getPseudoElement(el=${el},which=${which})`, { "event.action": "getPseudoElement", "logrite.el": el, "logrite.which": which });
    var elStyle = window.getComputedStyle(el, which);
    if(!elStyle || !elStyle.content || elStyle.content === "none" || elStyle.content === "-moz-alt-content" || elStyle.display === "none") {
	logger.error(`getPseudoElement(el=${el},which=${which}): elStyle → ${elStyle}`, { "event.action": "getPseudoElement", "logrite.el": el, "logrite.which": which });
      return;
    }
    var content = elStyle.content + '',
    first = content.substr( 0, 1 );
    //strips quotes
    if(first === content.substr( content.length - 1 ) && first.match(/'|"/)) {
      content = content.substr( 1, content.length - 2 );
	logger.error(`getPseudoElement(el=${el},which=${which}): content → ${content}`, { "event.action": "getPseudoElement", "logrite.el": el, "logrite.which": which });
    }

    var isImage = content.substr( 0, 3 ) === 'url',
    elps = document.createElement( isImage ? 'img' : 'span' );
	logger.error(`Parse(images=${images},options=${options}): isImage → ${isImage}`, { "event.action": "Parse", "logrite.images": images, "logrite.options": options });

    elps.className = pseudoHide + "-before " + pseudoHide + "-after";

    Object.keys(elStyle).filter(indexedProperty).forEach(function(prop) {
      // Prevent assigning of read only CSS Rules, ex. length, parentRule
      try {
        elps.style[prop] = elStyle[prop];
      } catch (e) {
        Util.log(['Tried to assign readonly property ', prop, 'Error:', e]);
      }
    });

    if(isImage) {
      elps.src = Util.parseBackgroundImage(content)[0].args[0];
    } else {
      elps.innerHTML = content;
    }
    return elps;
  }

  function indexedProperty(property) {
	logger.error(`>>> Entering indexedProperty(property=${property})`, { "event.action": "indexedProperty", "logrite.property": property });
	logger.warn(`<<< Exiting indexedProperty(property=${property})`, { "event.action": "indexedProperty", "logrite.property": property });
    return (isNaN(window.parseInt(property, 10)));
  }

  function injectPseudoElements(el, stack) {
	logger.error(`>>> Entering injectPseudoElements(el=${el},stack=${stack})`, { "event.action": "injectPseudoElements", "logrite.el": el, "logrite.stack": stack });
    var before = getPseudoElement(el, ':before'),
    after = getPseudoElement(el, ':after');
	logger.error(`injectPseudoElements(el=${el},stack=${stack}): before → ${before}`, { "event.action": "injectPseudoElements", "logrite.el": el, "logrite.stack": stack });
    if(!before && !after) {
      return;
    }

    if(before) {
      el.className += " " + pseudoHide + "-before";
      el.parentNode.insertBefore(before, el);
      parseElement(before, stack, true);
      el.parentNode.removeChild(before);
      el.className = el.className.replace(pseudoHide + "-before", "").trim();
    }

    if (after) {
      el.className += " " + pseudoHide + "-after";
      el.appendChild(after);
      parseElement(after, stack, true);
      el.removeChild(after);
      el.className = el.className.replace(pseudoHide + "-after", "").trim();
	logger.warn(`<<< Exiting injectPseudoElements(el=${el},stack=${stack})`, { "event.action": "injectPseudoElements", "logrite.el": el, "logrite.stack": stack });
    }

  }

  function renderBackgroundRepeat(ctx, image, backgroundPosition, bounds) {
	logger.error(`>>> Entering renderBackgroundRepeat(ctx=${ctx},image=${image},backgroundPosition=${backgroundPosition},bounds=${bounds})`, { "event.action": "renderBackgroundRepeat", "logrite.ctx": ctx, "logrite.image": image, "logrite.backgroundPosition": backgroundPosition, "logrite.bounds": bounds });
    var offsetX = Math.round(bounds.left + backgroundPosition.left),
    offsetY = Math.round(bounds.top + backgroundPosition.top);
	logger.error(`renderBackgroundRepeat(ctx=${ctx},image=${image},backgroundPosition=${backgroundPosition},bounds=${bounds}): offsetX → ${offsetX}`, { "event.action": "renderBackgroundRepeat", "logrite.ctx": ctx, "logrite.image": image, "logrite.backgroundPosition": backgroundPosition, "logrite.bounds": bounds });

    ctx.createPattern(image);
    ctx.translate(offsetX, offsetY);
    ctx.fill();
    ctx.translate(-offsetX, -offsetY);
	logger.warn(`<<< Exiting renderBackgroundRepeat(ctx=${ctx},image=${image},backgroundPosition=${backgroundPosition},bounds=${bounds})`, { "event.action": "renderBackgroundRepeat", "logrite.ctx": ctx, "logrite.image": image, "logrite.backgroundPosition": backgroundPosition, "logrite.bounds": bounds });
  }

  function backgroundRepeatShape(ctx, image, backgroundPosition, bounds, left, top, width, height) {
	logger.error(`>>> Entering backgroundRepeatShape(ctx=${ctx},image=${image},backgroundPosition=${backgroundPosition},bounds=${bounds},left=${left},top=${top},width=${width},height=${height})`, { "event.action": "backgroundRepeatShape", "logrite.ctx": ctx, "logrite.image": image, "logrite.backgroundPosition": backgroundPosition, "logrite.bounds": bounds, "logrite.left": left, "logrite.top": top, "logrite.width": width, "logrite.height": height });
    var args = [];
    args.push(["line", Math.round(left), Math.round(top)]);
    args.push(["line", Math.round(left + width), Math.round(top)]);
    args.push(["line", Math.round(left + width), Math.round(height + top)]);
    args.push(["line", Math.round(left), Math.round(height + top)]);
    createShape(ctx, args);
    ctx.save();
    ctx.clip();
    renderBackgroundRepeat(ctx, image, backgroundPosition, bounds);
    ctx.restore();
	logger.warn(`<<< Exiting backgroundRepeatShape(ctx=${ctx},image=${image},backgroundPosition=${backgroundPosition},bounds=${bounds},left=${left},top=${top},width=${width},height=${height})`, { "event.action": "backgroundRepeatShape", "logrite.ctx": ctx, "logrite.image": image, "logrite.backgroundPosition": backgroundPosition, "logrite.bounds": bounds, "logrite.left": left, "logrite.top": top, "logrite.width": width, "logrite.height": height });
  }

  function renderBackgroundColor(ctx, backgroundBounds, bgcolor) {
	logger.error(`>>> Entering renderBackgroundColor(ctx=${ctx},backgroundBounds=${backgroundBounds},bgcolor=${bgcolor})`, { "event.action": "renderBackgroundColor", "logrite.ctx": ctx, "logrite.backgroundBounds": backgroundBounds, "logrite.bgcolor": bgcolor });
    renderRect(
      ctx,
      backgroundBounds.left,
      backgroundBounds.top,
      backgroundBounds.width,
      backgroundBounds.height,
      bgcolor
      );
	logger.warn(`<<< Exiting renderBackgroundColor(ctx=${ctx},backgroundBounds=${backgroundBounds},bgcolor=${bgcolor})`, { "event.action": "renderBackgroundColor", "logrite.ctx": ctx, "logrite.backgroundBounds": backgroundBounds, "logrite.bgcolor": bgcolor });
  }

  function renderBackgroundRepeating(el, bounds, ctx, image, imageIndex) {
	logger.error(`>>> Entering renderBackgroundRepeating(el=${el},bounds=${bounds},ctx=${ctx},image=${image},imageIndex=${imageIndex})`, { "event.action": "renderBackgroundRepeating", "logrite.el": el, "logrite.bounds": bounds, "logrite.ctx": ctx, "logrite.image": image, "logrite.imageIndex": imageIndex });
    var backgroundSize = Util.BackgroundSize(el, bounds, image, imageIndex),
    backgroundPosition = Util.BackgroundPosition(el, bounds, image, imageIndex, backgroundSize),
    backgroundRepeat = getCSS(el, "backgroundRepeat").split(",").map(Util.trimText);
	logger.error(`renderBackgroundRepeating(el=${el},bounds=${bounds},ctx=${ctx},image=${image},imageIndex=${imageIndex}): backgroundSize → ${backgroundSize}`, { "event.action": "renderBackgroundRepeating", "logrite.el": el, "logrite.bounds": bounds, "logrite.ctx": ctx, "logrite.image": image, "logrite.imageIndex": imageIndex });

    image = resizeImage(image, backgroundSize);

	logger.error(`renderBackgroundRepeating(el=${el},bounds=${bounds},ctx=${ctx},image=${image},imageIndex=${imageIndex}): image → ${image}`, { "event.action": "renderBackgroundRepeating", "logrite.el": el, "logrite.bounds": bounds, "logrite.ctx": ctx, "logrite.image": image, "logrite.imageIndex": imageIndex });
    backgroundRepeat = backgroundRepeat[imageIndex] || backgroundRepeat[0];

    switch (backgroundRepeat) {
      case "repeat-x":
        backgroundRepeatShape(ctx, image, backgroundPosition, bounds,
          bounds.left, bounds.top + backgroundPosition.top, 99999, image.height);
        break;

      case "repeat-y":
        backgroundRepeatShape(ctx, image, backgroundPosition, bounds,
          bounds.left + backgroundPosition.left, bounds.top, image.width, 99999);
        break;

      case "no-repeat":
        backgroundRepeatShape(ctx, image, backgroundPosition, bounds,
          bounds.left + backgroundPosition.left, bounds.top + backgroundPosition.top, image.width, image.height);
        break;

      default:
        renderBackgroundRepeat(ctx, image, backgroundPosition, {
          top: bounds.top,
          left: bounds.left,
          width: image.width,
          height: image.height
        });
		logger.warn(`<<< Exiting renderBackgroundRepeating(el=${el},bounds=${bounds},ctx=${ctx},image=${image},imageIndex=${imageIndex})`, { "event.action": "renderBackgroundRepeating", "logrite.el": el, "logrite.bounds": bounds, "logrite.ctx": ctx, "logrite.image": image, "logrite.imageIndex": imageIndex });
        break;
    }
  }

  function renderBackgroundImage(element, bounds, ctx) {
	logger.error(`>>> Entering renderBackgroundImage(element=${element},bounds=${bounds},ctx=${ctx})`, { "event.action": "renderBackgroundImage", "logrite.element": element, "logrite.bounds": bounds, "logrite.ctx": ctx });
    var backgroundImage = getCSS(element, "backgroundImage"),
    backgroundImages = Util.parseBackgroundImage(backgroundImage),
    image,
    imageIndex = backgroundImages.length;

    while(imageIndex--) {
      backgroundImage = backgroundImages[imageIndex];

      if (!backgroundImage.args || backgroundImage.args.length === 0) {
        continue;
      }

      var key = backgroundImage.method === 'url' ?
      backgroundImage.args[0] :
	logger.error(`renderBackgroundImage(element=${element},bounds=${bounds},ctx=${ctx}): key → ${key}`, { "event.action": "renderBackgroundImage", "logrite.element": element, "logrite.bounds": bounds, "logrite.ctx": ctx });
      backgroundImage.value;

      image = loadImage(key);

      // TODO add support for background-origin
      if (image) {
        renderBackgroundRepeating(element, bounds, ctx, image, imageIndex);
      } else {
        Util.log("html2canvas: Error loading background:", backgroundImage);
		logger.warn(`<<< Exiting renderBackgroundImage(element=${element},bounds=${bounds},ctx=${ctx})`, { "event.action": "renderBackgroundImage", "logrite.element": element, "logrite.bounds": bounds, "logrite.ctx": ctx });
      }
    }
  }

  function resizeImage(image, bounds) {
	logger.error(`>>> Entering resizeImage(image=${image},bounds=${bounds})`, { "event.action": "resizeImage", "logrite.image": image, "logrite.bounds": bounds });
    if(image.width === bounds.width && image.height === bounds.height) {
      return image;
    }

    var ctx, canvas = doc.createElement('canvas');
    canvas.width = bounds.width;
    canvas.height = bounds.height;
    ctx = canvas.getContext("2d");
	logger.error(`resizeImage(image=${image},bounds=${bounds}): ctx → ${ctx}`, { "event.action": "resizeImage", "logrite.image": image, "logrite.bounds": bounds });
    drawImage(ctx, image, 0, 0, image.width, image.height, 0, 0, bounds.width, bounds.height );
	logger.warn(`<<< Exiting resizeImage(image=${image},bounds=${bounds})`, { "event.action": "resizeImage", "logrite.image": image, "logrite.bounds": bounds });
    return canvas;
  }

  function setOpacity(ctx, element, parentStack) {
	logger.error(`>>> Entering setOpacity(ctx=${ctx},element=${element},parentStack=${parentStack})`, { "event.action": "setOpacity", "logrite.ctx": ctx, "logrite.element": element, "logrite.parentStack": parentStack });
	logger.warn(`<<< Exiting setOpacity(ctx=${ctx},element=${element},parentStack=${parentStack})`, { "event.action": "setOpacity", "logrite.ctx": ctx, "logrite.element": element, "logrite.parentStack": parentStack });
    return ctx.setVariable("globalAlpha", getCSS(element, "opacity") * ((parentStack) ? parentStack.opacity : 1));
  }

  function removePx(str) {
	logger.error(`>>> Entering removePx(str=${str})`, { "event.action": "removePx", "logrite.str": str });
	logger.warn(`<<< Exiting removePx(str=${str})`, { "event.action": "removePx", "logrite.str": str });
    return str.replace("px", "");
  }

  var transformRegExp = /(matrix)\((.+)\)/;
logger.error(`Parse(images=${images},options=${options}): transformRegExp → ${transformRegExp}`, { "event.action": "Parse", "logrite.images": images, "logrite.options": options });

  function getTransform(element, parentStack) {
	logger.error(`>>> Entering getTransform(element=${element},parentStack=${parentStack})`, { "event.action": "getTransform", "logrite.element": element, "logrite.parentStack": parentStack });
    var transform = getCSS(element, "transform") || getCSS(element, "-webkit-transform") || getCSS(element, "-moz-transform") || getCSS(element, "-ms-transform") || getCSS(element, "-o-transform");
    var transformOrigin = getCSS(element, "transform-origin") || getCSS(element, "-webkit-transform-origin") || getCSS(element, "-moz-transform-origin") || getCSS(element, "-ms-transform-origin") || getCSS(element, "-o-transform-origin") || "0px 0px";
	logger.error(`getTransform(element=${element},parentStack=${parentStack}): transform → ${transform}`, { "event.action": "getTransform", "logrite.element": element, "logrite.parentStack": parentStack });

    transformOrigin = transformOrigin.split(" ").map(removePx).map(Util.asFloat);
	logger.error(`getTransform(element=${element},parentStack=${parentStack}): transformOrigin → ${transformOrigin}`, { "event.action": "getTransform", "logrite.element": element, "logrite.parentStack": parentStack });

    var matrix;
    if (transform && transform !== "none") {
      var match = transform.match(transformRegExp);
	logger.error(`getTransform(element=${element},parentStack=${parentStack}): match → ${match}`, { "event.action": "getTransform", "logrite.element": element, "logrite.parentStack": parentStack });
      if (match) {
        switch(match[1]) {
          case "matrix":
            matrix = match[2].split(",").map(Util.trimText).map(Util.asFloat);
			logger.error(`getTransform(element=${element},parentStack=${parentStack}): matrix → ${matrix}`, { "event.action": "getTransform", "logrite.element": element, "logrite.parentStack": parentStack });
            break;
        }
      }
    }

	logger.warn(`<<< Exiting getTransform(element=${element},parentStack=${parentStack})`, { "event.action": "getTransform", "logrite.element": element, "logrite.parentStack": parentStack });
    return {
      origin: transformOrigin,
      matrix: matrix
    };
  }

  function createStack(element, parentStack, bounds, transform) {
	logger.error(`>>> Entering createStack(element=${element},parentStack=${parentStack},bounds=${bounds},transform=${transform})`, { "event.action": "createStack", "logrite.element": element, "logrite.parentStack": parentStack, "logrite.bounds": bounds, "logrite.transform": transform });
    var ctx = h2cRenderContext((!parentStack) ? documentWidth() : bounds.width , (!parentStack) ? documentHeight() : bounds.height),
    stack = {
      ctx: ctx,
      opacity: setOpacity(ctx, element, parentStack),
      cssPosition: getCSS(element, "position"),
      borders: getBorderData(element),
      transform: transform,
      clip: (parentStack && parentStack.clip) ? Util.Extend( {}, parentStack.clip ) : null
    };

	logger.error(`createStack(element=${element},parentStack=${parentStack},bounds=${bounds},transform=${transform}): ctx → ${ctx}`, { "event.action": "createStack", "logrite.element": element, "logrite.parentStack": parentStack, "logrite.bounds": bounds, "logrite.transform": transform });
    setZ(element, stack, parentStack);

    // TODO correct overflow for absolute content residing under a static position
    if (options.useOverflow === true && /(hidden|scroll|auto)/.test(getCSS(element, "overflow")) === true && /(BODY)/i.test(element.nodeName) === false){
      stack.clip = (stack.clip) ? clipBounds(stack.clip, bounds) : bounds;
    }

	logger.warn(`<<< Exiting createStack(element=${element},parentStack=${parentStack},bounds=${bounds},transform=${transform})`, { "event.action": "createStack", "logrite.element": element, "logrite.parentStack": parentStack, "logrite.bounds": bounds, "logrite.transform": transform });
    return stack;
  }

  function getBackgroundBounds(borders, bounds, clip) {
	logger.error(`>>> Entering getBackgroundBounds(borders=${borders},bounds=${bounds},clip=${clip})`, { "event.action": "getBackgroundBounds", "logrite.borders": borders, "logrite.bounds": bounds, "logrite.clip": clip });
    var backgroundBounds = {
      left: bounds.left + borders[3].width,
      top: bounds.top + borders[0].width,
      width: bounds.width - (borders[1].width + borders[3].width),
      height: bounds.height - (borders[0].width + borders[2].width)
    };

    if (clip) {
      backgroundBounds = clipBounds(backgroundBounds, clip);
	logger.error(`getBackgroundBounds(borders=${borders},bounds=${bounds},clip=${clip}): backgroundBounds → ${backgroundBounds}`, { "event.action": "getBackgroundBounds", "logrite.borders": borders, "logrite.bounds": bounds, "logrite.clip": clip });
    }

	logger.warn(`<<< Exiting getBackgroundBounds(borders=${borders},bounds=${bounds},clip=${clip})`, { "event.action": "getBackgroundBounds", "logrite.borders": borders, "logrite.bounds": bounds, "logrite.clip": clip });
    return backgroundBounds;
  }

  function getBounds(element, transform) {
	logger.error(`>>> Entering getBounds(element=${element},transform=${transform})`, { "event.action": "getBounds", "logrite.element": element, "logrite.transform": transform });
    var bounds = (transform.matrix) ? Util.OffsetBounds(element) : Util.Bounds(element);
    transform.origin[0] += bounds.left;
	logger.error(`getBounds(element=${element},transform=${transform}): bounds → ${bounds}`, { "event.action": "getBounds", "logrite.element": element, "logrite.transform": transform });
    transform.origin[1] += bounds.top;
	logger.warn(`<<< Exiting getBounds(element=${element},transform=${transform})`, { "event.action": "getBounds", "logrite.element": element, "logrite.transform": transform });
    return bounds;
  }

  function renderElement(element, parentStack, pseudoElement, ignoreBackground) {
	logger.error(`>>> Entering renderElement(element=${element},parentStack=${parentStack},pseudoElement=${pseudoElement},ignoreBackground=${ignoreBackground})`, { "event.action": "renderElement", "logrite.element": element, "logrite.parentStack": parentStack, "logrite.pseudoElement": pseudoElement, "logrite.ignoreBackground": ignoreBackground });
    var transform = getTransform(element, parentStack),
    bounds = getBounds(element, transform),
    image,
    stack = createStack(element, parentStack, bounds, transform),
    borders = stack.borders,
    ctx = stack.ctx,
    backgroundBounds = getBackgroundBounds(borders, bounds, stack.clip),
    borderData = parseBorders(element, bounds, borders),
    backgroundColor = (ignoreElementsRegExp.test(element.nodeName)) ? "#efefef" : getCSS(element, "backgroundColor");
	logger.error(`renderElement(element=${element},parentStack=${parentStack},pseudoElement=${pseudoElement},ignoreBackground=${ignoreBackground}): transform → ${transform}`, { "event.action": "renderElement", "logrite.element": element, "logrite.parentStack": parentStack, "logrite.pseudoElement": pseudoElement, "logrite.ignoreBackground": ignoreBackground });


    createShape(ctx, borderData.clip);

    ctx.save();
    ctx.clip();

    if (backgroundBounds.height > 0 && backgroundBounds.width > 0 && !ignoreBackground) {
      renderBackgroundColor(ctx, bounds, backgroundColor);
      renderBackgroundImage(element, backgroundBounds, ctx);
    } else if (ignoreBackground) {
      stack.backgroundColor =  backgroundColor;
    }

    ctx.restore();

    borderData.borders.forEach(function(border) {
      renderBorders(ctx, border.args, border.color);
    });

    if (!pseudoElement) {
      injectPseudoElements(element, stack);
    }

    switch(element.nodeName){
      case "IMG":
        if ((image = loadImage(element.getAttribute('src')))) {
          renderImage(ctx, element, image, bounds, borders);
        } else {
          Util.log("html2canvas: Error loading <img>:" + element.getAttribute('src'));
        }
        break;
      case "INPUT":
        // TODO add all relevant type's, i.e. HTML5 new stuff
        // todo add support for placeholder attribute for browsers which support it
        if (/^(text|url|email|submit|button|reset)$/.test(element.type) && (element.value || element.placeholder || "").length > 0){
          renderFormValue(element, bounds, stack);
        }
        break;
      case "TEXTAREA":
        if ((element.value || element.placeholder || "").length > 0){
          renderFormValue(element, bounds, stack);
        }
        break;
      case "SELECT":
        if ((element.options||element.placeholder || "").length > 0){
          renderFormValue(element, bounds, stack);
        }
        break;
      case "LI":
        renderListItem(element, stack, backgroundBounds);
        break;
      case "CANVAS":
        renderImage(ctx, element, element, bounds, borders);
        break;
    }

    return stack;
  }

  function isElementVisible(element) {
	logger.error(`>>> Entering isElementVisible(element=${element})`, { "event.action": "isElementVisible", "logrite.element": element });
	logger.warn(`<<< Exiting isElementVisible(element=${element})`, { "event.action": "isElementVisible", "logrite.element": element });
    return (getCSS(element, 'display') !== "none" && getCSS(element, 'visibility') !== "hidden" && !element.hasAttribute("data-html2canvas-ignore"));
  }

  function parseElement (element, stack, pseudoElement) {
	logger.error(`>>> Entering parseElement(element=${element},stack=${stack},pseudoElement=${pseudoElement})`, { "event.action": "parseElement", "logrite.element": element, "logrite.stack": stack, "logrite.pseudoElement": pseudoElement });
    if (isElementVisible(element)) {
      stack = renderElement(element, stack, pseudoElement, false) || stack;
      if (!ignoreElementsRegExp.test(element.nodeName)) {
        parseChildren(element, stack, pseudoElement);
		logger.warn(`<<< Exiting parseElement(element=${element},stack=${stack},pseudoElement=${pseudoElement})`, { "event.action": "parseElement", "logrite.element": element, "logrite.stack": stack, "logrite.pseudoElement": pseudoElement });
      }
    }
	logger.error(`parseElement(element=${element},stack=${stack},pseudoElement=${pseudoElement}): stack → ${stack}`, { "event.action": "parseElement", "logrite.element": element, "logrite.stack": stack, "logrite.pseudoElement": pseudoElement });
  }

  function parseChildren(element, stack, pseudoElement) {
	logger.error(`>>> Entering parseChildren(element=${element},stack=${stack},pseudoElement=${pseudoElement})`, { "event.action": "parseChildren", "logrite.element": element, "logrite.stack": stack, "logrite.pseudoElement": pseudoElement });
    Util.Children(element).forEach(function(node) {
      if (node.nodeType === node.ELEMENT_NODE) {
        parseElement(node, stack, pseudoElement);
      } else if (node.nodeType === node.TEXT_NODE) {
        renderText(element, node, stack);
		logger.warn(`<<< Exiting parseChildren(element=${element},stack=${stack},pseudoElement=${pseudoElement})`, { "event.action": "parseChildren", "logrite.element": element, "logrite.stack": stack, "logrite.pseudoElement": pseudoElement });
      }
    });
  }

  function init() {
	logger.error('>>> Entering init()', { "event.action": "init" });
    var background = getCSS(document.documentElement, "backgroundColor"),
      transparentBackground = (Util.isTransparent(background) && element === document.body),
      stack = renderElement(element, null, false, transparentBackground);
    parseChildren(element, stack);

    if (transparentBackground) {
      background = stack.backgroundColor;
    }

    body.removeChild(hidePseudoElements);
	logger.warn('<<< Exiting init()', { "event.action": "init" });
    return {
      backgroundColor: background,
      stack: stack
    };
  }

  return init();
};

function h2czContext(zindex) {
logger.error(`>>> Entering h2czContext(zindex=${zindex})`, { "event.action": "h2czContext", "logrite.zindex": zindex });
logger.warn(`<<< Exiting h2czContext(zindex=${zindex})`, { "event.action": "h2czContext", "logrite.zindex": zindex });
  return {
    zindex: zindex,
    children: []
  };
}

_html2canvas.Preload = function( options ) {
logger.error(`>>> Entering Preload(options=${options})`, { "event.action": "Preload", "logrite.options": options });

  var images = {
    numLoaded: 0,   // also failed are counted here
    numFailed: 0,
    numTotal: 0,
    cleanupDone: false
  },
  pageOrigin,
  Util = _html2canvas.Util,
  methods,
  i,
  count = 0,
  element = options.elements[0] || document.body,
  doc = element.ownerDocument,
  domImages = element.getElementsByTagName('img'), // Fetch images of the present element only
  imgLen = domImages.length,
  link = doc.createElement("a"),
  supportCORS = (function( img ){
    return (img.crossOrigin !== undefined);
  })(new Image()),
  timeoutTimer;

  link.href = window.location.href;
logger.debug(`Preload(options=${options}): link.href → ${link.href}`, { "event.action": "Preload", "logrite.options": options });
logger.debug(`Preload(options=${options}): imageObj.callbackname → ${imageObj.callbackname}`, { "event.action": "Preload", "logrite.options": options });
logger.debug(`Preload(options=${options}): imageObj.succeeded → ${imageObj.succeeded}`, { "event.action": "Preload", "logrite.options": options });
  pageOrigin  = link.protocol + link.host;
logger.debug(`Preload(options=${options}): pageOrigin → ${pageOrigin}`, { "event.action": "Preload", "logrite.options": options });

  function isSameOrigin(url){
	logger.error(`>>> Entering isSameOrigin(url=${url})`, { "event.action": "isSameOrigin", "logrite.url": url });
    link.href = url;
    link.href = link.href; // YES, BELIEVE IT OR NOT, that is required for IE9 - http://jsfiddle.net/niklasvh/2e48b/
    var origin = link.protocol + link.host;
	logger.debug(`Preload(options=${options}): imageObj.script → ${imageObj.script}`, { "event.action": "Preload", "logrite.options": options });
	logger.warn(`<<< Exiting isSameOrigin(url=${url})`, { "event.action": "isSameOrigin", "logrite.url": url });
    return (origin === pageOrigin);
  }

  function start(){
	logger.error('>>> Entering start()', { "event.action": "start" });
    Util.log("html2canvas: start: images: " + images.numLoaded + " / " + images.numTotal + " (failed: " + images.numFailed + ")");
    if (!images.firstRun && images.numLoaded >= images.numTotal){
      Util.log("Finished loading images: # " + images.numTotal + " (failed: " + images.numFailed + ")");

      if (typeof options.complete === "function"){
        options.complete(images);
      }

	logger.warn('<<< Exiting start()', { "event.action": "start" });
    }
  }

  // TODO modify proxy to serve images with CORS enabled, where available
  function proxyGetImage(url, img, imageObj){
	logger.error(`>>> Entering proxyGetImage(url=${url},img=${img},imageObj=${imageObj})`, { "event.action": "proxyGetImage", "logrite.url": url, "logrite.img": img, "logrite.imageObj": imageObj });
    var callback_name,
    scriptUrl = options.proxy,
    script;

    link.href = url;
    url = link.href; // work around for pages with base href="" set - WARNING: this may change the url

    callback_name = 'html2canvas_' + (count++);
	logger.error(`proxyGetImage(url=${url},img=${img},imageObj=${imageObj}): callback_name → ${callback_name}`, { "event.action": "proxyGetImage", "logrite.url": url, "logrite.img": img, "logrite.imageObj": imageObj });
    imageObj.callbackname = callback_name;
	logger.debug(`Preload(options=${options}): callback_name → ${callback_name}`, { "event.action": "Preload", "logrite.options": options });

	logger.debug(`proxyGetImage(url=${url},img=${img},imageObj=${imageObj}): imageObj.callbackname → ${imageObj.callbackname}`, { "event.action": "proxyGetImage", "logrite.url": url, "logrite.img": img, "logrite.imageObj": imageObj });
    if (scriptUrl.indexOf("?") > -1) {
      scriptUrl += "&";
    } else {
      scriptUrl += "?";
    }
    scriptUrl += 'url=' + encodeURIComponent(url) + '&callback=' + callback_name;
    script = doc.createElement("script");
	logger.debug(`Preload(options=${options}): script → ${script}`, { "event.action": "Preload", "logrite.options": options });

    window[callback_name] = function(a){
      if (a.substring(0,6) === "error:"){
        imageObj.succeeded = false;
        images.numLoaded++;
		logger.debug(`proxyGetImage(url=${url},img=${img},imageObj=${imageObj}): imageObj.succeeded → ${imageObj.succeeded}`, { "event.action": "proxyGetImage", "logrite.url": url, "logrite.img": img, "logrite.imageObj": imageObj });
        images.numFailed++;
        start();
      } else {
        setImageLoadHandlers(img, imageObj);
        img.src = a;
      }
      window[callback_name] = undefined; // to work with IE<9  // NOTE: that the undefined callback property-name still exists on the window object (for IE<9)
      try {
	logger.debug(`proxyGetImage(url=${url},img=${img},imageObj=${imageObj}): img.src → ${img.src}`, { "event.action": "proxyGetImage", "logrite.url": url, "logrite.img": img, "logrite.imageObj": imageObj });
        delete window[callback_name];  // for all browser that support this
      } catch(ex) {}
	logger.error(`Exception in Preload(options=${options}): ${ex.message}`, ex, { "event.action": "Preload", "event.category": "process", "logrite.options": options });
      script.parentNode.removeChild(script);
	logger.error(`Exception in proxyGetImage(url=${url},img=${img},imageObj=${imageObj}): ${ex.message}`, ex, { "event.action": "proxyGetImage", "event.category": "process", "logrite.url": url, "logrite.img": img, "logrite.imageObj": imageObj });
      script = null;
      delete imageObj.script;
      delete imageObj.callbackname;
    };

    script.setAttribute("type", "text/javascript");
    script.setAttribute("src", scriptUrl);
    imageObj.script = script;
	logger.warn(`<<< Exiting proxyGetImage(url=${url},img=${img},imageObj=${imageObj})`, { "event.action": "proxyGetImage", "logrite.url": url, "logrite.img": img, "logrite.imageObj": imageObj });
    window.document.body.appendChild(script);

  }

  function loadPseudoElement(element, type) {
	logger.error(`>>> Entering loadPseudoElement(element=${element},type=${type})`, { "event.action": "loadPseudoElement", "logrite.element": element, "logrite.type": type });
    var style = window.getComputedStyle(element, type),
    content = style.content;
	logger.error(`loadPseudoElement(element=${element},type=${type}): style → ${style}`, { "event.action": "loadPseudoElement", "logrite.element": element, "logrite.type": type });
    if (content.substr(0, 3) === 'url') {
	logger.debug(`Preload(options=${options}): content → ${content}`, { "event.action": "Preload", "logrite.options": options });
      methods.loadImage(_html2canvas.Util.parseBackgroundImage(content)[0].args[0]);
    }
    loadBackgroundImages(style.backgroundImage, element);
	logger.warn(`<<< Exiting loadPseudoElement(element=${element},type=${type})`, { "event.action": "loadPseudoElement", "logrite.element": element, "logrite.type": type });
  }

  function loadPseudoElementImages(element) {
	logger.error(`>>> Entering loadPseudoElementImages(element=${element})`, { "event.action": "loadPseudoElementImages", "logrite.element": element });
    loadPseudoElement(element, ":before");
    loadPseudoElement(element, ":after");
	logger.warn(`<<< Exiting loadPseudoElementImages(element=${element})`, { "event.action": "loadPseudoElementImages", "logrite.element": element });
  }

  function loadGradientImage(backgroundImage, bounds) {
	logger.error(`>>> Entering loadGradientImage(backgroundImage=${backgroundImage},bounds=${bounds})`, { "event.action": "loadGradientImage", "logrite.backgroundImage": backgroundImage, "logrite.bounds": bounds });
    var img = _html2canvas.Generate.Gradient(backgroundImage, bounds);

	logger.error(`loadGradientImage(backgroundImage=${backgroundImage},bounds=${bounds}): img → ${img}`, { "event.action": "loadGradientImage", "logrite.backgroundImage": backgroundImage, "logrite.bounds": bounds });
    if (img !== undefined){
      images[backgroundImage] = {
        img: img,
        succeeded: true
      };
      images.numTotal++;
      images.numLoaded++;
      start();
	logger.warn(`<<< Exiting loadGradientImage(backgroundImage=${backgroundImage},bounds=${bounds})`, { "event.action": "loadGradientImage", "logrite.backgroundImage": backgroundImage, "logrite.bounds": bounds });
    }
  }

  function invalidBackgrounds(background_image) {
	logger.error(`>>> Entering invalidBackgrounds(background_image=${background_image})`, { "event.action": "invalidBackgrounds", "logrite.background_image": background_image });
	logger.warn(`<<< Exiting invalidBackgrounds(background_image=${background_image})`, { "event.action": "invalidBackgrounds", "logrite.background_image": background_image });
    return (background_image && background_image.method && background_image.args && background_image.args.length > 0 );
  }

  function loadBackgroundImages(background_image, el) {
	logger.error(`>>> Entering loadBackgroundImages(background_image=${background_image},el=${el})`, { "event.action": "loadBackgroundImages", "logrite.background_image": background_image, "logrite.el": el });
    var bounds;

    _html2canvas.Util.parseBackgroundImage(background_image).filter(invalidBackgrounds).forEach(function(background_image) {
      if (background_image.method === 'url') {
        methods.loadImage(background_image.args[0]);
      } else if(background_image.method.match(/\-?gradient$/)) {
        if(bounds === undefined) {
          bounds = _html2canvas.Util.Bounds(el);
        }
        loadGradientImage(background_image.value, bounds);
      }
    });
  }

  function getImages (el) {
	logger.error(`>>> Entering getImages(el=${el})`, { "event.action": "getImages", "logrite.el": el });
    var elNodeType = false;

    // Firefox fails with permission denied on pages with iframes
    try {
      Util.Children(el).forEach(getImages);
    }
    catch( e ) {}

    try {
      elNodeType = el.nodeType;
    } catch (ex) {
      elNodeType = false;
      Util.log("html2canvas: failed to access some element's nodeType - Exception: " + ex.message);
    }

    if (elNodeType === 1 || elNodeType === undefined) {
      loadPseudoElementImages(el);
      try {
        loadBackgroundImages(Util.getCSS(el, 'backgroundImage'), el);
      } catch(e) {
        Util.log("html2canvas: failed to get background-image - Exception: " + e.message);
      }
      loadBackgroundImages(el);
	logger.warn(`<<< Exiting getImages(el=${el})`, { "event.action": "getImages", "logrite.el": el });
    }
  }

  function setImageLoadHandlers(img, imageObj) {
	logger.error(`>>> Entering setImageLoadHandlers(img=${img},imageObj=${imageObj})`, { "event.action": "setImageLoadHandlers", "logrite.img": img, "logrite.imageObj": imageObj });
	logger.debug(`setImageLoadHandlers(img=${img},imageObj=${imageObj}): imageObj.succeeded → ${imageObj.succeeded}`, { "event.action": "setImageLoadHandlers", "logrite.img": img, "logrite.imageObj": imageObj });
    img.onload = function() {
      if ( imageObj.timer !== undefined ) {
		logger.error('>>> Entering onload()', { "event.action": "onload" });
        // CORS succeeded
		logger.warn('<<< Exiting onload()', { "event.action": "onload" });
        window.clearTimeout( imageObj.timer );
      }

      images.numLoaded++;
      imageObj.succeeded = true;
      img.onerror = img.onload = null;
      start();
    };
    img.onerror = function() {
	logger.error('>>> Entering onerror()', { "event.action": "onerror" });
      if (img.crossOrigin === "anonymous") {
        // CORS failed
        window.clearTimeout( imageObj.timer );

        // let's try with proxy instead
        if ( options.proxy ) {
          var src = img.src;
          img = new Image();
          imageObj.img = img;
          img.src = src;

          proxyGetImage( img.src, img, imageObj );
          return;
        }
      }

      images.numLoaded++;
      images.numFailed++;
      imageObj.succeeded = false;
      img.onerror = img.onload = null;
      start();
	logger.warn('<<< Exiting onerror()', { "event.action": "onerror" });
    };
	logger.warn(`<<< Exiting setImageLoadHandlers(img=${img},imageObj=${imageObj})`, { "event.action": "setImageLoadHandlers", "logrite.img": img, "logrite.imageObj": imageObj });
  }

  methods = {
    loadImage: function( src ) {
      var img, imageObj;
      if ( src && images[src] === undefined ) {
        img = new Image();
        if ( src.match(/data:image\/.*;base64,/i) ) {
          img.src = src.replace(/url\(['"]{0,}|['"]{0,}\)$/ig, '');
          imageObj = images[src] = {
            img: img
          };
          images.numTotal++;
          setImageLoadHandlers(img, imageObj);
        } else if ( isSameOrigin( src ) || options.allowTaint ===  true ) {
          imageObj = images[src] = {
            img: img
          };
          images.numTotal++;
          setImageLoadHandlers(img, imageObj);
          img.src = src;
        } else if ( supportCORS && !options.allowTaint && options.useCORS ) {
          // attempt to load with CORS

          img.crossOrigin = "anonymous";
          imageObj = images[src] = {
            img: img
          };
          images.numTotal++;
          setImageLoadHandlers(img, imageObj);
          img.src = src;
        } else if ( options.proxy ) {
          imageObj = images[src] = {
            img: img
          };
          images.numTotal++;
          proxyGetImage( src, img, imageObj );
        }
      }

    },
    cleanupDOM: function(cause) {
	logger.error(`>>> Entering cleanupDOM(cause=${cause})`, { "event.action": "cleanupDOM", "logrite.cause": cause });
      var img, src;
      if (!images.cleanupDone) {
        if (cause && typeof cause === "string") {
          Util.log("html2canvas: Cleanup because: " + cause);
        } else {
          Util.log("html2canvas: Cleanup after timeout: " + options.timeout + " ms.");
        }

        for (src in images) {
          if (images.hasOwnProperty(src)) {
            img = images[src];
            if (typeof img === "object" && img.callbackname && img.succeeded === undefined) {
              // cancel proxy image request
              window[img.callbackname] = undefined; // to work with IE<9  // NOTE: that the undefined callback property-name still exists on the window object (for IE<9)
              try {
                delete window[img.callbackname];  // for all browser that support this
              } catch(ex) {}
              if (img.script && img.script.parentNode) {
                img.script.setAttribute("src", "about:blank");  // try to cancel running request
                img.script.parentNode.removeChild(img.script);
              }
              images.numLoaded++;
              images.numFailed++;
              Util.log("html2canvas: Cleaned up failed img: '" + src + "' Steps: " + images.numLoaded + " / " + images.numTotal);
            }
          }
        }

        // cancel any pending requests
        if(window.stop !== undefined) {
          window.stop();
        } else if(document.execCommand !== undefined) {
          document.execCommand("Stop", false);
        }
        if (document.close !== undefined) {
          document.close();
        }
        images.cleanupDone = true;
        if (!(cause && typeof cause === "string")) {
          start();
		logger.warn(`<<< Exiting cleanupDOM(cause=${cause})`, { "event.action": "cleanupDOM", "logrite.cause": cause });
        }
      }
    },

    renderingDone: function() {
	logger.error('>>> Entering renderingDone()', { "event.action": "renderingDone" });
      if (timeoutTimer) {
        window.clearTimeout(timeoutTimer);
		logger.warn('<<< Exiting renderingDone()', { "event.action": "renderingDone" });
      }
    }
  };

  if (options.timeout > 0) {
    timeoutTimer = window.setTimeout(methods.cleanupDOM, options.timeout);
	logger.error(`Preload(options=${options}): timeoutTimer → ${timeoutTimer}`, { "event.action": "Preload", "logrite.options": options });
  }

  Util.log('html2canvas: Preload starts: finding background-images');
  images.firstRun = true;

  getImages(element);

  Util.log('html2canvas: Preload: Finding images');
  // load <img> images
  for (i = 0; i < imgLen; i+=1){
    methods.loadImage( domImages[i].getAttribute( "src" ) );
  }

  images.firstRun = false;
  Util.log('html2canvas: Preload: Done.');
  if (images.numTotal === images.numLoaded) {
    start();
  }

  return methods;
};

_html2canvas.Renderer = function(parseQueue, options){
logger.error(`>>> Entering Renderer(parseQueue=${parseQueue},options=${options})`, { "event.action": "Renderer", "logrite.parseQueue": parseQueue, "logrite.options": options });

  // http://www.w3.org/TR/CSS21/zindex.html
  function createRenderQueue(parseQueue) {
	logger.error(`>>> Entering createRenderQueue(parseQueue=${parseQueue})`, { "event.action": "createRenderQueue", "logrite.parseQueue": parseQueue });
    var queue = [],
    rootContext;

    rootContext = (function buildStackingContext(rootNode) {
      var rootContext = {};
      function insert(context, node, specialParent) {
		logger.error(`>>> Entering insert(context=${context},node=${node},specialParent=${specialParent})`, { "event.action": "insert", "logrite.context": context, "logrite.node": node, "logrite.specialParent": specialParent });
        var zi = (node.zIndex.zindex === 'auto') ? 0 : Number(node.zIndex.zindex),
        contextForChildren = context, // the stacking context for children
        isPositioned = node.zIndex.isPositioned,
        isFloated = node.zIndex.isFloated,
        stub = {node: node},
        childrenDest = specialParent; // where children without z-index should be pushed into

        if (node.zIndex.ownStacking) {
          // '!' comes before numbers in sorted array
          contextForChildren = stub.context = { '!': [{node:node, children: []}]};
          childrenDest = undefined;
        } else if (isPositioned || isFloated) {
          childrenDest = stub.children = [];
        }

        if (zi === 0 && specialParent) {
          specialParent.push(stub);
        } else {
          if (!context[zi]) { context[zi] = []; }
          context[zi].push(stub);
        }

        node.zIndex.children.forEach(function(childNode) {
          insert(contextForChildren, childNode, childrenDest);
        });
		logger.warn(`<<< Exiting insert(context=${context},node=${node},specialParent=${specialParent})`, { "event.action": "insert", "logrite.context": context, "logrite.node": node, "logrite.specialParent": specialParent });
      }
      insert(rootContext, rootNode);
      return rootContext;
    })(parseQueue);

    function sortZ(context) {
	logger.error(`>>> Entering sortZ(context=${context})`, { "event.action": "sortZ", "logrite.context": context });
      Object.keys(context).sort().forEach(function(zi) {
        var nonPositioned = [],
        floated = [],
        positioned = [],
        list = [];

        // positioned after static
        context[zi].forEach(function(v) {
          if (v.node.zIndex.isPositioned || v.node.zIndex.opacity < 1) {
            // http://www.w3.org/TR/css3-color/#transparency
            // non-positioned element with opactiy < 1 should be stacked as if it were a positioned element with ‘z-index: 0’ and ‘opacity: 1’.
            positioned.push(v);
          } else if (v.node.zIndex.isFloated) {
            floated.push(v);
          } else {
            nonPositioned.push(v);
          }
        });

        (function walk(arr) {
		logger.error(`>>> Entering walk(arr=${arr})`, { "event.action": "walk", "logrite.arr": arr });
          arr.forEach(function(v) {
            list.push(v);
            if (v.children) { walk(v.children); }
          });
		logger.warn(`<<< Exiting walk(arr=${arr})`, { "event.action": "walk", "logrite.arr": arr });
        })(nonPositioned.concat(floated, positioned));

        list.forEach(function(v) {
          if (v.context) {
            sortZ(v.context);
          } else {
            queue.push(v.node);
			logger.warn(`<<< Exiting sortZ(context=${context})`, { "event.action": "sortZ", "logrite.context": context });
          }
        });
      });
    }

    sortZ(rootContext);

    return queue;
  }

  function getRenderer(rendererName) {
	logger.error(`>>> Entering getRenderer(rendererName=${rendererName})`, { "event.action": "getRenderer", "logrite.rendererName": rendererName });
    var renderer;

    if (typeof options.renderer === "string" && _html2canvas.Renderer[rendererName] !== undefined) {
      renderer = _html2canvas.Renderer[rendererName](options);
    } else if (typeof rendererName === "function") {
      renderer = rendererName(options);
    } else {
	logger.error(`getRenderer(rendererName=${rendererName}): renderer → ${renderer}`, { "event.action": "getRenderer", "logrite.rendererName": rendererName });
	logger.debug(`getRenderer(rendererName=${rendererName}): renderer → ${renderer}`, { "event.action": "getRenderer", "logrite.rendererName": rendererName });
      throw new Error("Unknown renderer");
    }

    if ( typeof renderer !== "function" ) {
      throw new Error("Invalid renderer defined");
    }
	logger.warn(`<<< Exiting getRenderer(rendererName=${rendererName})`, { "event.action": "getRenderer", "logrite.rendererName": rendererName });
    return renderer;
  }

  return getRenderer(options.renderer)(parseQueue, options, document, createRenderQueue(parseQueue.stack), _html2canvas);
};

_html2canvas.Util.Support = function (options, doc) {
logger.error(`>>> Entering Support(options=${options},doc=${doc})`, { "event.action": "Support", "logrite.options": options, "logrite.doc": doc });

  function supportSVGRendering() {
	logger.error('>>> Entering supportSVGRendering()', { "event.action": "supportSVGRendering" });
    var img = new Image(),
    canvas = doc.createElement("canvas"),
    ctx = (canvas.getContext === undefined) ? false : canvas.getContext("2d");
	logger.error(`supportSVGRendering(): img → ${img}`, { "event.action": "supportSVGRendering" });
	logger.debug(`supportSVGRendering(): img.src → ${img.src}`, { "event.action": "supportSVGRendering" });
    if (ctx === false) {
      return false;
    }
    canvas.width = canvas.height = 10;
    img.src = [
    "data:image/svg+xml,",
    "<svg xmlns='http://www.w3.org/2000/svg' width='10' height='10'>",
    "<foreignObject width='10' height='10'>",
    "<div xmlns='http://www.w3.org/1999/xhtml' style='width:10;height:10;'>",
    "sup",
    "</div>",
    "</foreignObject>",
    "</svg>"
    ].join("");
    try {
      ctx.drawImage(img, 0, 0);
      canvas.toDataURL();
    } catch(e) {
      return false;
    }
    _html2canvas.Util.log('html2canvas: Parse: SVG powered rendering available');
	logger.warn('<<< Exiting supportSVGRendering()', { "event.action": "supportSVGRendering" });
    return true;
  }

  // Test whether we can use ranges to measure bounding boxes
  // Opera doesn't provide valid bounds.height/bottom even though it supports the method.

  function supportRangeBounds() {
	logger.error('>>> Entering supportRangeBounds()', { "event.action": "supportRangeBounds" });
    var r, testElement, rangeBounds, rangeHeight, support = false;

    if (doc.createRange) {
      r = doc.createRange();
	logger.error(`supportRangeBounds(): r → ${r}`, { "event.action": "supportRangeBounds" });
      if (r.getBoundingClientRect) {
        testElement = doc.createElement('boundtest');
        testElement.style.height = "123px";
        testElement.style.display = "block";
        doc.body.appendChild(testElement);

        r.selectNode(testElement);
        rangeBounds = r.getBoundingClientRect();
        rangeHeight = rangeBounds.height;

        if (rangeHeight === 123) {
          support = true;
        }
        doc.body.removeChild(testElement);
      }
    }

	logger.warn('<<< Exiting supportRangeBounds()', { "event.action": "supportRangeBounds" });
    return support;
  }

  return {
    rangeBounds: supportRangeBounds(),
    svgRendering: options.svgRendering && supportSVGRendering()
  };
};
window.html2canvas = function(elements, opts) {
logger.error(`>>> Entering html2canvas(elements=${elements},opts=${opts})`, { "event.action": "html2canvas", "logrite.elements": elements, "logrite.opts": opts });
  elements = (elements.length) ? elements : [elements];
  var queue,
  canvas,
  options = {
    // general
    logging: false,
    elements: elements,
    background: "#fff",

    // preload options
    proxy: null,
    timeout: 0,    // no timeout
    useCORS: false, // try to load images as CORS (where available), before falling back to proxy
    allowTaint: false, // whether to allow images to taint the canvas, won't need proxy if set to true

    // parse options
    svgRendering: false, // use svg powered rendering where available (FF11+)
    ignoreElements: "IFRAME|OBJECT|PARAM",
    useOverflow: true,
    letterRendering: false,
    chinese: false,

    // render options

    width: null,
    height: null,
    taintTest: true, // do a taint test with all images before applying to canvas
    renderer: "Canvas"
  };
logger.error(`html2canvas(elements=${elements},opts=${opts}): elements → ${elements}`, { "event.action": "html2canvas", "logrite.elements": elements, "logrite.opts": opts });

  options = _html2canvas.Util.Extend(opts, options);

  _html2canvas.logging = options.logging;
  options.complete = function( images ) {
logger.error(`>>> Entering complete(images=${images})`, { "event.action": "complete", "logrite.images": images });

    if (typeof options.onpreloaded === "function") {
      if ( options.onpreloaded( images ) === false ) {
        return;
      }
    }
    queue = _html2canvas.Parse( images, options );
	logger.debug(`html2canvas(elements=${elements},opts=${opts}): queue → ${queue}`, { "event.action": "html2canvas", "logrite.elements": elements, "logrite.opts": opts });

    if (typeof options.onparsed === "function") {
      if ( options.onparsed( queue ) === false ) {
        return;
      }
    }

    canvas = _html2canvas.Renderer( queue, options );

    if (typeof options.onrendered === "function") {
      options.onrendered( canvas );
    }


logger.warn(`<<< Exiting complete(images=${images})`, { "event.action": "complete", "logrite.images": images });
  };

  // for pages without images, we still want this to be async, i.e. return methods before executing
  window.setTimeout( function(){
    _html2canvas.Preload( options );
  }, 0 );

  return {
    render: function( queue, opts ) {
	logger.error(`>>> Entering render(queue=${queue},opts=${opts})`, { "event.action": "render", "logrite.queue": queue, "logrite.opts": opts });
      return _html2canvas.Renderer( queue, _html2canvas.Util.Extend(opts, options) );
    },
    parse: function( images, opts ) {
	logger.error(`>>> Entering parse(images=${images},opts=${opts})`, { "event.action": "parse", "logrite.images": images, "logrite.opts": opts });
      return _html2canvas.Parse( images, _html2canvas.Util.Extend(opts, options) );
    },
    preload: function( opts ) {
	logger.error(`>>> Entering preload(opts=${opts})`, { "event.action": "preload", "logrite.opts": opts });
	logger.warn(`<<< Exiting Preload(options=${options})`, { "event.action": "Preload", "logrite.options": options });
      return _html2canvas.Preload( _html2canvas.Util.Extend(opts, options) );
    },
    log: _html2canvas.Util.log
  };
logger.warn(`<<< Exiting html2canvas(elements=${elements},opts=${opts})`, { "event.action": "html2canvas", "logrite.elements": elements, "logrite.opts": opts });
};

window.html2canvas.log = _html2canvas.Util.log; // for renderers
window.html2canvas.Renderer = {
  Canvas: undefined // We are assuming this will be used
};
_html2canvas.Renderer.Canvas = function(options) {
logger.error(`>>> Entering Canvas(options=${options})`, { "event.action": "Canvas", "logrite.options": options });
  options = options || {};

  var doc = document,
  safeImages = [],
  testCanvas = document.createElement("canvas"),
  testctx = testCanvas.getContext("2d"),
  Util = _html2canvas.Util,
  canvas = options.canvas || doc.createElement('canvas');

  function createShape(ctx, args) {
    ctx.beginPath();
    args.forEach(function(arg) {
      ctx[arg.name].apply(ctx, arg['arguments']);
    });
    ctx.closePath();
  }

  function safeImage(item) {
	logger.error(`>>> Entering safeImage(item=${item})`, { "event.action": "safeImage", "logrite.item": item });
    if (safeImages.indexOf(item['arguments'][0].src ) === -1) {
      testctx.drawImage(item['arguments'][0], 0, 0);
      try {
        testctx.getImageData(0, 0, 1, 1);
      } catch(e) {
		logger.error(`Exception in Canvas(options=${options}): ${e.message}`, e, { "event.action": "Canvas", "event.category": "process", "logrite.options": options });
        testCanvas = doc.createElement("canvas");
        testctx = testCanvas.getContext("2d");
        return false;
      }
      safeImages.push(item['arguments'][0].src);
	logger.error(`Exception in safeImage(item=${item}): ${e.message}`, e, { "event.action": "safeImage", "event.category": "process", "logrite.item": item });
    }
	logger.warn(`<<< Exiting safeImage(item=${item})`, { "event.action": "safeImage", "logrite.item": item });
    return true;
  }

  function renderItem(ctx, item) {
	logger.error(`>>> Entering renderItem(ctx=${ctx},item=${item})`, { "event.action": "renderItem", "logrite.ctx": ctx, "logrite.item": item });
    switch(item.type){
      case "variable":
        ctx[item.name] = item['arguments'];
        break;
      case "function":
        switch(item.name) {
          case "createPattern":
            if (item['arguments'][0].width > 0 && item['arguments'][0].height > 0) {
              try {
                ctx.fillStyle = ctx.createPattern(item['arguments'][0], "repeat");
              }
              catch(e) {
                Util.log("html2canvas: Renderer: Error creating pattern", e.message);
              }
            }
            break;
          case "drawShape":
            createShape(ctx, item['arguments']);
            break;
          case "drawImage":
            if (item['arguments'][8] > 0 && item['arguments'][7] > 0) {
              if (!options.taintTest || (options.taintTest && safeImage(item))) {
                ctx.drawImage.apply( ctx, item['arguments'] );
              }
            }
            break;
          default:
            ctx[item.name].apply(ctx, item['arguments']);
        }
        break;
    }
	logger.warn(`<<< Exiting renderItem(ctx=${ctx},item=${item})`, { "event.action": "renderItem", "logrite.ctx": ctx, "logrite.item": item });
  }

  return function(parsedData, options, document, queue, _html2canvas) {
    var ctx = canvas.getContext("2d"),
    newCanvas,
    bounds,
    fstyle,
    zStack = parsedData.stack;
	logger.debug(`Canvas(options=${options}): ctx.fillStyle → ${ctx.fillStyle}`, { "event.action": "Canvas", "logrite.options": options });
	logger.debug(`Canvas(options=${options}): ctx.textBaseline → ${ctx.textBaseline}`, { "event.action": "Canvas", "logrite.options": options });

    canvas.width = canvas.style.width =  options.width || zStack.ctx.width;
    canvas.height = canvas.style.height = options.height || zStack.ctx.height;

    fstyle = ctx.fillStyle;
    ctx.fillStyle = (Util.isTransparent(zStack.backgroundColor) && options.background !== undefined) ? options.background : parsedData.backgroundColor;
    ctx.fillRect(0, 0, canvas.width, canvas.height);
    ctx.fillStyle = fstyle;

    queue.forEach(function(storageContext) {
      // set common settings for canvas
      ctx.textBaseline = "bottom";
      ctx.save();

      if (storageContext.transform.matrix) {
        ctx.translate(storageContext.transform.origin[0], storageContext.transform.origin[1]);
        ctx.transform.apply(ctx, storageContext.transform.matrix);
        ctx.translate(-storageContext.transform.origin[0], -storageContext.transform.origin[1]);
      }

      if (storageContext.clip){
        ctx.beginPath();
        ctx.rect(storageContext.clip.left, storageContext.clip.top, storageContext.clip.width, storageContext.clip.height);
        ctx.clip();
      }

      if (storageContext.ctx.storage) {
        storageContext.ctx.storage.forEach(function(item) {
          renderItem(ctx, item);
        });
      }

      ctx.restore();
    });

    Util.log("html2canvas: Renderer: Canvas renderer done - returning canvas obj");

    if (options.elements.length === 1) {
      if (typeof options.elements[0] === "object" && options.elements[0].nodeName !== "BODY") {
        // crop image to the bounds of selected (single) element
        bounds = _html2canvas.Util.Bounds(options.elements[0]);
        newCanvas = document.createElement('canvas');
        newCanvas.width = Math.ceil(bounds.width);
        newCanvas.height = Math.ceil(bounds.height);
        ctx = newCanvas.getContext("2d");

        ctx.drawImage(canvas, bounds.left, bounds.top, bounds.width, bounds.height, 0, 0, bounds.width, bounds.height);
        canvas = null;
		logger.debug(`Canvas(options=${options}): canvas.width → ${canvas.width}`, { "event.action": "Canvas", "logrite.options": options });
        return newCanvas;
      }
    }

	logger.debug(`Canvas(options=${options}): canvas.height → ${canvas.height}`, { "event.action": "Canvas", "logrite.options": options });
    return canvas;
  };
};
})(window,document);