Expand-Archive -Path "C:\azure-devops-deployment\HM.Operations.Secure.Web.zip" -DestinationPath "C:\azure-devops-deployment -Force
Copy-Item -Path "C:\azure-devops-deployment\Content\C_C\agent-ic-admin-dev\_work\2\s\HM.Operations.Secure.Web\obj\Local\Package\PackageTmp" -Destination "C:\azure-devops-deployment\deployment" -Recurse -Force
 Copy-Item -Path "C:\azure-devops-deployment\deployment\*" -Destination \\vmwclnwosdusc01\devops-share -Recurse -Force