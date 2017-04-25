---
layout: page
title: "Interactive CLI"
category: doc
date: 2017-04-24 22:16:52
---

The Interactive CLI exposes all of the custom functions that ETLyte includes.
It also supports a session command history and word completion.

### Running the REPL
The Interactive CLI can be started using the `-i` flag.  If no SQLite database
is specified to connect to, an in-memory SQLite database will be created and
connected to.  

To connect to an existing database:

`ETLyteExe.exe -i mydatabase.db`
