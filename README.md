## What is ETLyte?
The DLL is a cross platform .NET assembly that extracts data from flat files based on a JSON schema file that describes the data, inserts that data into a SQLite database, then validates it based on the constraints described in the file and whatever custom validations are provided.  It can also iterate through custom Transformations and can load ad-hoc queries in MSSQL using the datatypes described in the schema file.  This all adds up to never having to recompile any code when the flat file format changes - simply change JSON schema file.

The interactive CLI exposes the custom .NET functions that, among other useful things, check data typing.  It also features command history and word completion.

The inspiration and attribution for this tool goes to Dealers Assurance Corp, who hired me to consult and create this tool.

![etlyte-overview](https://cint.io/ETLyte/etlyte.png)

[See full documentation here](https://sorrell.github.io/etlyte)

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

## Installation
### Prerequisites
Linux: `mono-complete` (Follow the instructions from the Mono project on how to add the repository that contains this)  
Windows: .NET 4.5 and VC++ 2014u4 Redistributable

### Installing
You can either download the zip files (ETLyte.zip or ETLyte\_Example.zip) or clone the repo and build the solution yourself.  

## Usage
The general goal behind ETLyte is to validate CSV data, including across multiple files.  It can however
be extended to load MSSQL (in progress).

[See full documentation here](https://sorrell.github.io/etlyte)

Watch the overview [video here](https://www.youtube.com/watch?v=jsrXv0dwnLI).

**NOTE:** Because I'm still creating documentation and "recipes" for ETLyte (aka, there is scant documentation at the moment), feel free to contact me with any questions - cincyfire _at_ gmail.com

### General Usage
To use ETLyte, you will want to take a look at the examples in the ExampleConfig folder.  The general formula is: 

1. Setup the `config.json` as you see fit. 
2. Place your CSVs/flatfiles in the Flatfiles directory
3. Create schemas to describe the files and place them in the Schemas folder
4. (Optional) Create and Validations, Transformations, or Loads and place them in their respective folder 
5. Run ETLyte from the command line

### Interacting with the data
Since the result of a successful file read is a SQLite database with your data, you could use `sqlite3` to
perform queries and interrogate the data.

However, it can be more useful to use the ETLyte REPL since it contains many extra functions and will
soon feature a more comprehensive word completion tool (it will also feature DECLARE variables soon).

To do this, run `ETLyte.exe -i my.db`.


## Roadmap
- Finished MSSQL load target
- Postgres, MySql load targets
- Preprocess routing (an idea to specify different schemas based on sampling the data files)
- Schemaless loading
- Data type inferencing
- Load from URL
- Primary keys
