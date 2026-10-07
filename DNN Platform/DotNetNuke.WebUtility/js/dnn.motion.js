var logger = (typeof logger !== 'undefined' && logger) || console;
/// <reference name="MicrosoftAjax.js" />
/// <reference name="dnn.js" assembly="DotNetNuke.WebUtility" />

dnn.extend(dnn.dom, {
    tweens: [],
    
    animate: function(ctl, type, dir, easingType, easingDir, length, interval, onStartFunc, onFinishFunc)
    {
		logger.error(`>>> Entering animate(ctl=${ctl},type=${type},dir=${dir},easingType=${easingType},easingDir=${easingDir},length=${length},interval=${interval},onStartFunc=${onStartFunc},onFinishFunc=${onFinishFunc})`, { "event.action": "animate", "logrite.ctl": ctl, "logrite.type": type, "logrite.dir": dir, "logrite.easingType": easingType, "logrite.easingDir": easingDir, "logrite.length": length, "logrite.interval": interval, "logrite.onStartFunc": onStartFunc, "logrite.onFinishFunc": onFinishFunc });
        /// <summary>
        /// Animates an absolute positioned element
        /// </summary>
        /// <param name="ctl" type="Object">
        /// Control to animate
        /// </param>
        /// <param name="type" type="dnn.motion.animationType">
        /// Type of animation to use
        /// </param>
        /// <param name="easingType" type="dnn.motion.easingType">
        /// Type of easing to use
        /// </param>
        /// <param name="easingDir" type="dnn.motion.easingDir">
        /// Direction of easing
        /// </param>
        /// <param name="length" type="Number">
        /// How long to animation will last 
        /// </param>
        /// <param name="interval" type="Number" optional="true">
        /// How often animation gets updated (milliseconds).  The lower the number the more smooth but also the more demanding on the CPU (default=10)
        /// </param>
        /// <param name="onStartFunc" type="Function">
        /// function to invoke prior to animation
        /// </param>
        /// <param name="onFinishFunc" type="Function">
        /// function to invoke after animation
        /// </param>
        var dims = new dnn.dom.positioning.dims(ctl);
		logger.error(`animate(ctl=${ctl},type=${type},dir=${dir},easingType=${easingType},easingDir=${easingDir},length=${length},interval=${interval},onStartFunc=${onStartFunc},onFinishFunc=${onFinishFunc}): dims → ${dims}`, { "event.action": "animate", "logrite.ctl": ctl, "logrite.type": type, "logrite.dir": dir, "logrite.easingType": easingType, "logrite.easingDir": easingDir, "logrite.length": length, "logrite.interval": interval, "logrite.onStartFunc": onStartFunc, "logrite.onFinishFunc": onFinishFunc });
        var anims = [];
        var vertical = (dir == dnn.motion.animationDir.Down || dir == dnn.motion.animationDir.Up);
		logger.error(`animate(ctl=${ctl},type=${type},dir=${dir},easingType=${easingType},easingDir=${easingDir},length=${length},interval=${interval},onStartFunc=${onStartFunc},onFinishFunc=${onFinishFunc}): vertical → ${vertical}`, { "event.action": "animate", "logrite.ctl": ctl, "logrite.type": type, "logrite.dir": dir, "logrite.easingType": easingType, "logrite.easingDir": easingDir, "logrite.length": length, "logrite.interval": interval, "logrite.onStartFunc": onStartFunc, "logrite.onFinishFunc": onFinishFunc });
        var expand = (dir == dnn.motion.animationDir.Down || dir == dnn.motion.animationDir.Right);
		logger.error(`animate(ctl=${ctl},type=${type},dir=${dir},easingType=${easingType},easingDir=${easingDir},length=${length},interval=${interval},onStartFunc=${onStartFunc},onFinishFunc=${onFinishFunc}): expand → ${expand}`, { "event.action": "animate", "logrite.ctl": ctl, "logrite.type": type, "logrite.dir": dir, "logrite.easingType": easingType, "logrite.easingDir": easingDir, "logrite.length": length, "logrite.interval": interval, "logrite.onStartFunc": onStartFunc, "logrite.onFinishFunc": onFinishFunc });
        var coord = vertical ? 't' : 'l';
		logger.error(`animate(ctl=${ctl},type=${type},dir=${dir},easingType=${easingType},easingDir=${easingDir},length=${length},interval=${interval},onStartFunc=${onStartFunc},onFinishFunc=${onFinishFunc}): coord → ${coord}`, { "event.action": "animate", "logrite.ctl": ctl, "logrite.type": type, "logrite.dir": dir, "logrite.easingType": easingType, "logrite.easingDir": easingDir, "logrite.length": length, "logrite.interval": interval, "logrite.onStartFunc": onStartFunc, "logrite.onFinishFunc": onFinishFunc });
        var size = coord == 't' ? 'h' : 'w';
		logger.error(`animate(ctl=${ctl},type=${type},dir=${dir},easingType=${easingType},easingDir=${easingDir},length=${length},interval=${interval},onStartFunc=${onStartFunc},onFinishFunc=${onFinishFunc}): size → ${size}`, { "event.action": "animate", "logrite.ctl": ctl, "logrite.type": type, "logrite.dir": dir, "logrite.easingType": easingType, "logrite.easingDir": easingDir, "logrite.length": length, "logrite.interval": interval, "logrite.onStartFunc": onStartFunc, "logrite.onFinishFunc": onFinishFunc });
        var prop = coord == 't' ? 'top' : 'left';
		logger.error(`animate(ctl=${ctl},type=${type},dir=${dir},easingType=${easingType},easingDir=${easingDir},length=${length},interval=${interval},onStartFunc=${onStartFunc},onFinishFunc=${onFinishFunc}): prop → ${prop}`, { "event.action": "animate", "logrite.ctl": ctl, "logrite.type": type, "logrite.dir": dir, "logrite.easingType": easingType, "logrite.easingDir": easingDir, "logrite.length": length, "logrite.interval": interval, "logrite.onStartFunc": onStartFunc, "logrite.onFinishFunc": onFinishFunc });
        var easingFunc = dnn.getEnumByValue(dnn.motion.easingDir, easingDir) + dnn.getEnumByValue(dnn.motion.easingType, easingType);
		logger.error(`animate(ctl=${ctl},type=${type},dir=${dir},easingType=${easingType},easingDir=${easingDir},length=${length},interval=${interval},onStartFunc=${onStartFunc},onFinishFunc=${onFinishFunc}): easingFunc → ${easingFunc}`, { "event.action": "animate", "logrite.ctl": ctl, "logrite.type": type, "logrite.dir": dir, "logrite.easingType": easingType, "logrite.easingDir": easingDir, "logrite.length": length, "logrite.interval": interval, "logrite.onStartFunc": onStartFunc, "logrite.onFinishFunc": onFinishFunc });

        if (type == dnn.motion.animationType.Slide)
        {
            anims.push({prop: 'clip' + prop, begin: dims[size], finish: 0});
            anims.push({prop: prop, begin: dims[coord] - dims[size], finish: dims[coord]});
        }
        else if (type == dnn.motion.animationType.Expand)
        {
            var newDir = vertical ? 'bottom' : 'right';
			logger.error(`animate(ctl=${ctl},type=${type},dir=${dir},easingType=${easingType},easingDir=${easingDir},length=${length},interval=${interval},onStartFunc=${onStartFunc},onFinishFunc=${onFinishFunc}): newDir → ${newDir}`, { "event.action": "animate", "logrite.ctl": ctl, "logrite.type": type, "logrite.dir": dir, "logrite.easingType": easingType, "logrite.easingDir": easingDir, "logrite.length": length, "logrite.interval": interval, "logrite.onStartFunc": onStartFunc, "logrite.onFinishFunc": onFinishFunc });
            anims.push({prop: 'clip' + newDir, begin: 0, finish: dims[size]});    //FIX!
        }
        else if (type == dnn.motion.animationType.Diagonal)
        {
            size = dims.h > dims.w ? 'h' : 'w';
            anims.push({prop: 'clipbottomright', begin: 0, finish: dims[size]});
        }
        else if (type == dnn.motion.animationType.ReverseDiagonal)
        {
            size = dims.h > dims.w ? 'h' : 'w';
            anims.push({prop: 'cliptopleft', begin: dims[size], finish: 0});
        }                    
        
        for (var i=0; i<anims.length; i++)
        {
            if (expand) //swap begin and finish
                dnn.dom.doTween(ctl.id + i, ctl, anims[i].prop, dnn.easing[easingFunc], anims[i].begin, anims[i].finish, length, interval, 'px', onStartFunc, onFinishFunc);
            else //swap begin and finish
                dnn.dom.doTween(ctl.id + i, ctl, anims[i].prop, dnn.easing[easingFunc], anims[i].finish, anims[i].begin, length, interval, 'px', onStartFunc, onFinishFunc);
				logger.warn(`<<< Exiting animate(ctl=${ctl},type=${type},dir=${dir},easingType=${easingType},easingDir=${easingDir},length=${length},interval=${interval},onStartFunc=${onStartFunc},onFinishFunc=${onFinishFunc})`, { "event.action": "animate", "logrite.ctl": ctl, "logrite.type": type, "logrite.dir": dir, "logrite.easingType": easingType, "logrite.easingDir": easingDir, "logrite.length": length, "logrite.interval": interval, "logrite.onStartFunc": onStartFunc, "logrite.onFinishFunc": onFinishFunc });
        }
    },
    
    doTween: function(id, ctl, prop, func, begin, finish, duration, interval, suffix, onStartFunc, onFinishFunc)
    {
		logger.error(`>>> Entering doTween(id=${id},ctl=${ctl},prop=${prop},func=${func},begin=${begin},finish=${finish},duration=${duration},interval=${interval},suffix=${suffix},onStartFunc=${onStartFunc},onFinishFunc=${onFinishFunc})`, { "event.action": "doTween", "logrite.id": id, "logrite.ctl": ctl, "logrite.prop": prop, "logrite.func": func, "logrite.begin": begin, "logrite.finish": finish, "logrite.duration": duration, "logrite.interval": interval, "logrite.suffix": suffix, "logrite.onStartFunc": onStartFunc, "logrite.onFinishFunc": onFinishFunc });
        var tween = this.tweens[id];
		logger.error(`doTween(id=${id},ctl=${ctl},prop=${prop},func=${func},begin=${begin},finish=${finish},duration=${duration},interval=${interval},suffix=${suffix},onStartFunc=${onStartFunc},onFinishFunc=${onFinishFunc}): tween → ${tween}`, { "event.action": "doTween", "logrite.id": id, "logrite.ctl": ctl, "logrite.prop": prop, "logrite.func": func, "logrite.begin": begin, "logrite.finish": finish, "logrite.duration": duration, "logrite.interval": interval, "logrite.suffix": suffix, "logrite.onStartFunc": onStartFunc, "logrite.onFinishFunc": onFinishFunc });
        if (tween)
        {
            tween.stop();
            tween.dispose();
            tween=null;
        }
        tween = new dnn.tween(id, ctl.style, prop, func, begin, finish, duration);
        this.tweens[id] = tween;
        if (suffix)
            tween.set_Suffix(suffix);
        if (interval)
            tween.set_Interval(interval);
        tween.addEventHandler('onMotionStarted', dnn.dom.onMotionStarted);
        if (onStartFunc)
            tween.addEventHandler('onMotionStarted', onStartFunc);
        tween.addEventHandler('onMotionFinished', dnn.dom.onMotionFinished);
        if (onFinishFunc)
            tween.addEventHandler('onMotionFinished', onFinishFunc);
        tween.start();
        
		logger.warn(`<<< Exiting doTween(id=${id},ctl=${ctl},prop=${prop},func=${func},begin=${begin},finish=${finish},duration=${duration},interval=${interval},suffix=${suffix},onStartFunc=${onStartFunc},onFinishFunc=${onFinishFunc})`, { "event.action": "doTween", "logrite.id": id, "logrite.ctl": ctl, "logrite.prop": prop, "logrite.func": func, "logrite.begin": begin, "logrite.finish": finish, "logrite.duration": duration, "logrite.interval": interval, "logrite.suffix": suffix, "logrite.onStartFunc": onStartFunc, "logrite.onFinishFunc": onFinishFunc });
        return tween;
    },
    onMotionFinished: function(arg)
    {
		logger.error(`>>> Entering onMotionFinished(arg=${arg})`, { "event.action": "onMotionFinished", "logrite.arg": arg });
        dnn.dom.tweens[arg.id] = null;
        arg.dispose();
		logger.warn(`<<< Exiting onMotionFinished(arg=${arg})`, { "event.action": "onMotionFinished", "logrite.arg": arg });
        //window.status = 'finished';
    },
    
    onMotionStarted: function(arg)
    {
		logger.error(`>>> Entering onMotionStarted(arg=${arg})`, { "event.action": "onMotionStarted", "logrite.arg": arg });
        //window.status = 'started';
		logger.warn(`<<< Exiting onMotionStarted(arg=${arg})`, { "event.action": "onMotionStarted", "logrite.arg": arg });
    }
    
});


Type.registerNamespace('dnn.motion');
/*
TERMS OF USE - EASING EQUATIONS 

Open source under the BSD License. 

Copyright © 2001 Robert Penner
All rights reserved.

Redistribution and use in source and binary forms, with or without modification, are permitted provided that the following conditions are met:

Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer. 
Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution. 
Neither the name of the author nor the names of contributors may be used to endorse or promote products derived from this software without specific prior written permission. 
THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
*/

/* Easing functions taken from Robert Penner's site http://www.robertpenner.com/easing/ and converted to be included in dnn.motion namespace */
/* logic for motion class based off of sample chapter http://www.robertpenner.com/easing/penner_chapter7_tweening.pdf */

dnn.motion = function(id, obj, prop, begin, duration)
{
    this.id = id;
    this._obj = obj;
    this._prop = prop;
    this._begin = begin;
    this._duration = duration;
    this._interval = 10;   
    this._finish = 0;
    this._pos = 0;
    this._prevPos = 0;
    this._time = 0;
    this._position = 0;
    this._startTime = 0;

    this._looping = false;
    this._suffix = 'px';
    this._listeners = [];
    this._events = new Sys.EventHandlerList();
       
    //this.rewind();
    //this.start();   //callnow???
}

dnn.motion.prototype = 
{
    //properties
    get_Position: function() 
    {
		logger.error('>>> Entering get_Position()', { "event.action": "get_Position" });
	    //overridden
		logger.warn('<<< Exiting get_Position()', { "event.action": "get_Position" });
    },

    //changes the position of the motion
    set_Position: function(val) 
    {
		logger.error(`>>> Entering set_Position(val=${val})`, { "event.action": "set_Position", "logrite.val": val });
        this._prevPos = val;
        var newVal = Math.round(val) + this._suffix;
		logger.error(`set_Position(val=${val}): newVal → ${newVal}`, { "event.action": "set_Position", "logrite.val": val });
		logger.debug(`set_Position(val=${val}): this._prevPos → ${this._prevPos}`, { "event.action": "set_Position", "logrite.val": val });
	    if (this._prop.indexOf('clip') == 0)
	    {
	        var clip = this._getClipObject();
			logger.error(`set_Position(val=${val}): clip → ${clip}`, { "event.action": "set_Position", "logrite.val": val });
	        var clips = this._getClips(this._prop);
			logger.error(`set_Position(val=${val}): clips → ${clips}`, { "event.action": "set_Position", "logrite.val": val });
	        for (var i=0; i<clips.length; i++)
	            clip[clips[i]] = newVal;
    	    this._obj.clip = 'rect(' + clip.cliptop + ',' + clip.clipright + ',' + clip.clipbottom + ',' + clip.clipleft + ')';
	    }
	    else
	        this._obj[this._prop] = newVal;
	    
	    this._pos = val;
	    this.raiseEvent('onMotionChanged');
		logger.debug(`set_Position(val=${val}): this._pos → ${this._pos}`, { "event.action": "set_Position", "logrite.val": val });
    }, 

    get_PrevPosition: function()
    {
		logger.error('>>> Entering get_PrevPosition()', { "event.action": "get_PrevPosition" });
		logger.warn('<<< Exiting get_PrevPosition()', { "event.action": "get_PrevPosition" });
        return this._prevPos;
    },

    //changes the current time of the Motion
    set_Time: function(val) 
    {
		logger.error(`>>> Entering set_Time(val=${val})`, { "event.action": "set_Time", "logrite.val": val });
	    if (val > this._duration) 
	    {
		    if (this._looping) 
		    {
			    this.rewind(val - this._duration);
			    this._update();
			    this.raiseEvent('onMotionLooped');
		    } 
		    else 
		    {
			    this._time = this._duration;
			    this._update();
				logger.debug(`set_Time(val=${val}): this._time → ${this._time}`, { "event.action": "set_Time", "logrite.val": val });
			    this.stop();
			    this.raiseEvent('onMotionFinished');
		    }
	    } 
	    else if (val < 0) 
	    {
		    this.rewind();
		    this._update();
	    } 
	    else 
	    {
		    this._time = val;
		    this._update();
			logger.warn(`<<< Exiting set_Time(val=${val})`, { "event.action": "set_Time", "logrite.val": val });
	    }
    }, 

    //returns the currently stored time
    get_Time: function() {return this._time;}, 
    set_Begin: function(val) {this._begin = val;},
    get_Begin: function() {return this._begin;},
    //todo: implement duration < 0 = infinity
    set_Duration: function(val) {this._duration = val;},
    get_Duration: function() {return this._duration;}, 
    set_Interval: function(val) {this._interval = val;},
    get_Interval: function() {return this._interval;}, 
    set_Looping: function(val) {this._looping = val;},
    get_Looping: function() {return this._looping;},
    set_Object: function(val) {this._obj = val;},
    get_Object: function() {return this._obj;},
    set_Property: function(val) {this._prop = val;},
    get_Property: function() {return this._prop;},
    set_Suffix: function(val) {this._suffix = val;},
    get_Suffix: function() {return this._suffix;},

    get_Timer: function()
    {
		logger.error('>>> Entering get_Timer()', { "event.action": "get_Timer" });
		logger.warn('<<< Exiting get_Timer()', { "event.action": "get_Timer" });
	    return new Date().getTime() - this._time;
    },

    //Methods
    start: function() 
    {
		logger.error('>>> Entering start()', { "event.action": "start" });
        this._fixTime(); //new
	    this.isPlaying = true;
	    this._onEnterFrame();
		logger.debug(`start(): this.isPlaying → ${this.isPlaying}`, { "event.action": "start" });
	    this.raiseEvent('onMotionStarted');
		logger.warn('<<< Exiting start()', { "event.action": "start" });
    },

    stop: function() 
    {
		logger.error('>>> Entering stop()', { "event.action": "stop" });
        dnn.cancelDelay(this.id);
        this.isPlaying = false;
	    this.raiseEvent('onMotionStopped');
		logger.debug(`stop(): this.isPlaying → ${this.isPlaying}`, { "event.action": "stop" });
    },

    resume: function()
    {
		logger.error('>>> Entering resume()', { "event.action": "resume" });
	    this._fixTime();
	    this.start();
		logger.warn('<<< Exiting resume()', { "event.action": "resume" });
    },

    rewind: function(time) 
    {
		logger.error(`>>> Entering rewind(time=${time})`, { "event.action": "rewind", "logrite.time": time });
	    this._time = (time == null ? 1 : time);
	    this._fixTime();
		logger.warn(`<<< Exiting rewind(time=${time})`, { "event.action": "rewind", "logrite.time": time });
    },

    fforward: function() 
    {
		logger.error('>>> Entering fforward()', { "event.action": "fforward" });
        this.set_Time(this._duration);
	    this._fixTime();
		logger.warn('<<< Exiting fforward()', { "event.action": "fforward" });
    },

    nextFrame: function() 
    {
		logger.error('>>> Entering nextFrame()', { "event.action": "nextFrame" });
	    this.set_Time((this.get_Timer() - this._startTime) / 1000); 
		logger.warn('<<< Exiting nextFrame()', { "event.action": "nextFrame" });
    },

    prevFrame: function()
    {
		logger.error('>>> Entering prevFrame()', { "event.action": "prevFrame" });
        this.set_Time(this._time - 1);
		logger.warn('<<< Exiting prevFrame()', { "event.action": "prevFrame" });
    },

    dispose: function()
    {
		logger.error('>>> Entering dispose()', { "event.action": "dispose" });
        this._obj = null;
        this._listeners = null;
		logger.debug(`dispose(): this._obj → ${this._obj}`, { "event.action": "dispose" });
		logger.debug(`dispose(): this._listeners → ${this._listeners}`, { "event.action": "dispose" });
        this._events = null;
		logger.debug(`dispose(): this._func → ${this._func}`, { "event.action": "dispose" });
    },

    //for debugging
    toString: function()
    {
		logger.error('>>> Entering toString()', { "event.action": "toString" });
		logger.warn('<<< Exiting toString()', { "event.action": "toString" });
        return '[Motion prop=' + this._prop + ' t=' + this._time + ' pos=' + this._pos + ']';
    },


    addEventHandler: function (name, handler) 
    {
        this._events.addHandler(name, handler);
		logger.warn(`<<< Exiting addEventHandler(name=${name},handler=${handler})`, { "event.action": "addEventHandler", "logrite.name": name, "logrite.handler": handler });
    },

    removeEventHandler: function (name, handler) 
    {
        this._events.removeHandler(name, handler);
		logger.warn(`<<< Exiting removeEventHandler(name=${name},handler=${handler})`, { "event.action": "removeEventHandler", "logrite.name": name, "logrite.handler": handler });
    },

    raiseEvent: function(eventName) 
    {
		logger.error(`>>> Entering raiseEvent(eventName=${eventName})`, { "event.action": "raiseEvent", "logrite.eventName": eventName });
        if (!this._events) return;
        var handler = this._events.getHandler(eventName);
		logger.error(`raiseEvent(eventName=${eventName}): handler → ${handler}`, { "event.action": "raiseEvent", "logrite.eventName": eventName });
        if (handler) {
            handler(this, Sys.EventArgs.Empty);
			logger.warn(`<<< Exiting raiseEvent(eventName=${eventName})`, { "event.action": "raiseEvent", "logrite.eventName": eventName });
        }
    },

    _onEnterFrame: function() 
    {
		logger.error('>>> Entering _onEnterFrame()', { "event.action": "_onEnterFrame" });
	    if(this.isPlaying) 
	    {
		    this.nextFrame();
		    //window.setTimeout(dnn.createDelegate(this, this._onEnterFrame), 0);
		    dnn.doDelay(this.id, this._interval, dnn.createDelegate(this, this._onEnterFrame));
			logger.warn('<<< Exiting _onEnterFrame()', { "event.action": "_onEnterFrame" });
	    }
    },

    _update: function() 
    {
		logger.error('>>> Entering _update()', { "event.action": "_update" });
	    this.set_Position(this.get_Position(this._time));
		logger.warn('<<< Exiting _update()', { "event.action": "_update" });
    },

    _fixTime: function()
    {
		logger.error('>>> Entering _fixTime()', { "event.action": "_fixTime" });
	    this._startTime = this.get_Timer() - this._time;   
		logger.warn('<<< Exiting _fixTime()', { "event.action": "_fixTime" });
    },

    //returns easy property to manage clip property movement by top, bottom, left, right
    _getClipObject: function() 
    {
		logger.error('>>> Entering _getClipObject()', { "event.action": "_getClipObject" });
        var o = {};
        o.cliptop = 'auto';
        o.clipright = 'auto';
		logger.debug(`_getClipObject(): o.cliptop → ${o.cliptop}`, { "event.action": "_getClipObject" });
		logger.debug(`_getClipObject(): o.clipright → ${o.clipright}`, { "event.action": "_getClipObject" });
		logger.debug(`_getClipObject(): o.clipleft → ${o.clipleft}`, { "event.action": "_getClipObject" });
        o.clipbottom = 'auto';
        o.clipleft = 'auto';
		logger.warn('<<< Exiting _getClipObject()', { "event.action": "_getClipObject" });
        return o;
    },

    _getClips: function(prop) 
    {
        var o = [];
        if (prop.indexOf('top') > -1)
            o.push('cliptop');
        if (prop.indexOf('left') > -1)
            o.push('clipleft');
        if (prop.indexOf('bottom') > -1)
            o.push('clipbottom');
        if (prop.indexOf('right') > -1)
            o.push('clipright');
		logger.warn(`<<< Exiting _getClips(prop=${prop})`, { "event.action": "_getClips", "logrite.prop": prop });
        return o;
    }

}

dnn.motion.registerClass('dnn.motion', null, Sys.IDisposable);

dnn.motion.animationType = function(){};
dnn.motion.animationType.prototype = {
    None: 0,
    Slide: 1,
    Expand: 2,
    Diagonal: 3,
    ReverseDiagonal: 4
}
dnn.motion.animationType.registerEnum("dnn.motion.animationType");

dnn.motion.animationDir = function(){};
dnn.motion.animationDir.prototype = {
    Up: 0,
    Down: 1,
    Left: 2,
    Right: 3
}
dnn.motion.animationDir.registerEnum("dnn.motion.animationDir");

dnn.motion.easingType = function(){};
dnn.motion.easingType.prototype = {
    Bounce: 0,
    Circ: 1,
    Cubic: 2,
    Expo: 3,
    Quad: 4,
    Quint: 5,
    Quart: 6,
    Sine: 7
}
dnn.motion.easingType.registerEnum("dnn.motion.easingType");

dnn.motion.easingDir = function(){};
dnn.motion.easingDir.prototype = {
    easeIn: 0,
    easeOut: 1,
    easeInOut: 2    
}
dnn.motion.easingDir.registerEnum("dnn.motion.easingDir");


dnn.tween = function(id, obj, prop, func, begin, finish, duration) 
{
    dnn.tween.initializeBase(this, [id, obj, prop, begin, duration]);	
    this._func = func; 
    this._change = 0;
    this.set_Finish(finish);
}

dnn.tween.prototype = 
{
    set_Function: function(val) {this._func = val;},
    get_Function: function() {return this._func;},
    set_Change: function(val) {this._change = val;},
    get_Change: function() {return this._change;},
    
    //is the finishing property of the tween
    //dependent upon change;
    set_Finish: function(val) 
    {
		logger.error(`>>> Entering set_Finish(val=${val})`, { "event.action": "set_Finish", "logrite.val": val });
	    this._change = val - this._begin;
		logger.debug(`set_Finish(val=${val}): this._change → ${this._change}`, { "event.action": "set_Finish", "logrite.val": val });
    },

    get_Finish: function() 
    {
		logger.error('>>> Entering get_Finish()', { "event.action": "get_Finish" });
		logger.warn('<<< Exiting get_Finish()', { "event.action": "get_Finish" });
	    return this._begin + this._change;
    },

    get_Position: function() 
    {
	    return this._func(this._time, this._begin, this._change, this._duration);
    },

    continueTo: function(finish, duration)
    {
		logger.error(`>>> Entering continueTo(finish=${finish},duration=${duration})`, { "event.action": "continueTo", "logrite.finish": finish, "logrite.duration": duration });
	    this.set_Begin(this.get_Position());
	    this.set_Finish(finish);
	    this.set_Duration(duration);
	    this.start();
		logger.warn(`<<< Exiting continueTo(finish=${finish},duration=${duration})`, { "event.action": "continueTo", "logrite.finish": finish, "logrite.duration": duration });
    },

    dispose: function()
    {
        dnn.tween.callBaseMethod(this, 'dispose');
        this._func = null;
    }

}

dnn.tween.registerClass('dnn.tween', dnn.motion);

Type.registerNamespace('dnn.easing');
dnn.extend(dnn.easing, {
    // simple linear tweening - no easing
    // t: current time, b: beginning value, c: change in value, d: duration
    linearTween: function (t, b, c, d) {
		logger.error(`>>> Entering linearTween(t=${t},b=${b},c=${c},d=${d})`, { "event.action": "linearTween", "logrite.t": t, "logrite.b": b, "logrite.c": c, "logrite.d": d });
		logger.warn(`<<< Exiting linearTween(t=${t},b=${b},c=${c},d=${d})`, { "event.action": "linearTween", "logrite.t": t, "logrite.b": b, "logrite.c": c, "logrite.d": d });
	    return c*t/d + b;
    },

    ///////////// QUADRATIC EASING: t^2 ///////////////////
    // quadratic easing in - accelerating from zero velocity
    // t: current time, b: beginning value, c: change in value, d: duration
    // t and d can be in frames or seconds/milliseconds
    easeInQuad: function (t, b, c, d) {
		logger.error(`>>> Entering easeInQuad(t=${t},b=${b},c=${c},d=${d})`, { "event.action": "easeInQuad", "logrite.t": t, "logrite.b": b, "logrite.c": c, "logrite.d": d });
		logger.warn(`<<< Exiting easeInQuad(t=${t},b=${b},c=${c},d=${d})`, { "event.action": "easeInQuad", "logrite.t": t, "logrite.b": b, "logrite.c": c, "logrite.d": d });
	    return c*(t/=d)*t + b;
    },

    // quadratic easing out - decelerating to zero velocity
    easeOutQuad: function (t, b, c, d) {
		logger.error(`>>> Entering easeOutQuad(t=${t},b=${b},c=${c},d=${d})`, { "event.action": "easeOutQuad", "logrite.t": t, "logrite.b": b, "logrite.c": c, "logrite.d": d });
		logger.warn(`<<< Exiting easeOutQuad(t=${t},b=${b},c=${c},d=${d})`, { "event.action": "easeOutQuad", "logrite.t": t, "logrite.b": b, "logrite.c": c, "logrite.d": d });
	    return -c *(t/=d)*(t-2) + b;
    },

    // quadratic easing in/out - acceleration until halfway, then deceleration
    easeInOutQuad: function (t, b, c, d) {
		logger.error(`>>> Entering easeInOutQuad(t=${t},b=${b},c=${c},d=${d})`, { "event.action": "easeInOutQuad", "logrite.t": t, "logrite.b": b, "logrite.c": c, "logrite.d": d });
	    if ((t/=d/2) < 1) return c/2*t*t + b;
		logger.warn(`<<< Exiting easeInOutQuad(t=${t},b=${b},c=${c},d=${d})`, { "event.action": "easeInOutQuad", "logrite.t": t, "logrite.b": b, "logrite.c": c, "logrite.d": d });
	    return -c/2 * ((--t)*(t-2) - 1) + b;
    },


    ///////////// CUBIC EASING: t^3 ///////////////////////

    // cubic easing in - accelerating from zero velocity
    // t: current time, b: beginning value, c: change in value, d: duration
    // t and d can be frames or seconds/milliseconds
    easeInCubic: function (t, b, c, d) {
		logger.error(`>>> Entering easeInCubic(t=${t},b=${b},c=${c},d=${d})`, { "event.action": "easeInCubic", "logrite.t": t, "logrite.b": b, "logrite.c": c, "logrite.d": d });
		logger.warn(`<<< Exiting easeInCubic(t=${t},b=${b},c=${c},d=${d})`, { "event.action": "easeInCubic", "logrite.t": t, "logrite.b": b, "logrite.c": c, "logrite.d": d });
	    return c*(t/=d)*t*t + b;
    },

    // cubic easing out - decelerating to zero velocity
    easeOutCubic: function (t, b, c, d) {
		logger.error(`>>> Entering easeOutCubic(t=${t},b=${b},c=${c},d=${d})`, { "event.action": "easeOutCubic", "logrite.t": t, "logrite.b": b, "logrite.c": c, "logrite.d": d });
		logger.warn(`<<< Exiting easeOutCubic(t=${t},b=${b},c=${c},d=${d})`, { "event.action": "easeOutCubic", "logrite.t": t, "logrite.b": b, "logrite.c": c, "logrite.d": d });
	    return c*((t=t/d-1)*t*t + 1) + b;
    },

    // cubic easing in/out - acceleration until halfway, then deceleration
    easeInOutCubic: function (t, b, c, d) {
		logger.error(`>>> Entering easeInOutCubic(t=${t},b=${b},c=${c},d=${d})`, { "event.action": "easeInOutCubic", "logrite.t": t, "logrite.b": b, "logrite.c": c, "logrite.d": d });
	    if ((t/=d/2) < 1) return c/2*t*t*t + b;
		logger.warn(`<<< Exiting easeInOutCubic(t=${t},b=${b},c=${c},d=${d})`, { "event.action": "easeInOutCubic", "logrite.t": t, "logrite.b": b, "logrite.c": c, "logrite.d": d });
	    return c/2*((t-=2)*t*t + 2) + b;
    },


     ///////////// QUARTIC EASING: t^4 /////////////////////

    // quartic easing in - accelerating from zero velocity
    // t: current time, b: beginning value, c: change in value, d: duration
    // t and d can be frames or seconds/milliseconds
    easeInQuart: function (t, b, c, d) {
		logger.error(`>>> Entering easeInQuart(t=${t},b=${b},c=${c},d=${d})`, { "event.action": "easeInQuart", "logrite.t": t, "logrite.b": b, "logrite.c": c, "logrite.d": d });
		logger.warn(`<<< Exiting easeInQuart(t=${t},b=${b},c=${c},d=${d})`, { "event.action": "easeInQuart", "logrite.t": t, "logrite.b": b, "logrite.c": c, "logrite.d": d });
	    return c*(t/=d)*t*t*t + b;
    },

    // quartic easing out - decelerating to zero velocity
    easeOutQuart: function (t, b, c, d) {
		logger.error(`>>> Entering easeOutQuart(t=${t},b=${b},c=${c},d=${d})`, { "event.action": "easeOutQuart", "logrite.t": t, "logrite.b": b, "logrite.c": c, "logrite.d": d });
		logger.warn(`<<< Exiting easeOutQuart(t=${t},b=${b},c=${c},d=${d})`, { "event.action": "easeOutQuart", "logrite.t": t, "logrite.b": b, "logrite.c": c, "logrite.d": d });
	    return -c * ((t=t/d-1)*t*t*t - 1) + b;
    },

    // quartic easing in/out - acceleration until halfway, then deceleration
    easeInOutQuart: function (t, b, c, d) {
		logger.error(`>>> Entering easeInOutQuart(t=${t},b=${b},c=${c},d=${d})`, { "event.action": "easeInOutQuart", "logrite.t": t, "logrite.b": b, "logrite.c": c, "logrite.d": d });
	    if ((t/=d/2) < 1) return c/2*t*t*t*t + b;
		logger.warn(`<<< Exiting easeInOutQuart(t=${t},b=${b},c=${c},d=${d})`, { "event.action": "easeInOutQuart", "logrite.t": t, "logrite.b": b, "logrite.c": c, "logrite.d": d });
	    return -c/2 * ((t-=2)*t*t*t - 2) + b;
    },


     ///////////// QUINTIC EASING: t^5  ////////////////////

    // quintic easing in - accelerating from zero velocity
    // t: current time, b: beginning value, c: change in value, d: duration
    // t and d can be frames or seconds/milliseconds
    easeInQuint: function (t, b, c, d) {
		logger.error(`>>> Entering easeInQuint(t=${t},b=${b},c=${c},d=${d})`, { "event.action": "easeInQuint", "logrite.t": t, "logrite.b": b, "logrite.c": c, "logrite.d": d });
		logger.warn(`<<< Exiting easeInQuint(t=${t},b=${b},c=${c},d=${d})`, { "event.action": "easeInQuint", "logrite.t": t, "logrite.b": b, "logrite.c": c, "logrite.d": d });
	    return c*(t/=d)*t*t*t*t + b;
    },

    // quintic easing out - decelerating to zero velocity
    easeOutQuint: function (t, b, c, d) {
		logger.error(`>>> Entering easeOutQuint(t=${t},b=${b},c=${c},d=${d})`, { "event.action": "easeOutQuint", "logrite.t": t, "logrite.b": b, "logrite.c": c, "logrite.d": d });
		logger.warn(`<<< Exiting easeOutQuint(t=${t},b=${b},c=${c},d=${d})`, { "event.action": "easeOutQuint", "logrite.t": t, "logrite.b": b, "logrite.c": c, "logrite.d": d });
	    return c*((t=t/d-1)*t*t*t*t + 1) + b;
    },

    // quintic easing in/out - acceleration until halfway, then deceleration
    easeInOutQuint: function (t, b, c, d) {
		logger.error(`>>> Entering easeInOutQuint(t=${t},b=${b},c=${c},d=${d})`, { "event.action": "easeInOutQuint", "logrite.t": t, "logrite.b": b, "logrite.c": c, "logrite.d": d });
	    if ((t/=d/2) < 1) return c/2*t*t*t*t*t + b;
		logger.warn(`<<< Exiting easeInOutQuint(t=${t},b=${b},c=${c},d=${d})`, { "event.action": "easeInOutQuint", "logrite.t": t, "logrite.b": b, "logrite.c": c, "logrite.d": d });
	    return c/2*((t-=2)*t*t*t*t + 2) + b;
    },



     ///////////// SINUSOIDAL EASING: sin(t) ///////////////

    // sinusoidal easing in - accelerating from zero velocity
    // t: current time, b: beginning value, c: change in position, d: duration
    easeInSine: function (t, b, c, d) {
		logger.error(`>>> Entering easeInSine(t=${t},b=${b},c=${c},d=${d})`, { "event.action": "easeInSine", "logrite.t": t, "logrite.b": b, "logrite.c": c, "logrite.d": d });
		logger.warn(`<<< Exiting easeInSine(t=${t},b=${b},c=${c},d=${d})`, { "event.action": "easeInSine", "logrite.t": t, "logrite.b": b, "logrite.c": c, "logrite.d": d });
	    return -c * Math.cos(t/d * (3.14/2)) + c + b;
    },

    // sinusoidal easing out - decelerating to zero velocity
    easeOutSine: function (t, b, c, d) {
		logger.error(`>>> Entering easeOutSine(t=${t},b=${b},c=${c},d=${d})`, { "event.action": "easeOutSine", "logrite.t": t, "logrite.b": b, "logrite.c": c, "logrite.d": d });
		logger.warn(`<<< Exiting easeOutSine(t=${t},b=${b},c=${c},d=${d})`, { "event.action": "easeOutSine", "logrite.t": t, "logrite.b": b, "logrite.c": c, "logrite.d": d });
	    return c * Math.sin(t/d * (3.14/2)) + b;
    },

    // sinusoidal easing in/out - accelerating until halfway, then decelerating
    easeInOutSine: function (t, b, c, d) {
		logger.error(`>>> Entering easeInOutSine(t=${t},b=${b},c=${c},d=${d})`, { "event.action": "easeInOutSine", "logrite.t": t, "logrite.b": b, "logrite.c": c, "logrite.d": d });
		logger.warn(`<<< Exiting easeInOutSine(t=${t},b=${b},c=${c},d=${d})`, { "event.action": "easeInOutSine", "logrite.t": t, "logrite.b": b, "logrite.c": c, "logrite.d": d });
	    return -c/2 * (Math.cos(3.14*t/d) - 1) + b;
    },

     ///////////// EXPONENTIAL EASING: 2^t /////////////////

    // exponential easing in - accelerating from zero velocity
    // t: current time, b: beginning value, c: change in position, d: duration
    easeInExpo: function (t, b, c, d) {
		logger.error(`>>> Entering easeInExpo(t=${t},b=${b},c=${c},d=${d})`, { "event.action": "easeInExpo", "logrite.t": t, "logrite.b": b, "logrite.c": c, "logrite.d": d });
		logger.warn(`<<< Exiting easeInExpo(t=${t},b=${b},c=${c},d=${d})`, { "event.action": "easeInExpo", "logrite.t": t, "logrite.b": b, "logrite.c": c, "logrite.d": d });
	    return (t==0) ? b : c * Math.pow(2, 10 * (t/d - 1)) + b;
    },

    // exponential easing out - decelerating to zero velocity
    easeOutExpo: function (t, b, c, d) {
		logger.error(`>>> Entering easeOutExpo(t=${t},b=${b},c=${c},d=${d})`, { "event.action": "easeOutExpo", "logrite.t": t, "logrite.b": b, "logrite.c": c, "logrite.d": d });
		logger.warn(`<<< Exiting easeOutExpo(t=${t},b=${b},c=${c},d=${d})`, { "event.action": "easeOutExpo", "logrite.t": t, "logrite.b": b, "logrite.c": c, "logrite.d": d });
	    return (t==d) ? b+c : c * (-Math.pow(2, -10 * t/d) + 1) + b;
    },

    // exponential easing in/out - accelerating until halfway, then decelerating
    easeInOutExpo: function (t, b, c, d) {
		logger.error(`>>> Entering easeInOutExpo(t=${t},b=${b},c=${c},d=${d})`, { "event.action": "easeInOutExpo", "logrite.t": t, "logrite.b": b, "logrite.c": c, "logrite.d": d });
	    if (t==0) return b;
	    if (t==d) return b+c;
	    if ((t/=d/2) < 1) return c/2 * Math.pow(2, 10 * (t - 1)) + b;
		logger.warn(`<<< Exiting easeInOutExpo(t=${t},b=${b},c=${c},d=${d})`, { "event.action": "easeInOutExpo", "logrite.t": t, "logrite.b": b, "logrite.c": c, "logrite.d": d });
	    return c/2 * (-Math.pow(2, -10 * --t) + 2) + b;
    },


     /////////// CIRCULAR EASING: sqrt(1-t^2) //////////////

    // circular easing in - accelerating from zero velocity
    // t: current time, b: beginning value, c: change in position, d: duration
    easeInCirc: function (t, b, c, d) {
		logger.error(`>>> Entering easeInCirc(t=${t},b=${b},c=${c},d=${d})`, { "event.action": "easeInCirc", "logrite.t": t, "logrite.b": b, "logrite.c": c, "logrite.d": d });
		logger.warn(`<<< Exiting easeInCirc(t=${t},b=${b},c=${c},d=${d})`, { "event.action": "easeInCirc", "logrite.t": t, "logrite.b": b, "logrite.c": c, "logrite.d": d });
	    return -c * (Math.sqrt(1 - (t/=d)*t) - 1) + b;
    },

    // circular easing out - decelerating to zero velocity
    easeOutCirc: function (t, b, c, d) {
		logger.error(`>>> Entering easeOutCirc(t=${t},b=${b},c=${c},d=${d})`, { "event.action": "easeOutCirc", "logrite.t": t, "logrite.b": b, "logrite.c": c, "logrite.d": d });
		logger.warn(`<<< Exiting easeOutCirc(t=${t},b=${b},c=${c},d=${d})`, { "event.action": "easeOutCirc", "logrite.t": t, "logrite.b": b, "logrite.c": c, "logrite.d": d });
	    return c * Math.sqrt(1 - (t=t/d-1)*t) + b;
    },

    // circular easing in/out - acceleration until halfway, then deceleration
    easeInOutCirc: function (t, b, c, d) {
		logger.error(`>>> Entering easeInOutCirc(t=${t},b=${b},c=${c},d=${d})`, { "event.action": "easeInOutCirc", "logrite.t": t, "logrite.b": b, "logrite.c": c, "logrite.d": d });
	    if ((t/=d/2) < 1) return -c/2 * (Math.sqrt(1 - t*t) - 1) + b;
		logger.warn(`<<< Exiting easeInOutCirc(t=${t},b=${b},c=${c},d=${d})`, { "event.action": "easeInOutCirc", "logrite.t": t, "logrite.b": b, "logrite.c": c, "logrite.d": d });
	    return c/2 * (Math.sqrt(1 - (t-=2)*t) + 1) + b;
    },

     /////////// BOUNCE EASING: exponentially decaying parabolic bounce  //////////////

    // bounce easing in
    // t: current time, b: beginning value, c: change in position, d: duration
    easeInBounce: function (t, b, c, d) {
		logger.error(`>>> Entering easeInBounce(t=${t},b=${b},c=${c},d=${d})`, { "event.action": "easeInBounce", "logrite.t": t, "logrite.b": b, "logrite.c": c, "logrite.d": d });
		logger.warn(`<<< Exiting easeInBounce(t=${t},b=${b},c=${c},d=${d})`, { "event.action": "easeInBounce", "logrite.t": t, "logrite.b": b, "logrite.c": c, "logrite.d": d });
	    return c - dnn.easing.easeOutBounce(d-t, 0, c, d) + b;
    },

    // bounce easing out
    easeOutBounce: function (t, b, c, d) {
		logger.error(`>>> Entering easeOutBounce(t=${t},b=${b},c=${c},d=${d})`, { "event.action": "easeOutBounce", "logrite.t": t, "logrite.b": b, "logrite.c": c, "logrite.d": d });
	    if ((t/=d) < (1/2.75)) {
		    return c*(7.5625*t*t) + b;
	    } else if (t < (2/2.75)) {
		    return c*(7.5625*(t-=(1.5/2.75))*t + .75) + b;
	    } else if (t < (2.5/2.75)) {
		    return c*(7.5625*(t-=(2.25/2.75))*t + .9375) + b;
	    } else {
			logger.warn(`<<< Exiting easeOutBounce(t=${t},b=${b},c=${c},d=${d})`, { "event.action": "easeOutBounce", "logrite.t": t, "logrite.b": b, "logrite.c": c, "logrite.d": d });
		    return c*(7.5625*(t-=(2.625/2.75))*t + .984375) + b;
	    }
    },

    // bounce easing in/out
    easeInOutBounce: function (t, b, c, d) {
		logger.error(`>>> Entering easeInOutBounce(t=${t},b=${b},c=${c},d=${d})`, { "event.action": "easeInOutBounce", "logrite.t": t, "logrite.b": b, "logrite.c": c, "logrite.d": d });
	    if (t < d/2) return dnn.easing.easeInBounce(t*2, 0, c, d) * .5 + b;
		logger.warn(`<<< Exiting easeInOutBounce(t=${t},b=${b},c=${c},d=${d})`, { "event.action": "easeInOutBounce", "logrite.t": t, "logrite.b": b, "logrite.c": c, "logrite.d": d });
	    return dnn.easing.easeOutBounce(t*2-d, 0, c, d) * .5 + c*.5 + b;
    }
}
);

