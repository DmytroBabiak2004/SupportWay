// ─────────────────────────────────────────────────────────────────────────────
// Support type styling
// ─────────────────────────────────────────────────────────────────────────────
//
// Типи з бази даних (NameOfType):
//   1. Медична допомога
//   2. Гуманітарна допомога
//   3. Логістика
//   4. Дрони та БПЛА
//   5. Евакуація
//
// Вимоги до палітри:
//   - без червоного
//   - хороша читабельність на Voyager basemap
//   - достатній контраст між типами
//   - єдина спокійна, насичена дизайн-система

export interface SupportTypeDto {
  id: string;
  nameOfType: string;
}

export interface SupportTypeStyle {
  color: string;
  colorLight: string;
  colorText: string;
}

export const SUPPORT_TYPE_STYLES: Record<string, SupportTypeStyle> = {
  'Медична допомога': {
    color: '#0891b2',
    colorLight: 'rgba(8,145,178,0.13)',
    colorText: '#0e7490',
  },

  'Гуманітарна допомога': {
    color: '#d97706',
    colorLight: 'rgba(217,119,6,0.13)',
    colorText: '#b45309',
  },

  'Логістика': {
    color: '#2563eb',
    colorLight: 'rgba(37,99,235,0.13)',
    colorText: '#1d4ed8',
  },

  'Дрони та БПЛА': {
    color: '#7c3aed',
    colorLight: 'rgba(124,58,237,0.13)',
    colorText: '#6d28d9',
  },

  'Евакуація': {
    color: '#059669',
    colorLight: 'rgba(5,150,105,0.13)',
    colorText: '#047857',
  },

  default: {
    color: '#64748b',
    colorLight: 'rgba(100,116,139,0.13)',
    colorText: '#475569',
  },
};

export function getTypeStyle(supportTypeName?: string | null): SupportTypeStyle {
  if (!supportTypeName?.trim()) {
    return SUPPORT_TYPE_STYLES['default'];
  }

  const normalized = supportTypeName.trim();
  const normalizedLower = normalized.toLowerCase();

  const exactMatch = SUPPORT_TYPE_STYLES[normalized];
  if (exactMatch) {
    return exactMatch;
  }

  const caseInsensitiveKey = Object.keys(SUPPORT_TYPE_STYLES).find(
    (key) => key.toLowerCase() === normalizedLower
  );
  if (caseInsensitiveKey) {
    return SUPPORT_TYPE_STYLES[caseInsensitiveKey];
  }

  const partialMatch = Object.entries(SUPPORT_TYPE_STYLES).find(
    ([key]) =>
      key !== 'default' &&
      (key.toLowerCase().includes(normalizedLower) ||
        normalizedLower.includes(key.toLowerCase()))
  );
  if (partialMatch) {
    return partialMatch[1];
  }

  return SUPPORT_TYPE_STYLES['default'];
}

// ─────────────────────────────────────────────────────────────────────────────
// Map marker DTO
// ─────────────────────────────────────────────────────────────────────────────

export interface MapMarkerDto {
  requestItemId: string;
  helpRequestId: string;

  requestItemName: string;
  quantity: number;
  unitPrice: number;
  supportTypeId: string;
  supportTypeName: string;

  latitude: number;
  longitude: number;
  locationName: string;
  locationAddress: string;

  title: string;
  shortContent: string;
  targetAmount: number;
  collectedAmount: number;
  isActive: boolean;
  createdAt: string;

  userId: string;
  userName: string;

  likesCount: number;
  commentsCount: number;

  distanceKm?: number;
}

// ─────────────────────────────────────────────────────────────────────────────
// Filter params
// ─────────────────────────────────────────────────────────────────────────────

export interface MapFilterParams {
  supportTypeId?: string;
  isActive?: boolean;
  region?: string;
  minCollectedAmount?: number;
  maxTargetAmount?: number;
  search?: string;
  page?: number;
  size?: number;
}

// ─────────────────────────────────────────────────────────────────────────────
// Paged result
// ─────────────────────────────────────────────────────────────────────────────

export interface PagedResult<T> {
  items: T[];
  total: number;
  page: number;
  size: number;
}

// ─────────────────────────────────────────────────────────────────────────────
// Donate
// ─────────────────────────────────────────────────────────────────────────────

export type DonatePaymentMethod =
  | 'monobank_checkout'
  | 'bank_card'
  | 'iban'
  | 'payment_link';

export interface DonateResponseDto {
  paymentId: string;
  status: string;
  paymentMethod: DonatePaymentMethod | string;

  checkoutUrl?: string | null;
  orderReference?: string | null;

  recipientName?: string | null;
  cardNumber?: string | null;
  iban?: string | null;
  paymentLink?: string | null;
  instructions?: string | null;

  isManualTransfer: boolean;
}
