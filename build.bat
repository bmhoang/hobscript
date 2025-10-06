@echo off
echo Building HobScript Library...
echo.

echo Building main library...
dotnet build HobScript.csproj --configuration Release
if %ERRORLEVEL% neq 0 (
    echo Build failed!
    exit /b 1
)

echo.
echo Building test project...
dotnet build HobScript.Tests.csproj --configuration Release
if %ERRORLEVEL% neq 0 (
    echo Test build failed!
    exit /b 1
)

echo.
echo Running tests...
dotnet test HobScript.Tests.csproj --configuration Release --verbosity normal
if %ERRORLEVEL% neq 0 (
    echo Tests failed!
    exit /b 1
)

echo.
echo Running examples...
dotnet run --project HobScript.csproj
if %ERRORLEVEL% neq 0 (
    echo Examples failed!
    exit /b 1
)

echo.
echo Build completed successfully!
pause
