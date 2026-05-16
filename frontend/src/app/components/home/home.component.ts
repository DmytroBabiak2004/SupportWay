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
import { Subscription, catchError, of } from 'rxjs';

import { ProfileService } from '../../services/profile.service';
import { Profile } from '../../models/profile.model';

declare const L: any;

// ---------------------------------------------------------------------------
// Constants
// ---------------------------------------------------------------------------

const PIN_COLORS = [
  '#2563eb',
  '#059669',
  '#d97706',
  '#7c3aed',
  '#db2777',
] as const;

type PinColor = (typeof PIN_COLORS)[number];

const PAN_PX_PER_FRAME = 0.55;
const MAX_PINS = 50;
const MAX_LINES = 18;
const SPAWN_EVERY = 12;
const INIT_LAT = 48.5;
const INIT_LNG = 31.2;
// FIX 1: Зменшений zoom
const INIT_ZOOM = 6;

// ---------------------------------------------------------------------------
// Interfaces
// ---------------------------------------------------------------------------

interface LatLng {
  lat: number;
  lng: number;
}

interface ScreenPt {
  x: number;
  y: number;
}

interface Pin {
  lat: number;
  /** Normalised longitude – always in [-180, 180] */
  lng: number;

  /** Current screen coords, updated every frame */
  sx: number;
  sy: number;

  size: number;
  progress: number;

  color: PinColor;

  life: number;
  lifeSpeed: number;

  pulseOffset: number;
  pulseSpeed: number;

  alive: boolean;
}

interface DataLine {
  pinA: Pin;
  pinB: Pin;

  /** Control-point stored as lat/lng so it follows the map */
  cpLat: number;
  cpLng: number;

  t: number;
  tSpeed: number;

  alive: boolean;
}

// ---------------------------------------------------------------------------
// World spawn points – major cities across all continents (Russia excluded)
// ---------------------------------------------------------------------------

const WORLD_CITIES: ReadonlyArray<LatLng> = [
  // Ukraine – повне покриття включно зі сходом
  { lat: 50.4501, lng: 30.5234 },  // Kyiv
  { lat: 49.8397, lng: 24.0297 },  // Lviv
  { lat: 46.4825, lng: 30.7233 },  // Odesa
  { lat: 48.4647, lng: 35.0462 },  // Dnipro
  { lat: 47.8388, lng: 35.1396 },  // Zaporizhzhia
  { lat: 49.9935, lng: 36.2304 },  // Kharkiv
  { lat: 48.5132, lng: 39.3403 },  // Luhansk (partially controlled)
  { lat: 47.9799, lng: 37.8023 },  // Donetsk
  { lat: 47.1782, lng: 39.1833 },  // Mariupol
  { lat: 48.9226, lng: 24.7111 },  // Ivano-Frankivsk
  { lat: 49.5883, lng: 34.5514 },  // Poltava
  { lat: 48.6208, lng: 22.2879 },  // Uzhhorod
  { lat: 51.5010, lng: 31.2867 },  // Chernihiv
  { lat: 49.2328, lng: 28.4682 },  // Vinnytsia
  { lat: 50.6199, lng: 26.2516 },  // Rivne
  { lat: 50.7472, lng: 25.3254 },  // Lutsk
  { lat: 48.7521, lng: 37.5932 },  // Kramatorsk
  { lat: 47.5652, lng: 34.3615 },  // Melitopol
  { lat: 46.9651, lng: 31.9946 },  // Mykolaiv
  { lat: 49.4285, lng: 32.0883 },  // Cherkasy

  // Europe
  { lat: 52.2297, lng: 21.0122 },  // Warsaw
  { lat: 50.0755, lng: 14.4378 },  // Prague
  { lat: 48.2082, lng: 16.3738 },  // Vienna
  { lat: 52.5200, lng: 13.4050 },  // Berlin
  { lat: 48.8566, lng: 2.3522  },  // Paris
  { lat: 51.5074, lng: -0.1278 },  // London
  { lat: 41.9028, lng: 12.4964 },  // Rome
  { lat: 40.4168, lng: -3.7038 },  // Madrid
  { lat: 38.7169, lng: -9.1399 },  // Lisbon
  { lat: 52.3676, lng: 4.9041  },  // Amsterdam
  { lat: 50.8503, lng: 4.3517  },  // Brussels
  { lat: 59.9139, lng: 10.7522 },  // Oslo
  { lat: 59.3293, lng: 18.0686 },  // Stockholm
  { lat: 60.1699, lng: 24.9384 },  // Helsinki
  { lat: 55.6761, lng: 12.5683 },  // Copenhagen
  { lat: 47.3769, lng: 8.5417  },  // Zurich
  { lat: 45.8150, lng: 15.9819 },  // Zagreb
  { lat: 44.8176, lng: 20.4633 },  // Belgrade
  { lat: 42.6977, lng: 23.3219 },  // Sofia
  { lat: 37.9838, lng: 23.7275 },  // Athens
  { lat: 47.4979, lng: 19.0402 },  // Budapest
  { lat: 44.4268, lng: 26.1025 },  // Bucharest
  { lat: 54.6872, lng: 25.2797 },  // Vilnius
  { lat: 56.9496, lng: 24.1052 },  // Riga
  { lat: 59.4370, lng: 24.7536 },  // Tallinn
  { lat: 53.9045, lng: 27.5615 },  // Minsk
  { lat: 41.3275, lng: 19.8187 },  // Tirana
  { lat: 42.4304, lng: 19.2594 },  // Podgorica
  { lat: 43.8563, lng: 18.4131 },  // Sarajevo
  { lat: 41.9981, lng: 21.4254 },  // Skopje
  { lat: 46.0569, lng: 14.5058 },  // Ljubljana

  // North America
  { lat: 40.7128, lng: -74.0060 }, // New York
  { lat: 34.0522, lng: -118.2437 },// Los Angeles
  { lat: 41.8781, lng: -87.6298 }, // Chicago
  { lat: 29.7604, lng: -95.3698 }, // Houston
  { lat: 33.4484, lng: -112.0740 },// Phoenix
  { lat: 39.9526, lng: -75.1652 }, // Philadelphia
  { lat: 29.9511, lng: -90.0715 }, // New Orleans
  { lat: 47.6062, lng: -122.3321 },// Seattle
  { lat: 37.7749, lng: -122.4194 },// San Francisco
  { lat: 45.5051, lng: -122.6750 },// Portland
  { lat: 25.7617, lng: -80.1918 }, // Miami
  { lat: 43.6532, lng: -79.3832 }, // Toronto
  { lat: 45.5017, lng: -73.5673 }, // Montreal
  { lat: 49.2827, lng: -123.1207 },// Vancouver
  { lat: 19.4326, lng: -99.1332 }, // Mexico City
  { lat: 20.9674, lng: -89.5926 }, // Mérida
  { lat: 51.0447, lng: -114.0719 },// Calgary
  { lat: 53.5461, lng: -113.4938 },// Edmonton
  { lat: 45.4215, lng: -75.6972 }, // Ottawa
  { lat: 44.6488, lng: -63.5752 }, // Halifax

  // Central & South America
  { lat: -23.5505, lng: -46.6333 },// São Paulo
  { lat: -22.9068, lng: -43.1729 },// Rio de Janeiro
  { lat: -12.0464, lng: -77.0428 },// Lima
  { lat: -34.6037, lng: -58.3816 },// Buenos Aires
  { lat: -33.4569, lng: -70.6483 },// Santiago
  { lat: 4.7110,  lng: -74.0721 }, // Bogotá
  { lat: 10.4806, lng: -66.9036 }, // Caracas
  { lat: -0.2295, lng: -78.5243 }, // Quito
  { lat: -16.5000, lng: -68.1500 },// La Paz
  { lat: -25.2867, lng: -57.6470 },// Asunción
  { lat: 18.4655, lng: -66.1057 }, // San Juan
  { lat: -3.1190, lng: -60.0217 }, // Manaus
  { lat: -15.7801, lng: -47.9292 },// Brasília
  { lat: -8.0578, lng: -34.8829 }, // Recife
  { lat: -30.0346, lng: -51.2177 },// Porto Alegre

  // Africa
  { lat: 30.0444, lng: 31.2357 },  // Cairo
  { lat: 6.5244,  lng: 3.3792  },  // Lagos
  { lat: -1.2921, lng: 36.8219 },  // Nairobi
  { lat: 9.0579,  lng: 7.4951  },  // Abuja
  { lat: -26.2041, lng: 28.0473 }, // Johannesburg
  { lat: -33.9249, lng: 18.4241 }, // Cape Town
  { lat: 14.7167, lng: -17.4677 }, // Dakar
  { lat: 5.3600,  lng: -4.0083  }, // Abidjan
  { lat: 3.8480,  lng: 11.5021  }, // Yaoundé
  { lat: -4.3217, lng: 15.3222  }, // Kinshasa
  { lat: -11.2027, lng: 17.8739 }, // Luanda
  { lat: 15.5007, lng: 32.5599  }, // Khartoum
  { lat: 33.8869, lng: 9.5375   }, // Tunis
  { lat: 36.7525, lng: 3.0420   }, // Algiers
  { lat: -18.9137, lng: 47.5361 }, // Antananarivo
  { lat: 12.3647, lng: -1.5331  }, // Ouagadougou
  { lat: 13.5137, lng: 2.1098   }, // Niamey
  { lat: 4.3612,  lng: 18.5550  }, // Bangui
  { lat: -6.1728, lng: 35.7395  }, // Dodoma
  { lat: -25.9667, lng: 32.5833 }, // Maputo

  // Middle East
  { lat: 31.7683, lng: 35.2137 },  // Jerusalem
  { lat: 33.8938, lng: 35.5018 },  // Beirut
  { lat: 33.3152, lng: 44.3661 },  // Baghdad
  { lat: 35.6892, lng: 51.3890 },  // Tehran
  { lat: 24.7136, lng: 46.6753 },  // Riyadh
  { lat: 25.2048, lng: 55.2708 },  // Dubai
  { lat: 23.5880, lng: 58.3829 },  // Muscat
  { lat: 31.9539, lng: 35.9106 },  // Amman
  { lat: 41.0082, lng: 28.9784 },  // Istanbul
  { lat: 32.0853, lng: 34.7818 },  // Tel Aviv
  { lat: 36.2021, lng: 37.1343 },  // Aleppo
  { lat: 33.5138, lng: 36.2765 },  // Damascus
  { lat: 29.3759, lng: 47.9774 },  // Kuwait City
  { lat: 26.2235, lng: 50.5876 },  // Manama
  { lat: 25.2854, lng: 51.5310 },  // Doha

  // Asia (non-Russia)
  { lat: 35.6762, lng: 139.6503 }, // Tokyo
  { lat: 37.5665, lng: 126.9780 }, // Seoul
  { lat: 39.9042, lng: 116.4074 }, // Beijing
  { lat: 31.2304, lng: 121.4737 }, // Shanghai
  { lat: 22.3193, lng: 114.1694 }, // Hong Kong
  { lat: 22.5726, lng: 88.3639  }, // Kolkata
  { lat: 19.0760, lng: 72.8777  }, // Mumbai
  { lat: 12.9716, lng: 77.5946  }, // Bangalore
  { lat: 28.6139, lng: 77.2090  }, // New Delhi
  { lat: 13.7563, lng: 100.5018 }, // Bangkok
  { lat: 1.3521,  lng: 103.8198 }, // Singapore
  { lat: 3.1390,  lng: 101.6869 }, // Kuala Lumpur
  { lat: -6.2088, lng: 106.8456 }, // Jakarta
  { lat: 14.5995, lng: 120.9842 }, // Manila
  { lat: 10.8231, lng: 106.6297 }, // Ho Chi Minh
  { lat: 21.0245, lng: 105.8412 }, // Hanoi
  { lat: 23.8103, lng: 90.4125  }, // Dhaka
  { lat: 6.9271,  lng: 79.8612  }, // Colombo
  { lat: 27.7172, lng: 85.3240  }, // Kathmandu
  { lat: 33.7294, lng: 73.0931  }, // Islamabad
  { lat: 24.8607, lng: 67.0011  }, // Karachi
  { lat: 31.5497, lng: 74.3436  }, // Lahore
  { lat: 34.5553, lng: 69.2075  }, // Kabul
  { lat: 37.9601, lng: 58.3261  }, // Ashgabat
  { lat: 41.2995, lng: 69.2401  }, // Tashkent
  { lat: 42.8700, lng: 74.5900  }, // Bishkek
  { lat: 43.2220, lng: 76.8512  }, // Almaty
  { lat: 47.9077, lng: 106.8832 },// Ulaanbaatar

  // Australia & Pacific
  { lat: -33.8688, lng: 151.2093 },// Sydney
  { lat: -37.8136, lng: 144.9631 },// Melbourne
  { lat: -27.4698, lng: 153.0251 },// Brisbane
  { lat: -31.9505, lng: 115.8605 },// Perth
  { lat: -36.8485, lng: 174.7633 },// Auckland
  { lat: -41.2865, lng: 174.7762 },// Wellington
  { lat: -17.7333, lng: 168.3167 },// Port Vila
  { lat: -9.4438,  lng: 160.0000 },// Honiara
  { lat: -9.4438,  lng: 147.1803 },// Port Moresby
  { lat: -35.2809, lng: 149.1300 },// Canberra
];

// ---------------------------------------------------------------------------
// Russia polygon – ВИПРАВЛЕНО: не заходить в Україну (FIX 6)
// ---------------------------------------------------------------------------

/**
 * Coarse mainland Russia polygon (counter-clockwise vertices).
 * [lng, lat] format for PIP algorithm.
 * ВАЖЛИВО: південно-західна межа відсунута від України.
 */
const RUSSIA_POLYGON: ReadonlyArray<[number, number]> = [
  [28.0, 71.5],
  [32.0, 69.5],
  [60.0, 69.5],
  [65.0, 72.0],
  [82.0, 73.5],
  [105.0, 77.0],
  [133.0, 76.0],
  [141.0, 73.0],
  [160.0, 71.0],
  [180.0, 68.0],
  [180.0, 41.5],
  [142.0, 42.0],
  [135.0, 43.0],
  [130.0, 42.5],
  [125.0, 48.0],
  [120.0, 52.0],
  [110.0, 52.5],
  [85.0, 52.0],
  [80.0, 51.0],
  [60.0, 51.5],
  [55.0, 51.0],
  // FIX 6: виправлено – відсунуто від кордону України
  [52.0, 46.0],
  [50.0, 44.5],
  [44.0, 44.0],
  [42.0, 46.0],
  [40.0, 48.0],
  [38.0, 50.0],
  // ---
  [29.0, 57.0],
  [27.0, 60.0],
  [27.5, 65.0],
  [28.0, 71.5],
];

/**
 * Ray-casting point-in-polygon test.
 */
function pointInPolygon(
  lng: number,
  lat: number,
  polygon: ReadonlyArray<[number, number]>
): boolean {
  let inside = false;
  const n = polygon.length;

  for (let i = 0, j = n - 1; i < n; j = i++) {
    const [xi, yi] = polygon[i];
    const [xj, yj] = polygon[j];

    const intersect =
      yi > lat !== yj > lat &&
      lng < ((xj - xi) * (lat - yi)) / (yj - yi) + xi;

    if (intersect) {
      inside = !inside;
    }
  }

  return inside;
}

/**
 * Fast Russia check.
 */
function isInsideRussia(lat: number, lng: number): boolean {
  const nLng = ((lng + 540) % 360) - 180;

  if (lat < 41.5 || lat > 78.0) return false;
  if (nLng < 27.0 || nLng > 180.0) return false;

  return pointInPolygon(nLng, lat, RUSSIA_POLYGON);
}

// FIX 2: Виправлено LAND_BOXES – схід України тепер повністю покритий
const LAND_BOXES: ReadonlyArray<[number, number, number, number]> = [
  // [minLat, maxLat, minLng, maxLng]
  // Europe – розширено до lng 52 щоб включити Харків, Луганськ, Донецьк
  [35, 71, -11, 52],
  // Scandinavia extension
  [57, 71, 14, 32],
  // Turkey / Caucasus
  [36, 42, 26, 48],
  // Middle East
  [12, 42, 34, 60],
  // Central Asia
  [36, 56, 46, 88],
  // South Asia
  [5, 37, 60, 98],
  // Southeast Asia (mainland)
  [1, 29, 92, 110],
  // East Asia
  [18, 54, 100, 148],
  // Japan
  [30, 46, 129, 146],
  // Korea
  [33, 39, 124, 130],
  // Africa
  [-35, 37, -20, 52],
  // Madagascar
  [-26, -11, 43, 51],
  // Australia
  [-44, -10, 112, 155],
  // New Zealand
  [-47, -34, 166, 178],
  // North America
  [24, 70, -140, -52],
  // Central America & Caribbean
  [7, 24, -92, -60],
  // South America
  [-56, 13, -82, -34],
];

function isOnLand(lat: number, lng: number): boolean {
  const nLng = ((lng + 540) % 360) - 180;

  for (const [minLat, maxLat, minLng, maxLng] of LAND_BOXES) {
    if (
      lat >= minLat &&
      lat <= maxLat &&
      nLng >= minLng &&
      nLng <= maxLng
    ) {
      return true;
    }
  }

  return false;
}

/**
 * Checks all spawn validity rules.
 * FIX 7: розширено global latitude bounds
 */
function isValidSpawnPoint(lat: number, lng: number): boolean {
  // FIX 7: було if (lat < -60 || lat > 72), тепер розширено для Скандинавії
  if (lat < -58 || lat > 80) return false;
  if (isInsideRussia(lat, lng)) return false;
  if (!isOnLand(lat, lng)) return false;

  return true;
}

// ---------------------------------------------------------------------------
// Russia label anchor points – для canvas rendering (FIX 10)
// ---------------------------------------------------------------------------
const RUSSIA_LABEL_ANCHORS: ReadonlyArray<{ lat: number; lng: number }> = [
  { lat: 62.0, lng: 55.0  },  // Ural / West Siberia
  { lat: 58.0, lng: 38.0  },  // European Russia centre
  { lat: 55.0, lng: 83.0  },  // West Siberia
  { lat: 62.0, lng: 105.0 },  // East Siberia
  { lat: 55.0, lng: 130.0 },  // Far East
  { lat: 67.0, lng: 70.0  },  // North Siberia
];

// ---------------------------------------------------------------------------
// Component
// ---------------------------------------------------------------------------

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
  isLoading = true;

  userProfile: Profile | null = null;

  get displayName(): string {
    if (this.userProfile?.fullName) return this.userProfile.fullName;
    if (this.userProfile?.name) return this.userProfile.name;
    return this.userProfile?.username ?? '';
  }

  get greeting(): string {
    const h = new Date().getHours();
    if (h < 5) return 'Добрий вечір';
    if (h < 12) return 'Доброго ранку';
    if (h < 17) return 'Добрий день';
    return 'Добрий вечір';
  }

  // -------------------------------------------------------------------------
  // Leaflet map
  // -------------------------------------------------------------------------

  private map!: any;

  /**
   * FIX 8, 9: Накопичений pan у пікселях.
   * Скидається кожен раз після world-wrap щоб уникнути floating-point drift.
   */
  private totalPanPx = 0;

  /** Width of one full world in px at current zoom (updated on zoomend). */
  private worldWidthPx = 0;

  // -------------------------------------------------------------------------
  // Canvas
  // -------------------------------------------------------------------------

  private ctx!: CanvasRenderingContext2D;
  private raf = 0;

  private W = 0;
  private H = 0;
  private dpr = 1;
  private frame = 0;

  // -------------------------------------------------------------------------
  // Pins & lines
  // -------------------------------------------------------------------------

  private pins: Pin[] = [];
  private lines: DataLine[] = [];

  // -------------------------------------------------------------------------
  // Reusable scratch objects – FIX 12: no per-frame allocations
  // -------------------------------------------------------------------------
  private readonly _scratchSc: ScreenPt = { x: 0, y: 0 };

  // -------------------------------------------------------------------------
  // Interaction
  // -------------------------------------------------------------------------

  private userInteracting = false;
  private interactionTimer: ReturnType<typeof setTimeout> | null = null;

  // -------------------------------------------------------------------------
  // DI
  // -------------------------------------------------------------------------

  private sub = new Subscription();

  private readonly cdr = inject(ChangeDetectorRef);
  private readonly router = inject(Router);
  private readonly profileService = inject(ProfileService);

  // =========================================================================
  // Lifecycle
  // =========================================================================

  ngOnInit(): void {
    setTimeout(() => {
      this.showContent = true;
      this.cdr.markForCheck();
    }, 120);

    setTimeout(() => {
      this.showButtons = true;
      this.cdr.markForCheck();
    }, 500);

    this.sub.add(
      this.profileService
        .getProfile()
        .pipe(catchError(() => of(null)))
        .subscribe((p) => {
          this.userProfile = p;
          this.isLoading = false;
          this.cdr.markForCheck();
        })
    );
  }

  ngAfterViewInit(): void {
    this.initMap();
    this.initCanvas();

    setTimeout(() => {
      this.updateWorldWidth();

      // Pre-populate with initial pins spread across the world
      let attempts = 0;
      while (this.pins.length < 30 && attempts < 600) {
        this.spawnPin(true);
        attempts++;
      }

      this.loop();
    }, 300);
  }

  ngOnDestroy(): void {
    cancelAnimationFrame(this.raf);
    this.sub.unsubscribe();

    // FIX 12: cleanup без memory leaks
    this.pins.length = 0;
    this.lines.length = 0;

    if (this.map) {
      this.map.remove();
      (this as any).map = null;
    }

    if (this.interactionTimer) {
      clearTimeout(this.interactionTimer);
      this.interactionTimer = null;
    }
  }

  @HostListener('window:resize')
  onResize(): void {
    this.resizeCanvas();

    if (this.map) {
      this.map.invalidateSize();
      this.updateWorldWidth();
    }
  }

  // =========================================================================
  // Map initialisation
  // =========================================================================

  private initMap(): void {
    this.map = L.map('home-map', {
      center: [INIT_LAT, INIT_LNG],
      zoom: INIT_ZOOM,

      zoomControl: false,
      attributionControl: true,

      worldCopyJump: false,
      maxBoundsViscosity: 0,

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
        noWrap: false,
      }
    ).addTo(this.map);

    const pauseAutoPan = () => {
      this.userInteracting = true;

      if (this.interactionTimer) {
        clearTimeout(this.interactionTimer);
      }

      this.interactionTimer = setTimeout(() => {
        this.userInteracting = false;
      }, 1500);
    };

    this.map.on('dragstart', pauseAutoPan);
    this.map.on('zoomstart', pauseAutoPan);

    this.map.on('zoomend', () => {
      this.updateWorldWidth();
    });
  }

  // =========================================================================
  // Russia label rendering (FIX 10)
  // =========================================================================

  /**
   * Renders "Житомирська область" directly on canvas over Russia territory.
   * - Без прямокутних overlay (лише canvas text)
   * - World-wrap aware (перевіряється для кожного world copy)
   * - Напівпрозорий
   * - Мінімальний вплив на FPS (без allocations в циклі)
   */
  private drawRussiaLabel(): void {
    if (!this.map) return;

    const label = 'Житомирська область';
    const centreLng = this.map.getCenter().lng;

    this.ctx.save();
    this.ctx.font = '600 12px sans-serif';
    this.ctx.textAlign = 'center';
    this.ctx.textBaseline = 'middle';
    this.ctx.fillStyle = 'rgba(110,110,110,0.72)';

    for (const anchor of RUSSIA_LABEL_ANCHORS) {
      // FIX 10: world-wrap aware – перевіряємо nearest world copy
      let anchorLng = anchor.lng;
      while (anchorLng < centreLng - 180) anchorLng += 360;
      while (anchorLng > centreLng + 180) anchorLng -= 360;

      const pt = this.map.latLngToContainerPoint([anchor.lat, anchorLng]);
      const sx = pt.x;
      const sy = pt.y;

      // Показуємо лише якщо в межах canvas з невеликим відступом
      if (sx < -80 || sx > this.W + 80 || sy < -20 || sy > this.H + 20) {
        continue;
      }

      this.ctx.fillText(label, sx, sy);
    }

    this.ctx.restore();
  }

  // =========================================================================
  // Canvas initialisation
  // =========================================================================

  private initCanvas(): void {
    this.ctx = this.animCanvas.nativeElement.getContext('2d')!;
    this.resizeCanvas();
  }

  private resizeCanvas(): void {
    const el = this.animCanvas.nativeElement;
    const container = el.parentElement!;

    this.W = container.clientWidth;
    this.H = container.clientHeight;
    this.dpr = window.devicePixelRatio || 1;

    el.width = this.W * this.dpr;
    el.height = this.H * this.dpr;

    this.ctx.setTransform(1, 0, 0, 1, 0, 0);
    this.ctx.scale(this.dpr, this.dpr);
  }

  // =========================================================================
  // World width tracking
  // =========================================================================

  private updateWorldWidth(): void {
    if (!this.map) return;

    const zoom = this.map.getZoom();
    const p0 = this.map.project([0, -180], zoom);
    const p1 = this.map.project([0,  180], zoom);

    this.worldWidthPx = Math.abs(p1.x - p0.x);
  }

  // =========================================================================
  // Coordinate helpers
  // =========================================================================

  private latLngToScreen(lat: number, lng: number): ScreenPt {
    const pt = this.map.latLngToContainerPoint([lat, lng]);
    return { x: pt.x, y: pt.y };
  }

  /**
   * FIX 8, 9: Production-grade world-wrap aware screen projection.
   *
   * Нормалізує lng піна відносно поточного центру карти,
   * щоб завжди вибирати найближчу world copy.
   * Уникає floating-point drift після багатьох циклів.
   */
  private pinToScreen(pin: Pin): ScreenPt {
    const centreLng = this.map.getCenter().lng;

    // Shift pin lng to the world copy closest to the current centre
    let lng = pin.lng;
    // Normalize without infinite loop risk using modular arithmetic
    const delta = lng - centreLng;
    lng -= Math.round(delta / 360) * 360;

    const pt = this.map.latLngToContainerPoint([pin.lat, lng]);
    this._scratchSc.x = pt.x;
    this._scratchSc.y = pt.y;
    return this._scratchSc;
  }

  // =========================================================================
  // Spawn – FIX 3, 4, 5, 8: виправлено viewport filtering та cyclic spawn
  // =========================================================================

  /**
   * Pick a random city, apply a random offset, validate, and push a new pin.
   *
   * FIX 3: lngWindow і latWindow збільшено для стабільності після world-wrap
   * FIX 4: screen culling послаблено
   * FIX 5: spread збільшено для рівномірного розподілу
   * FIX 8: spawn ніколи не деградує – fallback на global pool
   */
  private spawnPin(randomLife = false): Pin | null {
    if (this.pins.length >= MAX_PINS) return null;

    const centreLng = this.map.getCenter().lng;
    const centreLatRaw = this.map.getCenter().lat;

    const useViewportBias = Math.random() < 0.75;

    // FIX 3: збільшено вікно для стабільної роботи після world-wrap
    const lngWindow = 75;
    const latWindow = 35;

    let pool: ReadonlyArray<LatLng>;

    if (useViewportBias) {
      const candidates = WORLD_CITIES.filter((c) => {
        // FIX 9: правильна нормалізація через modular arithmetic
        const delta = c.lng - centreLng;
        const normLng = c.lng - Math.round(delta / 360) * 360;
        return (
          Math.abs(normLng - centreLng) < lngWindow &&
          Math.abs(c.lat - centreLatRaw) < latWindow
        );
      });

      // Fall back to all cities if viewport filter catches too few
      pool = candidates.length >= 3 ? candidates : WORLD_CITIES;
    } else {
      pool = WORLD_CITIES;
    }

    // FIX 8: збільшено кількість спроб для стабільного cyclic spawn
    for (let attempt = 0; attempt < 20; attempt++) {
      const base = pool[Math.floor(Math.random() * pool.length)];

      // FIX 5: spread збільшено з 1.8 до 2.8
      const spread = 2.8;
      const lat = base.lat + (Math.random() - 0.5) * spread;

      let bLng = base.lng;
      // Normalize base city lng to nearest world copy of centre
      const bDelta = bLng - centreLng;
      bLng -= Math.round(bDelta / 360) * 360;

      let lng = bLng + (Math.random() - 0.5) * spread;

      // Validate against canonical coordinates
      const canonicalLng = ((lng + 540) % 360) - 180;
      if (!isValidSpawnPoint(lat, canonicalLng)) continue;

      const pt = this.map.latLngToContainerPoint([lat, lng]);
      const sx = pt.x;
      const sy = pt.y;

      // FIX 4: послаблено screen culling з -60/+60 до -300/+300
      if (useViewportBias) {
        if (sx < -300 || sx > this.W + 300 || sy < -300 || sy > this.H + 300) {
          continue;
        }
      }

      const color = PIN_COLORS[Math.floor(Math.random() * PIN_COLORS.length)];

      const pin: Pin = {
        lat,
        // FIX 8, 9: зберігаємо canonical [-180, 180] lng для стабільності
        lng: canonicalLng,

        sx,
        sy,

        size: Math.random() * 20 + 28,
        progress: Math.random() * 100,

        color,

        life: randomLife ? Math.random() * 0.78 : 0,
        lifeSpeed: Math.random() * 0.0026 + 0.0013,

        pulseOffset: Math.random() * Math.PI * 2,
        pulseSpeed: Math.random() * 0.028 + 0.02,

        alive: true,
      };

      this.pins.push(pin);
      this.tryLine(pin);

      return pin;
    }

    return null;
  }

  // =========================================================================
  // Lines
  // =========================================================================

  private tryLine(np: Pin): void {
    if (this.lines.length >= MAX_LINES || Math.random() > 0.42) {
      return;
    }

    const pool = this.pins.filter(
      (p) =>
        p !== np &&
        p.alive &&
        p.life > 0.1 &&
        p.life < 0.82
    );

    if (!pool.length) return;

    pool.sort(
      (a, b) =>
        Math.hypot(a.sx - np.sx, a.sy - np.sy) -
        Math.hypot(b.sx - np.sx, b.sy - np.sy)
    );

    const pt = pool[0];
    const dist = Math.hypot(pt.sx - np.sx, pt.sy - np.sy);

    if (dist > 320) return;

    const msx = (np.sx + pt.sx) / 2;
    const msy = (np.sy + pt.sy) / 2;

    const bow = dist * 0.28;

    const cpSx = msx + (Math.random() - 0.5) * bow;
    const cpSy = msy - bow * 0.5 + (Math.random() - 0.5) * bow;

    const cp = this.map.containerPointToLatLng([cpSx, cpSy]);

    this.lines.push({
      pinA: np,
      pinB: pt,

      cpLat: cp.lat,
      cpLng: cp.lng,

      t: 0,
      tSpeed: Math.random() * 0.009 + 0.005,

      alive: true,
    });
  }

  // =========================================================================
  // Main animation loop – FIX 8, 9: production-grade world-wrap
  // =========================================================================

  private loop = (): void => {
    this.frame++;

    // ------------------------------------------------------------------
    // Auto-pan
    // ------------------------------------------------------------------
    if (!this.userInteracting) {
      this.map.panBy([-PAN_PX_PER_FRAME, 0], {
        animate: false,
        noMoveStart: true,
      });

      this.totalPanPx += PAN_PX_PER_FRAME;

      // FIX 9: World-wrap з точним скиданням totalPanPx для уникнення drift
      if (this.worldWidthPx > 0 && this.totalPanPx >= this.worldWidthPx) {
        const centre = this.map.getCenter();
        // Зсуваємо центр на точно +360° (один world copy вправо)
        // Це скидає Leaflet internal projection без видимого стрибка
        this.map.setView(
          [centre.lat, centre.lng + 360],
          this.map.getZoom(),
          { animate: false }
        );
        // FIX 12: точне віднімання замість =0 щоб зберегти дробову частину
        this.totalPanPx -= this.worldWidthPx;
      }
    }

    // ------------------------------------------------------------------
    // Update pin screen coords
    // ------------------------------------------------------------------
    for (const p of this.pins) {
      const sc = this.pinToScreen(p);
      p.sx = sc.x;
      p.sy = sc.y;
    }

    // FIX 12: Прибираємо піни що надто далеко від екрану (off-screen buildup)
    // Але не видаляємо – лише маркуємо для наступного кадру якщо і life >= 1
    // (видалення відбувається в drawPins по lifecycle)

    // ------------------------------------------------------------------
    // Draw
    // ------------------------------------------------------------------
    this.ctx.clearRect(0, 0, this.W, this.H);

    this.drawLines();
    this.drawPins();
    this.drawRussiaLabel();

    // ------------------------------------------------------------------
    // FIX 8: Cyclic spawn – завжди тримаємо достатню кількість пінів
    // ------------------------------------------------------------------
    if (this.frame % SPAWN_EVERY === 0) {
      this.spawnPin();

      if (this.pins.length < MAX_PINS * 0.5) {
        this.spawnPin();
        this.spawnPin();
      }
    }

    // FIX 8: Додатковий spawn якщо пінів критично мало
    if (this.pins.length < 10) {
      for (let i = 0; i < 5; i++) {
        this.spawnPin(true);
      }
    }

    this.raf = requestAnimationFrame(this.loop);
  };

  // =========================================================================
  // Drawing – Pins
  // =========================================================================

  private drawPins(): void {
    for (let i = this.pins.length - 1; i >= 0; i--) {
      const p = this.pins[i];

      p.life += p.lifeSpeed;

      if (p.life >= 1) {
        p.alive = false;
        this.pins.splice(i, 1);
        continue;
      }

      const x = p.sx;
      const y = p.sy;

      // Cull pins that are far off-screen (skip draw only, don't delete)
      if (x < -120 || x > this.W + 120 || y < -120 || y > this.H + 120) {
        continue;
      }

      const alpha = this.pinAlpha(p);

      const scale = p.size / 36;

      const pinW = p.size;
      const pinH = p.size * (50 / 36);

      const ox = x - pinW / 2;
      const oy = y - pinH;

      const cx = ox + 18 * scale;
      const cy = oy + 16 * scale;

      const innerR = 9.5 * scale;
      const ringR = 8 * scale;
      const dotR = 3.5 * scale;

      const sp = (px: number, py: number): [number, number] =>
        [ox + px * scale, oy + py * scale];

      this.ctx.save();
      this.ctx.globalAlpha = alpha;

      // -- Shadow + body --
      this.ctx.shadowColor = 'rgba(0,0,0,0.28)';
      this.ctx.shadowBlur = 5;
      this.ctx.shadowOffsetY = 3;

      this.ctx.beginPath();
      this.ctx.moveTo(...sp(18, 2));
      this.ctx.bezierCurveTo(...sp(10.268, 2), ...sp(4, 8.268), ...sp(4, 16));
      this.ctx.bezierCurveTo(...sp(4, 26), ...sp(18, 48), ...sp(18, 48));
      this.ctx.bezierCurveTo(...sp(18, 48), ...sp(32, 26), ...sp(32, 16));
      this.ctx.bezierCurveTo(...sp(32, 8.268), ...sp(25.732, 2), ...sp(18, 2));
      this.ctx.closePath();

      this.ctx.fillStyle = p.color;
      this.ctx.fill();

      // -- Inner white circle --
      this.ctx.shadowColor = 'transparent';
      this.ctx.shadowBlur = 0;
      this.ctx.shadowOffsetY = 0;

      this.ctx.beginPath();
      this.ctx.arc(cx, cy, innerR, 0, Math.PI * 2);
      this.ctx.fillStyle = 'rgba(255,255,255,0.96)';
      this.ctx.fill();

      // -- Progress arc --
      const circ = 2 * Math.PI * ringR;
      const pct = Math.max(0, Math.min(100, p.progress));
      const dash = (pct / 100) * circ;
      const startA = -Math.PI / 2;
      const endA = startA + dash / ringR;

      this.ctx.beginPath();
      this.ctx.arc(cx, cy, ringR, startA, endA);
      this.ctx.strokeStyle = this.rgba(p.color, 0.75);
      this.ctx.lineWidth = 2 * scale;
      this.ctx.lineCap = 'round';
      this.ctx.stroke();
      this.ctx.lineCap = 'butt';

      // -- Centre dot --
      this.ctx.beginPath();
      this.ctx.arc(cx, cy, dotR, 0, Math.PI * 2);
      this.ctx.fillStyle = p.color;
      this.ctx.fill();

      this.ctx.restore();

      // -- Pulse rings --
      for (let r = 0; r < 3; r++) {
        const phase =
          (p.pulseOffset +
            (p.life / p.lifeSpeed) * p.pulseSpeed +
            r * ((Math.PI * 2) / 3)) %
          (Math.PI * 2);

        const t = Math.sin(phase) * 0.5 + 0.5;
        const rr = innerR + t * innerR * 1.4;
        const ra = alpha * (1 - t) * 0.38;

        if (ra < 0.01) continue;

        this.ctx.beginPath();
        this.ctx.arc(cx, cy, rr, 0, Math.PI * 2);
        this.ctx.strokeStyle = this.rgba(p.color, ra);
        this.ctx.lineWidth = 1.2;
        this.ctx.stroke();
      }
    }
  }

  // =========================================================================
  // Drawing – Lines
  // =========================================================================

  private drawLines(): void {
    for (let i = this.lines.length - 1; i >= 0; i--) {
      const l = this.lines[i];

      if (
        !l.pinA.alive ||
        !l.pinB.alive ||
        l.pinA.life >= 0.85 ||
        l.pinB.life >= 0.85
      ) {
        this.lines.splice(i, 1);
        continue;
      }

      const headOffsetA = l.pinA.size - 16 * (l.pinA.size / 50);
      const headOffsetB = l.pinB.size - 16 * (l.pinB.size / 50);

      const ax = l.pinA.sx;
      const ay = l.pinA.sy - headOffsetA;

      const bx = l.pinB.sx;
      const by = l.pinB.sy - headOffsetB;

      const cp = this.latLngToScreen(l.cpLat, l.cpLng);
      const cpx = cp.x;
      const cpy = cp.y;

      const op =
        Math.min(this.pinAlpha(l.pinA), this.pinAlpha(l.pinB)) * 0.65;

      if (op < 0.02) continue;

      // -- Dashed curve --
      this.ctx.beginPath();
      this.ctx.moveTo(ax, ay);
      this.ctx.quadraticCurveTo(cpx, cpy, bx, by);
      this.ctx.strokeStyle = `rgba(37,99,235,${(op * 0.55).toFixed(3)})`;
      this.ctx.lineWidth = 1.1;
      this.ctx.setLineDash([5, 9]);
      this.ctx.stroke();
      this.ctx.setLineDash([]);

      // -- Moving dot --
      l.t += l.tSpeed;
      if (l.t > 1) l.t = 0;

      const px = this.qbp(ax, cpx, bx, l.t);
      const py = this.qbp(ay, cpy, by, l.t);

      this.ctx.save();
      this.ctx.shadowColor = '#2563eb';
      this.ctx.shadowBlur = 10;

      const pg = this.ctx.createRadialGradient(px, py, 0, px, py, 6);
      pg.addColorStop(0, `rgba(255,255,255,${(op * 2).toFixed(2)})`);
      pg.addColorStop(0.4, `rgba(37,99,235,${(op * 1.6).toFixed(2)})`);
      pg.addColorStop(1, 'rgba(37,99,235,0)');

      this.ctx.beginPath();
      this.ctx.arc(px, py, 6, 0, Math.PI * 2);
      this.ctx.fillStyle = pg;
      this.ctx.fill();

      this.ctx.restore();
    }
  }

  // =========================================================================
  // Utilities
  // =========================================================================

  private pinAlpha(p: Pin): number {
    if (p.life < 0.12) return p.life / 0.12;
    if (p.life < 0.8) return 1;
    return 1 - (p.life - 0.8) / 0.2;
  }

  /** Quadratic Bézier point */
  private qbp(p0: number, p1: number, p2: number, t: number): number {
    return (1 - t) ** 2 * p0 + 2 * (1 - t) * t * p1 + t ** 2 * p2;
  }

  private rgba(hex: string, a: number): string {
    const r = parseInt(hex.slice(1, 3), 16);
    const g = parseInt(hex.slice(3, 5), 16);
    const b = parseInt(hex.slice(5, 7), 16);
    return `rgba(${r},${g},${b},${a.toFixed(3)})`;
  }

  // =========================================================================
  // Navigation
  // =========================================================================

  goToRequests(): void {
    this.router.navigate(['/requests']);
  }

  goToMap(): void {
    this.router.navigate(['/map']);
  }

  goToPosts(): void {
    this.router.navigate(['/posts']);
  }
}
