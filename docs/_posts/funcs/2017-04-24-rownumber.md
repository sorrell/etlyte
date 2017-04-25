---
layout: page
title: "ROW_NUMBER"
category: funcs
date: 2017-04-24 20:50:08
---

### `ROW_NUMBER (ColumnName)`  
  ROW_NUMBER creates a Dictionary in memory to create the row numbers
  for the given column name.  It works a little differently than the
  analogous function in other RDBMS where `OVER` and a window is used.

  In ETLyte, the user must first order the selection manually, and then
  apply the `ROW_NUMBER` function.

  The following example should illustrate that case and **the way to dodge
  thorny query flattening**:

```sql
CREATE TABLE test (name, state, mydate);
INSERT INTO test values('nick','OH', '2013-01-02');
INSERT INTO test values('nate','OH', '2017-03-02');
INSERT INTO test values('john','TN', '2017-01-02');
INSERT INTO test values('kara','KY', '2017-01-02');
INSERT INTO test values('kevin','KY', '2011-03-03');
INSERT INTO test values('jim','KY', '2012-11-02');

SELECT ROW_NUMBER(state) AS rn, * FROM (SELECT * FROM test ORDER BY state, mydate LIMIT -1 OFFSET 0);

-- Returns
-- rn|name|state|mydate
-- 1|kevin|KY|2011-03-03
-- 2|jim|KY|2012-11-02
-- 3|kara|KY|2017-01-02
-- 1|nick|OH|2013-01-02
-- 2|nate|OH|2017-03-02
-- 1|john|TN|2017-01-02

```

By default, SQLite will start with the lowest ROWID and increment to the highest,
so if the `LIMIT -1 OFFSET 0` is removed, then the query will be flattened
and return undesired results.  In the example below, the results seem incorrect,
but it is because `kara` was entered into the database before `kevin` and
so the query has been flattened.

```sql
SELECT ROW_NUMBER(state) AS rn, * FROM (SELECT * FROM test ORDER BY state, mydate);

-- Returns
-- rn|name|state|mydate
-- 2|kevin|KY|2011-03-03
-- 3|jim|KY|2012-11-02
-- 1|kara|KY|2017-01-02
-- 1|nick|OH|2013-01-02
-- 2|nate|OH|2017-03-02
-- 1|john|TN|2017-01-02

-- Query actually flattened to this:
SELECT ROW_NUMBER(state) AS rn, * FROM (SELECT * FROM test) ORDER BY state, mydate;
```
