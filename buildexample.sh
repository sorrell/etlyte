#!/bin/bash

rm -rf ETLyte
mkdir ETLyte
mkdir ETLyte/Flatfiles
mkdir ETLyte/x86
mkdir ETLyte/x64
cp -rf ExampleConfig/Flatfiles ETLyte
cp -rf ExampleConfig/Schemas ETLyte
cp -rf ExampleConfig/SeedData ETLyte
cp -rf ExampleConfig/Validation ETLyte
cp ExampleConfig/config.json ETLyte
cp ETLyteDLL/bin/Release/*.dll ETLyte
cp ETLyteExe/bin/Release/*.dll ETLyte
cp ETLyteDLL/bin/Release/x86/* ETLyte/x86
cp ETLyteDLL/bin/Release/x64/* ETLyte/x64
cp ETLyteExe/bin/Release/*.exe ETLyte
chmod +x ETLyte/ETLyteExe.exe
rm -f ETLyte/*vshost.exe
rm -f ETLyte.zip
zip -r ETLyte_Example.zip ETLyte
rm -rf ETLyte