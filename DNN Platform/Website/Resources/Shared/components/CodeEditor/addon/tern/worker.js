const logger = console;
// CodeMirror, copyright (c) by Marijn Haverbeke and others
// Distributed under an MIT license: http://codemirror.net/LICENSE

// declare global: tern, server

var server;

this.onmessage = function(e) {
logger.debug(`>>> Entering function(e=${e})`);
  var data = e.data;
  switch (data.type) {
  case "init": return startServer(data.defs, data.plugins, data.scripts);
  case "add": return server.addFile(data.name, data.text);
  case "del": return server.delFile(data.name);
  case "req": return server.request(data.body, function(err, reqData) {
logger.debug('>>> Entering request()');
    postMessage({id: data.id, body: reqData, err: err && String(err)});
  });
  case "getFile":
    var c = pending[data.id];
	logger.debug(`function(e=${e}): data → ${data}`);
    delete pending[data.id];
    return c(data.err, data.text);
  default: throw new Error("Unknown message type: " + data.type);
  }
};

var nextId = 0, pending = {};
function getFile(file, c) {
  postMessage({type: "getFile", name: file, id: ++nextId});
  pending[nextId] = c;
logger.debug(`<<< Exiting getFile(file=${file},c=${c})`);
}

function startServer(defs, plugins, scripts) {
logger.debug(`>>> Entering startServer(defs=${defs},plugins=${plugins},scripts=${scripts})`);
  if (scripts) importScripts.apply(null, scripts);

  server = new tern.Server({
    getFile: getFile,
    async: true,
    defs: defs,
    plugins: plugins
  });
logger.debug(`<<< Exiting startServer(defs=${defs},plugins=${plugins},scripts=${scripts})`);
}

this.console = {
  log: function(v) { postMessage({type: "debug", message: v}); }
};
