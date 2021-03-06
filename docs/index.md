---
layout: default
title: "ETLyte"
---

## What is ETLyte?
The DLL is a cross platform .NET assembly that extracts data from flat files based on a JSON schema file that describes the data, inserts that data into a SQLite database, then validates it based on the constraints described in the file and whatever custom validations are provided.  It can also iterate through custom Transformations and can load ad-hoc queries in MSSQL using the datatypes described in the schema file.  This all adds up to never having to recompile any code when the flat file format changes - simply change JSON schema file.

The interactive CLI exposes the custom .NET functions that, among other useful things, check data typing.  It also features command history and word completion.

The inspiration and attribution for this tool goes to Dealers Assurance Corp, who hired me to consult and create this tool.

![etlyte-overview](https://cint.io/ETLyte/etlyte.png)

More details on the DLL and EXE below.

### DLL

- Read multiple flat files in single execution
- All logic is JSON/SQL file based, no recompiliation needed for new file formats
- 4 configurable steps {Extract, Validate, Transform, Load}
- Incremental loading by reusing SQLite DB + referencing already read filenames
- Load to
   + MSSQL (OleDB, still in progress)
   + Another file
- Configurable
   + Line terminator
   + Quote char
   + Delimiter
   + Specify flat files with regex (useful for loading multiple of the same type)
- Column handling
   + Error on missing columns
   + Fill (custom value) missing columns
   + Ignore or error on too many columns
- Derived columns
   + Supply any values that would be valid in an UPDATE/SET statement after tables have been loaded (including functions like UUID() or CURRENT_TIMESTAMP)
- Validations
   + Constraint types:
      * Datatype (string, byte, sbyte, short, ushort, int, uint, long, ulong, float, double, decimal, Boolean, DateTime, object, char, TimeSpan, URI)
      * Unique
      * Required
      * Min/max length
      * Min/max values
   + Custom SQL-based validations
   + Customizable error-level for each constraint type
   + Configurable validation output (delimited/plain/JSON)
- Configurable output for each step (file/console/db)
- Ability to insert seed data before validating
- Uses SqlitePCL, so it's cross-platform
- Fast and efficient (current benchmark of reading 250MB of files and writing 104MB of errors takes about 2 minutes on i7 with SSD)

### EXE

- To use:  ./ETLyte.exe -i myDb.db
- SQLite wrapper with extras
- Exposes custom functions in DLL
   + REGEXP
   + ISDATETIME
   + ISBOOL
   + ISBYTE
   + ISSBYTE
   + ISSHORT
   + ISUSHORT
   + ISINT
   + ISUINT
   + ISLONG
   + ISULONG
   + ISFLOAT
   + ISDOUBLE
   + ISDECIMAL
   + ISCHAR
   + ISTIMESPAN
   + ISURI
   + DATECHK
   + COMPARE
   + UUID
- Command history
- Word completion (based on column/table names)
