try {
    $pw = ConvertTo-SecureString 'vmffpdl2@' -AsPlainText -Force
    $cred = New-Object System.Management.Automation.PSCredential('172.25.0.21\wnstn1342', $pw)
    Write-Host "Connecting..."
    $s = New-PSSession -ComputerName '172.25.0.21' -Credential $cred -ErrorAction Stop
    Write-Host "Connected. Checking app pool..."
    Invoke-Command -Session $s -ScriptBlock {
        Import-Module WebAdministration -ErrorAction Stop
        Get-WebAppPoolState -Name 'event.ifa.co.kr'
    }
    Remove-PSSession $s
    Write-Host "OK"
} catch {
    Write-Host "ERROR: $_"
    Write-Host "TYPE: $($_.Exception.GetType().FullName)"
}
