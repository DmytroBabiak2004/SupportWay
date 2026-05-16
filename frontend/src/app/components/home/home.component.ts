import {
  Component,
  ElementRef,
  ViewChild,
  AfterViewInit,
  OnDestroy,
  OnInit,
  HostListener,
  ChangeDetectorRef,
  ChangeDetectionStrategy,
  inject,
} from '@angular/core';
import { Router } from '@angular/router';
import { CommonModule } from '@angular/common';
import { Subscription } from 'rxjs';
import { catchError, of } from 'rxjs';

import { ProfileService } from '../../services/profile.service';
import { Profile } from '../../models/profile.model';

declare const L: any;

// ─── Geo helpers ──────────────────────────────────────────────────────────────

type LLPoly = [number, number][];  // [lat, lng][]

function pointInPolygon(lat: number, lng: number, poly: LLPoly): boolean {
  let inside = false;
  for (let i = 0, j = poly.length - 1; i < poly.length; j = i++) {
    const [yi, xi] = poly[i];
    const [yj, xj] = poly[j];
    if ((yi > lat) !== (yj > lat) && lng < ((xj - xi) * (lat - yi)) / (yj - yi) + xi) {
      inside = !inside;
    }
  }
  return inside;
}

// Спрощений полігон кордону Росії (lat, lng). Достатньо точний для завдання.
const RUSSIA_POLYGON: LLPoly = [
  [68.9, 33.1], [69.1, 36.5], [68.5, 40.0], [67.7, 44.5], [67.5, 49.0],
  [68.0, 54.0], [68.5, 58.0], [68.0, 63.0], [67.5, 68.0], [67.2, 72.5],
  [67.5, 77.5], [68.0, 82.0], [68.5, 86.5], [68.0, 91.0], [67.5, 96.5],
  [67.8, 102.0],[68.2, 107.0],[68.0, 112.0],[67.5, 117.0],[67.0, 122.0],
  [67.5, 127.0],[68.0, 132.5],[68.5, 137.0],[68.0, 141.5],[66.5, 144.0],
  [64.0, 141.5],[63.0, 143.0],[60.5, 143.0],[59.0, 141.0],[57.5, 141.5],
  [55.0, 137.0],[54.0, 136.0],[52.0, 141.2],[48.5, 140.5],[46.0, 137.5],
  [43.5, 133.5],[42.5, 131.0],[43.5, 129.0],[44.5, 131.5],[45.0, 133.0],
  [44.0, 134.0],[42.8, 132.5],[42.0, 130.5],[41.5, 129.5],[42.0, 128.0],
  [41.5, 126.5],[39.5, 124.0],[38.5, 122.0],[39.0, 119.5],[40.0, 118.5],
  [48.5, 117.0],[49.5, 116.5],[49.8, 117.5],[49.5, 119.0],[50.0, 119.5],
  [50.5, 120.5],[51.0, 119.5],[51.8, 119.0],[52.3, 120.0],[52.0, 121.0],
  [51.5, 122.5],[50.0, 124.0],[49.0, 126.5],[48.5, 130.0],[47.5, 130.5],
  [47.0, 132.0],[47.8, 134.5],[48.5, 135.0],[48.0, 136.5],[47.0, 138.0],
  [46.5, 141.0],[46.0, 142.5],[45.5, 143.5],[44.5, 143.0],[43.5, 141.5],
  [42.5, 139.5],[41.5, 138.0],[40.5, 136.5],[39.5, 133.0],[38.5, 130.0],
  [38.0, 126.5],[37.5, 122.0],[37.0, 117.5],[38.0, 113.0],[37.5, 109.5],
  [37.0, 105.5],[36.5, 102.0],[37.0, 97.5], [37.5, 92.0], [38.0, 87.5],
  [37.5, 82.5], [37.0, 78.0], [37.5, 73.5], [38.5, 69.5], [38.0, 65.5],
  [37.5, 61.5], [37.0, 57.5], [37.5, 54.0], [38.5, 50.0], [39.5, 46.5],
  [41.5, 43.0], [42.5, 40.5], [43.5, 40.0], [44.5, 41.5], [45.0, 43.5],
  [46.0, 44.5], [47.5, 44.0], [48.5, 44.5], [49.5, 44.0], [50.5, 45.0],
  [51.5, 46.5], [52.0, 48.5], [53.0, 49.5], [54.0, 50.5], [55.0, 51.0],
  [55.5, 52.5], [55.0, 54.5], [54.5, 56.0], [54.0, 58.5], [54.5, 60.0],
  [55.5, 61.5], [55.0, 63.5], [54.0, 65.0], [54.5, 68.0], [55.5, 69.5],
  [55.0, 73.0], [54.0, 76.5], [54.5, 80.0], [54.0, 83.5], [54.5, 86.0],
  [54.0, 91.0], [53.5, 95.5], [52.5, 99.5], [52.0, 104.0],[51.5, 108.5],
  [50.0, 113.5],[49.0, 117.0],[48.0, 118.5],[47.5, 116.5],[47.0, 113.0],
  [46.5, 109.5],[47.0, 106.0],[47.5, 102.5],[47.0, 99.0], [46.5, 95.5],
  [46.0, 92.0], [47.0, 88.5], [47.5, 85.5], [48.0, 83.0], [48.5, 80.5],
  [49.0, 78.0], [49.5, 75.5], [50.0, 73.0], [51.0, 70.5], [51.5, 68.5],
  [52.0, 65.5], [53.0, 62.5], [53.5, 59.5], [54.0, 57.0], [55.0, 54.0],
  [55.5, 51.5], [55.0, 49.5], [54.0, 47.5], [52.5, 46.0], [51.5, 46.5],
  [51.0, 48.0], [50.5, 50.5], [50.0, 51.5], [49.5, 52.0], [48.5, 51.0],
  [47.5, 50.0], [47.0, 48.5], [46.5, 47.0], [45.5, 46.5], [44.5, 47.0],
  [43.5, 47.5], [43.0, 46.0], [42.5, 44.5], [42.0, 43.5], [41.5, 42.0],
  [42.0, 40.5], [42.5, 39.5], [43.0, 40.0], [43.5, 40.5], [44.0, 41.0],
  [45.0, 41.5], [46.0, 42.0], [47.0, 42.5], [48.5, 43.5], [49.5, 44.0],
  [50.5, 43.5], [51.0, 42.5], [51.5, 42.0], [52.0, 41.5], [52.5, 41.0],
  [53.0, 40.5], [53.5, 40.0], [54.0, 39.5], [54.5, 38.5], [55.0, 37.5],
  [55.5, 36.5], [56.0, 35.5], [56.5, 34.5], [57.0, 33.0], [57.5, 31.5],
  [58.0, 30.5], [59.0, 29.5], [59.5, 28.0], [60.0, 28.5], [60.5, 29.5],
  [61.0, 29.0], [61.5, 28.5], [62.0, 29.5], [62.5, 30.5], [63.0, 31.5],
  [63.5, 31.0], [64.0, 30.0], [64.5, 30.5], [65.0, 31.0], [65.5, 31.5],
  [66.0, 30.5], [66.5, 30.0], [67.0, 31.5], [67.5, 32.0], [68.9, 33.1],
];

// Bounding boxes суші [s, n, w, e]. Океан = точка не потрапляє в жоден box.
// Перекриваємо всі материки та великі острови достатньо грубо.
interface LandBox { s: number; n: number; w: number; e: number; }
const LAND_BOXES: LandBox[] = [
  // Євразія
  { s: 35, n: 71,  w: -10, e: 40  }, // Зах. Європа
  { s: 35, n: 71,  w:  40, e: 180 }, // Схід. Євразія (включає РФ — але РФ блокується окремо)
  { s: 5,  n: 35,  w:  25, e: 60  }, // Близький Схід
  { s: 5,  n: 35,  w:  60, e: 100 }, // Індія / Пакистан
  { s: 5,  n: 55,  w: 100, e: 145 }, // Сх. Азія / Японія
  { s: -10,n: 25,  w:  95, e: 141 }, // Пд.-Сх. Азія
  // Африка
  { s: -35,n: 37,  w: -18, e: 52  },
  // Пн. Америка
  { s: 15, n: 75,  w: -170,e: -52 },
  // Пд. Америка
  { s: -56,n: 15,  w: -82, e: -34 },
  // Австралія та Океанія
  { s: -47,n: -10, w: 113, e: 154 },
  { s: -47,n: -10, w: 154, e: 179 }, // Зах. Австралія / PNG
  // Гренландія / Ісландія
  { s: 63, n: 84,  w: -74, e: -10 },
  { s: 62, n: 67,  w: -25, e: -12 },
  // Великобританія / Ірландія
  { s: 49, n: 61,  w: -11, e: 2   },
  // Нова Зеландія
  { s: -47,n: -34, w: 166, e: 178 },
  // Японія
  { s: 30, n: 46,  w: 129, e: 146 },
  // Шрі-Ланка
  { s: 5,  n: 10,  w: 79,  e: 82  },
  // Куба / Карибські о-ви
  { s: 14, n: 24,  w: -85, e: -59 },
  // Мадагаскар
  { s: -26,n: -12, w: 43,  e: 51  },
];

// ─── Pin ──────────────────────────────────────────────────────────────────────

const PIN_COLORS = ['#2563eb', '#059669', '#d97706', '#7c3aed', '#db2777'] as const;
type PinColor = (typeof PIN_COLORS)[number];

interface Pin {
  lat: number; lng: number;
  sx: number;  sy: number;
  size: number; progress: number; pulseR: number;
  color: PinColor;
  life: number; lifeSpeed: number;
  pulseOffset: number; pulseSpeed: number;
  alive: boolean;
}

interface DataLine {
  pinA: Pin; pinB: Pin;
  cpLat: number; cpLng: number;
  t: number; tSpeed: number;
  alive: boolean;
}

// ─── Component ────────────────────────────────────────────────────────────────

@Component({
  selector: 'app-home',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './home.component.html',
  styleUrls: ['./home.component.scss'],
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class HomeComponent implements OnInit, AfterViewInit, OnDestroy {
  @ViewChild('animCanvas', { static: true })
  animCanvas!: ElementRef<HTMLCanvasElement>;

  showContent = false;
  showButtons = false;
  isLoading   = true;

  userProfile: Profile | null = null;

  get displayName(): string {
    if (this.userProfile?.fullName) return this.userProfile.fullName;
    if (this.userProfile?.name)     return this.userProfile.name;
    return this.userProfile?.username ?? '';
  }

  get greeting(): string {
    const h = new Date().getHours();
    if (h < 5)  return 'Добрий вечір';
    if (h < 12) return 'Доброго ранку';
    if (h < 17) return 'Добрий день';
    return 'Добрий вечір';
  }

  private map!: any;
  private readonly PAN_PX_PER_FRAME = 0.55;

  private ctx!: CanvasRenderingContext2D;
  private raf = 0;
  private W = 0;
  private H = 0;
  private dpr = 1;
  private frame = 0;

  private readonly MAX_PINS    = 34;
  private readonly MAX_LINES   = 14;
  private readonly SPAWN_EVERY = 22;

  private pins:  Pin[]      = [];
  private lines: DataLine[] = [];

  private readonly INIT_LAT  = 49.0;
  private readonly INIT_LNG  = 31.5;
  private readonly INIT_ZOOM = 6;

  private userInteracting   = false;
  private interactionTimer: any = null;

  private sub = new Subscription();

  private readonly cdr            = inject(ChangeDetectorRef);
  private readonly router         = inject(Router);
  private readonly profileService = inject(ProfileService);

  ngOnInit(): void {
    setTimeout(() => { this.showContent = true; this.cdr.markForCheck(); }, 120);
    setTimeout(() => { this.showButtons = true; this.cdr.markForCheck(); }, 500);

    this.sub.add(
      this.profileService.getProfile().pipe(catchError(() => of(null))).subscribe(p => {
        this.userProfile = p;
        this.isLoading   = false;
        this.cdr.markForCheck();
      })
    );
  }

  ngAfterViewInit(): void {
    this.initMap();
    this.initCanvas();

    setTimeout(() => {
      for (let i = 0; i < 22; i++) this.spawnPin(true);
      this.loop();
    }, 300);
  }

  ngOnDestroy(): void {
    cancelAnimationFrame(this.raf);
    this.sub.unsubscribe();
    this.pins  = [];
    this.lines = [];
    if (this.map) this.map.remove();
    if (this.interactionTimer) clearTimeout(this.interactionTimer);
  }

  @HostListener('window:resize')
  onResize(): void {
    this.resizeCanvas();
    if (this.map) this.map.invalidateSize();
  }

  // ── Map ────────────────────────────────────────────────────────────────────

  private initMap(): void {
    this.map = L.map('home-map', {
      center: [this.INIT_LAT, this.INIT_LNG],
      zoom: this.INIT_ZOOM,
      zoomControl: false,
      attributionControl: true,
      dragging: true,
      scrollWheelZoom: true,
    });

    L.tileLayer(
      'https://{s}.basemaps.cartocdn.com/rastertiles/voyager/{z}/{x}/{y}{r}.png',
      {
        attribution:
          '&copy; <a href="https://www.openstreetmap.org/copyright">OSM</a>' +
          ' &copy; <a href="https://carto.com/">CARTO</a>',
        subdomains: 'abcd',
        maxZoom: 20,
      }
    ).addTo(this.map);

    const pauseAutoPan = () => {
      this.userInteracting = true;
      if (this.interactionTimer) clearTimeout(this.interactionTimer);
      this.interactionTimer = setTimeout(() => { this.userInteracting = false; }, 1500);
    };

    this.map.on('dragstart', pauseAutoPan);
    this.map.on('zoomstart', pauseAutoPan);
  }

  // ── Canvas ─────────────────────────────────────────────────────────────────

  private initCanvas(): void {
    this.ctx = this.animCanvas.nativeElement.getContext('2d')!;
    this.resizeCanvas();
  }

  private resizeCanvas(): void {
    const el        = this.animCanvas.nativeElement;
    const container = el.parentElement!;
    this.W   = container.clientWidth;
    this.H   = container.clientHeight;
    this.dpr = window.devicePixelRatio || 1;
    el.width  = this.W * this.dpr;
    el.height = this.H * this.dpr;
    this.ctx.setTransform(1, 0, 0, 1, 0, 0);
    this.ctx.scale(this.dpr, this.dpr);
  }

  private screenToLatLng(sx: number, sy: number): { lat: number; lng: number } {
    const ll = this.map.containerPointToLatLng([sx, sy]);
    return { lat: ll.lat, lng: ll.lng };
  }

  private latLngToScreen(lat: number, lng: number): { x: number; y: number } {
    const pt = this.map.latLngToContainerPoint([lat, lng]);
    return { x: pt.x, y: pt.y };
  }

  // ── Geo filters ────────────────────────────────────────────────────────────

  private isContentZone(sx: number, sy: number): boolean {
    const cx = this.W * 0.5;
    const cy = this.H * 0.5;
    return sx > cx - this.W * 0.22 && sx < cx + this.W * 0.22
      && sy > cy - this.H * 0.30 && sy < cy + this.H * 0.30;
  }

  private isBanned(sx: number, sy: number, lat: number, lng: number): boolean {
    if (this.isContentZone(sx, sy))          return true;
    if (pointInPolygon(lat, lng, RUSSIA_POLYGON)) return true;
    if (!LAND_BOXES.some(b => lat >= b.s && lat <= b.n && lng >= b.w && lng <= b.e)) return true;
    return false;
  }

  // ── Pin management ─────────────────────────────────────────────────────────

  private spawnPin(randomLife = false): void {
    if (this.pins.length >= this.MAX_PINS) return;

    let sx!: number, sy!: number, lat!: number, lng!: number;
    let attempts = 0;
    do {
      sx = Math.random() * this.W;
      sy = Math.random() * this.H * 0.86 + this.H * 0.07;
      const ll = this.screenToLatLng(sx, sy);
      lat = ll.lat; lng = ll.lng;
      attempts++;
    } while (this.isBanned(sx, sy, lat, lng) && attempts < 40);

    if (attempts >= 40) return;

    const color = PIN_COLORS[Math.floor(Math.random() * PIN_COLORS.length)];
    const pin: Pin = {
      lat, lng, sx, sy,
      size:        Math.random() * 20 + 28,
      progress:    Math.random() * 100,
      pulseR:      Math.random() * 30 + 18,
      color,
      life:        randomLife ? Math.random() * 0.78 : 0,
      lifeSpeed:   Math.random() * 0.0026 + 0.0013,
      pulseOffset: Math.random() * Math.PI * 2,
      pulseSpeed:  Math.random() * 0.028 + 0.020,
      alive: true,
    };
    this.pins.push(pin);
    this.tryLine(pin);
  }

  private tryLine(np: Pin): void {
    if (this.lines.length >= this.MAX_LINES || Math.random() > 0.42) return;
    const pool = this.pins.filter(
      p => p !== np && p.alive && p.life > 0.1 && p.life < 0.82
    );
    if (!pool.length) return;

    pool.sort((a, b) =>
      Math.hypot(a.sx - np.sx, a.sy - np.sy) -
      Math.hypot(b.sx - np.sx, b.sy - np.sy)
    );
    const pt   = pool[0];
    const dist = Math.hypot(pt.sx - np.sx, pt.sy - np.sy);
    if (dist > 320) return;

    const msx  = (np.sx + pt.sx) / 2;
    const msy  = (np.sy + pt.sy) / 2;
    const bow  = dist * 0.28;
    const cpSx = msx + (Math.random() - 0.5) * bow;
    const cpSy = msy - bow * 0.5 + (Math.random() - 0.5) * bow;
    const cp   = this.screenToLatLng(cpSx, cpSy);

    this.lines.push({
      pinA: np, pinB: pt,
      cpLat: cp.lat, cpLng: cp.lng,
      t: 0,
      tSpeed: Math.random() * 0.009 + 0.005,
      alive: true,
    });
  }

  // ── Animation loop ─────────────────────────────────────────────────────────

  private loop = (): void => {
    this.frame++;

    if (!this.userInteracting) {
      this.map.panBy([this.PAN_PX_PER_FRAME, 0], { animate: false, noMoveStart: true });
    }

    for (const p of this.pins) {
      const sc = this.latLngToScreen(p.lat, p.lng);
      p.sx = sc.x;
      p.sy = sc.y;
    }

    this.ctx.clearRect(0, 0, this.W, this.H);
    this.drawLines();
    this.drawPins();

    if (this.frame % this.SPAWN_EVERY === 0) this.spawnPin();

    this.raf = requestAnimationFrame(this.loop);
  };

  // ── Draw: Pins ─────────────────────────────────────────────────────────────

  private drawPins(): void {
    for (let i = this.pins.length - 1; i >= 0; i--) {
      const p = this.pins[i];
      p.life += p.lifeSpeed;

      if (p.life >= 1) {
        p.alive = false;
        this.pins.splice(i, 1);
        continue;
      }

      const x = p.sx, y = p.sy;
      if (x < -80 || x > this.W + 80 || y < -80 || y > this.H + 80) continue;

      const a = p.life < 0.12
        ? p.life / 0.12
        : p.life < 0.80 ? 1
          : 1 - (p.life - 0.80) / 0.20;
      const alpha = Math.max(0, Math.min(1, a));

      const pinW  = p.size;
      const pinH  = p.size * (50 / 36);
      const scale = p.size / 36;

      const ox = x - pinW / 2;
      const oy = y - pinH;

      const cx = ox + 18 * scale;
      const cy = oy + 16 * scale;

      const innerR = 9.5 * scale;
      const ringR  = 8   * scale;
      const dotR   = 3.5 * scale;

      this.ctx.save();
      this.ctx.globalAlpha   = alpha;
      this.ctx.shadowColor   = 'rgba(0,0,0,0.28)';
      this.ctx.shadowBlur    = 5;
      this.ctx.shadowOffsetX = 0;
      this.ctx.shadowOffsetY = 3;

      const sp = (px: number, py: number) =>
        [ox + px * scale, oy + py * scale] as [number, number];

      this.ctx.beginPath();
      this.ctx.moveTo(...sp(18, 2));
      this.ctx.bezierCurveTo(...sp(10.268, 2), ...sp(4, 8.268), ...sp(4, 16));
      this.ctx.bezierCurveTo(...sp(4, 26),     ...sp(18, 48),   ...sp(18, 48));
      this.ctx.bezierCurveTo(...sp(18, 48),    ...sp(32, 26),   ...sp(32, 16));
      this.ctx.bezierCurveTo(...sp(32, 8.268), ...sp(25.732, 2),...sp(18, 2));
      this.ctx.closePath();
      this.ctx.fillStyle = p.color;
      this.ctx.fill();

      this.ctx.shadowColor = 'transparent';
      this.ctx.shadowBlur  = 0;
      this.ctx.shadowOffsetY = 0;

      this.ctx.beginPath();
      this.ctx.arc(cx, cy, innerR, 0, Math.PI * 2);
      this.ctx.fillStyle = 'rgba(255,255,255,0.96)';
      this.ctx.fill();

      const circ   = 2 * Math.PI * ringR;
      const dash   = (Math.max(0, Math.min(100, p.progress)) / 100) * circ;
      const startA = -Math.PI / 2;

      this.ctx.beginPath();
      this.ctx.arc(cx, cy, ringR, startA, startA + dash / ringR);
      this.ctx.strokeStyle = this.rgba(p.color, 0.75);
      this.ctx.lineWidth   = 2 * scale;
      this.ctx.lineCap     = 'round';
      this.ctx.stroke();
      this.ctx.lineCap = 'butt';

      this.ctx.beginPath();
      this.ctx.arc(cx, cy, dotR, 0, Math.PI * 2);
      this.ctx.fillStyle = p.color;
      this.ctx.fill();

      this.ctx.restore();

      for (let r = 0; r < 3; r++) {
        const phase =
          (p.pulseOffset + (p.life / p.lifeSpeed) * p.pulseSpeed + r * ((Math.PI * 2) / 3)) %
          (Math.PI * 2);
        const t  = Math.sin(phase) * 0.5 + 0.5;
        const rr = innerR + t * innerR * 1.4;
        const ra = alpha * (1 - t) * 0.38;
        if (ra < 0.01) continue;

        this.ctx.beginPath();
        this.ctx.arc(cx, cy, rr, 0, Math.PI * 2);
        this.ctx.strokeStyle = this.rgba(p.color, ra);
        this.ctx.lineWidth   = 1.2;
        this.ctx.stroke();
      }
    }
  }

  // ── Draw: Lines ────────────────────────────────────────────────────────────

  private drawLines(): void {
    for (let i = this.lines.length - 1; i >= 0; i--) {
      const l = this.lines[i];

      if (!l.pinA.alive || !l.pinB.alive || l.pinA.life >= 0.85 || l.pinB.life >= 0.85) {
        this.lines.splice(i, 1);
        continue;
      }

      const headOffsetA = l.pinA.size - 16 * (l.pinA.size / 50);
      const headOffsetB = l.pinB.size - 16 * (l.pinB.size / 50);
      const ax = l.pinA.sx, ay = l.pinA.sy - headOffsetA;
      const bx = l.pinB.sx, by = l.pinB.sy - headOffsetB;
      const cp  = this.latLngToScreen(l.cpLat, l.cpLng);

      const op = Math.min(this.pinAlpha(l.pinA), this.pinAlpha(l.pinB)) * 0.65;
      if (op < 0.02) continue;

      this.ctx.beginPath();
      this.ctx.moveTo(ax, ay);
      this.ctx.quadraticCurveTo(cp.x, cp.y, bx, by);
      this.ctx.strokeStyle = `rgba(37,99,235,${(op * 0.55).toFixed(3)})`;
      this.ctx.lineWidth   = 1.1;
      this.ctx.setLineDash([5, 9]);
      this.ctx.stroke();
      this.ctx.setLineDash([]);

      l.t += l.tSpeed;
      if (l.t > 1) l.t = 0;

      const px = this.qbp(ax, cp.x, bx, l.t);
      const py = this.qbp(ay, cp.y, by, l.t);

      this.ctx.save();
      this.ctx.shadowColor = '#2563eb';
      this.ctx.shadowBlur  = 10;
      const pg = this.ctx.createRadialGradient(px, py, 0, px, py, 6);
      pg.addColorStop(0,   `rgba(255,255,255,${(op * 2.0).toFixed(2)})`);
      pg.addColorStop(0.4, `rgba(37,99,235,${(op * 1.6).toFixed(2)})`);
      pg.addColorStop(1,   'rgba(37,99,235,0)');
      this.ctx.beginPath();
      this.ctx.arc(px, py, 6, 0, Math.PI * 2);
      this.ctx.fillStyle = pg;
      this.ctx.fill();
      this.ctx.restore();
    }
  }

  // ── Utilities ──────────────────────────────────────────────────────────────

  private pinAlpha(p: Pin): number {
    if (p.life < 0.12) return p.life / 0.12;
    if (p.life < 0.80) return 1;
    return 1 - (p.life - 0.80) / 0.20;
  }

  private qbp(p0: number, p1: number, p2: number, t: number): number {
    return (1 - t) ** 2 * p0 + 2 * (1 - t) * t * p1 + t ** 2 * p2;
  }

  private rgba(hex: string, a: number): string {
    const r = parseInt(hex.slice(1, 3), 16);
    const g = parseInt(hex.slice(3, 5), 16);
    const b = parseInt(hex.slice(5, 7), 16);
    return `rgba(${r},${g},${b},${a.toFixed(3)})`;
  }

  goToRequests(): void { this.router.navigate(['/requests']); }
  goToMap():      void { this.router.navigate(['/map']);      }
  goToPosts():    void { this.router.navigate(['/posts']);    }
}
