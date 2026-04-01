export type VerificationType = 1 | 2 | 3; // 1=Volunteer 2=Military 3=User
export type VerificationStatus = 0 | 1 | 2; // 0=Pending 1=Approved 2=Rejected

export const VERIFICATION_LABELS: Record<number, string> = {
  1: 'Волонтер',
  2: 'Військовий',
  3: 'Користувач'
};

export const VERIFICATION_ICONS: Record<number, string> = {
  1: '🟢',
  2: '🛡️',
  3: '✅'
};

export const VERIFICATION_COLORS: Record<number, string> = {
  1: '#22c55e',
  2: '#3b82f6',
  3: '#a855f7'
};

export interface VerificationRequest {
  id: string;
  userId: string;
  username: string;
  photoBase64?: string | null;
  verificationType: VerificationType;
  status: VerificationStatus;
  notes?: string;
  adminComment?: string;
  createdAt: string;
  decidedAt?: string | null;
}

export interface SubmitVerificationDto {
  verificationType: VerificationType;
  notes?: string;
}
