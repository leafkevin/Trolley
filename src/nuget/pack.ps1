# 获取当前目录路径
cd $PSScriptRoot

# 定义所有项目
$projects = (
	"Trolley",
	"Trolley.MySqlConnector",
	"Trolley.SqlServer"
)
Remove-Item *.nupkg -recurse
Remove-Item *.snupkg -recurse

# 打包
foreach($project in $projects) {
	cd $PSScriptRoot
	cd ../$project
	Remove-Item ./bin/Release/*.nupkg -recurse
	Remove-Item ./bin/Release/*.snupkg -recurse
	dotnet clean
	dotnet build -c Release
    dotnet pack -c Release -p:IncludeSymbols=true -p:SymbolPackageFormat=snupkg
    mv ./bin/Release/*.nupkg ../nuget/
	mv ./bin/Release/*.snupkg ../nuget/
    cd ..
}
pause