edge-ms-sql
=======

SQL Server compiler for [edge.js](http://tjanczuk.github.com/edge). It allows accessing SQL Sever databases from Node.js using Edge.js and ADO.NET. 

This is a fork of [edge-sql](https://github.com/tjanczuk/edge-sql) providing improvements to the original implementation.

Usage is the same as edge-sql, replace 'sql' language definition with 'ms-sql':

```bash
npm install edge-ms-sql
```

```diff
var edge = require('edge');

-var getTop10Products = edge.func('sql', function () {/*
+var getTop10Products = edge.func('ms-sql', function () {/*
    select top 10 * from Products
*/});

getTop10Products(null, function (error, result) {
    if (error) throw error;
    console.log(result);
});
```

## Why use `edge-ms-sql`?

Differences from `edge-sql`

 * Provides optional `commandTimeout` parameter to set command timeout.
 * Attempts to treat all other types of SQL statements as `select` instead of throwing exception. This allows to execute complex SQL queries that declare variables and temp tables before running `select` statement.
 
 ## Basic usage
 
 For basic usage refer to [Edge.js](https://github.com/tjanczuk/edge#how-to-script-t-sql-in-a-nodejs-application) documentation.
 
 ### Advanced usage
 
Using variables

 ```js
var select = edge.func('ms-sql', {
    source: 'select top 10 * from Products',
    connectionString: 'SERVER=myserver;DATABASE=mydatabase;Integrated Security=SSPI',
    commandTimeout: 100
});

select(null, function (error, result) {
    if (error) throw error;
    console.log(result);
});
```
 
Stored proc with input parameters  

 ```js
var storedProcParams = {inputParm1: 'input1', inputParam2: 25};

var select = edge.func('ms-sql', {
    source: 'exec myStoredProc',
    connectionString: 'SERVER=myserver;DATABASE=mydatabase;Integrated Security=SSPI'
});

select(storedProcParams, function (error, result) {
    if (error) throw error;
    console.log(result);
});
```  
