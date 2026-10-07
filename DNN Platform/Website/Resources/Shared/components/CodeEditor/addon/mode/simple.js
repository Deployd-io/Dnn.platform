const logger = console;
// CodeMirror, copyright (c) by Marijn Haverbeke and others
// Distributed under an MIT license: http://codemirror.net/LICENSE

(function(mod) {
  if (typeof exports == "object" && typeof module == "object") // CommonJS
    mod(require("../../lib/codemirror"));
  else if (typeof define == "function" && define.amd) // AMD
    define(["../../lib/codemirror"], mod);
  else // Plain browser env
    mod(CodeMirror);
})(function(CodeMirror) {
  "use strict";

  CodeMirror.defineSimpleMode = function(name, states) {
	logger.error(`>>> Entering defineSimpleMode(name=${name},states=${states})`, { "event.action": "defineSimpleMode", "logrite.name": name, "logrite.states": states });
    CodeMirror.defineMode(name, function(config) {
      return CodeMirror.simpleMode(config, states);
    });
  };

  CodeMirror.simpleMode = function(config, states) {
	logger.error(`>>> Entering simpleMode(config=${config},states=${states})`, { "event.action": "simpleMode", "logrite.config": config, "logrite.states": states });
    ensureState(states, "start");
    var states_ = {}, meta = states.meta || {}, hasIndentation = false;
    for (var state in states) if (state != meta && states.hasOwnProperty(state)) {
      var list = states_[state] = [], orig = states[state];
      for (var i = 0; i < orig.length; i++) {
        var data = orig[i];
		logger.error(`simpleMode(config=${config},states=${states}): data → ${data}`, { "event.action": "simpleMode", "logrite.config": config, "logrite.states": states });
        list.push(new Rule(data, states));
        if (data.indent || data.dedent) hasIndentation = true;
      }
    }
    var mode = {
      startState: function() {
		logger.error('>>> Entering startState()', { "event.action": "startState" });
        return {state: "start", pending: null,
                local: null, localState: null,
                indent: hasIndentation ? [] : null};
				logger.warn('<<< Exiting startState()', { "event.action": "startState" });
      },
      copyState: function(state) {
		logger.error(`>>> Entering copyState(state=${state})`, { "event.action": "copyState", "logrite.state": state });
        var s = {state: state.state, pending: state.pending,
                 local: state.local, localState: null,
                 indent: state.indent && state.indent.slice(0)};
        if (state.localState)
          s.localState = CodeMirror.copyState(state.local.mode, state.localState);
        if (state.stack)
          s.stack = state.stack.slice(0);
        for (var pers = state.persistentStates; pers; pers = pers.next)
          s.persistentStates = {mode: pers.mode,
                                spec: pers.spec,
                                state: pers.state == state.localState ? s.localState : CodeMirror.copyState(pers.mode, pers.state),
                                next: s.persistentStates};
								logger.warn(`<<< Exiting copyState(state=${state})`, { "event.action": "copyState", "logrite.state": state });
        return s;
      },
      token: tokenFunction(states_, config),
      innerMode: function(state) {
          logger.error(`>>> Entering innerMode(state=${state})`, { "event.action": "innerMode", "logrite.state": state });
          logger.warn(`<<< Exiting innerMode(state=${state})`, { "event.action": "innerMode", "logrite.state": state });
          return state.local && {mode: state.local.mode, state: state.localState};
      },
      indent: indentFunction(states_, meta)
    };
    if (meta) for (var prop in meta) if (meta.hasOwnProperty(prop))
      mode[prop] = meta[prop];
    return mode;
  };

  function ensureState(states, name) {
	logger.error(`>>> Entering ensureState(states=${states},name=${name})`, { "event.action": "ensureState", "logrite.states": states, "logrite.name": name });
    if (!states.hasOwnProperty(name))
      throw new Error("Undefined state " + name + "in simple mode");
  }

  function toRegex(val, caret) {
	logger.error(`>>> Entering toRegex(val=${val},caret=${caret})`, { "event.action": "toRegex", "logrite.val": val, "logrite.caret": caret });
    if (!val) return /(?:)/;
    var flags = "";
	logger.error(`toRegex(val=${val},caret=${caret}): flags → ${flags}`, { "event.action": "toRegex", "logrite.val": val, "logrite.caret": caret });
    if (val instanceof RegExp) {
      if (val.ignoreCase) flags = "i";
      val = val.source;
    } else {
      val = String(val);
    }
	logger.warn(`<<< Exiting toRegex(val=${val},caret=${caret})`, { "event.action": "toRegex", "logrite.val": val, "logrite.caret": caret });
    return new RegExp((caret === false ? "" : "^") + "(?:" + val + ")", flags);
  }

  function asToken(val) {
	logger.error(`>>> Entering asToken(val=${val})`, { "event.action": "asToken", "logrite.val": val });
    if (!val) return null;
    if (typeof val == "string") return val.replace(/\./g, " ");
    var result = [];
	logger.error(`asToken(val=${val}): result → ${result}`, { "event.action": "asToken", "logrite.val": val });
    for (var i = 0; i < val.length; i++)
      result.push(val[i] && val[i].replace(/\./g, " "));
	logger.warn(`<<< Exiting asToken(val=${val})`, { "event.action": "asToken", "logrite.val": val });
    return result;
  }

  function Rule(data, states) {
	logger.error(`>>> Entering Rule(data=${data},states=${states})`, { "event.action": "Rule", "logrite.data": data, "logrite.states": states });
    if (data.next || data.push) ensureState(states, data.next || data.push);
    this.regex = toRegex(data.regex);
    this.token = asToken(data.token);
    this.data = data;
	logger.debug(`Rule(data=${data},states=${states}): this.data → ${this.data}`, { "event.action": "Rule", "logrite.data": data, "logrite.states": states });
  }

  function tokenFunction(states, config) {
	logger.error(`>>> Entering tokenFunction(states=${states},config=${config})`, { "event.action": "tokenFunction", "logrite.states": states, "logrite.config": config });
    return function(stream, state) {
      if (state.pending) {
        var pend = state.pending.shift();
        if (state.pending.length == 0) state.pending = null;
        stream.pos += pend.text.length;
        return pend.token;
      }

      if (state.local) {
        if (state.local.end && stream.match(state.local.end)) {
          var tok = state.local.endToken || null;
          state.local = state.localState = null;
          return tok;
        } else {
          var tok = state.local.mode.token(stream, state.localState), m;
          if (state.local.endScan && (m = state.local.endScan.exec(stream.current())))
            stream.pos = stream.start + m.index;
          return tok;
        }
      }

      var curState = states[state.state];
      for (var i = 0; i < curState.length; i++) {
        var rule = curState[i];
        var matches = (!rule.data.sol || stream.sol()) && stream.match(rule.regex);
        if (matches) {
          if (rule.data.next) {
            state.state = rule.data.next;
          } else if (rule.data.push) {
            (state.stack || (state.stack = [])).push(state.state);
            state.state = rule.data.push;
          } else if (rule.data.pop && state.stack && state.stack.length) {
            state.state = state.stack.pop();
          }

          if (rule.data.mode)
            enterLocalMode(config, state, rule.data.mode, rule.token);
          if (rule.data.indent)
            state.indent.push(stream.indentation() + config.indentUnit);
          if (rule.data.dedent)
            state.indent.pop();
          if (matches.length > 2) {
            state.pending = [];
            for (var j = 2; j < matches.length; j++)
              if (matches[j])
                state.pending.push({text: matches[j], token: rule.token[j - 1]});
            stream.backUp(matches[0].length - (matches[1] ? matches[1].length : 0));
            return rule.token[0];
          } else if (rule.token && rule.token.join) {
            return rule.token[0];
          } else {
            return rule.token;
          }
        }
      }
      stream.next();
      return null;
    };
  }

  function cmp(a, b) {
	logger.error(`>>> Entering cmp(a=${a},b=${b})`, { "event.action": "cmp", "logrite.a": a, "logrite.b": b });
    if (a === b) return true;
    if (!a || typeof a != "object" || !b || typeof b != "object") return false;
    var props = 0;
    for (var prop in a) if (a.hasOwnProperty(prop)) {
      if (!b.hasOwnProperty(prop) || !cmp(a[prop], b[prop])) return false;
      props++;
    }
    for (var prop in b) if (b.hasOwnProperty(prop)) props--;
	logger.warn(`<<< Exiting cmp(a=${a},b=${b})`, { "event.action": "cmp", "logrite.a": a, "logrite.b": b });
    return props == 0;
  }

  function enterLocalMode(config, state, spec, token) {
    var pers;
    if (spec.persistent) for (var p = state.persistentStates; p && !pers; p = p.next)
      if (spec.spec ? cmp(spec.spec, p.spec) : spec.mode == p.mode) pers = p;
    var mode = pers ? pers.mode : spec.mode || CodeMirror.getMode(config, spec.spec);
    var lState = pers ? pers.state : CodeMirror.startState(mode);
    if (spec.persistent && !pers)
      state.persistentStates = {mode: mode, spec: spec.spec, state: lState, next: state.persistentStates};

    state.localState = lState;
    state.local = {mode: mode,
                   end: spec.end && toRegex(spec.end),
                   endScan: spec.end && spec.forceEnd !== false && toRegex(spec.end, false),
                   endToken: token && token.join ? token[token.length - 1] : token};
  }

  function indexOf(val, arr) {
	logger.error(`>>> Entering indexOf(val=${val},arr=${arr})`, { "event.action": "indexOf", "logrite.val": val, "logrite.arr": arr });
    for (var i = 0; i < arr.length; i++) if (arr[i] === val) return true;
	logger.warn(`<<< Exiting indexOf(val=${val},arr=${arr})`, { "event.action": "indexOf", "logrite.val": val, "logrite.arr": arr });
  }

  function indentFunction(states, meta) {
	logger.error(`>>> Entering indentFunction(states=${states},meta=${meta})`, { "event.action": "indentFunction", "logrite.states": states, "logrite.meta": meta });
    return function(state, textAfter, line) {
      if (state.local && state.local.mode.indent)
        return state.local.mode.indent(state.localState, textAfter, line);
      if (state.indent == null || state.local || meta.dontIndentStates && indexOf(state.state, meta.dontIndentStates) > -1)
        return CodeMirror.Pass;

      var pos = state.indent.length - 1, rules = states[state.state];
      scan: for (;;) {
        for (var i = 0; i < rules.length; i++) {
          var rule = rules[i];
          if (rule.data.dedent && rule.data.dedentIfLineStart !== false) {
            var m = rule.regex.exec(textAfter);
            if (m && m[0]) {
              pos--;
              if (rule.next || rule.push) rules = states[rule.next || rule.push];
              textAfter = textAfter.slice(m[0].length);
              continue scan;
            }
          }
        }
        break;
      }
      return pos < 0 ? 0 : state.indent[pos];
    };
  }
});
