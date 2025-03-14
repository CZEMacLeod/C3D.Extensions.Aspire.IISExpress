Import-Module .\build\ConvertTo-MarkdownTable.ps1
$dir = $env:BUILD_ARTIFACTSTAGINGDIRECTORY + "\*.nupkg"
Write-Host "Package Directory: $dir"
$packages = Get-ChildItem -Path $dir -Recurse
$ids = $packages | Select-Object -ExpandProperty name
$pkgs = @()
$origin = "HEAD:$env:BUILD_SOURCEBRANCHNAME"
ForEach ($id in $ids) {
	$names = $id.Split(".")
	$name = $names[(0..($names.Length-5))] -join "."
	$version = $names[(($names.Length-4)..($names.Length-2))] -join "."
	$pkg = New-Object -TypeName PSObject -Property @{
		Name = $name
		Version = $version
	}
	Write-Host "Tagging Build: $id"
	$message = "Package $pkg.Name Version $pkg.Version"
	git tag -$id
	$pkgs += $pkg
}
git push origin --tags
$pkgs | Format-Table -Property Name, Version
Write-Host "Package Count: $($packages.Count)"
Write-Host ("##vso[task.setvariable variable=package_count;]$($packages.Count)")
$releaseNotes = $env:AGENT_TEMPDIRECTORY + "\ReleaseNotes.md"
"## Packages" | Out-File $releaseNotes
$pkgs | ConvertTo-MarkdownTable | Add-Content $releaseNotes