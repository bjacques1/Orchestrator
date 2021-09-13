<#
.SYNOPSIS
Dump the information in an Orchestrator Integration Pack, including the SQL scripts.
#>

[CmdletBinding(SupportsShouldProcess=$false)]

Param(
	[Parameter(Mandatory=$true, ValueFromPipeline=$true, HelpMessage="File name of the OIP file")]
	[ValidateNotNullOrEmpty()]
	[String]
	# File name of the OIP file
	$FileName,
	
	[Parameter(HelpMessage="Display the raw SQL script, no formatting")]
	[Switch]
	# Display the raw SQL script, no formatting
	$RawSql
)

Process
{
	$tmpZipFile = [System.IO.Path]::GetTempFileName() + ".zip"
	Copy-Item -Path $FileName -Destination $tmpZipFile -ErrorAction Stop

	$tmpFolder = [System.IO.Path]::GetTempPath()

	$shellApp = New-Object -ComObject Shell.Application
	$zipFile = $shellApp.NameSpace($tmpZipFile)
	$tmpCapFile = ""

	$folder = $shellApp.NameSpace($tmpFolder)

	foreach ($item in $zipFile.Items())
	{
		if ($item.Name.EndsWith(".cap"))
		{
			$folder.CopyHere($item)
			$tmpCapFile = [System.IO.Path]::Combine($tmpFolder, $item.Name)
		}
	}

	$content = Get-Content $tmpCapFile -ErrorAction Stop

	#Remove all temp files
	Remove-Item -Force -Path $tmpZipFile
	Remove-Item -Force -Path $tmpZipFile.Replace(".zip", "")
	Remove-Item -Force -Path $tmpCapFile

	$xmlCap = [XML]$content

	Write-Host("Integration Pack: {0}" -f $xmlCap.Cap.Name)
	Write-Host("            GUID: {0}" -f $xmlCap.Cap.UniqueID)
	Write-Host("     Description: {0}" -f $xmlCap.Cap.Description)
	Write-Host("         Version: {0}" -f $xmlCap.Cap.Version)
	Write-Host(" ServerExtension: {0}" -f $xmlCap.Cap.Library)
	Write-Host

	Write-Host "=== ACTIVITIES ==="
	$activities = $xmlCap.SelectNodes("//ObjectType")
	foreach ($a in $activities)
	{
		Write-Host("  {0} {1}" -f $a.UniqueID, $a.Name)
		Write-Host("    >> {0}" -f $a.Description)
		Write-Host
	}

	Write-Host "=== SQL SCRIPTS for INSTALL ==="
	$scripts = $xmlCap.Cap.Install.SelectNodes("//Query")
	foreach ($s in $scripts)
	{
		if (!$RawSql)
		{
			$s = $s.InnerText.Replace(",",",`n        ").Replace(")",")`n        ")
		}
		else
		{
			$s = $s.InnerText
		}
		Write-Host "  " $s
	}
	
	Write-Host

	Write-Host "=== SQL SCRIPTS for UPGRADE ==="
	if ($xmlCap.Cap.Upgrade -ne $null)
	{
		$scripts = $xmlCap.Cap.Upgrade.SelectNodes("//Query")
		foreach ($s in $scripts)
		{
			if (!$RawSql)
			{
				$s = $s.InnerText.Replace(",",",`n        ").Replace(")",")`n        ")
			}
			else
			{
				$s = $s.InnerText
			}
			Write-Host "  " $s
		}
	}
	else
	{
		Write-Host "  N.A."
		Write-Host
	}
}