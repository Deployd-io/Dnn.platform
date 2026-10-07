const logger = console;
// CodeMirror, copyright (c) by Marijn Haverbeke and others
// Distributed under an MIT license: http://codemirror.net/LICENSE

/*jshint unused:true, eqnull:true, curly:true, bitwise:true */
/*jshint undef:true, latedef:true, trailing:true */
/*global CodeMirror:true */

// erlang mode.
// tokenizer -> token types -> CodeMirror styles
// tokenizer maintains a parse stack
// indenter uses the parse stack

// TODO indenter:
//   bit syntax
//   old guard/bif/conversion clashes (e.g. "float/1")
//   type/spec/opaque

(function(mod) {
  if (typeof exports == "object" && typeof module == "object") // CommonJS
    mod(require("../../lib/codemirror"));
  else if (typeof define == "function" && define.amd) // AMD
    define(["../../lib/codemirror"], mod);
  else // Plain browser env
    mod(CodeMirror);
})(function(CodeMirror) {
"use strict";

CodeMirror.defineMIME("text/x-erlang", "erlang");

CodeMirror.defineMode("erlang", function(cmCfg) {
  "use strict";

/////////////////////////////////////////////////////////////////////////////
// constants

  var typeWords = [
    "-type", "-spec", "-export_type", "-opaque"];

  var keywordWords = [
    "after","begin","catch","case","cond","end","fun","if",
    "let","of","query","receive","try","when"];

  var separatorRE    = /[\->,;]/;
  var separatorWords = [
    "->",";",","];

  var operatorAtomWords = [
    "and","andalso","band","bnot","bor","bsl","bsr","bxor",
    "div","not","or","orelse","rem","xor"];

  var operatorSymbolRE    = /[\+\-\*\/<>=\|:!]/;
  var operatorSymbolWords = [
    "=","+","-","*","/",">",">=","<","=<","=:=","==","=/=","/=","||","<-","!"];

  var openParenRE    = /[<\(\[\{]/;
  var openParenWords = [
    "<<","(","[","{"];

  var closeParenRE    = /[>\)\]\}]/;
  var closeParenWords = [
    "}","]",")",">>"];

  var guardWords = [
    "is_atom","is_binary","is_bitstring","is_boolean","is_float",
    "is_function","is_integer","is_list","is_number","is_pid",
    "is_port","is_record","is_reference","is_tuple",
    "atom","binary","bitstring","boolean","function","integer","list",
    "number","pid","port","record","reference","tuple"];

  var bifWords = [
    "abs","adler32","adler32_combine","alive","apply","atom_to_binary",
    "atom_to_list","binary_to_atom","binary_to_existing_atom",
    "binary_to_list","binary_to_term","bit_size","bitstring_to_list",
    "byte_size","check_process_code","contact_binary","crc32",
    "crc32_combine","date","decode_packet","delete_module",
    "disconnect_node","element","erase","exit","float","float_to_list",
    "garbage_collect","get","get_keys","group_leader","halt","hd",
    "integer_to_list","internal_bif","iolist_size","iolist_to_binary",
    "is_alive","is_atom","is_binary","is_bitstring","is_boolean",
    "is_float","is_function","is_integer","is_list","is_number","is_pid",
    "is_port","is_process_alive","is_record","is_reference","is_tuple",
    "length","link","list_to_atom","list_to_binary","list_to_bitstring",
    "list_to_existing_atom","list_to_float","list_to_integer",
    "list_to_pid","list_to_tuple","load_module","make_ref","module_loaded",
    "monitor_node","node","node_link","node_unlink","nodes","notalive",
    "now","open_port","pid_to_list","port_close","port_command",
    "port_connect","port_control","pre_loaded","process_flag",
    "process_info","processes","purge_module","put","register",
    "registered","round","self","setelement","size","spawn","spawn_link",
    "spawn_monitor","spawn_opt","split_binary","statistics",
    "term_to_binary","time","throw","tl","trunc","tuple_size",
    "tuple_to_list","unlink","unregister","whereis"];

// upper case: [A-Z] [Ø-Þ] [À-Ö]
// lower case: [a-z] [ß-ö] [ø-ÿ]
  var anumRE       = /[\w@Ø-ÞÀ-Öß-öø-ÿ]/;
  var escapesRE    =
    /[0-7]{1,3}|[bdefnrstv\\"']|\^[a-zA-Z]|x[0-9a-zA-Z]{2}|x{[0-9a-zA-Z]+}/;

/////////////////////////////////////////////////////////////////////////////
// tokenizer

  function tokenizer(stream,state) {
	logger.error(`>>> Entering tokenizer(stream=${stream},state=${state})`, { "event.action": "tokenizer", "logrite.stream": stream, "logrite.state": state });
    // in multi-line string
    if (state.in_string) {
      state.in_string = (!doubleQuote(stream));
      return rval(state,stream,"string");
    }

    // in multi-line atom
    if (state.in_atom) {
      state.in_atom = (!singleQuote(stream));
      return rval(state,stream,"atom");
    }

    // whitespace
    if (stream.eatSpace()) {
      return rval(state,stream,"whitespace");
    }

    // attributes and type specs
    if (!peekToken(state) &&
        stream.match(/-\s*[a-zß-öø-ÿ][\wØ-ÞÀ-Öß-öø-ÿ]*/)) {
      if (is_member(stream.current(),typeWords)) {
        return rval(state,stream,"type");
      }else{
        return rval(state,stream,"attribute");
      }
    }

    var ch = stream.next();
	logger.error(`tokenizer(stream=${stream},state=${state}): ch → ${ch}`, { "event.action": "tokenizer", "logrite.stream": stream, "logrite.state": state });

    // comment
    if (ch == '%') {
      stream.skipToEnd();
      return rval(state,stream,"comment");
    }

    // colon
    if (ch == ":") {
      return rval(state,stream,"colon");
    }

    // macro
    if (ch == '?') {
      stream.eatSpace();
      stream.eatWhile(anumRE);
      return rval(state,stream,"macro");
    }

    // record
    if (ch == "#") {
      stream.eatSpace();
      stream.eatWhile(anumRE);
      return rval(state,stream,"record");
    }

    // dollar escape
    if (ch == "$") {
      if (stream.next() == "\\" && !stream.match(escapesRE)) {
        return rval(state,stream,"error");
      }
      return rval(state,stream,"number");
    }

    // dot
    if (ch == ".") {
      return rval(state,stream,"dot");
    }

    // quoted atom
    if (ch == '\'') {
      if (!(state.in_atom = (!singleQuote(stream)))) {
        if (stream.match(/\s*\/\s*[0-9]/,false)) {
          stream.match(/\s*\/\s*[0-9]/,true);
          return rval(state,stream,"fun");      // 'f'/0 style fun
        }
        if (stream.match(/\s*\(/,false) || stream.match(/\s*:/,false)) {
          return rval(state,stream,"function");
        }
      }
      return rval(state,stream,"atom");
    }

    // string
    if (ch == '"') {
      state.in_string = (!doubleQuote(stream));
      return rval(state,stream,"string");
    }

    // variable
    if (/[A-Z_Ø-ÞÀ-Ö]/.test(ch)) {
      stream.eatWhile(anumRE);
      return rval(state,stream,"variable");
    }

    // atom/keyword/BIF/function
    if (/[a-z_ß-öø-ÿ]/.test(ch)) {
      stream.eatWhile(anumRE);

      if (stream.match(/\s*\/\s*[0-9]/,false)) {
        stream.match(/\s*\/\s*[0-9]/,true);
        return rval(state,stream,"fun");      // f/0 style fun
      }

      var w = stream.current();

      if (is_member(w,keywordWords)) {
        return rval(state,stream,"keyword");
      }else if (is_member(w,operatorAtomWords)) {
        return rval(state,stream,"operator");
      }else if (stream.match(/\s*\(/,false)) {
        // 'put' and 'erlang:put' are bifs, 'foo:put' is not
        if (is_member(w,bifWords) &&
            ((peekToken(state).token != ":") ||
             (peekToken(state,2).token == "erlang"))) {
          return rval(state,stream,"builtin");
        }else if (is_member(w,guardWords)) {
          return rval(state,stream,"guard");
        }else{
          return rval(state,stream,"function");
        }
      }else if (lookahead(stream) == ":") {
        if (w == "erlang") {
          return rval(state,stream,"builtin");
        } else {
          return rval(state,stream,"function");
        }
      }else if (is_member(w,["true","false"])) {
        return rval(state,stream,"boolean");
      }else{
        return rval(state,stream,"atom");
      }
    }

    // number
    var digitRE      = /[0-9]/;
    var radixRE      = /[0-9a-zA-Z]/;         // 36#zZ style int
    if (digitRE.test(ch)) {
      stream.eatWhile(digitRE);
      if (stream.eat('#')) {                // 36#aZ  style integer
        if (!stream.eatWhile(radixRE)) {
          stream.backUp(1);                 //"36#" - syntax error
        }
      } else if (stream.eat('.')) {       // float
        if (!stream.eatWhile(digitRE)) {
          stream.backUp(1);        // "3." - probably end of function
        } else {
          if (stream.eat(/[eE]/)) {        // float with exponent
            if (stream.eat(/[-+]/)) {
              if (!stream.eatWhile(digitRE)) {
                stream.backUp(2);            // "2e-" - syntax error
              }
            } else {
              if (!stream.eatWhile(digitRE)) {
                stream.backUp(1);            // "2e" - syntax error
              }
            }
          }
        }
      }
      return rval(state,stream,"number");   // normal integer
    }

    // open parens
    if (nongreedy(stream,openParenRE,openParenWords)) {
      return rval(state,stream,"open_paren");
    }

    // close parens
    if (nongreedy(stream,closeParenRE,closeParenWords)) {
      return rval(state,stream,"close_paren");
    }

    // separators
    if (greedy(stream,separatorRE,separatorWords)) {
      return rval(state,stream,"separator");
    }

    // operators
    if (greedy(stream,operatorSymbolRE,operatorSymbolWords)) {
      return rval(state,stream,"operator");
    }

	logger.warn(`<<< Exiting tokenizer(stream=${stream},state=${state})`, { "event.action": "tokenizer", "logrite.stream": stream, "logrite.state": state });
    return rval(state,stream,null);
  }

/////////////////////////////////////////////////////////////////////////////
// utilities
  function nongreedy(stream,re,words) {
	logger.error(`>>> Entering nongreedy(stream=${stream},re=${re},words=${words})`, { "event.action": "nongreedy", "logrite.stream": stream, "logrite.re": re, "logrite.words": words });
    if (stream.current().length == 1 && re.test(stream.current())) {
      stream.backUp(1);
      while (re.test(stream.peek())) {
        stream.next();
        if (is_member(stream.current(),words)) {
          return true;
        }
      }
      stream.backUp(stream.current().length-1);
    }
	logger.error(`>>> Entering greedy(stream=${stream},re=${re},words=${words})`, { "event.action": "greedy", "logrite.stream": stream, "logrite.re": re, "logrite.words": words });
	logger.warn(`<<< Exiting greedy(stream=${stream},re=${re},words=${words})`, { "event.action": "greedy", "logrite.stream": stream, "logrite.re": re, "logrite.words": words });
	logger.warn(`<<< Exiting nongreedy(stream=${stream},re=${re},words=${words})`, { "event.action": "nongreedy", "logrite.stream": stream, "logrite.re": re, "logrite.words": words });
    return false;
  }

  function greedy(stream,re,words) {
    if (stream.current().length == 1 && re.test(stream.current())) {
      while (re.test(stream.peek())) {
        stream.next();
      }
      while (0 < stream.current().length) {
        if (is_member(stream.current(),words)) {
          return true;
        }else{
          stream.backUp(1);
        }
      }
      stream.next();
    }
    return false;
  }

  function doubleQuote(stream) {
	logger.error(`>>> Entering doubleQuote(stream=${stream})`, { "event.action": "doubleQuote", "logrite.stream": stream });
	logger.warn(`<<< Exiting doubleQuote(stream=${stream})`, { "event.action": "doubleQuote", "logrite.stream": stream });
    return quote(stream, '"', '\\');
  }

  function singleQuote(stream) {
	logger.error(`>>> Entering singleQuote(stream=${stream})`, { "event.action": "singleQuote", "logrite.stream": stream });
	logger.warn(`<<< Exiting singleQuote(stream=${stream})`, { "event.action": "singleQuote", "logrite.stream": stream });
    return quote(stream,'\'','\\');
  }

  function quote(stream,quoteChar,escapeChar) {
	logger.error(`>>> Entering quote(stream=${stream},quoteChar=${quoteChar},escapeChar=${escapeChar})`, { "event.action": "quote", "logrite.stream": stream, "logrite.quoteChar": quoteChar, "logrite.escapeChar": escapeChar });
    while (!stream.eol()) {
      var ch = stream.next();
      if (ch == quoteChar) {
        return true;
      }else if (ch == escapeChar) {
        stream.next();
      }
    }
	logger.error(`quote(stream=${stream},quoteChar=${quoteChar},escapeChar=${escapeChar}): ch → ${ch}`, { "event.action": "quote", "logrite.stream": stream, "logrite.quoteChar": quoteChar, "logrite.escapeChar": escapeChar });
    return false;
  }

  function lookahead(stream) {
	logger.error(`>>> Entering lookahead(stream=${stream})`, { "event.action": "lookahead", "logrite.stream": stream });
    var m = stream.match(/([\n\s]+|%[^\n]*\n)*(.)/,false);
	logger.error(`lookahead(stream=${stream}): m → ${m}`, { "event.action": "lookahead", "logrite.stream": stream });
    return m ? m.pop() : "";
  }

  function is_member(element,list) {
    return (-1 < list.indexOf(element));
  }

  function rval(state,stream,type) {
logger.error(`>>> Entering rval(state=${state},stream=${stream},type=${type})`, { "event.action": "rval", "logrite.state": state, "logrite.stream": stream, "logrite.type": type });

    // parse stack
    pushToken(state,realToken(type,stream));

    // map erlang token type to CodeMirror style class
    //     erlang             -> CodeMirror tag
    switch (type) {
      case "atom":        return "atom";
      case "attribute":   return "attribute";
      case "boolean":     return "atom";
      case "builtin":     return "builtin";
      case "close_paren": return null;
      case "colon":       return null;
      case "comment":     return "comment";
      case "dot":         return null;
      case "error":       return "error";
      case "fun":         return "meta";
      case "function":    return "tag";
      case "guard":       return "property";
      case "keyword":     return "keyword";
      case "macro":       return "variable-2";
      case "number":      return "number";
      case "open_paren":  return null;
      case "operator":    return "operator";
      case "record":      return "bracket";
      case "separator":   return null;
      case "string":      return "string";
      case "type":        return "def";
      case "variable":    return "variable";
      default:            return null;
	logger.warn(`<<< Exiting rval(state=${state},stream=${stream},type=${type})`, { "event.action": "rval", "logrite.state": state, "logrite.stream": stream, "logrite.type": type });
    }
  }

  function aToken(tok,col,ind,typ) {
	logger.error(`>>> Entering aToken(tok=${tok},col=${col},ind=${ind},typ=${typ})`, { "event.action": "aToken", "logrite.tok": tok, "logrite.col": col, "logrite.ind": ind, "logrite.typ": typ });
	logger.warn(`<<< Exiting aToken(tok=${tok},col=${col},ind=${ind},typ=${typ})`, { "event.action": "aToken", "logrite.tok": tok, "logrite.col": col, "logrite.ind": ind, "logrite.typ": typ });
    return {token:  tok,
            column: col,
            indent: ind,
            type:   typ};
  }

  function realToken(type,stream) {
	logger.error(`>>> Entering realToken(type=${type},stream=${stream})`, { "event.action": "realToken", "logrite.type": type, "logrite.stream": stream });
	logger.warn(`<<< Exiting realToken(type=${type},stream=${stream})`, { "event.action": "realToken", "logrite.type": type, "logrite.stream": stream });
    return aToken(stream.current(),
                 stream.column(),
                 stream.indentation(),
                 type);
  }

  function fakeToken(type) {
	logger.error(`>>> Entering fakeToken(type=${type})`, { "event.action": "fakeToken", "logrite.type": type });
	logger.warn(`<<< Exiting fakeToken(type=${type})`, { "event.action": "fakeToken", "logrite.type": type });
    return aToken(type,0,0,type);
  }

  function peekToken(state,depth) {
    var len = state.tokenStack.length;
    var dep = (depth ? depth : 1);

    if (len < dep) {
      return false;
    }else{
      return state.tokenStack[len-dep];
    }
  }

  function pushToken(state,token) {

    if (!(token.type == "comment" || token.type == "whitespace")) {
      state.tokenStack = maybe_drop_pre(state.tokenStack,token);
      state.tokenStack = maybe_drop_post(state.tokenStack);
    }
  }

  function maybe_drop_pre(s,token) {
    var last = s.length-1;

    if (0 < last && s[last].type === "record" && token.type === "dot") {
      s.pop();
    }else if (0 < last && s[last].type === "group") {
      s.pop();
      s.push(token);
    }else{
      s.push(token);
    }
    return s;
  }

  function maybe_drop_post(s) {
	logger.error(`>>> Entering maybe_drop_post(s=${s})`, { "event.action": "maybe_drop_post", "logrite.s": s });
    var last = s.length-1;

    if (s[last].type === "dot") {
      return [];
    }
    if (s[last].type === "fun" && s[last-1].token === "fun") {
      return s.slice(0,last-1);
    }
    switch (s[s.length-1].token) {
      case "}":    return d(s,{g:["{"]});
      case "]":    return d(s,{i:["["]});
      case ")":    return d(s,{i:["("]});
      case ">>":   return d(s,{i:["<<"]});
      case "end":  return d(s,{i:["begin","case","fun","if","receive","try"]});
      case ",":    return d(s,{e:["begin","try","when","->",
                                  ",","(","[","{","<<"]});
      case "->":   return d(s,{r:["when"],
                               m:["try","if","case","receive"]});
      case ";":    return d(s,{E:["case","fun","if","receive","try","when"]});
      case "catch":return d(s,{e:["try"]});
      case "of":   return d(s,{e:["case"]});
      case "after":return d(s,{e:["receive","try"]});
      default:     return s;
	logger.warn(`<<< Exiting maybe_drop_post(s=${s})`, { "event.action": "maybe_drop_post", "logrite.s": s });
    }
  }

  function d(stack,tt) {
    // stack is a stack of Token objects.
    // tt is an object; {type:tokens}
    // type is a char, tokens is a list of token strings.
    // The function returns (possibly truncated) stack.
    // It will descend the stack, looking for a Token such that Token.token
    //  is a member of tokens. If it does not find that, it will normally (but
    //  see "E" below) return stack. If it does find a match, it will remove
    //  all the Tokens between the top and the matched Token.
    // If type is "m", that is all it does.
    // If type is "i", it will also remove the matched Token and the top Token.
    // If type is "g", like "i", but add a fake "group" token at the top.
    // If type is "r", it will remove the matched Token, but not the top Token.
    // If type is "e", it will keep the matched Token but not the top Token.
    // If type is "E", it behaves as for type "e", except if there is no match,
    //  in which case it will return an empty stack.

    for (var type in tt) {
      var len = stack.length-1;
      var tokens = tt[type];
      for (var i = len-1; -1 < i ; i--) {
        if (is_member(stack[i].token,tokens)) {
          var ss = stack.slice(0,i);
          switch (type) {
              case "m": return ss.concat(stack[i]).concat(stack[len]);
              case "r": return ss.concat(stack[len]);
              case "i": return ss;
              case "g": return ss.concat(fakeToken("group"));
              case "E": return ss.concat(stack[i]);
              case "e": return ss.concat(stack[i]);
          }
        }
      }
    }
    return (type == "E" ? [] : stack);
  }

/////////////////////////////////////////////////////////////////////////////
// indenter

  function indenter(state,textAfter) {
	logger.error(`>>> Entering indenter(state=${state},textAfter=${textAfter})`, { "event.action": "indenter", "logrite.state": state, "logrite.textAfter": textAfter });
    var t;
    var unit = cmCfg.indentUnit;
    var wordAfter = wordafter(textAfter);
	logger.error(`indenter(state=${state},textAfter=${textAfter}): wordAfter → ${wordAfter}`, { "event.action": "indenter", "logrite.state": state, "logrite.textAfter": textAfter });
    var currT = peekToken(state,1);
	logger.error(`indenter(state=${state},textAfter=${textAfter}): currT → ${currT}`, { "event.action": "indenter", "logrite.state": state, "logrite.textAfter": textAfter });
    var prevT = peekToken(state,2);
	logger.error(`indenter(state=${state},textAfter=${textAfter}): prevT → ${prevT}`, { "event.action": "indenter", "logrite.state": state, "logrite.textAfter": textAfter });

    if (state.in_string || state.in_atom) {
      return CodeMirror.Pass;
    }else if (!prevT) {
      return 0;
    }else if (currT.token == "when") {
      return currT.column+unit;
    }else if (wordAfter === "when" && prevT.type === "function") {
      return prevT.indent+unit;
    }else if (wordAfter === "(" && currT.token === "fun") {
      return  currT.column+3;
    }else if (wordAfter === "catch" && (t = getToken(state,["try"]))) {
      return t.column;
    }else if (is_member(wordAfter,["end","after","of"])) {
      t = getToken(state,["begin","case","fun","if","receive","try"]);
      return t ? t.column : CodeMirror.Pass;
    }else if (is_member(wordAfter,closeParenWords)) {
      t = getToken(state,openParenWords);
      return t ? t.column : CodeMirror.Pass;
    }else if (is_member(currT.token,[",","|","||"]) ||
              is_member(wordAfter,[",","|","||"])) {
      t = postcommaToken(state);
      return t ? t.column+t.token.length : unit;
    }else if (currT.token == "->") {
      if (is_member(prevT.token, ["receive","case","if","try"])) {
        return prevT.column+unit+unit;
      }else{
        return prevT.column+unit;
      }
    }else if (is_member(currT.token,openParenWords)) {
      return currT.column+currT.token.length;
    }else{
      t = defaultToken(state);
      return truthy(t) ? t.column+unit : 0;
    }
  }

  function wordafter(str) {
	logger.error(`>>> Entering wordafter(str=${str})`, { "event.action": "wordafter", "logrite.str": str });
    var m = str.match(/,|[a-z]+|\}|\]|\)|>>|\|+|\(/);

	logger.error(`wordafter(str=${str}): m → ${m}`, { "event.action": "wordafter", "logrite.str": str });
	logger.warn(`<<< Exiting wordafter(str=${str})`, { "event.action": "wordafter", "logrite.str": str });
    return truthy(m) && (m.index === 0) ? m[0] : "";
  }

  function postcommaToken(state) {
	logger.error(`>>> Entering postcommaToken(state=${state})`, { "event.action": "postcommaToken", "logrite.state": state });
    var objs = state.tokenStack.slice(0,-1);
    var i = getTokenIndex(objs,"type",["open_paren"]);
	logger.error(`postcommaToken(state=${state}): objs → ${objs}`, { "event.action": "postcommaToken", "logrite.state": state });

	logger.warn(`<<< Exiting postcommaToken(state=${state})`, { "event.action": "postcommaToken", "logrite.state": state });
    return truthy(objs[i]) ? objs[i] : false;
  }

  function defaultToken(state) {
	logger.error(`>>> Entering defaultToken(state=${state})`, { "event.action": "defaultToken", "logrite.state": state });
    var objs = state.tokenStack;
    var stop = getTokenIndex(objs,"type",["open_paren","separator","keyword"]);
	logger.error(`defaultToken(state=${state}): stop → ${stop}`, { "event.action": "defaultToken", "logrite.state": state });
    var oper = getTokenIndex(objs,"type",["operator"]);
	logger.error(`defaultToken(state=${state}): oper → ${oper}`, { "event.action": "defaultToken", "logrite.state": state });

    if (truthy(stop) && truthy(oper) && stop < oper) {
      return objs[stop+1];
    } else if (truthy(stop)) {
      return objs[stop];
    } else {
	logger.warn(`<<< Exiting defaultToken(state=${state})`, { "event.action": "defaultToken", "logrite.state": state });
      return false;
    }
  }

  function getToken(state,tokens) {
    var objs = state.tokenStack;
    var i = getTokenIndex(objs,"token",tokens);

    return truthy(objs[i]) ? objs[i] : false;
  }

  function getTokenIndex(objs,propname,propvals) {
logger.error(`>>> Entering getTokenIndex(objs=${objs},propname=${propname},propvals=${propvals})`, { "event.action": "getTokenIndex", "logrite.objs": objs, "logrite.propname": propname, "logrite.propvals": propvals });

    for (var i = objs.length-1; -1 < i ; i--) {
      if (is_member(objs[i][propname],propvals)) {
        return i;
      }
    }
	logger.warn(`<<< Exiting getTokenIndex(objs=${objs},propname=${propname},propvals=${propvals})`, { "event.action": "getTokenIndex", "logrite.objs": objs, "logrite.propname": propname, "logrite.propvals": propvals });
    return false;
  }

  function truthy(x) {
	logger.warn(`<<< Exiting truthy(x=${x})`, { "event.action": "truthy", "logrite.x": x });
    return (x !== false) && (x != null);
  }

/////////////////////////////////////////////////////////////////////////////
// this object defines the mode

  return {
    startState:
      function() {
        return {tokenStack: [],
                in_string:  false,
                in_atom:    false};
      },

    token:
      function(stream, state) {
        return tokenizer(stream, state);
      },

    indent:
      function(state, textAfter) {
        return indenter(state,textAfter);
      },

    lineComment: "%"
  };
});

});
