const logger = console;
// CodeMirror, copyright (c) by Marijn Haverbeke and others
// Distributed under an MIT license: http://codemirror.net/LICENSE

/*
 * =====================================================================================
 *
 *       Filename:  mode/asterisk/asterisk.js
 *
 *    Description:  CodeMirror mode for Asterisk dialplan
 *
 *        Created:  05/17/2012 09:20:25 PM
 *       Revision:  none
 *
 *         Author:  Stas Kobzar (stas@modulis.ca),
 *        Company:  Modulis.ca Inc.
 *
 * =====================================================================================
 */

(function(mod) {
  if (typeof exports == "object" && typeof module == "object") // CommonJS
    mod(require("../../lib/codemirror"));
  else if (typeof define == "function" && define.amd) // AMD
    define(["../../lib/codemirror"], mod);
  else // Plain browser env
    mod(CodeMirror);
})(function(CodeMirror) {
"use strict";

CodeMirror.defineMode("asterisk", function() {
  var atoms    = ["exten", "same", "include","ignorepat","switch"],
      dpcmd    = ["#include","#exec"],
      apps     = [
                  "addqueuemember","adsiprog","aelsub","agentlogin","agentmonitoroutgoing","agi",
                  "alarmreceiver","amd","answer","authenticate","background","backgrounddetect",
                  "bridge","busy","callcompletioncancel","callcompletionrequest","celgenuserevent",
                  "changemonitor","chanisavail","channelredirect","chanspy","clearhash","confbridge",
                  "congestion","continuewhile","controlplayback","dahdiacceptr2call","dahdibarge",
                  "dahdiras","dahdiscan","dahdisendcallreroutingfacility","dahdisendkeypadfacility",
                  "datetime","dbdel","dbdeltree","deadagi","dial","dictate","directory","disa",
                  "dumpchan","eagi","echo","endwhile","exec","execif","execiftime","exitwhile","extenspy",
                  "externalivr","festival","flash","followme","forkcdr","getcpeid","gosub","gosubif",
                  "goto","gotoif","gotoiftime","hangup","iax2provision","ices","importvar","incomplete",
                  "ivrdemo","jabberjoin","jabberleave","jabbersend","jabbersendgroup","jabberstatus",
                  "jack","log","macro","macroexclusive","macroexit","macroif","mailboxexists","meetme",
                  "meetmeadmin","meetmechanneladmin","meetmecount","milliwatt","minivmaccmess","minivmdelete",
                  "minivmgreet","minivmmwi","minivmnotify","minivmrecord","mixmonitor","monitor","morsecode",
                  "mp3player","mset","musiconhold","nbscat","nocdr","noop","odbc","odbc","odbcfinish",
                  "originate","ospauth","ospfinish","osplookup","ospnext","page","park","parkandannounce",
                  "parkedcall","pausemonitor","pausequeuemember","pickup","pickupchan","playback","playtones",
                  "privacymanager","proceeding","progress","queue","queuelog","raiseexception","read","readexten",
                  "readfile","receivefax","receivefax","receivefax","record","removequeuemember",
                  "resetcdr","retrydial","return","ringing","sayalpha","saycountedadj","saycountednoun",
                  "saycountpl","saydigits","saynumber","sayphonetic","sayunixtime","senddtmf","sendfax",
                  "sendfax","sendfax","sendimage","sendtext","sendurl","set","setamaflags",
                  "setcallerpres","setmusiconhold","sipaddheader","sipdtmfmode","sipremoveheader","skel",
                  "slastation","slatrunk","sms","softhangup","speechactivategrammar","speechbackground",
                  "speechcreate","speechdeactivategrammar","speechdestroy","speechloadgrammar","speechprocessingsound",
                  "speechstart","speechunloadgrammar","stackpop","startmusiconhold","stopmixmonitor","stopmonitor",
                  "stopmusiconhold","stopplaytones","system","testclient","testserver","transfer","tryexec",
                  "trysystem","unpausemonitor","unpausequeuemember","userevent","verbose","vmauthenticate",
                  "vmsayname","voicemail","voicemailmain","wait","waitexten","waitfornoise","waitforring",
                  "waitforsilence","waitmusiconhold","waituntil","while","zapateller"
                 ];

  function basicToken(stream,state){
	logger.error(`>>> Entering basicToken(stream=${stream},state=${state})`, { "event.action": "basicToken", "logrite.stream": stream, "logrite.state": state });
    var cur = '';
    var ch = stream.next();
	logger.error(`basicToken(stream=${stream},state=${state}): cur → ${cur}`, { "event.action": "basicToken", "logrite.stream": stream, "logrite.state": state });
    // comment
    if(ch == ";") {
	logger.error(`basicToken(stream=${stream},state=${state}): ch → ${ch}`, { "event.action": "basicToken", "logrite.stream": stream, "logrite.state": state });
      stream.skipToEnd();
      return "comment";
    }
    // context
    if(ch == '[') {
      stream.skipTo(']');
      stream.eat(']');
      return "header";
    }
    // string
    if(ch == '"') {
      stream.skipTo('"');
      return "string";
    }
    if(ch == "'") {
      stream.skipTo("'");
      return "string-2";
    }
    // dialplan commands
    if(ch == '#') {
      stream.eatWhile(/\w/);
      cur = stream.current();
      if(dpcmd.indexOf(cur) !== -1) {
        stream.skipToEnd();
        return "strong";
      }
    }
    // application args
    if(ch == '$'){
      var ch1 = stream.peek();
	logger.error(`basicToken(stream=${stream},state=${state}): ch1 → ${ch1}`, { "event.action": "basicToken", "logrite.stream": stream, "logrite.state": state });
      if(ch1 == '{'){
        stream.skipTo('}');
        stream.eat('}');
		logger.debug(`basicToken(stream=${stream},state=${state}): state.extenStart → ${state.extenStart}`, { "event.action": "basicToken", "logrite.stream": stream, "logrite.state": state });
		logger.warn(`<<< Exiting basicToken(stream=${stream},state=${state})`, { "event.action": "basicToken", "logrite.stream": stream, "logrite.state": state });
        return "variable-3";
      }
    }
    // extension
    stream.eatWhile(/\w/);
    cur = stream.current();
    if(atoms.indexOf(cur) !== -1) {
      state.extenStart = true;
      switch(cur) {
        case 'same': state.extenSame = true; break;
        case 'include':
        case 'switch':
        case 'ignorepat':
          state.extenInclude = true;break;
        default:break;
      }
      return "atom";
    }
  }

  return {
    startState: function() {
	logger.error('>>> Entering startState()', { "event.action": "startState" });
      return {
        extenStart: false,
        extenSame:  false,
        extenInclude: false,
        extenExten: false,
        extenPriority: false,
        extenApplication: false
      };
	logger.warn('<<< Exiting startState()', { "event.action": "startState" });
    },
    token: function(stream, state) {
	logger.error(`>>> Entering token(stream=${stream},state=${state})`, { "event.action": "token", "logrite.stream": stream, "logrite.state": state });

      var cur = '';
      if(stream.eatSpace()) return null;
	logger.error(`token(stream=${stream},state=${state}): cur → ${cur}`, { "event.action": "token", "logrite.stream": stream, "logrite.state": state });
      // extension started
      if(state.extenStart){
        stream.eatWhile(/[^\s]/);
        cur = stream.current();
        if(/^=>?$/.test(cur)){
          state.extenExten = true;
		logger.debug(`token(stream=${stream},state=${state}): state.extenExten → ${state.extenExten}`, { "event.action": "token", "logrite.stream": stream, "logrite.state": state });
          state.extenStart = false;
		logger.debug(`token(stream=${stream},state=${state}): state.extenStart → ${state.extenStart}`, { "event.action": "token", "logrite.stream": stream, "logrite.state": state });
          return "strong";
        } else {
          state.extenStart = false;
          stream.skipToEnd();
          return "error";
        }
      } else if(state.extenExten) {
        // set exten and priority
        state.extenExten = false;
        state.extenPriority = true;
		logger.debug(`token(stream=${stream},state=${state}): state.extenPriority → ${state.extenPriority}`, { "event.action": "token", "logrite.stream": stream, "logrite.state": state });
        stream.eatWhile(/[^,]/);
        if(state.extenInclude) {
          stream.skipToEnd();
          state.extenPriority = false;
          state.extenInclude = false;
		logger.debug(`token(stream=${stream},state=${state}): state.extenInclude → ${state.extenInclude}`, { "event.action": "token", "logrite.stream": stream, "logrite.state": state });
        }
        if(state.extenSame) {
          state.extenPriority = false;
          state.extenSame = false;
		logger.debug(`token(stream=${stream},state=${state}): state.extenSame → ${state.extenSame}`, { "event.action": "token", "logrite.stream": stream, "logrite.state": state });
          state.extenApplication = true;
		logger.debug(`token(stream=${stream},state=${state}): state.extenApplication → ${state.extenApplication}`, { "event.action": "token", "logrite.stream": stream, "logrite.state": state });
        }
        return "tag";
      } else if(state.extenPriority) {
        state.extenPriority = false;
        state.extenApplication = true;
        stream.next(); // get comma
        if(state.extenSame) return null;
        stream.eatWhile(/[^,]/);
        return "number";
      } else if(state.extenApplication) {
        stream.eatWhile(/,/);
        cur = stream.current();
        if(cur === ',') return null;
        stream.eatWhile(/\w/);
        cur = stream.current().toLowerCase();
        state.extenApplication = false;
        if(apps.indexOf(cur) !== -1){
          return "def strong";
        }
      } else{
	logger.warn(`<<< Exiting token(stream=${stream},state=${state})`, { "event.action": "token", "logrite.stream": stream, "logrite.state": state });
        return basicToken(stream,state);
      }

      return null;
    }
  };
});

CodeMirror.defineMIME("text/x-asterisk", "asterisk");

});
