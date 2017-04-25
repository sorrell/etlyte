---
layout: page
title: "Datatype Checks"
category: funcs
date: 2017-04-25 09:34:19
---


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
