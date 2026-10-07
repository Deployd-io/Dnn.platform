var logger = (typeof logger !== 'undefined' && logger) || console;
// CodeMirror, copyright (c) by Marijn Haverbeke and others
// Distributed under an MIT license: http://codemirror.net/LICENSE

// declare global: tern, server

var server;

this.onmessage = function(e) {
logger.error(`>>> Entering onmessage(e=${e})`, { "event.action": "onmessage", "logrite.e": e });
  var data = e.data;
  switch (data.type) {
  case "init": return startServer(data.defs, data.plugins, data.scripts);
  case "add": return server.addFile(data.name, data.text);
  case "del": return server.delFile(data.name);
  case "req": return server.request(data.body, function(err, reqData) {
    postMessage({id: data.id, body: reqData, err: err && String(err)});
  });
  case "getFile":
	logger.error(`onmessage(e=${e}): data → ${data}`, { "event.action": "onmessage", "logrite.e": e });
    var c = pending[data.id];
    delete pending[data.id];
    return c(data.err, data.text);
  default: throw new Error("Unknown message type: " + data.type);
  }
logger.warn(`<<< Exiting onmessage(e=${e})`, { "event.action": "onmessage", "logrite.e": e });
};

var nextId = 0, pending = {};
function getFile(file, c) {
logger.error(`>>> Entering getFile(file=${file},c=${c})`, { "event.action": "getFile", "logrite.file": file, "logrite.c": c });
  postMessage({type: "getFile", name: file, id: ++nextId});
  pending[nextId] = c;
logger.warn(`<<< Exiting getFile(file=${file},c=${c})`, { "event.action": "getFile", "logrite.file": file, "logrite.c": c });
}

function startServer(defs, plugins, scripts) {
logger.error(`>>> Entering startServer(defs=${defs},plugins=${plugins},scripts=${scripts})`, { "event.action": "startServer", "logrite.defs": defs, "logrite.plugins": plugins, "logrite.scripts": scripts });
  if (scripts) importScripts.apply(null, scripts);

  server = new tern.Server({
    getFile: getFile,
    async: true,
    defs: defs,
    plugins: plugins
  });
logger.warn(`<<< Exiting startServer(defs=${defs},plugins=${plugins},scripts=${scripts})`, { "event.action": "startServer", "logrite.defs": defs, "logrite.plugins": plugins, "logrite.scripts": scripts });
}

this.console = {
  log: function(v) {
      logger.error(`>>> Entering log(v=${v})`, { "event.action": "log", "logrite.v": v });
      logger.warn(`<<< Exiting log(v=${v})`, { "event.action": "log", "logrite.v": v });
      postMessage({type: "debug", message: v});
  }
};
