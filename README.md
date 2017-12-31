edge-ms-sql
=======

SQL Server compiler for [Edge.js](https://github.com/agracio/edge-js). It allows accessing SQL Sever databases from Node.js using Edge.js and ADO.NET. 

This is a fork of [edge-sql](https://github.com/tjanczuk/edge-sql) providing improvements to the original implementation.

>Example code uses `edge-js` instead of `edge` but it will work with either flavour of Edge.js.

## Why use `edge-ms-sql`?

Differences from `edge-sql`

 * Provides optional `commandTimeout` parameter to set SQL command timeout. [SqlCommand.CommandTimeout](https://msdn.microsoft.com/en-us/library/system.data.sqlclient.sqlcommand.commandtimeout(v=vs.110).aspx)
 * Attempts to treat all other types of SQL statements as `select` instead of throwing exception. This allows to execute complex SQL queries that declare variables and temp tables before running `select` statement.

## Usage 

Usage is the same as edge-sql, replace 'sql' language definition with 'ms-sql':

```bash
npm install edge-js
npm install edge-ms-sql
```

```diff
var edge = require('edge-js');

-var getTop10Products = edge.func('sql', function () {/*
+var getTop10Products = edge.func('ms-sql', function () {/*
    select top 10 * from Products
*/});

getTop10Products(null, function (error, result) {
    if (error) throw error;
    console.log(result);
});
```


### Supported SQL statements

 * **select**
 * **update**
 * **insert**
 * **delete**
 * **exec**

All other statements will be interpreted as `select` and will try to use `ExecuteReaderAsync` .NET method of `SqlCommand` class instance.

Select statement will always return last result of SQL command, there is no support for multiple results sets if you have multiple `select` statements in your SQL.
 
### Basic usage

You can set your SQL connection string using environment variable. For passing connection string as a parameter see [Advanced usage](#advanced-usage).

```
set EDGE_SQL_CONNECTION_STRING=Data Source=localhost;Initial Catalog=Northwind;Integrated Security=True
```

#### Simple select

```js
const edge = require('edge-js');

var getTop10Products = edge.func('ms-sql', function () {/*
    select top 10 * from Products
*/});

getTop10Products(null, function (error, result) {
    if (error) throw error;
    console.log(result);
});
```

#### Parameterized queries

You can construct a parameterized query once and provide parameter values on a per-call basis:

**SELECT**

```js
const edge = require('edge-js');

var getProduct = edge.func('ms-sql', function () {/*
    select * from Products 
    where ProductId = @myProductId
*/});

getProduct({ myProductId: 10 }, function (error, result) {
    if (error) throw error;
    console.log(result);
});
```

**UPDATE**

```js
const edge = require('edge-js');

var updateProductName = edge.func('ms-sql', function () {/*
    update Products
    set ProductName = @newName 
    where ProductId = @myProductId
*/});

updateProductName({ myProductId: 10, newName: 'New Product' }, function (error, result) {
    if (error) throw error;
    console.log(result);
});
```

### Advanced usage
 
##### Using parameterised function

```js
const edge = require('edge-js');

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
 
##### Stored proc with input parameters  

 ```js
const edge = require('edge-js');

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
