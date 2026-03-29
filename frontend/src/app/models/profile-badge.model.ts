export interface BadgeType {
  id: string;
  name: string;
}

export interface Badge {
  id: string;
  name: string;
  description?: string;
  threshold?: number;
  badgeType?: BadgeType;
  imageBase64?: string | null;
}

export interface ProfileBadgeViewModel extends Badge {
  unlocked: boolean;
}
