@echo off
echo 正在移除目錄維護工具的定時任務...
schtasks /Delete /TN "DirectoryMaintenanceTool" /F
echo 移除完成！
pause 