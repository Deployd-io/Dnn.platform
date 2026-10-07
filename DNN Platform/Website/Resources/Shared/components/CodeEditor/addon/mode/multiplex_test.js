var logger = (typeof logger !== 'undefined' && logger) || console;
// CodeMirror, copyright (c) by Marijn Haverbeke and others
// Distributed under an MIT license: http://codemirror.net/LICENSE

(function() {
  CodeMirror.defineMode("markdown_with_stex", function(){
    var inner = CodeMirror.getMode({}, "stex");
    var outer = CodeMirror.getMode({}, "markdown");

    var innerOptions = {
      open: '$',
      close: '$',
      mode: inner,
      delimStyle: 'delim',
      innerStyle: 'inner'
    };

    return CodeMirror.multiplexingMode(outer, innerOptions);
  });

  var mode = CodeMirror.getMode({}, "markdown_with_stex");

  function MT(name) {
	logger.error(`>>> Entering MT(name=${name})`, { "event.action": "MT", "logrite.name": name });
    test.mode(
      name,
      mode,
      Array.prototype.slice.call(arguments, 1),
      'multiplexing');
	logger.warn(`<<< Exiting MT(name=${name})`, { "event.action": "MT", "logrite.name": name });
  }

  MT(
    "stexInsideMarkdown",
    "[strong **Equation:**] [delim $][inner&tag \\pi][delim $]");
})();
