#!/usr/bin/env bash
# Script de déploiement GestionHopital sur serveur distant
# Usage: ./deploy.sh <user@serveur>

set -e

REMOTE="${1:-user@votre-serveur.com}"
APP_DIR="/opt/gestionhopital"

echo "==> Déploiement sur $REMOTE"

# Créer le répertoire sur le serveur
ssh "$REMOTE" "mkdir -p $APP_DIR"

# Copier les fichiers nécessaires
rsync -az --exclude='.git' --exclude='bin' --exclude='obj' --exclude='*.db' \
  ./ "$REMOTE:$APP_DIR/"

# Sur le serveur : build et démarrage
ssh "$REMOTE" bash << EOF
  cd $APP_DIR
  docker compose down --remove-orphans 2>/dev/null || true
  docker compose build --no-cache
  docker compose up -d
  echo "==> Statut des conteneurs :"
  docker compose ps
  echo ""
  echo "==> Application disponible sur http://\$(hostname -I | awk '{print \$1}'):5050"
EOF
