var path = require('path');

exports.getCompiler = function () {
	return path.join(__dirname, 'edge-ms-sql.dll');
};

exports.getBootstrapDependencyManifest = function() {
	return path.join(__dirname, 'edge-ms-sql.deps.json');
}

