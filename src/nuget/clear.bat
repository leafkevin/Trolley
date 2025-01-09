cd /d %~dp0
cd ..
cd ..

rd /s /q src\Trolley\bin
rd /s /q src\Trolley\obj
rd /s /q src\Trolley.MySqlConnector\bin
rd /s /q src\Trolley.MySqlConnector\obj
rd /s /q src\Trolley.PostgreSql\bin
rd /s /q src\Trolley.PostgreSql\obj
rd /s /q src\Trolley.SqlServer\bin
rd /s /q src\Trolley.SqlServer\obj
rd /s /q src\Trolley.Sqlite\bin
rd /s /q src\Trolley.Sqlite\obj
rd /s /q test\Trolley.Test\bin
rd /s /q test\Trolley.Test\obj
rd /s /q test\Trolley.Test.MySqlConnector\bin
rd /s /q test\Trolley.Test.MySqlConnector\obj
rd /s /q test\Trolley.Test.PostgreSql\bin
rd /s /q test\Trolley.Test.PostgreSql\obj
rd /s /q test\Trolley.Test.SqlServer\bin
rd /s /q test\Trolley.Test.SqlServer\obj