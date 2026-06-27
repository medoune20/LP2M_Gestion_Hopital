param(
    [string]$BaseUrl = "http://localhost:5000/hopital"
)

$ErrorActionPreference = "Stop"

function Test-Get($Path) {
    $url = "$BaseUrl$Path"
    $res = Invoke-WebRequest -Uri $url -UseBasicParsing -MaximumRedirection 0 -SkipHttpErrorCheck
    Write-Host "$($res.StatusCode) GET $url"
    if ($res.StatusCode -lt 200 -or $res.StatusCode -ge 400) { throw "GET failed: $url" }
}

Test-Get "/Auth/Connexion"
Test-Get "/health"

$session = New-Object Microsoft.PowerShell.Commands.WebRequestSession
$login = Invoke-WebRequest -Uri "$BaseUrl/Auth/Connexion" -UseBasicParsing -WebSession $session
$form = @{ login = "admin"; motDePasse = "admin" }
$post = Invoke-WebRequest -Uri "$BaseUrl/Auth/Connexion" -Method Post -Body $form -UseBasicParsing -WebSession $session -MaximumRedirection 0 -SkipHttpErrorCheck
Write-Host "$($post.StatusCode) POST $BaseUrl/Auth/Connexion"
if ($post.StatusCode -ne 302) { throw "Login did not redirect" }

$pages = @(
    "/Accueil/Index",
    "/Patient",
    "/RendezVous",
    "/Consultation",
    "/Examen",
    "/Hospitalisation",
    "/Departement",
    "/FileAttente",
    "/Paiement",
    "/Comptabilite",
    "/Alerte",
    "/Mail/Journal",
    "/Utilisateur",
    "/Etablissement",
    "/Lp2m"
)

foreach ($p in $pages) {
    $url = "$BaseUrl$p"
    $res = Invoke-WebRequest -Uri $url -UseBasicParsing -WebSession $session -SkipHttpErrorCheck
    Write-Host "$($res.StatusCode) GET $url"
    if ($res.StatusCode -lt 200 -or $res.StatusCode -ge 400) { throw "Module failed: $url" }
}

Write-Host "Smoke test OK" -ForegroundColor Green
