const logger = console;
// CodeMirror, copyright (c) by Marijn Haverbeke and others
// Distributed under an MIT license: http://codemirror.net/LICENSE

(function(mod) {
logger.debug(`>>> Entering function(exports=${exports})`);
  if (typeof exports == "object" && typeof module == "object") // CommonJS
    mod(require("../../lib/codemirror"));
  else if (typeof define == "function" && define.amd) // AMD
    define(["../../lib/codemirror"], mod);
  else // Plain browser env
    mod(CodeMirror);
})(function(CodeMirror) {
  "use strict";

  CodeMirror.defineSimpleMode = function(name, states) {
    CodeMirror.defineMode(name, function(config) {
	logger.debug(`>>> Entering defineMode(name=${name})`);
      return CodeMirror.simpleMode(config, states);
    });
  };

  CodeMirror.simpleMode = function(config, states) {
    ensureState(states, "start");
    var states_ = {}, meta = states.meta || {}, hasIndentation = false;
    for (var state in states) if (state != meta && states.hasOwnProperty(state)) {
      var list = states_[state] = [], orig = states[state];
      for (var i = 0; i < orig.length; i++) {
        var data = orig[i];
        list.push(new Rule(data, states));
        if (data.indent || data.dedent) hasIndentation = true;
      }
    }
    var mode = {
      startState: function() {
		logger.debug('>>> Entering startState()');
        return {state: "start", pending: null,
                local: null, localState: null,
                indent: hasIndentation ? [] : null};
				logger.debug('<<< Exiting startState()');
      },
      copyState: function(state) {
		logger.debug(`>>> Entering copyState(state=${state})`);
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
								logger.debug(`function(config=${config},states=${states}): s.persistentStates → ${s.persistentStates}`);
								logger.debug(`function(exports=${exports}): s.persistentStates → ${s.persistentStates}`);
		logger.debug(`<<< Exiting copyState(state=${state})`);
		logger.debug(`copyState(state=${state}): s.persistentStates → ${s.persistentStates}`);
        return s;
      },
      token: tokenFunction(states_, config),
      innerMode: function(state) { return state.local && {mode: state.local.mode, state: state.localState}; },
      indent: indentFunction(states_, meta)
    };
    if (meta) for (var prop in meta) if (meta.hasOwnProperty(prop))
      mode[prop] = meta[prop];
	logger.debug(`<<< Exiting function(exports=${exports})`);
    return mode;
  };

  function ensureState(states, name) {
	logger.debug(`>>> Entering ensureState(states=${states},name=${name})`);
    if (!states.hasOwnProperty(name))
	logger.debug(`<<< Exiting ensureState(states=${states},name=${name})`);
      throw new Error("Undefined state " + name + "in simple mode");
  }

  function toRegex(val, caret) {
	logger.debug(`>>> Entering toRegex(val=${val},caret=${caret})`);
    if (!val) return /(?:)/;
    var flags = "";
	logger.debug(`toRegex(val=${val},caret=${caret}): flags → ${flags}`);
    if (val instanceof RegExp) {
      if (val.ignoreCase) flags = "i";
      val = val.source;
    } else {
      val = String(val);
    }
	logger.debug(`toRegex(val=${val},caret=${caret}): val → ${val}`);
	logger.debug(`<<< Exiting toRegex(val=${val},caret=${caret})`);
    return new RegExp((caret === false ? "" : "^") + "(?:" + val + ")", flags);
  }

  function asToken(val) {
	logger.debug(`>>> Entering asToken(val=${val})`);
    if (!val) return null;
    if (typeof val == "string") return val.replace(/\./g, " ");
    var result = [];
	logger.debug(`asToken(val=${val}): result → ${result}`);
    for (var i = 0; i < val.length; i++)
      result.push(val[i] && val[i].replace(/\./g, " "));
	logger.debug(`<<< Exiting asToken(val=${val})`);
    return result;
  }

  function Rule(data, states) {
    if (data.next || data.push) ensureState(states, data.next || data.push);
    this.regex = toRegex(data.regex);
    this.token = asToken(data.token);
    this.data = data;
	logger.debug(`Rule(data=${data},states=${states}): data → ${data}`);
	logger.debug(`Rule(data=${data},states=${states}): this.data → ${this.data}`);
	logger.debug(`<<< Exiting Rule(data=${data},states=${states})`);
  }

  function tokenFunction(states, config) {
    return function(stream, state) {
	logger.debug(`>>> Entering tokenFunction(states=${states},config=${config})`);
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
		logger.debug(`tokenFunction(states=${states},config=${config}): state.local → ${state.local}`);
		logger.debug(`function(stream=${stream},state=${state}): state.local → ${state.local}`);
          return tok;
        } else {
          var tok = state.local.mode.token(stream, state.localState), m;
          if (state.local.endScan && (m = state.local.endScan.exec(stream.current())))
            stream.pos = stream.start + m.index;
		logger.debug(`tokenFunction(states=${states},config=${config}): stream.pos → ${stream.pos}`);
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
			logger.debug(`tokenFunction(states=${states},config=${config}): state.state → ${state.state}`);
          } else if (rule.data.push) {
			logger.debug(`function(stream=${stream},state=${state}): state.state → ${state.state}`);
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
			logger.debug(`tokenFunction(states=${states},config=${config}): state.pending → ${state.pending}`);
            for (var j = 2; j < matches.length; j++)
			logger.debug(`function(stream=${stream},state=${state}): state.pending → ${state.pending}`);
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
	logger.debug(`<<< Exiting tokenFunction(states=${states},config=${config})`);
      return null;
    };
  }

  function cmp(a, b) {
	logger.debug(`>>> Entering cmp(a=${a},b=${b})`);
    if (a === b) return true;
    if (!a || typeof a != "object" || !b || typeof b != "object") return false;
    var props = 0;
    for (var prop in a) if (a.hasOwnProperty(prop)) {
      if (!b.hasOwnProperty(prop) || !cmp(a[prop], b[prop])) return false;
      props++;
    }
    for (var prop in b) if (b.hasOwnProperty(prop)) props--;
	logger.debug(`<<< Exiting cmp(a=${a},b=${b})`);
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
	logger.debug(`>>> Entering indexOf(val=${val},arr=${arr})`);
    for (var i = 0; i < arr.length; i++) if (arr[i] === val) return true;
	logger.debug(`<<< Exiting indexOf(val=${val},arr=${arr})`);
  }

  function indentFunction(states, meta) {
	logger.debug(`>>> Entering indentFunction(states=${states},meta=${meta})`);
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
	logger.debug(`<<< Exiting indentFunction(states=${states},meta=${meta})`);
      return pos < 0 ? 0 : state.indent[pos];
    };
  }
});
