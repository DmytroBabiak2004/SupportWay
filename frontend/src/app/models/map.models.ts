// ─── Support type styling ────────────────────────────────────────────────────

export interface SupportTypeDto {
  id: string;
  nameOfType: string;
}

export const SUPPORT_TYPE_STYLE: Record<string, { color: string; icon: string }> = {
  'Дрони':       { color: '#f59e0b', icon: '🚁' },
  'Автомобілі':  { color: '#3b82f6', icon: '🚗' },
  'Медицина':    { color: '#ef4444', icon: '💊' },
  'Амуніція':    { color: '#8b5cf6', icon: '🛡️' },
  'Спорядження': { color: '#10b981', icon: '🎒' },
  'Харчування':  { color: '#f97316', icon: '🍞' },
  'default':     { color: '#6b7280', icon: '🙏' },
};

export function getTypeStyle(supportTypeName: string): { color: string; icon: string } {
  return SUPPORT_TYPE_STYLE[supportTypeName] ?? SUPPORT_TYPE_STYLE['default'];
}

// ─── Map marker DTO (один маркер = один RequestItem) ────────────────────────

export interface MapMarkerDto {
  requestItemId: string;
  helpRequestId: string;

  // RequestItem
  requestItemName: string;
  quantity: number;
  unitPrice: number;
  supportTypeId: string;
  supportTypeName: string;

  // Coordinates
  latitude: number;
  longitude: number;
  locationName: string;
  locationAddress: string;

  // HelpRequest preview (для tooltip)
  title: string;
  shortContent: string;
  targetAmount: number;
  collectedAmount: number;
  isActive: boolean;
  createdAt: string;

  // Author
  userId: string;
  userName: string;

  // Stats
  likesCount: number;
  commentsCount: number;
}

// ─── Filter params ────────────────────────────────────────────────────────────

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

// ─── Paged result ─────────────────────────────────────────────────────────────

export interface PagedResult<T> {
  items: T[];
  total: number;
  page: number;
  size: number;
}

// ─── Donate ──────────────────────────────────────────────────────────────────

export interface DonateResponseDto {
  checkoutUrl: string;
  orderReference: string;
}
