const logger = console;
// CodeMirror, copyright (c) by Marijn Haverbeke and others
// Distributed under an MIT license: http://codemirror.net/LICENSE

(function() {
logger.debug('>>> Entering function()');
  CodeMirror.defineMode("markdown_with_stex", function(){
    var inner = CodeMirror.getMode({}, "stex");
    var outer = CodeMirror.getMode({}, "markdown");
	logger.debug(`defineMode(inner=${inner}): inner → ${inner}`);
	logger.debug(`defineMode(inner=${inner}): outer → ${outer}`);

    var innerOptions = {
      open: '$',
      close: '$',
      mode: inner,
      delimStyle: 'delim',
      innerStyle: 'inner'
    };

	logger.debug(`<<< Exiting defineMode(inner=${inner})`);
logger.debug('<<< Exiting function()');
    return CodeMirror.multiplexingMode(outer, innerOptions);
  });

  var mode = CodeMirror.getMode({}, "markdown_with_stex");
logger.debug(`function(): mode → ${mode}`);

  function MT(name) {
	logger.debug(`>>> Entering MT(name=${name})`);
    test.mode(
      name,
      mode,
      Array.prototype.slice.call(arguments, 1),
      'multiplexing');
	logger.debug(`<<< Exiting MT(name=${name})`);
  }

  MT(
    "stexInsideMarkdown",
    "[strong **Equation:**] [delim $][inner&tag \\pi][delim $]");
})();
