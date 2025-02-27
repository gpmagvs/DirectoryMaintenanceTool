@echo off
chcp 65001
echo 正在安裝目錄維護工具的定時任務...

REM 獲取當前目錄的完整路徑
set DEPLOYPATH=%~dp0
set DEPLOYPATH=%DEPLOYPATH:~0,-1%

REM 替換 XML 中的路徑變數
powershell -Command "(Get-Content '%DEPLOYPATH%\MaintenanceTask.xml') -replace '%%DEPLOYPATH%%', '%DEPLOYPATH%' | Set-Content '%DEPLOYPATH%\MaintenanceTask_temp.xml'"

REM 創建工作排程（使用 SYSTEM 帳戶）
schtasks /Create /TN "DirectoryMaintenanceTool" /XML "%DEPLOYPATH%\MaintenanceTask_temp.xml" /RU "SYSTEM" /F

REM 清理臨時文件
del "%DEPLOYPATH%\MaintenanceTask_temp.xml"

echo 安裝完成！
pause 