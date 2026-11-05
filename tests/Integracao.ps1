# Requer PowerShell 7 e a compilação Release do projeto.
param([int]$Porta = 5189)

$ErrorActionPreference = 'Stop'
$raiz = Split-Path -Parent $PSScriptRoot
$dll = Join-Path $raiz 'bin/Release/net10.0/SistemaAvaliacao.dll'
$temporario = New-Item -ItemType Directory -Path (Join-Path ([IO.Path]::GetTempPath()) "avaliacoes-$([guid]::NewGuid())")
$url = "http://127.0.0.1:$Porta"
$api = "$url/api/avaliacoes"

function Confirmar($Condicao, $Descricao) {
    if (!$Condicao) { throw "Falhou: $Descricao" }
    Write-Host "OK - $Descricao"
}

function Iniciar {
    $opcoes = @{
        FilePath = 'dotnet'
        ArgumentList = @('exec', ('"' + $dll + '"'))
        WorkingDirectory = $raiz
        Environment = @{
            ASPNETCORE_URLS = $url
            ASPNETCORE_ENVIRONMENT = 'Development'
            Avaliacoes__Arquivo = Join-Path $temporario.FullName 'avaliacoes.json'
        }
        RedirectStandardOutput = Join-Path $temporario.FullName 'stdout.log'
        RedirectStandardError = Join-Path $temporario.FullName 'stderr.log'
        PassThru = $true
    }
    if ($IsWindows) { $opcoes.WindowStyle = 'Hidden' }
    Start-Process @opcoes
}

function AguardarApi {
    for ($tentativa = 0; $tentativa -lt 50 -and !$servidor.HasExited; $tentativa++) {
        try {
            Invoke-WebRequest $api -NoProxy -TimeoutSec 2 | Out-Null
            return
        } catch {
            Start-Sleep -Milliseconds 200
        }
    }
    Get-Content (Join-Path $temporario.FullName '*.log') | Write-Host
    throw "A API não iniciou. $($Error[0])"
}

function Requisitar($Metodo, $Caminho, $Corpo = $null) {
    $opcoes = @{ Method = $Metodo; Uri = "$api$Caminho"; SkipHttpErrorCheck = $true; NoProxy = $true }
    if ($null -ne $Corpo) {
        $opcoes.ContentType = 'application/json; charset=utf-8'
        $opcoes.Body = $Corpo | ConvertTo-Json -Compress
    }
    Invoke-WebRequest @opcoes
}

try {
    if (!(Test-Path -LiteralPath $dll)) { throw 'Execute dotnet build -c Release primeiro.' }
    $servidor = Iniciar
    AguardarApi
    Confirmar ((Invoke-WebRequest -NoProxy $url).Content.Contains('Painel de avaliações')) 'Página inicial servida'
    Confirmar ((Invoke-WebRequest -NoProxy "$url/avaliacoes.js").StatusCode -eq 200) 'Script da interface servido'
    Confirmar ((Invoke-WebRequest -NoProxy "$url/swagger/v1/swagger.json").StatusCode -eq 200) 'OpenAPI disponível'
    $lista = (Invoke-RestMethod -NoProxy $api)
    Confirmar ($lista.Count -eq 3 -and $lista[0].nota -eq 5 -and $lista[1].nota -eq 5.5 -and $lista[2].nota -eq 9) 'Fórmulas dos três tipos'

    $prova = @{ titulo = '  Prova de integração  '; tipo = 'Prova'; acertos = 7; total = 12 }
    $criada = Requisitar Post '' $prova
    $resultado = $criada.Content | ConvertFrom-Json
    $id = $resultado.id
    Confirmar ($criada.StatusCode -eq 201 -and $resultado.nota -eq 5.83 -and $resultado.titulo -eq 'Prova de integração') 'Cadastro e arredondamento'
    Confirmar (($criada.Headers.Location[0]) -eq "$api/$id") 'Location aponta para a avaliação criada'

    $resultado = (Requisitar Put "/$id/recuperacao" @{ nota = 9 }).Content | ConvertFrom-Json
    Confirmar ($resultado.nota -eq 9) 'Recuperação pode elevar a nota'
    $resultado = (Requisitar Put '/1/recuperacao' @{ nota = 2 }).Content | ConvertFrom-Json
    Confirmar ($resultado.nota -eq 5) 'Recuperação abaixo da nota original'
    $prova.acertos = 8
    $resultado = (Requisitar Put "/$id" $prova).Content | ConvertFrom-Json
    Confirmar ($resultado.acertos -eq 8 -and $resultado.notaRecuperacao -eq 9) 'Edição de prova preserva recuperação'

    $prova.total = 0
    Confirmar ((Requisitar Post '' $prova).StatusCode -eq 400) 'Total igual a zero é rejeitado'
    $prova.total = 7
    Confirmar ((Requisitar Post '' $prova).StatusCode -eq 400) 'Acertos acima do total são rejeitados'
    Confirmar ((Requisitar Post '' @{ titulo = 'Trabalho'; tipo = 'Trabalho'; conteudo = 7 }).StatusCode -eq 400) 'Campos do trabalho são obrigatórios'
    Confirmar ((Requisitar Post '' @{ titulo = 'Frequência'; tipo = 'Participacao'; presencas = 4; aulas = 3 }).StatusCode -eq 400) 'Presenças acima do total são rejeitadas'
    Confirmar ((Requisitar Put '/2/recuperacao' @{ nota = 9 }).StatusCode -eq 409) 'Recuperação é exclusiva de provas'
    Confirmar ((Requisitar Put "/$id/recuperacao" @{ nota = 11 }).StatusCode -eq 400) 'Recuperação fora da escala é rejeitada'
    Confirmar ((Requisitar Delete '/3').StatusCode -eq 204) 'Exclusão retorna 204'
    Confirmar ((Requisitar Get '/3').StatusCode -eq 404) 'Consulta de item excluído retorna 404'

    Stop-Process -InputObject $servidor
    $servidor.WaitForExit()
    $servidor = Iniciar
    AguardarApi
    $resultado = Invoke-RestMethod -NoProxy "$api/$id"
    Confirmar ($resultado.acertos -eq 8 -and $resultado.nota -eq 9) 'Cadastro, edição e recuperação sobrevivem ao reinício'
    Confirmar ((Requisitar Get '/3').StatusCode -eq 404) 'Exclusão sobrevive ao reinício'
} finally {
    if ($servidor -and !$servidor.HasExited) {
        Stop-Process -InputObject $servidor
        $servidor.WaitForExit()
    }
    foreach ($arquivo in 'avaliacoes.json', 'avaliacoes.json.tmp', 'stdout.log', 'stderr.log') {
        $caminho = Join-Path $temporario.FullName $arquivo
        if (Test-Path -LiteralPath $caminho) { Remove-Item -LiteralPath $caminho }
    }
    Remove-Item -LiteralPath $temporario.FullName
}
