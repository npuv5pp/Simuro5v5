#Created by AzureFx on 2019/4/19

$msbuild = (Get-Command 'MSBuild.exe' -ErrorAction Ignore).Source
if ($null -eq $msbuild) {
    $msbuild = (Get-ChildItem -Path "${env:ProgramFiles(x86)}\Microsoft Visual Studio\2017\Community\MSBuild" -Recurse -File 'MSBuild.exe' | Where-Object { $_.Directory.Name -eq 'Bin' } | Sort-Object -Property LastWriteTime | Select-Object -Last 1).FullName
}
if ($null -eq $msbuild) {
    $msbuild = Read-Host -Prompt '请提供 MSBuild.exe 可执行程序的路径'
}
$msbuild = Resolve-Path $msbuild -ErrorAction Stop

$sln = Resolve-Path '.\V5RPC\V5RPC.sln' -ErrorAction Stop
$outputPath = '.\Assets\BinDeps\V5RPC\'
New-Item -Path $outputPath -ItemType Directory -ErrorAction Ignore | Out-Null
$outputPath = Resolve-Path $outputPath -ErrorAction Stop
$escapedPath = '"' + ($outputPath -replace '\\', '/') + '"'
&$msbuild '/t:Restore;Build' "/p:Configuration=Release;OutputPath=$escapedPath" $sln
Get-ChildItem -Path $outputPath | Where-Object { $_.Extension -in '.pdb', '.json' } | Remove-Item
