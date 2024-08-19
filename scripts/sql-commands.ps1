# Set variables
$DoubleQuote = '"'
$rootDirSql = "..\HMSQLDBProject\HM_DMA"
$dirSql = "${rootDirSql}\CreateScripts"
$ChangeSql = "${rootDirSql}\ChangeScripts"
$DataSql = "${rootDirSql}\DataScripts"
$DMAStagingTypes = "${rootDirSql}\DMAStagingTypes.sql"

# Set SQL Server and Database details
$sqlServer = "vmwclnsqldusc01"
$sqlDB = "HM_WIRES"

Write-Host "Executing Scripts to DB: $sqlDB in server: $sqlServer"
Write-Host hostname
Write-Host "Current Directory: $(Get-Location)"

# Execute SQL files
$scriptPaths = @("${dirSql}\Tables\*.sql", "${ChangeSql}\*.sql", "${dirSql}\Views\*.sql", "${DataSql}\*.sql")


foreach ($scriptPath in $scriptPaths) {
    foreach ($sqlScript in Get-ChildItem -Path $scriptPath -Recurse) {
        $scriptFileName = $sqlScript.Name
        Write-Host "* Executing $scriptFileName"
        sqlcmd -I -S $sqlServer -E -d $sqlDB -i $DoubleQuote$sqlScript$DoubleQuote
    }
}

# Execute Stored Procedures and Functions
$SPPaths = @("${dirSql}\StoredProcedures\*.sql", "${dirSql}\Functions\*.sql")

foreach ($SPPath in $SPPaths) {
    foreach ($spScript in Get-ChildItem -Path $SPPath -Recurse) {
        $spFileName = $spScript.Name
        Write-Host "* Executing $spFileName"
        sqlcmd -I -S $sqlServer -E -d $sqlDB -i $DoubleQuote$spScript$DoubleQuote
    }
}
