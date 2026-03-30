export interface SupportTypeDto {
  id: string;
  nameOfType: string;
}
export interface DonateResponseDto {
  checkoutUrl: string;
  orderReference: string;
}

export interface RequestMapDto {
  id: string;
  title: string;
  latitude: number;
  longitude: number;
  region: string;
  targetAmount: number;
  collectedAmount: number;
  isActive: boolean;
  createdAt: string;
  supportTypes: SupportTypeDto[]; // Список типів — один запит може мати кілька
}

export interface MapFilterParams {
  supportTypeId?: string;  // Guid як string у JS/TS
  isActive?: boolean;
  region?: string;
  maxTarget?: number;
  minCollected?: number;
  page?: number;
  size?: number;
}

export interface PagedResult<T> {
  items: T[];
  total: number;
  page: number;
  size: number;
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

export function getPrimaryTypeStyle(supportTypes: SupportTypeDto[]) {
  const name = supportTypes[0]?.nameOfType ?? '';
  return SUPPORT_TYPE_STYLE[name] ?? SUPPORT_TYPE_STYLE['default'];
}
