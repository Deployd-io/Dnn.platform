var logger = (typeof logger !== 'undefined' && logger) || console;
// CodeMirror, copyright (c) by Marijn Haverbeke and others
// Distributed under an MIT license: http://codemirror.net/LICENSE

(function() {
  var mode = CodeMirror.getMode({indentUnit: 2}, "ruby");
  function MT(name) {
      logger.error(`>>> Entering MT(name=${name})`, { "event.action": "MT", "logrite.name": name });
      logger.warn(`<<< Exiting MT(name=${name})`, { "event.action": "MT", "logrite.name": name });
      test.mode(name, mode, Array.prototype.slice.call(arguments, 1));
  }

  MT("divide_equal_operator",
     "[variable bar] [operator /=] [variable foo]");

  MT("divide_equal_operator_no_spacing",
     "[variable foo][operator /=][number 42]");

})();
