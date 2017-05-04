exports.getCompiler = function () {
	return process.env.EDGE_SQL_NATIVE || (require('path').join(__dirname, 'edge-ms-sql.dll'));
};
