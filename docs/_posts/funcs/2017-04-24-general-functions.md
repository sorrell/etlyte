---
layout: page
title: "General Functions"
category: funcs
date: 2017-04-24 23:12:50
---

ETLyte comes with many builtin functions to give SQLite a stronger typing
system and more robust feature set.  The following list of functions
are available to call in any query, using either the DLL or the REPL.

### `REGEXP (searchStringOrColumnName  , pattern  )  `  
  REGEXP returns true if the pattern is matched, false otherwise. The regular
  expression should be any valid expression that can be interpreted by C#.
  It can either be called as a function or as inline SQL.  

  Function Call:
```sql
SELECT REGEXP('bearcat', 'cat');
```  
  Inline SQL:
```sql
SELECT 'cat' REGEXP 'bearcat';
```

### `DATECHK (dateValue1  ,  patternArray  , comparator  , dateValue2  )`  
  DATECHK returns true when (a) the dates can be parsed according to the `patternArray` and (b) when conditional value of `dateValue1 comparator dateValue2` returns true.

  Valid comparators are:

  - `'>'`  
  - `'>='`  
  - `'<'`  
  - `'<='`  
  - `'<>'`  
  - `'='`  

  Function Call:
```sql
SELECT DATECHK('2016/07/12', 'yyyy/MM/dd|yyyyMMdd', '>', '20151231');
```

### `COMPARE (value1  , comparator  , value2  , datatype  )`  
  COMPARE returns true when (a) both `value1` and `value2` can be parsed according
  to the `datatype` and (b) when the conditional value of `value1 comparator value2`
  returns true.  

  Valid datatypes are:  

  - `float`  
  - `double`  
  - `decimal`  
  - `byte`  
  - `sbyte`  
  - `short`  
  - `ushort`  
  - `int`  
  - `uint`  
  - `long`  
  - `ulong`  

  Valid comparators are:

  - `'>'`  
  - `'>='`  
  - `'<'`  
  - `'<='`  
  - `'<>'`  
  - `'='`  

  Function Call:
```sql
SELECT COMPARE(1.618, '>', 1, 'float');
```


### `UUID ()`  
  UUID generates a UUID using `System.Guid.NewGuid()`.

  Function call:
```sql
SELECT UUID();
```
