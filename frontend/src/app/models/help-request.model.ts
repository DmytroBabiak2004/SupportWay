export interface HelpRequestItem {
  id: string;
  helpRequestId: string;
  name: string;
  quantity: number;
  unitPrice: number;
  supportTypeId: string;
  supportTypeName: string;
}

export interface HelpRequest {
  id: string;
  locationId?: string | null;
  locationName?: string;
  locationAddress?: string;
  latitude?: number;
  longitude?: number;
  totalPayments: number;

  title: string;
  content: string;
  image?: string | null;
  createdAt: string;

  userId: string;
  userName: string;
  authorUserName?: string;
  authorPhotoBase64?: string | null;
  userPhotoBase64?: string | null; // Додай це, якщо бек шле таку назву

  likesCount: number;
  commentsCount: number;

  isLikedByCurrentUser?: boolean;

  // Funding
  targetAmount: number;
  collectedAmount: number;
  isActive: boolean;
  progressPercent: number;
  collectionStatus: 'active' | 'completed' | 'closed';

  requestItems: HelpRequestItem[];
}

export interface HelpRequestDetails {
  id: string;

  locationId?: string | null;
  locationName: string;
  locationAddress: string;
  latitude?: number | null;
  longitude?: number | null;

  title: string;
  content: string;
  /** Base64-рядок зображення (без префіксу) */
  imageBase64?: string | null;
  createdAt: string;

  userId: string;
  userName: string;
  authorPhotoBase64?: string | null;

  likesCount: number;
  commentsCount: number;

  targetAmount: number;
  collectedAmount: number;
  totalPayments: number;
  isActive: boolean;
  progressPercent: number;

  requestItems: HelpRequestItemDetails[];
}

export interface HelpRequestItemDetails {
  id: string;
  helpRequestId: string;
  name: string;
  quantity: number;
  unitPrice: number;
  supportTypeId: string;
  supportTypeName: string;
}
