# Chep thanh scripts/deploy.local.ps1 (da .gitignore) roi dien gia tri that.
$Server    = 'deploy@<ip-hoac-ten-mien>'
$HostKey   = 'SHA256:<fingerprint-host-key>'
$PublicUrl = 'http://<ip-hoac-ten-mien>:<port>/'
$KeyFile   = Join-Path $env:USERPROFILE '.ssh\<ten-key>'
