### To install Nuget package
1. position yourself inside the project
2. create Nuget package using command: **dotnet pack**
3. install the package using command: **dotnet tool install --global --add-source ./nupkg Kodecta.Generator.Schematics**
4. use the project anywhere on your system using command: **SchematicsGenerator -t <generatorName> <modelName>**, for example: **SchematicsGenerator -t component TestName**