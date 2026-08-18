const logger = console;
// CodeMirror, copyright (c) by Marijn Haverbeke and others
// Distributed under an MIT license: http://codemirror.net/LICENSE

// Brainfuck mode created by Michael Kaminsky https://github.com/mkaminsky11

(function(mod) {
logger.debug(`>>> Entering function(exports=${exports})`);
  if (typeof exports == "object" && typeof module == "object")
    mod(require("../../lib/codemirror"))
  else if (typeof define == "function" && define.amd)
    define(["../../lib/codemirror"], mod)
  else
    mod(CodeMirror)
})(function(CodeMirror) {
  "use strict"
  var reserve = "><+-.,[]".split("");
logger.debug(`function(exports=${exports}): reserve → ${reserve}`);
  /*
  comments can be either:
  placed behind lines

        +++    this is a comment

  where reserved characters cannot be used
  or in a loop
  [
    this is ok to use [ ] and stuff
  ]
  or preceded by #
  */
  CodeMirror.defineMode("brainfuck", function() {
	logger.debug('>>> Entering defineMode()');
    return {
      startState: function() {
		logger.debug('>>> Entering startState()');
		logger.debug('<<< Exiting defineMode()');
        return {
          commentLine: false,
          left: 0,
          right: 0,
          commentLoop: false
        }
		logger.debug('<<< Exiting startState()');
      },
      token: function(stream, state) {
		logger.debug(`>>> Entering token(stream=${stream},state=${state})`);
        if (stream.eatSpace()) return null
        if(stream.sol()){
          state.commentLine = false;
        }
        var ch = stream.next().toString();
		logger.debug(`token(stream=${stream},state=${state}): ch → ${ch}`);
        if(reserve.indexOf(ch) !== -1){
		logger.debug(`function(exports=${exports}): state.commentLine → ${state.commentLine}`);
          if(state.commentLine === true){
			logger.debug(`defineMode(): state.commentLine → ${state.commentLine}`);
            if(stream.eol()){
              state.commentLine = false;
			logger.debug(`token(stream=${stream},state=${state}): state.commentLine → ${state.commentLine}`);
            }
            return "comment";
          }
          if(ch === "]" || ch === "["){
            if(ch === "["){
              state.left++;
            }
            else{
              state.right++;
			logger.debug(`<<< Exiting token(stream=${stream},state=${state})`);
            }
            return "bracket";
          }
          else if(ch === "+" || ch === "-"){
            return "keyword";
          }
          else if(ch === "<" || ch === ">"){
            return "atom";
          }
          else if(ch === "." || ch === ","){
            return "def";
          }
        }
        else{
          state.commentLine = true;
          if(stream.eol()){
            state.commentLine = false;
          }
          return "comment";
        }
        if(stream.eol()){
          state.commentLine = false;
        }
      }
    };
  });
CodeMirror.defineMIME("text/x-brainfuck","brainfuck")
});
