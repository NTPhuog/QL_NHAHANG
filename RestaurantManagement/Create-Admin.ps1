$ErrorActionPreference = 'Stop'
$env:RESTAURANT_ADMIN_EMAIL = Read-Host 'Email admin'
$secure = Read-Host 'Mật khẩu admin (12–128 ký tự)' -AsSecureString
try {
    $env:RESTAURANT_ADMIN_PASSWORD = [System.Net.NetworkCredential]::new('', $secure).Password
    dotnet run --project (Join-Path $PSScriptRoot 'RestaurantManagement.csproj') -- --create-admin
} finally {
    Remove-Item Env:RESTAURANT_ADMIN_PASSWORD -ErrorAction SilentlyContinue
    Remove-Item Env:RESTAURANT_ADMIN_EMAIL -ErrorAction SilentlyContinue
    $secure.Dispose()
}
