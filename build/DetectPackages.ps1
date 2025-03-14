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
	if ($env:BUILD_SOURCEBRANCHNAME -eq "refs/heads/main") {
		$tag = "$pkg.Name_v$pkg.Version"
		Write-Host "Tagging Build: $tag"
		$message = "Package $pkg.Name Version $pkg.Version"
		git tag $tag
	}
	$pkgs += $pkg
}
if ($env:BUILD_SOURCEBRANCHNAME -eq "refs/heads/main") {
	git push origin --tags
}
$pkgs | Format-Table -Property Name, Version
Write-Host "Package Count: $($packages.Count)"
Write-Host ("##vso[task.setvariable variable=package_count;]$($packages.Count)")
$pushPackages = ($env:BUILD_SOURCEBRANCHNAME -eq "refs/heads/main") -and ($packages.Count -gt 0)
Write-Host ("##vso[task.setvariable variable=push_packages;]$($pushPackages)")
$releaseNotes = $env:AGENT_TEMPDIRECTORY + "\ReleaseNotes.md"
$header = "## Packages$([Environment]::NewLine)"
$header | Out-File $releaseNotes
$pkgs | ConvertTo-MarkdownTable | Add-Content $releaseNotes