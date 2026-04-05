// ─── Support type styling ─────────────────────────────────────────────────────
//
// Типи з бази даних (NameOfType):
//   1. Медична допомога
//   2. Гуманітарна допомога
//   3. Логістика
//   4. Дрони та БПЛА
//   5. Евакуація
//
// Правила палітри:
//   - Без червоного (#ef4444 і подібних) — він асоціюється з тривогою/негайністю
//   - Кольори мають добре читатися на Voyager basemap
//   - Достатній контраст між типами при відображенні поруч
//   - Єдина дизайн-система: насичені але не кислотні відтінки

export interface SupportTypeDto {
  id: string;
  nameOfType: string;
}

export interface SupportTypeStyle {
  color: string;       // основний колір — fill піну, stroke прогрес-арки
  colorLight: string;  // напівпрозорий варіант — для тегів/чіпів у деталях
  colorText: string;   // колір тексту всередині чіпа
}

export const SUPPORT_TYPE_STYLES: Record<string, SupportTypeStyle> = {
  // Медична допомога — синьо-зелений (лазур медицини, без червоного)
  'Медична допомога': {
    color:      '#0891b2',   // cyan-600
    colorLight: 'rgba(8,145,178,0.13)',
    colorText:  '#0e7490',
  },

  // Гуманітарна допомога — тепло-янтарний (тепло, людяність, надія)
  'Гуманітарна допомога': {
    color:      '#d97706',   // amber-600
    colorLight: 'rgba(217,119,6,0.13)',
    colorText:  '#b45309',
  },

  // Логістика — сталево-синій (рух, доставка, структура)
  'Логістика': {
    color:      '#2563eb',   // blue-600
    colorLight: 'rgba(37,99,235,0.13)',
    colorText:  '#1d4ed8',
  },

  // Дрони та БПЛА — індиго (технологічність, авіація)
  'Дрони та БПЛА': {
    color:      '#7c3aed',   // violet-600
    colorLight: 'rgba(124,58,237,0.13)',
    colorText:  '#6d28d9',
  },

  // Евакуація — смарагдово-зелений (безпека, шлях, вихід)
  'Евакуація': {
    color:      '#059669',   // emerald-600
    colorLight: 'rgba(5,150,105,0.13)',
    colorText:  '#047857',
  },

  // Fallback для нових типів, яких ще нема в цьому маппінгу
  'default': {
    color:      '#64748b',   // slate-500
    colorLight: 'rgba(100,116,139,0.13)',
    colorText:  '#475569',
  },
};

export function getTypeStyle(supportTypeName: string): SupportTypeStyle {
  if (!supportTypeName) return SUPPORT_TYPE_STYLES['default'];

  // 1) Точний збіг
  if (SUPPORT_TYPE_STYLES[supportTypeName]) return SUPPORT_TYPE_STYLES[supportTypeName];

  // 2) Case-insensitive + trim (захист від зайвих пробілів із бекенду)
  const trimmed = supportTypeName.trim();
  const lower   = trimmed.toLowerCase();

  const exactCi = Object.keys(SUPPORT_TYPE_STYLES).find(
    k => k.toLowerCase() === lower
  );
  if (exactCi) return SUPPORT_TYPE_STYLES[exactCi];

  // 3) Partial match (підрядок) — на випадок незначних розходжень
  const partial = Object.entries(SUPPORT_TYPE_STYLES).find(
    ([k]) => k !== 'default' && (
      k.toLowerCase().includes(lower) || lower.includes(k.toLowerCase())
    )
  );
  if (partial) return partial[1];

  return SUPPORT_TYPE_STYLES['default'];
}

// ─── Map marker DTO ───────────────────────────────────────────────────────────

export interface MapMarkerDto {
  requestItemId: string;
  helpRequestId: string;

  requestItemName: string;
  quantity:        number;
  unitPrice:       number;
  supportTypeId:   string;
  supportTypeName: string;

  latitude:        number;
  longitude:       number;
  locationName:    string;
  locationAddress: string;

  title:           string;
  shortContent:    string;
  targetAmount:    number;
  collectedAmount: number;
  isActive:        boolean;
  createdAt:       string;

  userId:   string;
  userName: string;

  likesCount:    number;
  commentsCount: number;
}

// ─── Filter params ────────────────────────────────────────────────────────────

export interface MapFilterParams {
  supportTypeId?:       string;
  isActive?:            boolean;
  region?:              string;
  minCollectedAmount?:  number;
  maxTargetAmount?:     number;
  search?:              string;
  page?:                number;
  size?:                number;
}

// ─── Paged result ─────────────────────────────────────────────────────────────

export interface PagedResult<T> {
  items: T[];
  total: number;
  page:  number;
  size:  number;
}

// ─── Donate ───────────────────────────────────────────────────────────────────

export interface DonateResponseDto {
  checkoutUrl:     string;
  orderReference:  string;
}
