---
layout: page
title: "Functions"
category: doc
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

### `ISDATETIME (stringOrColumnName  , patternArray  )  `  
  ISDATETIME returns true if one of the patterns from the pipe delimited
  pattern array.

  Function Call:
```sql
SELECT ISDATETIME('20170402', 'MM/dd/yyyy|yyyyMMdd');
```

### `ISBOOL (stringOrColumnName  )  `  
  ISBOOL returns true if the string or column name can be parsed as boolean values.
  Supported inputs are `1`, `0`, and case insensitive variants of `'True'` and `'False'`.

  Function Call:
```sql
SELECT ISBOOL(1);
```

### `ISBYTE (stringOrColumnName  )  `  
  ISBYTE returns true if the string or column name can be parsed as byte values.  

  Function Call:  
```sql
SELECT ISBYTE(1);
```

### `ISSBYTE`  
  ISSBYTE returns true if the string or column name can be parsed as signed byte values.  

  Function Call:  
```sql
SELECT ISSBYTE(1);
```

### `ISSHORT`  
  ISSHORT returns true if the string or column name can be parsed as short values.  

  Function Call:  
```sql
SELECT ISSHORT(1);
```

### `ISUSHORT`  
  ISUSHORT returns true if the string or column name can be parsed as unsigned short values.  

  Function Call:  
```sql
SELECT ISUSHORT(1);
```

### `ISINT`  
  ISINT returns true if the string or column name can be parsed as integer values.  

  Function Call:  
```sql
SELECT ISINT(1);
```

### `ISUINT`  
  ISUINT returns true if the string or column name can be parsed as unsigned integer values.  

  Function Call:  
```sql
SELECT ISUINT(1);
```

### `ISLONG`  
  ISLONG returns true if the string or column name can be parsed as long values.  

  Function Call:  
```sql
SELECT ISLONG(1);
```

### `ISULONG`  
  ISULONG returns true if the string or column name can be parsed as unsigned long values.  

  Function Call:  
```sql
SELECT ISULONG(1);
```

### `ISFLOAT`  
  ISFLOAT returns true if the string or column name can be parsed as float values.  

  Function Call:  
```sql
SELECT ISFLOAT(1);
```

### `ISDOUBLE`  
  ISDOUBLE returns true if the string or column name can be parsed as double values.  

  Function Call:  
```sql
SELECT ISDOUBLE(1);
```

### `ISDECIMAL`  
  ISDECIMAL returns true if the string or column name can be parsed as decimal values.  

  Function Call:  
```sql
SELECT ISDECIMAL(1);
```

### `ISCHAR`  
  ISCHAR returns true if the string or column name can be parsed as char values.  

  Function Call:  
```sql
SELECT ISCHAR(1);
```

### `ISTIMESPAN`  
  ISTIMESPAN returns true if the string or column name can be parsed as timespan values.  

  Function Call:  
```sql
SELECT ISTIMESPAN(1);
```

### `ISURI`  
  ISURI returns true if the string or column name can be parsed as URI values.  

  Function Call:  
```sql
SELECT ISURI(1);
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
