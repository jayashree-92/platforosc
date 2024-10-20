param (
    [string]$rootDir,
    [string]$rootDirSql = "$rootDir\HMSQLDBProject\HM_DMA",  # Default value, can be overridden
    [string]$sqlServer = "vmwclnsqldusc01",  # Default value, can be overridden
    [string]$sqlDB = "HM_WIRES"              # Default value, can be overridden
)

# Set variables
$DoubleQuote = '"'
$dirSql = "${rootDirSql}\CreateScripts"
$ChangeSql = "${rootDirSql}\ChangeScripts"
$DataSql = "${rootDirSql}\DataScripts"
$DMAStagingTypes = "${rootDirSql}\DMAStagingTypes.sql"

Write-Host "Executing Scripts to DB: $sqlDB in server: $sqlServer"
hostname
Write-Host "Current Directory: $(Get-Location)"

# Execute SQL files
$scriptPaths = @("${dirSql}\Tables\*.sql", "${ChangeSql}\*.sql", "${dirSql}\Views\*.sql", "${DataSql}\*.sql")

foreach ($scriptPath in $scriptPaths) {
    foreach ($sqlScript in Get-ChildItem -Path $scriptPath -Recurse) {
        $scriptFileName = $sqlScript.Name
        Write-Host "* Executing $scriptFileName"
        sqlcmd -I -S $sqlServer -E -d $sqlDB -i "$sqlScript"
    }
}

# Execute Stored Procedures and Functions
$SPPaths = @("${dirSql}\StoredProcedures\*.sql", "${dirSql}\Functions\*.sql")

foreach ($SPPath in $SPPaths) {
    foreach ($spScript in Get-ChildItem -Path $SPPath -Recurse) {
        $spFileName = $spScript.Name
        Write-Host "* Executing $spFileName"
        sqlcmd -I -S $sqlServer -E -d $sqlDB -i "$spScript"
    }
}
