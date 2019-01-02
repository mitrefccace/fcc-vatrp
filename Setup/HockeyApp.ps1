if ($env:APPVEYOR_REPO_BRANCH -ne "master")
{
 Write-Host 'Skip "HockeyApp" deployment as no branches matched (build branch is "'"$env:APPVEYOR_REPO_BRANCH"'", deploy on branches "master")'
 exit 0
}



Get-ChildItem .\Setup\Out\*.msi | ForEach-Object {
  $msi=$_.FullName

  $id=$(& C:\ProgramData\Chocolatey\bin\curl -sLk `
    -F "bundle_version=$env:APPVEYOR_BUILD_NUMBER" `
    -F "bundle_short_version=$env:APPVEYOR_BUILD_VERSION" `
    -F "notes=$env:APPVEYOR_REPO_COMMIT_MESSAGE" `
    -F "notes_type=1" `
    -F "status=2" `
    -F "teams=$env:HOCKEYAPP_TEAM_IDS" `
    -H "X-HockeyAppToken: $env:HOCKEYAPP_TOKEN" `
    https://rink.hockeyapp.net/api/2/apps/$env:HOCKEYAPP_ID/app_versions/new | jq -r .id)

  & C:\ProgramData\Chocolatey\bin\curl -sLk `
    -X PUT `
    -F "ipa=@$msi" `
    -F "notes=$env:APPVEYOR_REPO_COMMIT_MESSAGE" `
    -F "notes_type=1" `
    -F "notify=1" `
    -F "status=2" `
    -F "teams=$env:HOCKEYAPP_TEAM_IDS" `
    -F "mandatory=0" `
    -H "X-HockeyAppToken: $env:HOCKEYAPP_TOKEN" `
    https://rink.hockeyapp.net/api/2/apps/$env:HOCKEYAPP_ID/app_versions/$id | jq .
}

