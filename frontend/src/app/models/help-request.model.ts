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
