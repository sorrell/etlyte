---
layout: page
title: "Quickstart"
category: ref
date: 2017-04-24 22:21:07
---

The general goal behind ETLyte is to validate CSV data, including across multiple files.  It can however
be extended to load MSSQL (in progress).

Watch the overview [video here](https://www.youtube.com/watch?v=jsrXv0dwnLI).

### General Usage
To use ETLyte, you will want to take a look at the examples in the ExampleConfig folder.  The general formula is:

1. Setup the `config.json` as you see fit.
2. Place your CSVs/flatfiles in the Flatfiles directory
3. Create schemas to describe the files and place them in the Schemas folder
4. (Optional) Create Validations, Transformations, or Loads and place them in their respective folders
5. Run ETLyte from the command line

### Interacting with the data
Since the result of a successful file read is a SQLite database with your data, you could use `sqlite3` to perform queries and interrogate the data.

However, it can be more useful to use the ETLyte REPL since it contains many extra functions and will
soon feature a more comprehensive word completion tool (it will also feature DECLARE variables soon).

To do this, run `ETLyte.exe -i my.db`.
