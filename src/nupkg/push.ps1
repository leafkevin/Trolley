# 获取当前目录路径
cd $PSScriptRoot
$filelist=dir *.nupkg

foreach($file in $filelist) {
    dotnet nuget push $file -k oy2fcjf5ldovgdempxpq5aneean4lznepeel5corlghfsu -s https://api.nuget.org/v3/index.json
}
pause
