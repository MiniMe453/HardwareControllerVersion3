@echo off
taskkill /IM asperascp.exe /F
cd "C:\Program Files (x86)\Aspera\Point-to-Point\bin\"
Start "" /b asperascp.exe 