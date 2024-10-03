param (
    [string]$Command,
    [string]$VerificationCode,
    [string]$PromptType
)

function Set-TelegramVerificationCode {
    if (-not $VerificationCode) {
        Write-Host "Please provide a verification code."
        exit
    }
    docker-compose exec app bash -c "echo '$VerificationCode' > /app/data/telegram_verification_code.txt"
    Write-Host "Telegram verification code set successfully."
}

function Update-PromptType {
    if (-not $PromptType) {
        Write-Host "Please provide a prompt type."
        exit
    }
    docker-compose exec app bash -c "echo '$PromptType' > /app/data/summarizer_prompt_type.txt"
    Write-Host "Prompt type updated successfully."
}

switch ($Command) {
    "set-verification-code" {
        Set-TelegramVerificationCode
    }
    "update-prompt" {
        Update-PromptType
    }
    default {
        Write-Host "Usage: Utils.ps1 -Command <set-verification-code|update-prompt> -VerificationCode <code> -PromptType <prompt>"
    }
}
