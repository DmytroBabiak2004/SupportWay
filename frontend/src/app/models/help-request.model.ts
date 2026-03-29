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
  totalPayments: number;

  title: string;
  content: string;
  image?: string | null;
  createdAt: string;

  userId: string;
  userName: string;

  likesCount: number;
  commentsCount: number;

  authorUserName?: string;
  authorPhotoBase64?: string;
  isLikedByCurrentUser?: boolean;

  requestItems: HelpRequestItem[];
}
