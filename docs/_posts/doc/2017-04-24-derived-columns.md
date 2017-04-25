---
layout: page
title: "Derived Columns"
category: doc
date: 2017-04-24 22:15:23
---

Derived columns work the way a SQL `UPDATE` does and is performed after table load,
so that you have access to all of the tables columns and values at derivation time.  

### Examples
In the following example, we will create the `TestCol` column and set it equal
to `'john doe'`.  Because this value will be included in a SQLite `UPDATE` statement
it's important that we quote the text.

```json
  {
    "name": "TestCol",
    "columnType": "Derived",
    "derivation": "'john doe'"
  }
```

In the next example, we will utilize a column (`MyInteger`) and create a new column
that squares that value.

```json
  {
    "name": "TestCol",
    "columnType": "Derived",
    "derivation": "MyInteger * MyInteger",
    "datatype": "int"
  }
```

In the next example, we will utilize the function `UUID` to create UUIDs for
the derived column.

```json
{
  "name": "TestCol",
  "columnType": "Derived",
  "derivation": "UUID()"
}
```
In the final example, we will show how useful the derivation can be by utilizing
a `CASE` statement.

```json
{
  "name": "TestCol",
  "columnType": "Derived",
  "derivation": "CASE
                  WHEN (ROWID % 2) = 0
                    THEN ROWID || ' IS EVEN'
                  ELSE ROWID || ' IS ODD'
                 END"
}
```


### Under the Hood
When ETLyte reads the schema file it will create the Derived Column in the `CREATE TABLE`
statement to SQLite.  Then, when data is being imported to SQLite, ETLyte gets a list
of all the column names that aren't Derived for the `INSERT` statement (if they aren't derived, they must be in the source).  Then, ETLyte loads the table with all of
the values in the flatfiles, and afterwards performs the derivations.
