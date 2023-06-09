#!/bin/bash

dotnet build
cp bin/Debug/netstandard2.1/Bots.dll ../GameRunner
cd ../GameRunner
dotnet build
cd ../Bots
