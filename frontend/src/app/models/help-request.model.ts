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
  districtName?: string;
  address?: string;
  latitude?: number;
  longitude?: number;
  totalPayments: number;

  title: string;
  content: string;
  image?: string | null;
  imageBase64?: string | null;
  createdAt: string;

  userId: string;
  userName: string;
  authorUserName?: string;
  authorName?: string;
  authorFullName?: string;
  authorPhotoBase64?: string | null;
  userPhotoBase64?: string | null;
  profilePhotoBase64?: string | null;
  photoBase64?: string | null;
  photo?: string | null;
  authorIsVerified?: boolean;
  authorVerifiedAs?: number | null;

  likesCount: number;
  commentsCount: number;

  isLikedByCurrentUser?: boolean;

  preferredDonationMethod?: string | null;
  donationRecipientName?: string | null;
  donationRecipientCardNumber?: string | null;
  donationRecipientIban?: string | null;
  donationPaymentLink?: string | null;
  donationNotes?: string | null;

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
  imageBase64?: string | null;
  createdAt: string;

  userId: string;
  userName: string;
  authorUserName?: string;
  authorPhotoBase64?: string | null;
  userPhotoBase64?: string | null;
  profilePhotoBase64?: string | null;
  authorIsVerified?: boolean;
  authorVerifiedAs?: number | null;

  likesCount: number;
  commentsCount: number;
  isLikedByCurrentUser?: boolean;

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
