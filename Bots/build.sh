#!/bin/bash

dotnet build
cp bin/Debug/netstandard2.1/Bots.dll ../GameRunner
cp bin/Debug/netstandard2.1/Bots.dll /mnt/c/Users/adamc/source/repos/ScriptsOfTribute-GUI/Assets/StreamingAssets
# cp bin/Debug/netstandard2.1/Bots.dll ../ParallelRunner
cd ../GameRunner
dotnet build
# cd ../ParallelRunner
# dotnet build
cd ../Bots
