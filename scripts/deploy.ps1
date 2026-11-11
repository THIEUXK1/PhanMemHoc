<#
  Deploy PhanMemHoc len server (giu nguyen DB tren server, tu backup DB).
  Dung:  powershell -ExecutionPolicy Bypass -File scripts/deploy.ps1 [-SkipTests]
  Cau hinh server: scripts/deploy.local.ps1 (KHONG commit; mau: deploy.local.example.ps1).
  Xac thuc: SSH key; khong co key thi dung $env:PMH_SSH_PASSWORD (plink/pscp).
#>
param([switch]$SkipTests)
$ErrorActionPreference = 'Stop'

$LocalCfg = Join-Path $PSScriptRoot 'deploy.local.ps1'
if (-not (Test-Path $LocalCfg)) { throw "Thieu $LocalCfg - chep tu deploy.local.example.ps1 roi dien thong tin server." }
. $LocalCfg   # dat $Server, $HostKey, $PublicUrl, $KeyFile
foreach ($v in 'Server','HostKey','PublicUrl','KeyFile') {
    if (-not (Get-Variable $v -ValueOnly -ErrorAction SilentlyContinue)) { throw "deploy.local.ps1 thieu `$$v" }
}
$Root    = Split-Path $PSScriptRoot -Parent
$Work    = Join-Path $env:TEMP 'phanmemhoc-deploy'
$Publish = Join-Path $Work 'publish'
$Tar     = Join-Path $Work 'pmh.tar.gz'
$Remote  = Join-Path $PSScriptRoot 'server-redeploy.sh'
$sw = [Diagnostics.Stopwatch]::StartNew()

function Step($msg) { Write-Host ("[{0,5:N0}s] {1}" -f $sw.Elapsed.TotalSeconds, $msg) -ForegroundColor Cyan }
function Check($what) { if ($LASTEXITCODE -ne 0) { throw "$what that bai (exit $LASTEXITCODE)" } }

$UseKey = Test-Path $KeyFile
if (-not $UseKey -and -not $env:PMH_SSH_PASSWORD) {
    throw "Chua co SSH key $KeyFile va chua dat `$env:PMH_SSH_PASSWORD."
}
function Upload($local, $dest) {
    if ($UseKey) { scp -q -i $KeyFile -o IdentitiesOnly=yes -o StrictHostKeyChecking=accept-new $local "${Server}:$dest" }
    else { pscp -batch -q -hostkey $HostKey -pw $env:PMH_SSH_PASSWORD $local "${Server}:$dest" }
    Check "Upload $local"
}
function Remote($cmd) {
    if ($UseKey) { ssh -i $KeyFile -o IdentitiesOnly=yes -o StrictHostKeyChecking=accept-new $Server $cmd }
    else { plink -ssh -batch -hostkey $HostKey -pw $env:PMH_SSH_PASSWORD $Server $cmd }
    Check "Lenh tren server"
}

Push-Location $Root
try {
    New-Item -ItemType Directory -Force $Work | Out-Null

    if (-not $SkipTests) {
        Step 'Test...'
        # artifacts rieng: khong dung bin/obj Debug (app local dang chay se khoa file)
        dotnet test -v q --nologo --artifacts-path (Join-Path $Work 'testart') | Select-String 'Passed!|Failed!|error' | Select-Object -First 20
        Check 'dotnet test'
    }

    Step 'Publish linux-x64 self-contained...'
    if (Test-Path $Publish) { Remove-Item -Recurse -Force $Publish }
    dotnet publish src/PhanMemHoc -c Release -r linux-x64 --self-contained true -o $Publish -v q -clp:ErrorsOnly --nologo
    Check 'dotnet publish'
    Get-ChildItem $Publish -Filter *.db | Remove-Item -Force   # khong bao gio day DB local len

    Step 'Nen...'
    if (Test-Path $Tar) { Remove-Item -Force $Tar }
    tar -czf $Tar -C $Publish .
    Check 'tar'

    Step ("Upload {0:N0} MB..." -f ((Get-Item $Tar).Length / 1MB))
    Upload $Tar '/tmp/pmh.tar.gz'
    Upload $Remote '/tmp/pmh-redeploy.sh'

    Step 'Cai dat tren server...'
    Remote 'tr -d ''\r'' < /tmp/pmh-redeploy.sh | bash'

    Step 'Kiem tra tu ngoai...'
    $code = (Invoke-WebRequest $PublicUrl -UseBasicParsing -TimeoutSec 15).StatusCode
    Step "XONG - $PublicUrl tra $code"
}
finally { Pop-Location }
