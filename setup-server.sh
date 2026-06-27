#!/usr/bin/env bash
# setup-server.sh — Installation complète de GestionHopital sur un serveur Debian/Ubuntu
# Exécuter en tant que root : bash setup-server.sh

set -e

REPO="https://github.com/medoune20/LP2M_Gestion_Hopital.git"
APP_DIR="/opt/gestionhopital"
DOMAIN="lp2medoune.com"
APP_PORT="5050"

echo "======================================================"
echo "  Déploiement GestionHopital → https://$DOMAIN"
echo "======================================================"

# ── 1. Docker ────────────────────────────────────────────
if ! command -v docker &>/dev/null; then
    echo "[1/5] Installation de Docker..."
    apt-get update -qq
    apt-get install -y ca-certificates curl gnupg lsb-release
    install -m 0755 -d /etc/apt/keyrings
    curl -fsSL https://download.docker.com/linux/ubuntu/gpg \
      | gpg --dearmor -o /etc/apt/keyrings/docker.gpg
    echo \
      "deb [arch=$(dpkg --print-architecture) signed-by=/etc/apt/keyrings/docker.gpg] \
      https://download.docker.com/linux/ubuntu $(lsb_release -cs) stable" \
      > /etc/apt/sources.list.d/docker.list
    apt-get update -qq
    apt-get install -y docker-ce docker-ce-cli containerd.io docker-compose-plugin
    systemctl enable --now docker
    echo "  Docker installé."
else
    echo "[1/5] Docker déjà installé — OK"
fi

# ── 2. Clone / mise à jour du dépôt ─────────────────────
echo "[2/5] Récupération du code source..."
if [ -d "$APP_DIR/.git" ]; then
    git -C "$APP_DIR" pull origin main
else
    git clone "$REPO" "$APP_DIR"
fi

# ── 3. Lancement Docker Compose ──────────────────────────
echo "[3/5] Build et démarrage de l'application..."
cd "$APP_DIR"
docker compose down --remove-orphans 2>/dev/null || true
docker compose build --no-cache
docker compose up -d

echo "  Attente du démarrage (15 s)..."
sleep 15
docker compose ps

# Vérification santé
if curl -sf "http://localhost:$APP_PORT/health" &>/dev/null; then
    echo "  Application opérationnelle sur le port $APP_PORT ✓"
else
    echo "  ATTENTION : health check non disponible (normal si /health n'est pas exposé)"
fi

# ── 4. Nginx ─────────────────────────────────────────────
echo "[4/5] Configuration nginx..."
if ! command -v nginx &>/dev/null; then
    apt-get install -y nginx
fi

# Certbot / Let's Encrypt
if ! command -v certbot &>/dev/null; then
    apt-get install -y certbot python3-certbot-nginx
fi

CERT_DIR="/etc/letsencrypt/live/$DOMAIN"
if [ ! -d "$CERT_DIR" ]; then
    echo "  Obtention du certificat SSL pour $DOMAIN..."
    certbot certonly --nginx --non-interactive --agree-tos \
      --email admin@$DOMAIN \
      -d "$DOMAIN" -d "www.$DOMAIN" || \
    certbot certonly --standalone --non-interactive --agree-tos \
      --email admin@$DOMAIN \
      -d "$DOMAIN" -d "www.$DOMAIN"
fi

# Copie de la config nginx
cp "$APP_DIR/nginx/gestionhopital.conf" /etc/nginx/sites-available/gestionhopital
ln -sf /etc/nginx/sites-available/gestionhopital /etc/nginx/sites-enabled/gestionhopital
rm -f /etc/nginx/sites-enabled/default 2>/dev/null || true

nginx -t && systemctl reload nginx
echo "  Nginx configuré pour $DOMAIN ✓"

# ── 5. Renouvellement automatique SSL ────────────────────
echo "[5/5] Planification du renouvellement SSL..."
(crontab -l 2>/dev/null | grep -q "certbot renew") || \
  (crontab -l 2>/dev/null; echo "0 3 * * 0 certbot renew --quiet && systemctl reload nginx") | crontab -

echo ""
echo "======================================================"
echo "  Déploiement terminé !"
echo "  → https://$DOMAIN"
echo "======================================================"
