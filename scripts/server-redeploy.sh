# Chay tren server (root). Goi boi scripts/deploy.ps1 — khong chay tay tren Windows.
set -e
APP=/var/www/PhanMemHoc
BK=/var/backups/phanmemhoc
TS=$(date +%Y%m%d-%H%M%S)

systemctl stop phanmemhoc
mkdir -p $BK
for f in $APP/phanmemhoc*.db; do [ -f "$f" ] && cp "$f" $BK/$(basename "$f").$TS; done
for p in phanmemhoc.db phanmemhoc-log.db; do
  ls -t $BK/$p.* 2>/dev/null | tail -n +4 | xargs -r rm -f   # giu 3 ban gan nhat
done

tar -xzf /tmp/pmh.tar.gz -C $APP --exclude='*.db' --warning=no-timestamp
rm -f /tmp/pmh.tar.gz /tmp/pmh-redeploy.sh
chmod +x $APP/PhanMemHoc
chown -R www-data:www-data $APP
systemctl start phanmemhoc

for i in 1 2 3 4 5 6 7 8 9 10; do
  code=$(curl -s -o /dev/null -w "%{http_code}" http://127.0.0.1:8090/ || true)
  [ "$code" = "200" ] && break
  sleep 1
done
echo "service: $(systemctl is-active phanmemhoc) | http 8090: $code | backup: $BK/*.$TS"
if [ "$code" != "200" ]; then
  journalctl -u phanmemhoc --no-pager -n 30
  exit 1
fi
df -h / | tail -n 1
