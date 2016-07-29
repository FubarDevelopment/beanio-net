param(
	[Parameter(Mandatory=$True)]
	[string]$Version,
	[string]$VersionSuffix = $null
)

$packageOutputPath = Join-Path $PSScriptRoot "package-output"
if (test-path $packageOutputPath) {
	remove-item -whatif -recurse -force $packageOutputPath
}

new-item $packageOutputPath

$fullVersion = $Version + (&{if ($VersionSuffix) { "-" + $VersionSuffix } else { "" }})

$nuspecPath = join-path (Join-Path $PSScriptRoot "packaging") "*.nuspec"
$nugetPath = join-path (Join-Path $PSScriptRoot "packaging") "NuGet.exe"

get-item $nuspecPath | % {
	$packageId = $_.Basename
	&$nugetPath pack $_.FullName -Properties "Configuration=Release;Id=$packageId" -OutputDirectory $packageOutputPath -Version $fullVersion
	if (!$?) { exit 1 }
}
