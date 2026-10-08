$ErrorActionPreference='Stop'
$sites=@(@{Name='Customer';Port=5097},@{Name='Staff';Port=5098},@{Name='Admin';Port=5099})
# Reserve-check each port before building; never stop a process owned by another terminal.
foreach($site in $sites){
 $listener=[Net.Sockets.TcpListener]::new([Net.IPAddress]::Loopback,$site.Port)
 try{$listener.Start()}catch{throw "Cong $($site.Port) dang duoc su dung. Dung ung dung cu bang Ctrl+C trong terminal cua no, roi chay lai."}finally{$listener.Stop()}
}
dotnet build (Join-Path $PSScriptRoot 'Restaurant.slnx') --nologo -m:1
if($LASTEXITCODE -ne 0){throw 'Build failed.'}
$children=[Collections.Generic.List[object]]::new()
$logDir=Join-Path $PSScriptRoot '.runlogs'
New-Item -ItemType Directory -Path $logDir -Force|Out-Null
$previousEnvironment=$env:ASPNETCORE_ENVIRONMENT
try{
 $env:ASPNETCORE_ENVIRONMENT='Development'
 foreach($site in $sites){
  $dir=Join-Path $PSScriptRoot ('Restaurant.'+$site.Name)
  $exe=Join-Path $dir "bin/Debug/net10.0/Restaurant.$($site.Name).exe"
  $child=Start-Process -FilePath $exe -ArgumentList @('--urls',"http://localhost:$($site.Port)") -WorkingDirectory $dir -WindowStyle Hidden -PassThru -RedirectStandardOutput (Join-Path $logDir "$($site.Name).log") -RedirectStandardError (Join-Path $logDir "$($site.Name)-error.log")
  $children.Add($child)
  Write-Host "$($site.Name): http://localhost:$($site.Port)"
 }
 Start-Sleep -Seconds 2
 foreach($child in $children){if($child.HasExited){throw 'Mot ung dung khong khoi dong duoc; xem thu muc .runlogs.'}}
 Write-Host 'Ba ung dung dang chay. Ctrl+C tai terminal nay de dung ca ba. Log: .runlogs'
 while(@($children|Where-Object {!$_.HasExited}).Count -gt 0){Start-Sleep -Seconds 1}
}finally{
 foreach($child in $children){if(!$child.HasExited){Stop-Process -Id $child.Id -ErrorAction SilentlyContinue}}
 $env:ASPNETCORE_ENVIRONMENT=$previousEnvironment
}
