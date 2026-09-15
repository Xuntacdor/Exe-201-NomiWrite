[CmdletBinding()]
param(
    [string[]] $Service,
    [string] $EnvironmentFile = ".env",
    [switch] $DryRun
)

$ErrorActionPreference = "Stop"

$backendRoot = Resolve-Path (Join-Path $PSScriptRoot "..")
Set-Location $backendRoot

function Import-EnvFile {
    param([string] $Path)

    if (-not (Test-Path -LiteralPath $Path)) {
        Write-Host "Environment file not found: $Path"
        Write-Host "Continuing with existing process environment and appsettings.Development.json."
        return
    }

    Get-Content -LiteralPath $Path | ForEach-Object {
        $line = $_.Trim()
        if ($line.Length -eq 0 -or $line.StartsWith("#")) {
            return
        }

        $equalsIndex = $line.IndexOf("=")
        if ($equalsIndex -le 0) {
            return
        }

        $key = $line.Substring(0, $equalsIndex).Trim()
        $value = $line.Substring($equalsIndex + 1).Trim()

        if ($value.Length -ge 2 -and (($value.StartsWith('"') -and $value.EndsWith('"')) -or ($value.StartsWith("'") -and $value.EndsWith("'")))) {
            $value = $value.Substring(1, $value.Length - 2)
        }

        [Environment]::SetEnvironmentVariable($key, $value, "Process")
    }
}

$migrations = @(
    @{
        Name = "Auth"
        Context = "AuthDbContext"
        Connection = "ConnectionStrings__AuthDb"
        Project = "src/Services/Auth/NomiWrite.Auth.Infrastructure/NomiWrite.Auth.Infrastructure.csproj"
        StartupProject = "src/Services/Auth/NomiWrite.Auth.API/NomiWrite.Auth.API.csproj"
    },
    @{
        Name = "User"
        Context = "UserDbContext"
        Connection = "ConnectionStrings__UserDb"
        Project = "src/Services/User/NomiWrite.User.Infrastructure/NomiWrite.User.Infrastructure.csproj"
        StartupProject = "src/Services/User/NomiWrite.User.API/NomiWrite.User.API.csproj"
    },
    @{
        Name = "Writing"
        Context = "WritingDbContext"
        Connection = "ConnectionStrings__WritingDb"
        Project = "src/Services/Writing/NomiWrite.Writing.Infrastructure/NomiWrite.Writing.Infrastructure.csproj"
        StartupProject = "src/Services/Writing/NomiWrite.Writing.API/NomiWrite.Writing.API.csproj"
    },
    @{
        Name = "Grading"
        Context = "GradingDbContext"
        Connection = "ConnectionStrings__GradingDb"
        Project = "src/Services/AICoordinator/NomiWrite.AICoordinator.Infrastructure/NomiWrite.AICoordinator.Infrastructure.csproj"
        StartupProject = "src/Services/AICoordinator/NomiWrite.AICoordinator.API/NomiWrite.AICoordinator.API.csproj"
    },
    @{
        Name = "Payment"
        Context = "PaymentDbContext"
        Connection = "ConnectionStrings__PaymentDb"
        Project = "src/Services/Payment/NomiWrite.Payment.Infrastructure/NomiWrite.Payment.Infrastructure.csproj"
        StartupProject = "src/Services/Payment/NomiWrite.Payment.API/NomiWrite.Payment.API.csproj"
    },
    @{
        Name = "Subscription"
        Context = "SubscriptionDbContext"
        Connection = "ConnectionStrings__SubscriptionDb"
        Project = "src/Services/Subscription/NomiWrite.Subscription.Infrastructure/NomiWrite.Subscription.Infrastructure.csproj"
        StartupProject = "src/Services/Subscription/NomiWrite.Subscription.API/NomiWrite.Subscription.API.csproj"
    },
    @{
        Name = "Learning"
        Context = "LearningDbContext"
        Connection = "ConnectionStrings__LearningDb"
        Project = "src/Services/Learning/NomiWrite.Learning.Infrastructure/NomiWrite.Learning.Infrastructure.csproj"
        StartupProject = "src/Services/Learning/NomiWrite.Learning.API/NomiWrite.Learning.API.csproj"
    },
    @{
        Name = "Notification"
        Context = "NotificationDbContext"
        Connection = "ConnectionStrings__NotificationDb"
        Project = "src/Services/Notification/NomiWrite.Notification.Infrastructure/NomiWrite.Notification.Infrastructure.csproj"
        StartupProject = "src/Services/Notification/NomiWrite.Notification.API/NomiWrite.Notification.API.csproj"
    },
    @{
        Name = "Admin"
        Context = "AdminDbContext"
        Connection = "ConnectionStrings__AdminDb"
        Project = "src/Services/Admin/NomiWrite.Admin.Infrastructure/NomiWrite.Admin.Infrastructure.csproj"
        StartupProject = "src/Services/Admin/NomiWrite.Admin.API/NomiWrite.Admin.API.csproj"
    }
)

Import-EnvFile -Path (Join-Path $backendRoot $EnvironmentFile)
[Environment]::SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "Development", "Process")

if ($Service -and $Service.Count -gt 0) {
    $requested = $Service | ForEach-Object { $_.ToLowerInvariant() }
    $migrations = $migrations | Where-Object { $requested -contains $_.Name.ToLowerInvariant() }

    if ($migrations.Count -eq 0) {
        throw "No matching service names. Valid names: Auth, User, Writing, Grading, Payment, Subscription, Learning, Notification, Admin."
    }
}

foreach ($migration in $migrations) {
    $connectionValue = [Environment]::GetEnvironmentVariable($migration.Connection, "Process")
    if ([string]::IsNullOrWhiteSpace($connectionValue)) {
        if ($DryRun) {
            Write-Warning "Missing $($migration.Connection). A real migration run will require it in $EnvironmentFile or the process environment."
        } else {
            throw "Missing $($migration.Connection). Add it to $EnvironmentFile or set it in the process environment."
        }
    }

    $arguments = @(
        "ef", "database", "update",
        "--project", $migration.Project,
        "--startup-project", $migration.StartupProject,
        "--context", $migration.Context
    )

    Write-Host ""
    Write-Host "[$($migration.Name)] dotnet $($arguments -join ' ')"

    if (-not $DryRun) {
        & dotnet @arguments
        if ($LASTEXITCODE -ne 0) {
            throw "Migration failed for $($migration.Name)."
        }
    }
}

Write-Host ""
if ($DryRun) {
    Write-Host "Dry run complete. No database changes were applied."
} else {
    Write-Host "All selected database migrations completed."
}
