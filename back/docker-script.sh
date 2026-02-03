#!/bin/bash

CONTAINER_NAME="$1"

# Intentar obtener la imagen si se pasó un nombre
if [ -n "$CONTAINER_NAME" ]; then
  IMAGE_NAME=$(docker inspect "$CONTAINER_NAME" --format='{{.Config.Image}}' 2>/dev/null)
fi

echo "Ejecutando docker compose down -v..."
docker compose down -v

# Solo borra la imagen si el contenedor existe y tiene imagen
if [ -n "$IMAGE_NAME" ]; then
  echo "Eliminando imagen $IMAGE_NAME..."
  docker rmi -f "$IMAGE_NAME"
else
  if [ -n "$CONTAINER_NAME" ]; then
    echo "Contenedor '$CONTAINER_NAME' no encontrado. Saltando eliminación de imagen."
  else
    echo "No se pasó nombre de contenedor. Saltando eliminación de imagen."
  fi
fi

echo " Ejecutando docker compose up -d..."
docker compose up -d

echo " Listo."
