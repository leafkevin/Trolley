# ��ȡ��ǰĿ¼·��
cd $PSScriptRoot
$filelist=dir *.nupkg,*.snupkg

foreach($file in $filelist) {
    dotnet nuget push $file -k 123456 -s http://localhost:8085/v3/index.json
}
pause
