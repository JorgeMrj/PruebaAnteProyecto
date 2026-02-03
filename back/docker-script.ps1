param (
    [string]$Name
)

$ImageName = $null

#  Intentar como CONTENEDOR
if ($Name) {
    $ImageName = docker inspect $Name --format='{{.Config.Image}}' 2>$null
}

#  Si no era contenedor, intentar como IMAGEN
if (-not $ImageName -and $Name) {
    $exists = docker images -q $Name 2>$null
    if ($exists) {
        $ImageName = $Name
    }
}

Write-Host "Ejecutando docker compose down -v..."
docker compose down -v

# Eliminar imagen solo si se resolvió
if ($ImageName) {
    Write-Host "Eliminando imagen $ImageName..."
    docker rmi -f $ImageName
} else {
    Write-Host "No se encontró contenedor ni imagen válida. Saltando eliminación de imagen."
}

Write-Host "Ejecutando docker compose up -d..."
docker compose up -d

Write-Host "Listo."
